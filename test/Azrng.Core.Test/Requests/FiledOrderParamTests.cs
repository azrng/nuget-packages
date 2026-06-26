using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Requests;

public class FiledOrderParamTests
{
    [Fact]
    public void FiledOrderParam_DefaultValues()
    {
        var param = new FiledOrderParam();

        param.IsAsc.Should().BeFalse();
        param.PropertyName.Should().BeNull();
    }

    [Fact]
    public void FiledOrderParam_CanSetAndGetIsAsc_True()
    {
        var param = new FiledOrderParam();
        param.IsAsc = true;

        param.IsAsc.Should().BeTrue();
    }

    [Fact]
    public void FiledOrderParam_CanSetAndGetIsAsc_False()
    {
        var param = new FiledOrderParam();
        param.IsAsc = false;

        param.IsAsc.Should().BeFalse();
    }

    [Fact]
    public void FiledOrderParam_CanSetAndGetPropertyName()
    {
        var param = new FiledOrderParam();
        param.PropertyName = "CreatedTime";

        param.PropertyName.Should().Be("CreatedTime");
    }

    [Fact]
    public void FiledOrderParam_CanSetPropertyNameToNull()
    {
        var param = new FiledOrderParam();
        param.PropertyName = "SomeField";
        param.PropertyName = null;

        param.PropertyName.Should().BeNull();
    }

    [Fact]
    public void FiledOrderParam_ObjectInitializer_IsAsc()
    {
        var param = new FiledOrderParam { IsAsc = true, PropertyName = "Id" };

        param.IsAsc.Should().BeTrue();
        param.PropertyName.Should().Be("Id");
    }
}
