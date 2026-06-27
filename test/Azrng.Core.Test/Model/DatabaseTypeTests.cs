using Azrng.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Model;

public class DatabaseTypeTests
{
    [Fact]
    public void MySql_ShouldHaveValue0()
    {
        ((int)DatabaseType.MySql).Should().Be(0);
    }

    [Fact]
    public void SqlServer_ShouldHaveValue1()
    {
        ((int)DatabaseType.SqlServer).Should().Be(1);
    }

    [Fact]
    public void Sqlite_ShouldHaveValue2()
    {
        ((int)DatabaseType.Sqlite).Should().Be(2);
    }

    [Fact]
    public void Oracle_ShouldHaveValue3()
    {
        ((int)DatabaseType.Oracle).Should().Be(3);
    }

    [Fact]
    public void PostgresSql_ShouldHaveValue4()
    {
        ((int)DatabaseType.PostgresSql).Should().Be(4);
    }

    [Fact]
    public void Dm_ShouldHaveValue5()
    {
        ((int)DatabaseType.Dm).Should().Be(5);
    }

    [Fact]
    public void Kdbndp_ShouldHaveValue6()
    {
        ((int)DatabaseType.Kdbndp).Should().Be(6);
    }

    [Fact]
    public void Oscar_ShouldHaveValue7()
    {
        ((int)DatabaseType.Oscar).Should().Be(7);
    }

    [Fact]
    public void MySqlConnector_ShouldHaveValue8()
    {
        ((int)DatabaseType.MySqlConnector).Should().Be(8);
    }

    [Fact]
    public void Access_ShouldHaveValue9()
    {
        ((int)DatabaseType.Access).Should().Be(9);
    }

    [Fact]
    public void OpenGauss_ShouldHaveValue10()
    {
        ((int)DatabaseType.OpenGauss).Should().Be(10);
    }

    [Fact]
    public void QuestDB_ShouldHaveValue11()
    {
        ((int)DatabaseType.QuestDB).Should().Be(11);
    }

    [Fact]
    public void HG_ShouldHaveValue12()
    {
        ((int)DatabaseType.HG).Should().Be(12);
    }

    [Fact]
    public void ClickHouse_ShouldHaveValue13()
    {
        ((int)DatabaseType.ClickHouse).Should().Be(13);
    }

    [Fact]
    public void GBase_ShouldHaveValue14()
    {
        ((int)DatabaseType.GBase).Should().Be(14);
    }

    [Fact]
    public void Odbc_ShouldHaveValue15()
    {
        ((int)DatabaseType.Odbc).Should().Be(15);
    }

    [Fact]
    public void OceanBaseForOracle_ShouldHaveValue16()
    {
        ((int)DatabaseType.OceanBaseForOracle).Should().Be(16);
    }

    [Fact]
    public void TDengine_ShouldHaveValue17()
    {
        ((int)DatabaseType.TDengine).Should().Be(17);
    }

    [Fact]
    public void GaussDB_ShouldHaveValue18()
    {
        ((int)DatabaseType.GaussDB).Should().Be(18);
    }

    [Fact]
    public void OceanBase_ShouldHaveValue19()
    {
        ((int)DatabaseType.OceanBase).Should().Be(19);
    }

    [Fact]
    public void Tidb_ShouldHaveValue20()
    {
        ((int)DatabaseType.Tidb).Should().Be(20);
    }

    [Fact]
    public void Vastbase_ShouldHaveValue21()
    {
        ((int)DatabaseType.Vastbase).Should().Be(21);
    }

    [Fact]
    public void PolarDB_ShouldHaveValue22()
    {
        ((int)DatabaseType.PolarDB).Should().Be(22);
    }

    [Fact]
    public void Doris_ShouldHaveValue23()
    {
        ((int)DatabaseType.Doris).Should().Be(23);
    }

    [Fact]
    public void Xugu_ShouldHaveValue24()
    {
        ((int)DatabaseType.Xugu).Should().Be(24);
    }

    [Fact]
    public void GoldenDB_ShouldHaveValue25()
    {
        ((int)DatabaseType.GoldenDB).Should().Be(25);
    }

    [Fact]
    public void TDSQLForPGODBC_ShouldHaveValue26()
    {
        ((int)DatabaseType.TDSQLForPGODBC).Should().Be(26);
    }

    [Fact]
    public void TDSQL_ShouldHaveValue27()
    {
        ((int)DatabaseType.TDSQL).Should().Be(27);
    }

    [Fact]
    public void HANA_ShouldHaveValue28()
    {
        ((int)DatabaseType.HANA).Should().Be(28);
    }

    [Fact]
    public void DB2_ShouldHaveValue29()
    {
        ((int)DatabaseType.DB2).Should().Be(29);
    }

    [Fact]
    public void GaussDBNative_ShouldHaveValue30()
    {
        ((int)DatabaseType.GaussDBNative).Should().Be(30);
    }

    [Fact]
    public void DuckDB_ShouldHaveValue31()
    {
        ((int)DatabaseType.DuckDB).Should().Be(31);
    }

    [Fact]
    public void MongoDb_ShouldHaveValue32()
    {
        ((int)DatabaseType.MongoDb).Should().Be(32);
    }

    [Fact]
    public void InMemory_ShouldHaveValue800()
    {
        ((int)DatabaseType.InMemory).Should().Be(800);
    }

    [Fact]
    public void Custom_ShouldHaveValue900()
    {
        ((int)DatabaseType.Custom).Should().Be(900);
    }

    [Fact]
    public void EnumValues_ShouldHaveExpectedCount()
    {
        var values = Enum.GetValues(typeof(DatabaseType));
        values.Length.Should().Be(35);
    }

    [Fact]
    public void DefaultEnumValue_ShouldBeMySql()
    {
        var defaultValue = default(DatabaseType);
        defaultValue.Should().Be(DatabaseType.MySql);
    }

    [Fact]
    public void Parse_MySql_ShouldReturnMySql()
    {
        Enum.TryParse<DatabaseType>("MySql", out var result).Should().BeTrue();
        result.Should().Be(DatabaseType.MySql);
    }

    [Fact]
    public void Parse_Custom_ShouldReturnCustom()
    {
        Enum.TryParse<DatabaseType>("Custom", out var result).Should().BeTrue();
        result.Should().Be(DatabaseType.Custom);
    }
}
