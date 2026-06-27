using Azrng.Core.Helpers;
using FluentAssertions;
using System.ComponentModel;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class EnumHelperTests
{
    #region Test Enums

    private enum Status
    {
        [Description("活跃状态")]
        Active = 1,
        [Description("非活跃状态")]
        Inactive = 2,
        [Description("待审核状态")]
        Pending = 3
    }

    private enum SimpleEnum
    {
        First = 0,
        Second = 1,
        Third = 2
    }

    private enum EmptyEnum
    {
    }

    #endregion

    #region GetEnumValue Tests

    [Fact]
    public void GetEnumValue_WithDescriptionAttribute_ReturnsCorrectValue()
    {
        var result = EnumHelper.GetEnumValue<Status>("活跃状态");
        result.Should().Be(Status.Active);
    }

    [Fact]
    public void GetEnumValue_WithEnumName_ReturnsCorrectValue()
    {
        var result = EnumHelper.GetEnumValue<SimpleEnum>("First");
        result.Should().Be(SimpleEnum.First);
    }

    [Fact]
    public void GetEnumValue_WithInvalidDescription_ThrowsArgumentException()
    {
        Action act = () => EnumHelper.GetEnumValue<Status>("不存在的描述");
        act.Should().Throw<ArgumentException>()
           .WithMessage("*未能找到对应的枚举*");
    }

    [Fact]
    public void GetEnumValue_WithInvalidName_ThrowsArgumentException()
    {
        Action act = () => EnumHelper.GetEnumValue<SimpleEnum>("NonExistent");
        act.Should().Throw<ArgumentException>()
           .WithMessage("*未能找到对应的枚举*");
    }

    #endregion

    #region EnumToDictionary Tests

    [Fact]
    public void EnumToDictionary_WithDescriptionAttribute_ReturnsDictionaryWithDescriptions()
    {
        var result = EnumHelper.EnumToDictionary<Status>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[1].Should().Be("活跃状态");
        result[2].Should().Be("非活跃状态");
        result[3].Should().Be("待审核状态");
    }

    [Fact]
    public void EnumToDictionary_WithoutDescriptionAttribute_ReturnsDictionaryWithEmptyStrings()
    {
        var result = EnumHelper.EnumToDictionary<SimpleEnum>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Should().BeEmpty();
        result[1].Should().BeEmpty();
        result[2].Should().BeEmpty();
    }

    [Fact]
    public void EnumToDictionary_WithEmptyEnum_ReturnsEmptyDictionary()
    {
        var result = EnumHelper.EnumToDictionary<EmptyEnum>();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetKeys Tests

    [Fact]
    public void GetKeys_ReturnsAllEnumNames()
    {
        var result = EnumHelper.GetKeys<SimpleEnum>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain("First");
        result.Should().Contain("Second");
        result.Should().Contain("Third");
    }

    [Fact]
    public void GetKeys_WithDescriptionAttribute_ReturnsEnumNamesNotDescriptions()
    {
        var result = EnumHelper.GetKeys<Status>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain("Active");
        result.Should().Contain("Inactive");
        result.Should().Contain("Pending");
    }

    [Fact]
    public void GetKeys_WithEmptyEnum_ReturnsEmptyList()
    {
        var result = EnumHelper.GetKeys<EmptyEnum>();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetValues Tests

    [Fact]
    public void GetValues_ReturnsAllEnumValues()
    {
        var result = EnumHelper.GetValues<SimpleEnum>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(0);
        result.Should().Contain(1);
        result.Should().Contain(2);
    }

    [Fact]
    public void GetValues_WithCustomValues_ReturnsCorrectIntValues()
    {
        var result = EnumHelper.GetValues<Status>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(1);
        result.Should().Contain(2);
        result.Should().Contain(3);
    }

    [Fact]
    public void GetValues_WithEmptyEnum_ReturnsEmptyList()
    {
        var result = EnumHelper.GetValues<EmptyEnum>();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion
}
