using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class DictionaryExtensionTests
{
    #region GetOrDefault

    [Fact]
    public void GetOrDefault_ShouldReturnValue_WhenKeyExists()
    {
        var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

        dict.GetOrDefault("a", 99).Should().Be(1);
    }

    [Fact]
    public void GetOrDefault_ShouldReturnDefault_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, int> { { "a", 1 } };

        dict.GetOrDefault("z", 99).Should().Be(99);
    }

    [Fact]
    public void GetOrDefault_ShouldReturnDefault_WhenDictionaryIsNull()
    {
        Dictionary<string, int>? dict = null;

        dict.GetOrDefault("a", 42).Should().Be(42);
    }

    [Fact]
    public void GetOrDefault_ShouldReturnDefault_WhenDictionaryIsEmpty()
    {
        var dict = new Dictionary<string, string>();

        dict.GetOrDefault("key", "fallback").Should().Be("fallback");
    }

    #endregion

    #region GetColumnValueByName (IDictionary<string, string>)

    [Fact]
    public void GetColumnValueByName_StringDict_ShouldReturnValue_WhenKeyExists()
    {
        var dict = new Dictionary<string, string> { { "Name", "Alice" }, { "Age", "30" } };

        dict.GetColumnValueByName("Name").Should().Be("Alice");
    }

    [Fact]
    public void GetColumnValueByName_StringDict_ShouldReturnNull_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, string> { { "Name", "Alice" } };

        dict.GetColumnValueByName("Missing").Should().BeNull();
    }

    [Fact]
    public void GetColumnValueByName_StringDict_ShouldReturnNull_WhenDictIsNull()
    {
        Dictionary<string, string>? dict = null;

        dict.GetColumnValueByName("key").Should().BeNull();
    }

    [Fact]
    public void GetColumnValueByName_StringDict_ShouldReturnNull_WhenKeyIsNull()
    {
        var dict = new Dictionary<string, string> { { "Name", "Alice" } };

        dict.GetColumnValueByName(null).Should().BeNull();
    }

    #endregion

    #region GetColumnValueByName (IDictionary<string, object?>)

    [Fact]
    public void GetColumnValueByName_ObjectDict_ShouldReturnStringValue_WhenKeyExists()
    {
        var dict = new Dictionary<string, object?> { { "Name", "Alice" } };

        dict.GetColumnValueByName("Name").Should().Be("Alice");
    }

    [Fact]
    public void GetColumnValueByName_ObjectDict_ShouldReturnEmpty_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, object?> { { "Name", "Alice" } };

        dict.GetColumnValueByName("Missing").Should().Be("");
    }

    [Fact]
    public void GetColumnValueByName_ObjectDict_ShouldReturnEmpty_WhenDictIsNull()
    {
        Dictionary<string, object?>? dict = null;

        dict.GetColumnValueByName("key").Should().Be("");
    }

    [Fact]
    public void GetColumnValueByName_ObjectDict_ShouldReturnEmpty_WhenKeyIsNull()
    {
        var dict = new Dictionary<string, object?> { { "Name", "Alice" } };

        dict.GetColumnValueByName(null).Should().Be("");
    }

    [Fact]
    public void GetColumnValueByName_ObjectDict_ShouldReturnDecimalFormatted_WhenValueIsDecimal()
    {
        var dict = new Dictionary<string, object?> { { "Price", 123.45m } };

        dict.GetColumnValueByName("Price").Should().Be("123.45");
    }

    [Fact]
    public void GetColumnValueByName_ObjectDict_ShouldReturnEmpty_WhenValueIsNull()
    {
        var dict = new Dictionary<string, object?> { { "Name", null } };

        dict.GetColumnValueByName("Name").Should().Be("");
    }

    #endregion

    #region GetColumnValueByName<T>

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnInt_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Age", 30 } };

        dict.GetColumnValueByName<int?>("Age").Should().Be(30);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDecimal_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Price", 99.9m } };

        dict.GetColumnValueByName<decimal?>("Price").Should().Be(99.9m);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDouble_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Score", 88.5 } };

        dict.GetColumnValueByName<double?>("Score").Should().Be(88.5);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnBool_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Active", true } };

        dict.GetColumnValueByName<bool?>("Active").Should().BeTrue();
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDateTime_WhenValueIsConvertible()
    {
        var dt = new DateTime(2024, 1, 15);
        var dict = new Dictionary<string, object?> { { "Created", dt } };

        dict.GetColumnValueByName<DateTime?>("Created").Should().Be(dt);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnLong_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Count", 123456789L } };

        dict.GetColumnValueByName<long?>("Count").Should().Be(123456789L);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnByte_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Flag", (byte)7 } };

        dict.GetColumnValueByName<byte?>("Flag").Should().Be(7);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnShort_WhenValueIsConvertible()
    {
        var dict = new Dictionary<string, object?> { { "Val", (short)42 } };

        dict.GetColumnValueByName<short?>("Val").Should().Be(42);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnGuid_WhenValueIsConvertible()
    {
        var guid = Guid.NewGuid();
        var dict = new Dictionary<string, object?> { { "Id", guid } };

        dict.GetColumnValueByName<Guid?>("Id").Should().Be(guid);
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDefault_WhenDictIsNull()
    {
        Dictionary<string, object?>? dict = null;

        dict.GetColumnValueByName<int?>("key").Should().BeNull();
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDefault_WhenKeyIsNull()
    {
        var dict = new Dictionary<string, object?> { { "Age", 30 } };

        dict.GetColumnValueByName<int?>(null).Should().BeNull();
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDefault_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, object?> { { "Age", 30 } };

        dict.GetColumnValueByName<int?>("Missing").Should().BeNull();
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnDefault_WhenValueIsNull()
    {
        var dict = new Dictionary<string, object?> { { "Age", null } };

        dict.GetColumnValueByName<int?>("Age").Should().BeNull();
    }

    [Fact]
    public void GetColumnValueByName_Generic_ShouldReturnString_WhenTypeIsString()
    {
        var dict = new Dictionary<string, object?> { { "Name", "Alice" } };

        dict.GetColumnValueByName<string>("Name").Should().Be("Alice");
    }

    #endregion

    #region AddOrUpdate

    [Fact]
    public void AddOrUpdate_ShouldAddNewKey_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, int> { { "a", 1 } };

        dict.AddOrUpdate("b", 2);

        dict.Should().ContainKey("b").WhoseValue.Should().Be(2);
        dict.Should().HaveCount(2);
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingKey_WhenKeyExists()
    {
        var dict = new Dictionary<string, int> { { "a", 1 } };

        dict.AddOrUpdate("a", 99);

        dict["a"].Should().Be(99);
        dict.Should().HaveCount(1);
    }

    [Fact]
    public void AddOrUpdate_ShouldThrow_WhenDictIsNull()
    {
        Dictionary<string, int>? dict = null;

        var action = () => dict!.AddOrUpdate("a", 1);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CreateOrInsert

    [Fact]
    public void CreateOrInsert_ShouldCreateNewList_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, List<int>>();

        dict.CreateOrInsert("a", 1);

        dict.Should().ContainKey("a");
        dict["a"].Should().Equal(1);
    }

    [Fact]
    public void CreateOrInsert_ShouldAppendToList_WhenKeyExists()
    {
        var dict = new Dictionary<string, List<int>>
        {
            { "a", new List<int> { 1, 2 } }
        };

        dict.CreateOrInsert("a", 3);

        dict["a"].Should().Equal(1, 2, 3);
    }

    [Fact]
    public void CreateOrInsert_ShouldThrow_WhenDictIsNull()
    {
        Dictionary<string, List<int>>? dict = null;

        var action = () => dict!.CreateOrInsert("a", 1);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region AddOrAppend (int)

    [Fact]
    public void AddOrAppend_Int_ShouldAddNewKey_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, int>();

        dict.AddOrAppend("count", 5);

        dict["count"].Should().Be(5);
    }

    [Fact]
    public void AddOrAppend_Int_ShouldAccumulate_WhenKeyExists()
    {
        var dict = new Dictionary<string, int> { { "count", 10 } };

        dict.AddOrAppend("count", 3);

        dict["count"].Should().Be(13);
    }

    [Fact]
    public void AddOrAppend_Int_ShouldThrow_WhenDictIsNull()
    {
        Dictionary<string, int>? dict = null;

        var action = () => dict!.AddOrAppend("a", 1);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOrAppend_Int_ShouldHandleMultipleAccumulations()
    {
        var dict = new Dictionary<string, int>();

        dict.AddOrAppend("total", 1);
        dict.AddOrAppend("total", 2);
        dict.AddOrAppend("total", 3);

        dict["total"].Should().Be(6);
    }

    #endregion

    #region AddOrAppend (long)

    [Fact]
    public void AddOrAppend_Long_ShouldAddNewKey_WhenKeyNotExists()
    {
        var dict = new Dictionary<string, long>();

        dict.AddOrAppend("count", 100L);

        dict["count"].Should().Be(100L);
    }

    [Fact]
    public void AddOrAppend_Long_ShouldAccumulate_WhenKeyExists()
    {
        var dict = new Dictionary<string, long> { { "count", 200L } };

        dict.AddOrAppend("count", 50L);

        dict["count"].Should().Be(250L);
    }

    [Fact]
    public void AddOrAppend_Long_ShouldThrow_WhenDictIsNull()
    {
        Dictionary<string, long>? dict = null;

        var action = () => dict!.AddOrAppend("a", 1L);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOrAppend_Long_ShouldHandleMultipleAccumulations()
    {
        var dict = new Dictionary<string, long>();

        dict.AddOrAppend("total", 1000000000L);
        dict.AddOrAppend("total", 2000000000L);
        dict.AddOrAppend("total", 3L);

        dict["total"].Should().Be(3000000003L);
    }

    #endregion
}
