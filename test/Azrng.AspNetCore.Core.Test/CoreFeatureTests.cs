using Azrng.AspNetCore.Core.AuditLog;
using Azrng.AspNetCore.Core.Extension;
using Azrng.AspNetCore.Core.Filter;
using Azrng.AspNetCore.Core.JsonConverters;
using Azrng.AspNetCore.Core.Middleware;
using Azrng.AspNetCore.Core.Model;
using Azrng.AspNetCore.Core.PreConfigure;
using Azrng.Core.Results;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using Xunit;

namespace Azrng.AspNetCore.Core.Test;

public class CoreFeatureTests
{
    [Fact]
    public void CollectionNotEmptyAttribute_ValidatesEnumerableContent()
    {
        var attribute = new CollectionNotEmptyAttribute();

        attribute.IsValid(new[] { 1 }).Should().BeTrue();
        attribute.IsValid(Array.Empty<int>()).Should().BeFalse();
        attribute.IsValid(null).Should().BeFalse();
    }

    [Fact]
    public void MinValueAttribute_ValidatesNumericTypesAgainstMinimum()
    {
        var attribute = new MinValueAttribute(5);

        attribute.IsValid(5).Should().BeTrue();
        attribute.IsValid(6L).Should().BeTrue();
        attribute.IsValid(4m).Should().BeFalse();
        attribute.IsValid(4.9d).Should().BeFalse();
        attribute.IsValid("not-a-number").Should().BeTrue();
    }

