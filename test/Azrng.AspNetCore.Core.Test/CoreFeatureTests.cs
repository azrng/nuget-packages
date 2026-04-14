using Azrng.AspNetCore.Core.Extension;
using Azrng.AspNetCore.Core.JsonConverters;
using Azrng.AspNetCore.Core.Model;
using Azrng.AspNetCore.Core.PreConfigure;
using Azrng.Core.Results;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
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
}

public class SampleOptions
{
    public string? Name { get; set; }

    public List<string> Trace { get; } = [];
}
