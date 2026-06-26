using Azrng.Core.Results;
using Azrng.Core.Service;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Service;

public class BaseServiceTests
{
    private readonly TestService _service = new();

    private class TestService : BaseService
    {
        public IResultModel CallSuccess() => Success();
        public IResultModel<T> CallSuccess<T>(T data) => Success(data);
        public IResultModel CallFail(string message = "错误") => Fail(message);
        public IResultModel<T> CallFail<T>(string message = "错误") => Fail<T>(message);
        public IResultModel CallFail(string message, string errorCode) => Fail(message, errorCode);
        public IResultModel<T> CallFail<T>(string message, string errorCode) => Fail<T>(message, errorCode);
    }

    [Fact]
    public void Success_ShouldReturnSuccessfulResult()
    {
        var result = _service.CallSuccess();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Success_WithGenericData_ShouldReturnResultWithData()
    {
        var result = _service.CallSuccess(42);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(42);
        result.Message.Should().Be("success");
        result.Code.Should().Be("200");
    }

    [Fact]
    public void Success_WithStringData_ShouldReturnResultWithData()
    {
        var result = _service.CallSuccess("hello");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("hello");
        result.Message.Should().Be("success");
        result.Code.Should().Be("200");
    }

    [Fact]
    public void Success_WithNullData_ShouldReturnResultWithNullData()
    {
        var result = _service.CallSuccess<string?>(null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldReturnFailedResultWithDefaultMessage()
    {
        var result = _service.CallFail();

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Message.Should().Be("错误");
        result.Code.Should().Be("400");
    }

    [Fact]
    public void Fail_WithCustomMessage_ShouldReturnFailedResultWithMessage()
    {
        var result = _service.CallFail("自定义错误");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("自定义错误");
        result.Code.Should().Be("400");
    }

    [Fact]
    public void Fail_Generic_ShouldReturnFailedResultWithDefaultData()
    {
        var result = _service.CallFail<int>();

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Data.Should().Be(default(int));
        result.Message.Should().Be("错误");
        result.Code.Should().Be("400");
    }

    [Fact]
    public void Fail_Generic_WithCustomMessage_ShouldReturnFailedResultWithMessage()
    {
        var result = _service.CallFail<string>("业务异常");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("业务异常");
        result.Code.Should().Be("400");
    }

    [Fact]
    public void Fail_WithMessageAndErrorCode_ShouldReturnFailedResult()
    {
        var result = _service.CallFail("服务器错误", "500");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("服务器错误");
        result.Code.Should().Be("500");
    }

    [Fact]
    public void Fail_Generic_WithMessageAndErrorCode_ShouldReturnFailedResult()
    {
        var result = _service.CallFail<string>("未授权", "401");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("未授权");
        result.Code.Should().Be("401");
    }
}
