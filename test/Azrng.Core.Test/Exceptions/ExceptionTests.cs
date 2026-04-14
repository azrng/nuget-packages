using Azrng.Core.Exceptions;
using FluentAssertions;
using System.Net;
using Xunit;

namespace Azrng.Core.Test.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void BaseException_ShouldUseDefaultCodeAndHttpCode()
    {
        var exception = new BaseException("boom");

        exception.ErrorCode.Should().Be("500");
        exception.HttpCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Message.Should().Be("boom");
    }

    [Fact]
    public void BaseException_WithCode_ShouldPreserveCodeAndMessage()
    {
        var exception = new BaseException("401", "denied");

        exception.ErrorCode.Should().Be("401");
        exception.Message.Should().Be("denied");
    }

    [Fact]
    public void ParameterExceptionThrowIfNull_ShouldThrow_WhenArgumentIsNull()
    {
        var action = () => ParameterException.ThrowIfNull(null, "userId");

        action.Should().Throw<ParameterException>()
            .Which.Message.Should().Be("userId");
    }

    [Fact]
    public void ParameterExceptionThrowIfNull_ShouldNotThrow_WhenArgumentExists()
    {
        var action = () => ParameterException.ThrowIfNull(new object(), "userId");

        action.Should().NotThrow();
    }

    [Fact]
    public void RetryMarkException_WithInnerException_ShouldPreserveInnerException()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new RetryMarkException(inner);

        exception.Message.Should().Be("需要重试");
        exception.InnerException.Should().BeSameAs(inner);
        exception.ErrorCode.Should().Be("500");
    }
}
