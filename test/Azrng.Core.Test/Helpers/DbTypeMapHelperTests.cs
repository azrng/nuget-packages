using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class DbTypeMapHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MySqlMapCsharpType_NullOrEmpty_ReturnsInput(string? dbType)
    {
        var result = DbTypeMapHelper.MySqlMapCsharpType(dbType!, false);

        result.Should().Be(dbType);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("mediumint")]
    [InlineData("integer")]
    [InlineData("year")]
    public void MySqlMapCsharpType_IntTypes_NonNullable_ReturnsInt(string dbType)
    {
        var result = DbTypeMapHelper.MySqlMapCsharpType(dbType, false);

        result.Should().Be("int");
    }

    [Theory]
    [InlineData("int")]
    [InlineData("mediumint")]
    [InlineData("integer")]
    [InlineData("year")]
    public void MySqlMapCsharpType_IntTypes_Nullable_ReturnsIntNullable(string dbType)
    {
        var result = DbTypeMapHelper.MySqlMapCsharpType(dbType, true);

        result.Should().Be("int?");
    }

    [Theory]
    [InlineData("varchar")]
    [InlineData("text")]
    [InlineData("char")]
    [InlineData("enum")]
    [InlineData("mediumtext")]
    [InlineData("tinytext")]
    [InlineData("longtext")]
    public void MySqlMapCsharpType_StringTypes_ReturnsString(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, false).Should().Be("string");
        DbTypeMapHelper.MySqlMapCsharpType(dbType, true).Should().Be("string");
    }

    [Fact]
    public void MySqlMapCsharpType_Tinyint_NonNullable_ReturnsByte()
    {
        DbTypeMapHelper.MySqlMapCsharpType("tinyint", false).Should().Be("byte");
    }

    [Fact]
    public void MySqlMapCsharpType_Tinyint_Nullable_ReturnsByteNullable()
    {
        DbTypeMapHelper.MySqlMapCsharpType("tinyint", true).Should().Be("byte?");
    }

    [Fact]
    public void MySqlMapCsharpType_Smallint_NonNullable_ReturnsShort()
    {
        DbTypeMapHelper.MySqlMapCsharpType("smallint", false).Should().Be("short");
    }

    [Fact]
    public void MySqlMapCsharpType_Smallint_Nullable_ReturnsShortNullable()
    {
        DbTypeMapHelper.MySqlMapCsharpType("smallint", true).Should().Be("short?");
    }

    [Fact]
    public void MySqlMapCsharpType_Bigint_NonNullable_ReturnsLong()
    {
        DbTypeMapHelper.MySqlMapCsharpType("bigint", false).Should().Be("long");
    }

    [Fact]
    public void MySqlMapCsharpType_Bigint_Nullable_ReturnsLongNullable()
    {
        DbTypeMapHelper.MySqlMapCsharpType("bigint", true).Should().Be("long?");
    }

    [Fact]
    public void MySqlMapCsharpType_Bit_NonNullable_ReturnsBool()
    {
        DbTypeMapHelper.MySqlMapCsharpType("bit", false).Should().Be("bool");
    }

    [Fact]
    public void MySqlMapCsharpType_Bit_Nullable_ReturnsBoolNullable()
    {
        DbTypeMapHelper.MySqlMapCsharpType("bit", true).Should().Be("bool?");
    }

    [Theory]
    [InlineData("real")]
    [InlineData("double")]
    public void MySqlMapCsharpType_DoubleTypes_NonNullable_ReturnsDouble(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, false).Should().Be("double");
    }

    [Theory]
    [InlineData("real")]
    [InlineData("double")]
    public void MySqlMapCsharpType_DoubleTypes_Nullable_ReturnsDoubleNullable(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, true).Should().Be("double?");
    }

    [Fact]
    public void MySqlMapCsharpType_Float_NonNullable_ReturnsFloat()
    {
        DbTypeMapHelper.MySqlMapCsharpType("float", false).Should().Be("float");
    }

    [Fact]
    public void MySqlMapCsharpType_Float_Nullable_ReturnsFloatNullable()
    {
        DbTypeMapHelper.MySqlMapCsharpType("float", true).Should().Be("float?");
    }

    [Theory]
    [InlineData("decimal")]
    [InlineData("numeric")]
    public void MySqlMapCsharpType_DecimalTypes_NonNullable_ReturnsDecimal(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, false).Should().Be("decimal");
    }

    [Theory]
    [InlineData("decimal")]
    [InlineData("numeric")]
    public void MySqlMapCsharpType_DecimalTypes_Nullable_ReturnsDecimalNullable(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, true).Should().Be("decimal?");
    }

    [Theory]
    [InlineData("datetime")]
    [InlineData("timestamp")]
    [InlineData("date")]
    [InlineData("time")]
    public void MySqlMapCsharpType_DateTimeTypes_NonNullable_ReturnsDateTime(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, false).Should().Be("DateTime");
    }

    [Theory]
    [InlineData("datetime")]
    [InlineData("timestamp")]
    [InlineData("date")]
    [InlineData("time")]
    public void MySqlMapCsharpType_DateTimeTypes_Nullable_ReturnsDateTimeNullable(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, true).Should().Be("DateTime?");
    }

    [Theory]
    [InlineData("blob")]
    [InlineData("longblob")]
    [InlineData("tinyblob")]
    [InlineData("varbinary")]
    [InlineData("binary")]
    [InlineData("multipoint")]
    [InlineData("geometry")]
    [InlineData("multilinestring")]
    [InlineData("polygon")]
    [InlineData("mediumblob")]
    public void MySqlMapCsharpType_BinaryTypes_ReturnsByteArray(string dbType)
    {
        DbTypeMapHelper.MySqlMapCsharpType(dbType, false).Should().Be("byteArray");
        DbTypeMapHelper.MySqlMapCsharpType(dbType, true).Should().Be("byteArray");
    }

    [Fact]
    public void MySqlMapCsharpType_UnknownType_ReturnsObject()
    {
        DbTypeMapHelper.MySqlMapCsharpType("unknown", false).Should().Be("object");
    }

    [Fact]
    public void MySqlMapCsharpType_CaseInsensitive_HandlesUpperCase()
    {
        DbTypeMapHelper.MySqlMapCsharpType("INT", false).Should().Be("int");
        DbTypeMapHelper.MySqlMapCsharpType("VARCHAR", false).Should().Be("string");
    }

    // --- Oracle ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OracleMapCsharpType_NullOrEmpty_ReturnsInput(string? dbType)
    {
        var result = DbTypeMapHelper.OracleMapCsharpType(dbType!, false);

        result.Should().Be(dbType);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("integer")]
    [InlineData("interval year to  month")]
    [InlineData("interval day to  second")]
    [InlineData("number")]
    public void OracleMapCsharpType_IntTypes_NonNullable_ReturnsInt(string dbType)
    {
        DbTypeMapHelper.OracleMapCsharpType(dbType, false).Should().Be("int");
    }

    [Theory]
    [InlineData("int")]
    [InlineData("integer")]
    [InlineData("interval year to  month")]
    [InlineData("interval day to  second")]
    [InlineData("number")]
    public void OracleMapCsharpType_IntTypes_Nullable_ReturnsIntNullable(string dbType)
    {
        DbTypeMapHelper.OracleMapCsharpType(dbType, true).Should().Be("int?");
    }

    [Fact]
    public void OracleMapCsharpType_Decimal_NonNullable_ReturnsDecimal()
    {
        DbTypeMapHelper.OracleMapCsharpType("decimal", false).Should().Be("decimal");
    }

    [Fact]
    public void OracleMapCsharpType_Decimal_Nullable_ReturnsDecimalNullable()
    {
        DbTypeMapHelper.OracleMapCsharpType("decimal", true).Should().Be("decimal?");
    }

    [Theory]
    [InlineData("varchar")]
    [InlineData("varchar2")]
    [InlineData("nvarchar2")]
    [InlineData("char")]
    [InlineData("nchar")]
    [InlineData("clob")]
    [InlineData("long")]
    [InlineData("nclob")]
    [InlineData("rowid")]
    public void OracleMapCsharpType_StringTypes_ReturnsString(string dbType)
    {
        DbTypeMapHelper.OracleMapCsharpType(dbType, false).Should().Be("string");
        DbTypeMapHelper.OracleMapCsharpType(dbType, true).Should().Be("string");
    }

    [Theory]
    [InlineData("date")]
    [InlineData("timestamp")]
    [InlineData("timestamp with local time zone")]
    [InlineData("timestamp with time zone")]
    public void OracleMapCsharpType_DateTimeTypes_NonNullable_ReturnsDateTime(string dbType)
    {
        DbTypeMapHelper.OracleMapCsharpType(dbType, false).Should().Be("DateTime");
    }

    [Theory]
    [InlineData("date")]
    [InlineData("timestamp")]
    [InlineData("timestamp with local time zone")]
    [InlineData("timestamp with time zone")]
    public void OracleMapCsharpType_DateTimeTypes_Nullable_ReturnsDateTimeNullable(string dbType)
    {
        DbTypeMapHelper.OracleMapCsharpType(dbType, true).Should().Be("DateTime?");
    }

    [Fact]
    public void OracleMapCsharpType_UnknownType_ReturnsObject()
    {
        DbTypeMapHelper.OracleMapCsharpType("unknown", false).Should().Be("object");
    }

    [Fact]
    public void OracleMapCsharpType_CaseInsensitive()
    {
        DbTypeMapHelper.OracleMapCsharpType("VARCHAR2", false).Should().Be("string");
    }

    // --- PostgreSQL ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PostgreMapCsharpType_NullOrEmpty_ReturnsInput(string? dbType)
    {
        var result = DbTypeMapHelper.PostgreMapCsharpType(dbType!, false);

        result.Should().Be(dbType);
    }

    [Theory]
    [InlineData("int2")]
    [InlineData("smallint")]
    public void PostgreMapCsharpType_ShortTypes_NonNullable_ReturnsShort(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("short");
    }

    [Theory]
    [InlineData("int2")]
    [InlineData("smallint")]
    public void PostgreMapCsharpType_ShortTypes_Nullable_ReturnsShortNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("short?");
    }

    [Theory]
    [InlineData("int4")]
    [InlineData("double precision")]
    [InlineData("integer")]
    public void PostgreMapCsharpType_IntTypes_NonNullable_ReturnsInt(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("int");
    }

    [Theory]
    [InlineData("int4")]
    [InlineData("double precision")]
    [InlineData("integer")]
    public void PostgreMapCsharpType_IntTypes_Nullable_ReturnsIntNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("int?");
    }

    [Theory]
    [InlineData("int8")]
    [InlineData("bigint")]
    public void PostgreMapCsharpType_LongTypes_NonNullable_ReturnsLong(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("long");
    }

    [Theory]
    [InlineData("int8")]
    [InlineData("bigint")]
    public void PostgreMapCsharpType_LongTypes_Nullable_ReturnsLongNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("long?");
    }

    [Theory]
    [InlineData("float4")]
    [InlineData("real")]
    public void PostgreMapCsharpType_FloatTypes_NonNullable_ReturnsFloat(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("float");
    }

    [Theory]
    [InlineData("float4")]
    [InlineData("real")]
    public void PostgreMapCsharpType_FloatTypes_Nullable_ReturnsFloatNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("float?");
    }

    [Fact]
    public void PostgreMapCsharpType_Float8_NonNullable_ReturnsDouble()
    {
        DbTypeMapHelper.PostgreMapCsharpType("float8", false).Should().Be("double");
    }

    [Fact]
    public void PostgreMapCsharpType_Float8_Nullable_ReturnsDoubleNullable()
    {
        DbTypeMapHelper.PostgreMapCsharpType("float8", true).Should().Be("double?");
    }

    [Theory]
    [InlineData("numeric")]
    [InlineData("decimal")]
    [InlineData("path")]
    [InlineData("point")]
    [InlineData("interval")]
    [InlineData("lseg")]
    [InlineData("macaddr")]
    [InlineData("money")]
    [InlineData("polygon")]
    public void PostgreMapCsharpType_DecimalTypes_NonNullable_ReturnsDecimal(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("decimal");
    }

    [Theory]
    [InlineData("numeric")]
    [InlineData("decimal")]
    [InlineData("path")]
    [InlineData("point")]
    [InlineData("interval")]
    [InlineData("lseg")]
    [InlineData("macaddr")]
    [InlineData("money")]
    [InlineData("polygon")]
    public void PostgreMapCsharpType_DecimalTypes_Nullable_ReturnsDecimalNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("decimal?");
    }

    [Theory]
    [InlineData("boolean")]
    [InlineData("bool")]
    [InlineData("box")]
    [InlineData("bytea")]
    public void PostgreMapCsharpType_BoolTypes_NonNullable_ReturnsBool(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("bool");
    }

    [Theory]
    [InlineData("boolean")]
    [InlineData("bool")]
    [InlineData("box")]
    [InlineData("bytea")]
    public void PostgreMapCsharpType_BoolTypes_Nullable_ReturnsBoolNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("bool?");
    }

    [Theory]
    [InlineData("varchar")]
    [InlineData("character varying")]
    [InlineData("geometry")]
    [InlineData("name")]
    [InlineData("text")]
    [InlineData("char")]
    [InlineData("character")]
    [InlineData("cidr")]
    [InlineData("circle")]
    [InlineData("tsquery")]
    [InlineData("tsvector")]
    [InlineData("xml")]
    [InlineData("json")]
    [InlineData("txid_snapshot")]
    public void PostgreMapCsharpType_StringTypes_ReturnsString(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("string");
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("string");
    }

    [Fact]
    public void PostgreMapCsharpType_Uuid_NonNullable_ReturnsGuid()
    {
        DbTypeMapHelper.PostgreMapCsharpType("uuid", false).Should().Be("Guid");
    }

    [Fact]
    public void PostgreMapCsharpType_Uuid_Nullable_ReturnsGuidNullable()
    {
        DbTypeMapHelper.PostgreMapCsharpType("uuid", true).Should().Be("Guid?");
    }

    [Theory]
    [InlineData("timestamp")]
    [InlineData("timestamp with time zone")]
    [InlineData("timestamptz")]
    [InlineData("timestamp without time zone")]
    [InlineData("date")]
    [InlineData("time")]
    [InlineData("time with time zone")]
    [InlineData("timetz")]
    [InlineData("time without time zone")]
    public void PostgreMapCsharpType_DateTimeTypes_NonNullable_ReturnsDateTime(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("DateTime");
    }

    [Theory]
    [InlineData("timestamp")]
    [InlineData("timestamp with time zone")]
    [InlineData("timestamptz")]
    [InlineData("timestamp without time zone")]
    [InlineData("date")]
    [InlineData("time")]
    [InlineData("time with time zone")]
    [InlineData("timetz")]
    [InlineData("time without time zone")]
    public void PostgreMapCsharpType_DateTimeTypes_Nullable_ReturnsDateTimeNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("DateTime?");
    }

    [Theory]
    [InlineData("bit")]
    [InlineData("bit varying")]
    public void PostgreMapCsharpType_BitTypes_NonNullable_ReturnsByteArray(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("byteArray");
    }

    [Theory]
    [InlineData("bit")]
    [InlineData("bit varying")]
    public void PostgreMapCsharpType_BitTypes_Nullable_ReturnsByteArrayNullable(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("byteArray?");
    }

    [Fact]
    public void PostgreMapCsharpType_Varbit_NonNullable_ReturnsByte()
    {
        DbTypeMapHelper.PostgreMapCsharpType("varbit", false).Should().Be("byte");
    }

    [Fact]
    public void PostgreMapCsharpType_Varbit_Nullable_ReturnsByteNullable()
    {
        DbTypeMapHelper.PostgreMapCsharpType("varbit", true).Should().Be("byte?");
    }

    [Theory]
    [InlineData("public.geometry")]
    [InlineData("inet")]
    public void PostgreMapCsharpType_ObjectTypes_ReturnsObject(string dbType)
    {
        DbTypeMapHelper.PostgreMapCsharpType(dbType, false).Should().Be("object");
        DbTypeMapHelper.PostgreMapCsharpType(dbType, true).Should().Be("object");
    }

    [Fact]
    public void PostgreMapCsharpType_UnknownType_ReturnsObject()
    {
        DbTypeMapHelper.PostgreMapCsharpType("unknown", false).Should().Be("object");
    }

    [Fact]
    public void PostgreMapCsharpType_CaseInsensitive()
    {
        DbTypeMapHelper.PostgreMapCsharpType("VARCHAR", false).Should().Be("string");
        DbTypeMapHelper.PostgreMapCsharpType("UUID", false).Should().Be("Guid");
    }

    // --- SQL Server ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SqlServerMapCsharpType_NullOrEmpty_ReturnsInput(string? dbType)
    {
        var result = DbTypeMapHelper.SqlServerMapCsharpType(dbType!, false);

        result.Should().Be(dbType);
    }

    [Fact]
    public void SqlServerMapCsharpType_Bigint_NonNullable_ReturnsLong()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("bigint", false).Should().Be("long");
    }

    [Fact]
    public void SqlServerMapCsharpType_Bigint_Nullable_ReturnsLongNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("bigint", true).Should().Be("long?");
    }

    [Theory]
    [InlineData("binary")]
    [InlineData("image")]
    [InlineData("timestamp")]
    [InlineData("varbinary")]
    public void SqlServerMapCsharpType_ByteArrayTypes_ReturnsByteArray(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, false).Should().Be("byte[]");
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, true).Should().Be("byte[]");
    }

    [Fact]
    public void SqlServerMapCsharpType_Bit_NonNullable_ReturnsBool()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("bit", false).Should().Be("bool");
    }

    [Fact]
    public void SqlServerMapCsharpType_Bit_Nullable_ReturnsBoolNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("bit", true).Should().Be("bool?");
    }

    [Theory]
    [InlineData("char")]
    [InlineData("nchar")]
    [InlineData("ntext")]
    [InlineData("nvarchar")]
    [InlineData("text")]
    [InlineData("varchar")]
    [InlineData("xml")]
    public void SqlServerMapCsharpType_StringTypes_ReturnsString(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, false).Should().Be("string");
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, true).Should().Be("string");
    }

    [Theory]
    [InlineData("date")]
    [InlineData("datetime")]
    [InlineData("datetime2")]
    [InlineData("smalldatetime")]
    public void SqlServerMapCsharpType_DateTimeTypes_NonNullable_ReturnsDateTime(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, false).Should().Be("DateTime");
    }

    [Theory]
    [InlineData("date")]
    [InlineData("datetime")]
    [InlineData("datetime2")]
    [InlineData("smalldatetime")]
    public void SqlServerMapCsharpType_DateTimeTypes_Nullable_ReturnsDateTimeNullable(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, true).Should().Be("DateTime?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Datetimeoffset_NonNullable_ReturnsDateTimeOffset()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("datetimeoffset", false).Should().Be("DateTimeOffset");
    }

    [Fact]
    public void SqlServerMapCsharpType_Datetimeoffset_Nullable_ReturnsDateTimeOffsetNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("datetimeoffset", true).Should().Be("DateTimeOffset?");
    }

    [Theory]
    [InlineData("decimal")]
    [InlineData("money")]
    [InlineData("numeric")]
    [InlineData("smallmoney")]
    public void SqlServerMapCsharpType_DecimalTypes_NonNullable_ReturnsDecimal(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, false).Should().Be("decimal");
    }

    [Theory]
    [InlineData("decimal")]
    [InlineData("money")]
    [InlineData("numeric")]
    [InlineData("smallmoney")]
    public void SqlServerMapCsharpType_DecimalTypes_Nullable_ReturnsDecimalNullable(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, true).Should().Be("decimal?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Float_NonNullable_ReturnsDouble()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("float", false).Should().Be("double");
    }

    [Fact]
    public void SqlServerMapCsharpType_Float_Nullable_ReturnsDoubleNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("float", true).Should().Be("double?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Int_NonNullable_ReturnsInt()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("int", false).Should().Be("int");
    }

    [Fact]
    public void SqlServerMapCsharpType_Int_Nullable_ReturnsIntNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("int", true).Should().Be("int?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Real_NonNullable_ReturnsSingle()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("real", false).Should().Be("Single");
    }

    [Fact]
    public void SqlServerMapCsharpType_Real_Nullable_ReturnsSingleNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("real", true).Should().Be("Single?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Smallint_NonNullable_ReturnsShort()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("smallint", false).Should().Be("short");
    }

    [Fact]
    public void SqlServerMapCsharpType_Smallint_Nullable_ReturnsShortNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("smallint", true).Should().Be("short?");
    }

    [Theory]
    [InlineData("sql_variant")]
    [InlineData("sysname")]
    public void SqlServerMapCsharpType_ObjectTypes_ReturnsObject(string dbType)
    {
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, false).Should().Be("object");
        DbTypeMapHelper.SqlServerMapCsharpType(dbType, true).Should().Be("object");
    }

    [Fact]
    public void SqlServerMapCsharpType_Time_NonNullable_ReturnsTimeSpan()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("time", false).Should().Be("TimeSpan");
    }

    [Fact]
    public void SqlServerMapCsharpType_Time_Nullable_ReturnsTimeSpanNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("time", true).Should().Be("TimeSpan?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Tinyint_NonNullable_ReturnsByte()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("tinyint", false).Should().Be("byte");
    }

    [Fact]
    public void SqlServerMapCsharpType_Tinyint_Nullable_ReturnsByteNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("tinyint", true).Should().Be("byte?");
    }

    [Fact]
    public void SqlServerMapCsharpType_Uniqueidentifier_NonNullable_ReturnsGuid()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("uniqueidentifier", false).Should().Be("Guid");
    }

    [Fact]
    public void SqlServerMapCsharpType_Uniqueidentifier_Nullable_ReturnsGuidNullable()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("uniqueidentifier", true).Should().Be("Guid?");
    }

    [Fact]
    public void SqlServerMapCsharpType_UnknownType_ReturnsObject()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("unknown", false).Should().Be("object");
    }

    [Fact]
    public void SqlServerMapCsharpType_CaseInsensitive()
    {
        DbTypeMapHelper.SqlServerMapCsharpType("BIGINT", false).Should().Be("long");
        DbTypeMapHelper.SqlServerMapCsharpType("NTEXT", false).Should().Be("string");
    }

    // --- SQLite ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SqliteMapCsharpType_NullOrEmpty_ReturnsInput(string? dbType)
    {
        var result = DbTypeMapHelper.SqliteMapCsharpType(dbType!, false);

        result.Should().Be(dbType);
    }

    [Theory]
    [InlineData("integer")]
    [InlineData("int")]
    [InlineData("int32")]
    [InlineData("integer32")]
    [InlineData("number")]
    public void SqliteMapCsharpType_IntTypes_NonNullable_ReturnsInt(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("int");
    }

    [Theory]
    [InlineData("integer")]
    [InlineData("int")]
    [InlineData("int32")]
    [InlineData("integer32")]
    [InlineData("number")]
    public void SqliteMapCsharpType_IntTypes_Nullable_ReturnsIntNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("int?");
    }

    [Theory]
    [InlineData("varchar")]
    [InlineData("varchar2")]
    [InlineData("nvarchar")]
    [InlineData("nvarchar2")]
    [InlineData("text")]
    [InlineData("ntext")]
    [InlineData("blob_text")]
    [InlineData("char")]
    [InlineData("nchar")]
    [InlineData("num")]
    [InlineData("currency")]
    [InlineData("datetext")]
    [InlineData("word")]
    [InlineData("graphic")]
    public void SqliteMapCsharpType_StringTypes_ReturnsString(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("string");
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("string");
    }

    [Theory]
    [InlineData("tinyint")]
    [InlineData("unsignedinteger8")]
    public void SqliteMapCsharpType_ByteTypes_NonNullable_ReturnsByte(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("byte");
    }

    [Theory]
    [InlineData("tinyint")]
    [InlineData("unsignedinteger8")]
    public void SqliteMapCsharpType_ByteTypes_Nullable_ReturnsByteNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("byte?");
    }

    [Theory]
    [InlineData("smallint")]
    [InlineData("int16")]
    public void SqliteMapCsharpType_ShortTypes_NonNullable_ReturnsShort(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("short");
    }

    [Theory]
    [InlineData("smallint")]
    [InlineData("int16")]
    public void SqliteMapCsharpType_ShortTypes_Nullable_ReturnsShortNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("short?");
    }

    [Theory]
    [InlineData("bigint")]
    [InlineData("int64")]
    [InlineData("long")]
    [InlineData("integer64")]
    public void SqliteMapCsharpType_LongTypes_NonNullable_ReturnsLong(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("long");
    }

    [Theory]
    [InlineData("bigint")]
    [InlineData("int64")]
    [InlineData("long")]
    [InlineData("integer64")]
    public void SqliteMapCsharpType_LongTypes_Nullable_ReturnsLongNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("long?");
    }

    [Theory]
    [InlineData("bit")]
    [InlineData("bool")]
    [InlineData("boolean")]
    public void SqliteMapCsharpType_BoolTypes_NonNullable_ReturnsBool(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("bool");
    }

    [Theory]
    [InlineData("bit")]
    [InlineData("bool")]
    [InlineData("boolean")]
    public void SqliteMapCsharpType_BoolTypes_Nullable_ReturnsBoolNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("bool?");
    }

    [Theory]
    [InlineData("real")]
    [InlineData("double")]
    public void SqliteMapCsharpType_DoubleTypes_NonNullable_ReturnsDouble(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("double");
    }

    [Theory]
    [InlineData("real")]
    [InlineData("double")]
    public void SqliteMapCsharpType_DoubleTypes_Nullable_ReturnsDoubleNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("double?");
    }

    [Fact]
    public void SqliteMapCsharpType_Float_NonNullable_ReturnsFloat()
    {
        DbTypeMapHelper.SqliteMapCsharpType("float", false).Should().Be("float");
    }

    [Fact]
    public void SqliteMapCsharpType_Float_Nullable_ReturnsFloatNullable()
    {
        DbTypeMapHelper.SqliteMapCsharpType("float", true).Should().Be("float?");
    }

    [Theory]
    [InlineData("decimal")]
    [InlineData("dec")]
    [InlineData("numeric")]
    [InlineData("money")]
    [InlineData("smallmoney")]
    public void SqliteMapCsharpType_DecimalTypes_NonNullable_ReturnsDecimal(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("decimal");
    }

    [Theory]
    [InlineData("decimal")]
    [InlineData("dec")]
    [InlineData("numeric")]
    [InlineData("money")]
    [InlineData("smallmoney")]
    public void SqliteMapCsharpType_DecimalTypes_Nullable_ReturnsDecimalNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("decimal?");
    }

    [Theory]
    [InlineData("datetime")]
    [InlineData("timestamp")]
    [InlineData("date")]
    [InlineData("time")]
    public void SqliteMapCsharpType_DateTimeTypes_NonNullable_ReturnsDateTime(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("DateTime");
    }

    [Theory]
    [InlineData("datetime")]
    [InlineData("timestamp")]
    [InlineData("date")]
    [InlineData("time")]
    public void SqliteMapCsharpType_DateTimeTypes_Nullable_ReturnsDateTimeNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("DateTime?");
    }

    [Theory]
    [InlineData("blob")]
    [InlineData("clob")]
    [InlineData("raw")]
    [InlineData("oleobject")]
    [InlineData("binary")]
    [InlineData("photo")]
    [InlineData("picture")]
    public void SqliteMapCsharpType_BinaryTypes_NonNullable_ReturnsByteArray(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, false).Should().Be("byteArray");
    }

    [Theory]
    [InlineData("blob")]
    [InlineData("clob")]
    [InlineData("raw")]
    [InlineData("oleobject")]
    [InlineData("binary")]
    [InlineData("photo")]
    [InlineData("picture")]
    public void SqliteMapCsharpType_BinaryTypes_Nullable_ReturnsByteArrayNullable(string dbType)
    {
        DbTypeMapHelper.SqliteMapCsharpType(dbType, true).Should().Be("byteArray?");
    }

    [Fact]
    public void SqliteMapCsharpType_Uniqueidentifier_NonNullable_ReturnsGuid()
    {
        DbTypeMapHelper.SqliteMapCsharpType("uniqueidentifier", false).Should().Be("Guid");
    }

    [Fact]
    public void SqliteMapCsharpType_Uniqueidentifier_Nullable_ReturnsGuidNullable()
    {
        DbTypeMapHelper.SqliteMapCsharpType("uniqueidentifier", true).Should().Be("Guid?");
    }

    [Fact]
    public void SqliteMapCsharpType_UnknownType_ReturnsObject()
    {
        DbTypeMapHelper.SqliteMapCsharpType("unknown", false).Should().Be("object");
    }

    [Fact]
    public void SqliteMapCsharpType_CaseInsensitive()
    {
        DbTypeMapHelper.SqliteMapCsharpType("INTEGER", false).Should().Be("int");
        DbTypeMapHelper.SqliteMapCsharpType("VARCHAR", false).Should().Be("string");
    }
}
