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
    public void GenericFailureFactory_ShouldCarryFailureMetadata()
    {
        var result = ResultModel<string>.Failure("failed", "500");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().Be("500");
        result.Message.Should().Be("failed");
    }
}