    [Fact]
    public void LongToStringConverter_SerializesAndDeserializesAsExpected()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new LongToStringConverter());

        var json = JsonSerializer.Serialize(1234567890123L, options);
        var fromString = JsonSerializer.Deserialize<long>("\"1234567890123\"", options);
        var fromNumber = JsonSerializer.Deserialize<long>("1234567890123", options);

        json.Should().Be("\"1234567890123\"");
        fromString.Should().Be(1234567890123L);
        fromNumber.Should().Be(1234567890123L);
    }

    [Fact]
    public void PreConfigureActionList_ExecutesActionsInOrderAndCanInstantiateOptions()
    {
        var list = new PreConfigureActionList<SampleOptions>
        {
            options => options.Trace.Add("first"),
            options => options.Trace.Add("second")
        };

        var configured = list.Configure();

        configured.Trace.Should().Equal("first", "second");
    }

    [Fact]
    public void AddObjectAccessor_RegistersAccessorAndPreventsDuplicates()
    {
        var services = new ServiceCollection();
        var accessor = services.AddObjectAccessor(new SampleOptions { Name = "demo" });

        services.GetObjectOrNull<SampleOptions>()!.Name.Should().Be("demo");
        accessor.Value!.Name.Should().Be("demo");

        var act = () => services.AddObjectAccessor(new ObjectAccessor<SampleOptions>(new SampleOptions()));

        act.Should().Throw<Exception>()
            .WithMessage("*object accessor*");
    }

    [Fact]
    public void PreConfigure_StoresAndReusesActionList()
    {
        var services = new ServiceCollection();

        services.PreConfigure<SampleOptions>(options => options.Trace.Add("configured"));
        var list1 = services.GetPreConfigureActions<SampleOptions>();
        var list2 = services.GetPreConfigureActions<SampleOptions>();

        ReferenceEquals(list1, list2).Should().BeTrue();
        list1.Configure().Trace.Should().ContainSingle().Which.Should().Be("configured");
    }

    [Fact]
    public void AddAnyCors_RegistersPermissiveDefaultPolicy()
    {
        var services = new ServiceCollection();
        services.AddAnyCors();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = options.GetPolicy("DefaultCors");

        policy.Should().NotBeNull();
        policy!.Origins.Should().Contain("*");
        policy.Methods.Should().Contain("*");
        policy.Headers.Should().Contain("*");
    }

    [Fact]
    public void AddCorsByOrigins_RegistersOriginsAndCredentials()
    {
        var services = new ServiceCollection();
        services.AddCorsByOrigins(["https://a.example", "https://b.example"], allowCredentials: true);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = options.GetPolicy("DefaultCors");

        policy.Should().NotBeNull();
        policy!.Origins.Should().Contain(["https://a.example", "https://b.example"]);
        policy.SupportsCredentials.Should().BeTrue();
    }

    [Fact]
    public void AddShowAllServices_StoresPathAndServiceSnapshot()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SampleOptions>();

        services.AddShowAllServices("/services");

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ShowServiceConfig>>().Value;

        options.Path.Should().Be("/services");
        options.Services.Should().Contain(x => x.ServiceType == typeof(SampleOptions));
    }

    [Fact]
    public async Task ShowAllServicesMiddleware_HtmlEncodesServiceNames()
    {
        var dangerousType = CreateDynamicType("Html<Dangerous>Service");
        var descriptor = ServiceDescriptor.Singleton(dangerousType, dangerousType);
        var context = new DefaultHttpContext();
        context.Request.Path = "/services";
        context.Response.Body = new MemoryStream();
        var options = Options.Create(new ShowServiceConfig
        {
            Path = "/services",
            Services = [descriptor]
        });
        var middleware = new ShowAllServicesMiddleware(_ => Task.CompletedTask, options);

        await middleware.Invoke(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var html = await reader.ReadToEndAsync();
        html.Should().Contain("Html&lt;Dangerous&gt;Service");
        html.Should().NotContain("Html<Dangerous>Service");
    }

    [Fact]
    public void AddMvcModelVerifyFilter_BuildsBadRequestResultFromModelState()
    {
        var services = new ServiceCollection();
        services.AddMvcModelVerifyFilter();

        using var provider = services.BuildServiceProvider();
        var apiBehaviorOptions = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        var httpContext = new DefaultHttpContext { RequestServices = provider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        actionContext.ModelState.AddModelError("Name", "Name is required");
        actionContext.ModelState.AddModelError("Age", "Age is invalid");

        var result = apiBehaviorOptions.InvalidModelStateResponseFactory!(actionContext);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var payload = badRequest.Value.Should().BeOfType<ResultModel>().Subject;

        payload.IsSuccess.Should().BeFalse();
        payload.Message.Should().Be("参数格式不正确");
        payload.Code.Should().Be(StatusCodes.Status400BadRequest.ToString());
        payload.Errors.Should().BeAssignableTo<IReadOnlyCollection<ErrorInfo>>();
    }

    [Fact]
    public void CustomResultPackFilter_SkipsValuesImplementingIResultModel()
    {
        var filter = new CustomResultPackFilter();
        var payload = new CustomResult("ok");
        var context = CreateResultExecutingContext(new ObjectResult(payload));

        filter.OnResultExecuting(context);

        var objectResult = context.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.Value.Should().BeSameAs(payload);
    }

    [Fact]
    public void GetBaseUrl_ReturnsSchemeWhenHostIsEmpty()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = default;

        context.Request.GetBaseUrl().Should().Be("https");
    }

    [Fact]
    public async Task AuditLogMiddleware_RestoresResponseBodyWhenNextThrows()
    {
        var responseFeature = new CapturingResponseFeature();
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();
        var logger = new CapturingLoggerService();
        services.AddSingleton<ILoggerService>(logger);
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        var originalBody = context.Response.Body;
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/orders";
        var middleware = new AuditLogMiddleware(_ => throw new InvalidOperationException("boom"));

        var act = () => middleware.Invoke(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("boom");
        context.Response.Body.Should().BeSameAs(originalBody);

        await responseFeature.InvokeCompletedAsync();
        logger.Log.Should().NotBeNull();
        logger.Log!.EndTime.Should().BeOnOrAfter(logger.Log.StartTime);
        logger.Log.ResponseBody.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestIdMiddleware_GeneratesRequestIdWhenHeaderIsMissing()
    {
        var context = new DefaultHttpContext();
        string? traceIdentifierInNext = null;
        var middleware = new RequestIdMiddleware(next =>
        {
            traceIdentifierInNext = next.TraceIdentifier;
            return Task.CompletedTask;
        });

        await middleware.Invoke(context);

        context.Response.Headers["X-RequestId"].ToString().Should().NotBeNullOrWhiteSpace();
        context.TraceIdentifier.Should().Be(context.Response.Headers["X-RequestId"].ToString());
        traceIdentifierInNext.Should().Be(context.TraceIdentifier);
    }

    [Fact]
    public async Task RequestIdMiddleware_UsesRequestIdFromHeader()
    {
        const string requestId = "request-id-from-client";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-RequestId"] = requestId;
        var middleware = new RequestIdMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context);

        context.TraceIdentifier.Should().Be(requestId);
        context.Response.Headers["X-RequestId"].ToString().Should().Be(requestId);
    }

    private static ResultExecutingContext CreateResultExecutingContext(IActionResult result)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ResultExecutingContext(actionContext, [], result, controller: new object());
    }

    private static Type CreateDynamicType(string typeName)
    {
        var assemblyName = new AssemblyName("Azrng.AspNetCore.Core.Test.DynamicTypes");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("Main");
        return moduleBuilder.DefineType(typeName, TypeAttributes.Public).CreateType();
    }
}

public class SampleOptions
{
    public string? Name { get; set; }

    public List<string> Trace { get; } = [];
}

internal class CustomResult : IResultModel
{
    public CustomResult(string message)
    {
        Message = message;
    }

    public bool IsSuccess => true;

    public bool IsFailure => false;

    public string Message { get; }

    public string Code => "200";

    public IEnumerable<ErrorInfo> Errors => [];
}

internal class CapturingLoggerService : ILoggerService
{
    public AuditLogInfo? Log { get; private set; }

    public void Write(AuditLogInfo log)
    {
        Log = log;
    }

    public Task WriteAsync(AuditLogInfo log)
    {
        Log = log;
        return Task.CompletedTask;
    }
}

internal class CapturingResponseFeature : IHttpResponseFeature
{
    private readonly List<(Func<object, Task> Callback, object State)> _completedCallbacks = [];

    public int StatusCode { get; set; } = StatusCodes.Status200OK;

    public string? ReasonPhrase { get; set; }

    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

    public Stream Body { get; set; } = new MemoryStream();

    public bool HasStarted { get; set; }

    public void OnStarting(Func<object, Task> callback, object state) { }

    public void OnCompleted(Func<object, Task> callback, object state)
    {
        _completedCallbacks.Add((callback, state));
    }

    public async Task InvokeCompletedAsync()
    {
        foreach (var (callback, state) in _completedCallbacks)
        {
            await callback(state);
        }
    }
}
