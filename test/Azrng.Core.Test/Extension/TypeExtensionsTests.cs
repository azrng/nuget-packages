using System.ComponentModel;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class TypeExtensionsTests
{
    private enum TestEnum
    {
        [Description("值一")]
        First = 0,

        [Description("值二")]
        Second = 1,

        Third = 2
    }

    [Fact]
    public void CustomAttributeCommon_ShouldReturnNull_WhenFieldNameIsNull()
    {
        var result = typeof(TestEnum).CustomAttributeCommon<DescriptionAttribute>(null);

        result.Should().BeNull();
    }

    [Fact]
    public void CustomAttributeCommon_ShouldReturnNull_WhenFieldNameIsEmpty()
    {
        var result = typeof(TestEnum).CustomAttributeCommon<DescriptionAttribute>("");

        result.Should().BeNull();
    }

    [Fact]
    public void CustomAttributeCommon_ShouldReturnNull_WhenFieldNotFound()
    {
        var result = typeof(TestEnum).CustomAttributeCommon<DescriptionAttribute>("NonExistent");

        result.Should().BeNull();
    }

    [Fact]
    public void CustomAttributeCommon_ShouldReturnAttribute_WhenFieldHasAttribute()
    {
        var result = typeof(TestEnum).CustomAttributeCommon<DescriptionAttribute>("First");

        result.Should().NotBeNull();
        result!.Description.Should().Be("值一");
    }

    [Fact]
    public void CustomAttributeCommon_ShouldReturnNull_WhenFieldLacksAttribute()
    {
        var result = typeof(TestEnum).CustomAttributeCommon<DescriptionAttribute>("Third");

        result.Should().BeNull();
    }

    [Fact]
    public void ToEnumAndAttributes_ShouldReturnAllEnumValues()
    {
        var result = typeof(TestEnum).ToEnumAndAttributes<DescriptionAttribute>();

        result.Should().HaveCount(3);
        result.Should().ContainKey(TestEnum.First);
        result.Should().ContainKey(TestEnum.Second);
        result.Should().ContainKey(TestEnum.Third);
    }

    [Fact]
    public void ToEnumAndAttributes_ShouldReturnCorrectAttributes()
    {
        var result = typeof(TestEnum).ToEnumAndAttributes<DescriptionAttribute>();

        result[TestEnum.First]!.Description.Should().Be("值一");
        result[TestEnum.Second]!.Description.Should().Be("值二");
    }
}
