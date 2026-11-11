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

    [Fact]
    public void UnauthorizedException_Default_ShouldUse401CodeAndMessage()
    {
        var exception = new UnauthorizedException();

        exception.ErrorCode.Should().Be("401");
        exception.Message.Should().Be("未认证");
    }

    [Fact]
    public void UnauthorizedException_WithMessage_ShouldPreserveCodeAndMessage()
    {
        var exception = new UnauthorizedException("token 已过期");

        exception.ErrorCode.Should().Be("401");
        exception.Message.Should().Be("token 已过期");
    }

    [Fact]
    public void ForbiddenException_AllConstructors_ShouldUseConsistent403Code()
    {
        new ForbiddenException().ErrorCode.Should().Be("403");
        new ForbiddenException("无权限").ErrorCode.Should().Be("403");
        new ForbiddenException("无权限", new InvalidOperationException()).ErrorCode.Should().Be("403");
    }

    [Fact]
    public void LogicBusinessException_AllConstructors_ShouldUseConsistent400Code()
    {
        new LogicBusinessException().ErrorCode.Should().Be("400");
        new LogicBusinessException("业务失败").ErrorCode.Should().Be("400");
        new LogicBusinessException("业务失败", new InvalidOperationException()).ErrorCode.Should().Be("400");
    }

    [Fact]
    public void ParameterException_WithInnerException_ShouldUse400Code()
    {
        new ParameterException("参数缺失", new InvalidOperationException()).ErrorCode.Should().Be("400");
    }

    [Fact]
    public void NotFoundException_AllConstructors_ShouldUseConsistent404Code()
    {
        new NotFoundException().ErrorCode.Should().Be("404");
        new NotFoundException("用户不存在").ErrorCode.Should().Be("404");
        new NotFoundException("用户不存在", new InvalidOperationException()).ErrorCode.Should().Be("404");
    }

    [Fact]
    public void RetryMarkExceptionThrowIfNull_ShouldThrowRetryMarkException_WhenArgumentIsNull()
    {
        var action = () => RetryMarkException.ThrowIfNull(null, "userId");

        action.Should().Throw<RetryMarkException>()
            .Which.Message.Should().Be("userId");
    }
}
