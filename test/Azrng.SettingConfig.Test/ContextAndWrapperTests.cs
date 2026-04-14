using System.Reflection;
using System.Text;
using System.Net;
using Azrng.SettingConfig.Dto;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Azrng.SettingConfig.Test;

public class ContextAndWrapperTests
{
    [Fact]
    public void GetHttpContext_WithNullContext_ShouldThrow()
    {
        var act = () => AspNetCoreDashboardContextExtensions.GetHttpContext(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void GetHttpContext_WithNonAspNetCoreContext_ShouldThrow()
    {
        var context = new FakeDashboardContext(TestInfrastructure.CreateOptions(), new DefaultHttpContext());

        var act = () => context.GetHttpContext();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*AspNetCoreDashboardContext*");
    }

    [Fact]
    public void GetHttpContext_WithAspNetCoreDashboardContext_ShouldReturnWrappedHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        var context = TestInfrastructure.CreateDashboardContext(httpContext);

        context.GetHttpContext().Should().BeSameAs(httpContext);
    }

    [Fact]
    public async Task AspNetCoreDashboardRequest_ShouldExposeRequestData()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/systemSetting/update";
        httpContext.Request.PathBase = "/base";
        httpContext.Request.QueryString = new QueryString("?key=demo");
        httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["value"] = "123"
        });

        var request = new AspNetCoreDashboardRequest(httpContext);

        request.Method.Should().Be(HttpMethods.Post);
        request.Path.Should().Be("/systemSetting/update");
        request.PathBase.Should().Be("/base");
        request.GetQuery("key").Should().Be("demo");
        request.LocalIpAddress.Should().Be("127.0.0.1");
        request.RemoteIpAddress.Should().Be("127.0.0.1");
        (await request.GetFormValuesAsync("value")).Should().Equal("123");
    }

    [Fact]
    public async Task AspNetCoreDashboardResponse_ShouldSetHeadersAndWriteBody()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var response = new AspNetCoreDashboardResponse(httpContext)
        {
            ContentType = "text/plain",
            StatusCode = 202
        };

        await response.WriteAsync("hello");
        httpContext.Response.Body.Position = 0;

        response.ContentType.Should().Be("text/plain");
        response.StatusCode.Should().Be(202);
        response.Body.Should().BeSameAs(httpContext.Response.Body);
        Encoding.UTF8.GetString(((MemoryStream)httpContext.Response.Body).ToArray()).Should().Be("hello");
    }

    [Fact]
    public async Task ManifestResourceService_ShouldReturnEmbeddedIndexHtml()
    {
        var service = new ManifestResourceService();

        var bytes = await service.GetManifestResource();
        var html = Encoding.UTF8.GetString(bytes);

        bytes.Should().NotBeEmpty();
        html.Should().Contain("%(PageTitle)%");
        html.Should().Contain("%(BaseUrl)%");
    }
}
