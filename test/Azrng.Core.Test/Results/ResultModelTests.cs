using Azrng.Core.Exceptions;
using Azrng.Core.Results;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Results;

public class ResultModelTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeDefaultState()
    {
        var result = new ResultModel();

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be("200");
        result.Message.Should().Be("success");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void SuccessFactory_ShouldReturnSuccessfulResult()
    {
        var result = ResultModel.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Code.Should().Be("200");
        result.Message.Should().Be("success");
    }

    [Fact]
    public void ErrorFactory_ShouldReturnFailedResultAndPreserveErrors()
    {
        var errors = new[]
        {
            new ErrorInfo { Field = "Name", Message = "Required" }
        };

        var result = ResultModel.Error("bad request", "400", errors);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be("400");
        result.Message.Should().Be("bad request");
        result.Errors.Should().ContainSingle().Which.Field.Should().Be("Name");
    }

    [Fact]
    public void GenericSuccessFactory_ShouldCarryData()
    {
        var result = ResultModel<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(42);
        result.Code.Should().Be("200");
        result.Message.Should().Be("success");
    }

    [Fact]
    public void GenericSuccessFactory_ShouldPreserveCustomMessage()
    {
        var result = ResultModel<int>.Success(42, "created");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(42);
        result.Code.Should().Be("200");
        result.Message.Should().Be("created");
    }

    [Fact]
    public void GenericFailureFactory_ShouldCarryFailureMetadata()
    {
        var result = ResultModel<string>.Failure("failed", "500");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().Be("500");
        result.Message.Should().Be("failed");
    }

    [Fact]
    public void ResultModelFactorySuccess_ShouldPreserveMessage()
    {
        var result = ResultModelFactory.Success("value", "loaded");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("value");
        result.Code.Should().Be("200");
        result.Message.Should().Be("loaded");
    }

    [Fact]
    public void ResultModelFactoryFailure_ShouldUseProvidedErrorCode()
    {
        var result = ResultModelFactory.Failure<string>("failed", "E001");

        result.IsFailure.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Code.Should().Be("E001");
        result.Message.Should().Be("failed");
    }

    [Fact]
    public void FromException_ShouldUseBaseExceptionErrorCode()
    {
        var exception = new BaseException("BIZ001", "business failed");

        var result = ResultModelFactory.FromException<string>(exception);

        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be("BIZ001");
        result.Message.Should().Be("business failed");
    }

    [Fact]
    public void ToFailureResult_ShouldAllowMessageOverride()
    {
        var exception = new InvalidOperationException("internal details");

        var result = exception.ToFailureResult<string>("operation failed");

        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be("ERROR");
        result.Message.Should().Be("operation failed");
    }

    [Fact]
    public void DataOrEmpty_ShouldReturnEmptyList_WhenDataIsNull()
    {
        var result = ResultModel<List<int>>.Failure("failed");

        var data = result.DataOrEmpty();

        data.Should().BeEmpty();
    }

    [Fact]
    public void DataOrEmpty_ShouldReturnEmptyReadOnlyList_WhenDataIsNull()
    {
        IResultModel<IReadOnlyList<int>> result = ResultModel<IReadOnlyList<int>>.Failure("failed");

        var data = result.DataOrEmpty();

        data.Should().BeEmpty();
    }

    [Fact]
    public void DataOrDefault_ShouldReturnDefaultValue_WhenDataIsNull()
    {
        var result = ResultModel<string>.Failure("failed");

        var data = result.DataOrDefault("default");

        data.Should().Be("default");
    }
}
