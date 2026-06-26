using System.Dynamic;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class ObjectExtensionsTests
{
    #region Test Helper Classes

    private class TestModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? City { get; set; }
        public bool Active { get; set; }
    }

    private class NullableModel
    {
        public string? Name { get; set; }
        public int? Score { get; set; }
    }

    #endregion

    #region ToUrlParameter

    [Fact]
    public void ToUrlParameter_WithNonNullObject_ReturnsUrlString()
    {
        var model = new TestModel { Name = "test", Age = 25, City = "beijing", Active = true };

        var result = model.ToUrlParameter();

        result.Should().Contain("Name=test");
        result.Should().Contain("Age=25");
        result.Should().Contain("City=beijing");
        result.Should().Contain("Active=True");
        result.Should().NotEndWith("&");
    }

    [Fact]
    public void ToUrlParameter_WithParamLower_ReturnsLowercaseKeys()
    {
        var model = new TestModel { Name = "test", Age = 25 };

        var result = model.ToUrlParameter(paramLower: true);

        result.Should().Contain("name=test");
        result.Should().Contain("age=25");
    }

    [Fact]
    public void ToUrlParameter_WithNullProperty_SkipsNullValues()
    {
        var model = new TestModel { Name = null, Age = 25, City = null };

        var result = model.ToUrlParameter();

        result.Should().NotContain("Name=");
        result.Should().NotContain("City=");
        result.Should().Contain("Age=25");
    }

    [Fact]
    public void ToUrlParameter_WithNullSource_ThrowsArgumentNullException()
    {
        TestModel? model = null;

        Action act = () => model!.ToUrlParameter();

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToDictionary

    [Fact]
    public void ToDictionary_WithNonNullObject_ReturnsDictionary()
    {
        var model = new TestModel { Name = "test", Age = 25, City = "beijing" };

        var result = model.ToDictionary();

        result.Should().ContainKey("Name").WhoseValue.Should().Be("test");
        result.Should().ContainKey("Age").WhoseValue.Should().Be("25");
        result.Should().ContainKey("City").WhoseValue.Should().Be("beijing");
    }

    [Fact]
    public void ToDictionary_WithParamLower_ReturnsLowercaseKeys()
    {
        var model = new TestModel { Name = "test" };

        var result = model.ToDictionary(paramLower: true);

        result.Should().ContainKey("name").WhoseValue.Should().Be("test");
    }

    [Fact]
    public void ToDictionary_WithNullProperty_SkipsNullValues()
    {
        var model = new TestModel { Name = null, Age = 30 };

        var result = model.ToDictionary();

        result.Should().NotContainKey("Name");
        result.Should().ContainKey("Age");
    }

    [Fact]
    public void ToDictionary_WithValueType_ReturnsStringRepresentation()
    {
        var model = new TestModel { Active = true };

        var result = model.ToDictionary();

        result.Should().ContainKey("Active").WhoseValue.Should().Be("True");
    }

    [Fact]
    public void ToDictionary_WithEmptyObject_ReturnsEmptyDictionary()
    {
        var model = new NullableModel();

        var result = model.ToDictionary();

        result.Should().BeEmpty();
    }

    #endregion

    #region To<TFrom, TTo> with defaultValue

    [Fact]
    public void To_WithNonNullValue_ConvertsType()
    {
        int from = 123;

        var result = from.To(0);

        result.Should().Be(123);
    }

    [Fact]
    public void To_WithNullValue_ReturnsDefault()
    {
        string? from = null;

        var result = from.To("default");

        result.Should().Be("default");
    }

    [Fact]
    public void To_WithIncompatibleType_ReturnsDefault()
    {
        string from = "not_a_number";

        var result = from.To(0);

        result.Should().Be(0);
    }

    [Fact]
    public void To_WithEnumConversion_ParsesEnum()
    {
        string from = "Red";

        var result = from.To(TestEnum.Blue);

        result.Should().Be(TestEnum.Red);
    }

    [Fact]
    public void To_WithInvalidEnum_ReturnsDefault()
    {
        string from = "InvalidColor";

        var result = from.To(TestEnum.Blue);

        result.Should().Be(TestEnum.Blue);
    }

    [Fact]
    public void To_WithNullableTargetType_ConvertsSuccessfully()
    {
        string from = "42";

        var result = from.To<int?>(null);

        result.Should().Be(42);
    }

    [Fact]
    public void To_WithNullableTargetAndNull_ReturnsDefault()
    {
        string? from = null;

        var result = from.To<int?>(99);

        result.Should().Be(99);
    }

    [Fact]
    public void To_StringToInt_Converts()
    {
        string from = "456";

        var result = from.To(0);

        result.Should().Be(456);
    }

    [Fact]
    public void To_IntToString_Converts()
    {
        int from = 789;

        var result = from.To("");

        result.Should().Be("789");
    }

    #endregion

    #region To<TFrom, TTo> without defaultValue

    [Fact]
    public void To_NoDefault_WithNonNull_ConvertsType()
    {
        int from = 100;

        var result = from.To<int, string>();

        result.Should().Be("100");
    }

    [Fact]
    public void To_NoDefault_WithNull_ReturnsDefaultOfTarget()
    {
        string? from = null;

        var result = from!.To<string, int>();

        result.Should().Be(0);
    }

    #endregion

    #region To<TTo> string overloads

    [Fact]
    public void ToString_ToInt_WithDefault_ReturnsConverted()
    {
        string? from = "123";

        var result = from.To<int>(0);

        result.Should().Be(123);
    }

    [Fact]
    public void ToString_ToInt_WithNull_ReturnsDefault()
    {
        string? from = null;

        var result = from.To<int>(-1);

        result.Should().Be(-1);
    }

    [Fact]
    public void ToString_ToInt_NoDefault_ReturnsConverted()
    {
        string? from = "456";

        var result = from.To<int>();

        result.Should().Be(456);
    }

    [Fact]
    public void ToString_ToInt_NoDefault_Null_ReturnsDefaultOfInt()
    {
        string? from = null;

        var result = from.To<int>();

        result.Should().Be(0);
    }

    [Fact]
    public void ToString_ToBool_Converts()
    {
        string? from = "true";

        var result = from.To<bool>();

        result.Should().BeTrue();
    }

    #endregion

    #region To with getProperty and defaultValue

    [Fact]
    public void To_WithGetPropertyAndDefault_ExtractsAndConverts()
    {
        var model = new TestModel { Name = "123", Age = 25 };

        var result = model.To<TestModel, string, int>(m => m.Name!, 0);

        result.Should().Be(123);
    }

    [Fact]
    public void To_WithGetPropertyAndDefault_NullSource_ReturnsDefault()
    {
        TestModel? model = null;

        var result = model.To<TestModel, string, int>(m => m.Name!, 99);

        result.Should().Be(99);
    }

    [Fact]
    public void To_WithGetPropertyAndDefault_IncompatibleValue_ReturnsDefault()
    {
        var model = new TestModel { Name = "not_a_number" };

        var result = model.To<TestModel, string, int>(m => m.Name!, 0);

        result.Should().Be(0);
    }

    #endregion

    #region To with getProperty without defaultValue

    [Fact]
    public void To_WithGetProperty_ExtractsAndConverts()
    {
        var model = new TestModel { Age = 42 };

        var result = model.To<TestModel, int, string>(m => m.Age);

        result.Should().Be("42");
    }

    [Fact]
    public void To_WithGetProperty_NullSource_ReturnsDefault()
    {
        TestModel? model = null;

        var result = model.To<TestModel, int, string>(m => m.Age);

        result.Should().BeNull();
    }

    #endregion

    #region To with Func<TFrom, object>

    [Fact]
    public void To_WithObjectPropertyFunc_Converts()
    {
        var model = new TestModel { Name = "100" };

        var result = model.To<TestModel, int>(m => m.Name!);

        result.Should().Be(100);
    }

    [Fact]
    public void To_WithObjectPropertyFunc_NullSource_ReturnsDefault()
    {
        TestModel? model = null;

        var result = model.To<TestModel, int>(m => m.Name!);

        result.Should().Be(0);
    }

    #endregion

    #region ToExpandoObject

    [Fact]
    public void ToExpandoObject_WithNonNullObject_ReturnsExpandoObject()
    {
        var model = new TestModel { Name = "test", Age = 25, City = "beijing" };

        dynamic result = model.ToExpandoObject();

        string name = result.Name;
        int age = result.Age;
        string city = result.City;

        name.Should().Be("test");
        age.Should().Be(25);
        city.Should().Be("beijing");
    }

    [Fact]
    public void ToExpandoObject_WithNullProperty_PropertyValueIsNull()
    {
        var model = new TestModel { Name = null, Age = 30 };

        dynamic result = model.ToExpandoObject();

        object name = result.Name;
        int age = result.Age;

        name.Should().BeNull();
        age.Should().Be(30);
    }

    [Fact]
    public void ToExpandoObject_ReturnsExpandoObjectType()
    {
        var model = new TestModel { Name = "test" };

        var result = model.ToExpandoObject();

        result.Should().BeOfType<ExpandoObject>();
    }

    #endregion

    #region Test Enums

    private enum TestEnum
    {
        Red,
        Blue,
        Green
    }

    #endregion
}
