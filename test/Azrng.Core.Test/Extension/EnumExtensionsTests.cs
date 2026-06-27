using System.ComponentModel;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class EnumExtensionsTests
{
    private enum TestEnum
    {
        [Description("第一个值")]
        [EnglishDescription("First value")]
        First = 0,

        [Description("第二个值")]
        [EnglishDescription("Second value")]
        Second = 1,

        Third = 2
    }

    [Fact]
    public void GetDescription_ShouldReturnDescription_WhenAttributeExists()
    {
        TestEnum.First.GetDescription().Should().Be("第一个值");
        TestEnum.Second.GetDescription().Should().Be("第二个值");
    }

    [Fact]
    public void GetDescription_ShouldReturnEmpty_WhenNoAttribute()
    {
        TestEnum.Third.GetDescription().Should().BeEmpty();
    }

    [Fact]
    public void GetEnglishDescription_ShouldReturnEnglishDescription_WhenAttributeExists()
    {
        TestEnum.First.GetEnglishDescription().Should().Be("First value");
        TestEnum.Second.GetEnglishDescription().Should().Be("Second value");
    }

    [Fact]
    public void GetEnglishDescription_ShouldReturnEmpty_WhenNoAttribute()
    {
        TestEnum.Third.GetEnglishDescription().Should().BeEmpty();
    }

    [Fact]
    public void GetCustomerAttribute_ShouldReturnAttribute_WhenExists()
    {
        var attr = TestEnum.First.GetCustomerAttribute<DescriptionAttribute>();

        attr.Should().NotBeNull();
        attr!.Description.Should().Be("第一个值");
    }

    [Fact]
    public void GetCustomerAttribute_ShouldReturnNull_WhenNotExists()
    {
        var attr = TestEnum.Third.GetCustomerAttribute<EnglishDescriptionAttribute>();

        attr.Should().BeNull();
    }

    [Fact]
    public void IsDefined_ShouldReturnTrue_ForDefinedValue()
    {
        TestEnum.First.IsDefined().Should().BeTrue();
        TestEnum.Second.IsDefined().Should().BeTrue();
        TestEnum.Third.IsDefined().Should().BeTrue();
    }

    [Fact]
    public void IsDefined_ShouldReturnFalse_ForUndefinedValue()
    {
        var undefined = (TestEnum)99;
        undefined.IsDefined().Should().BeFalse();
    }
}
