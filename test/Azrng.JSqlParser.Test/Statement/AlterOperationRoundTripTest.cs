using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Alter;
using Xunit;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-14：ALTER TABLE 扩展操作 + ALTER SEQUENCE 的解析与 round-trip 测试。
/// 覆盖 DROP PRIMARY/UNIQUE/FOREIGN KEY/CONSTRAINT、RENAME INDEX/KEY/CONSTRAINT、
/// ENGINE/COMMENT、分区操作族、ALTER SEQUENCE 选项。
/// </summary>
public class AlterOperationRoundTripTest
{
    public static TheoryData<string> RoundTripCases => new()
    {
        // DROP 约束族
        "ALTER TABLE t DROP PRIMARY KEY",
        "ALTER TABLE t DROP UNIQUE uk1",
        "ALTER TABLE t DROP FOREIGN KEY fk1",
        "ALTER TABLE t DROP CONSTRAINT c1",
        // RENAME INDEX/KEY/CONSTRAINT
        "ALTER TABLE t RENAME INDEX i1 TO i2",
        "ALTER TABLE t RENAME KEY k1 TO k2",
        "ALTER TABLE t RENAME CONSTRAINT c1 TO c2",
        // ENGINE / COMMENT（带/不带等号）
        "ALTER TABLE t ENGINE = InnoDB",
        "ALTER TABLE t ENGINE InnoDB",
        "ALTER TABLE t COMMENT 'hi'",
        "ALTER TABLE t COMMENT = 'hi'",
        // 分区操作族
        "ALTER TABLE t ADD PARTITION (PARTITION p0 VALUES LESS THAN (10))",
        "ALTER TABLE t DROP PARTITION p0, p1",
        "ALTER TABLE t TRUNCATE PARTITION p0",
        "ALTER TABLE t COALESCE PARTITION 3",
        "ALTER TABLE t REORGANIZE PARTITION p0 INTO (PARTITION p1 VALUES IN (1))",
        "ALTER TABLE t EXCHANGE PARTITION p0 WITH TABLE other",
        "ALTER TABLE t REMOVE PARTITIONING",
        // ALTER SEQUENCE
        "ALTER SEQUENCE seq RESTART WITH 100 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999 CACHE 20 CYCLE",
        "ALTER SEQUENCE s RESTART",
        // ALTER COLUMN 子句（T093：此前 grammar 已解析但 visitor 静默丢弃）
        "ALTER TABLE t ALTER COLUMN b SET DEFAULT 100",
        "ALTER TABLE t ALTER COLUMN b DROP DEFAULT",
        "ALTER TABLE t ALTER COLUMN b SET NOT NULL",
        "ALTER TABLE t ALTER COLUMN b DROP NOT NULL",
        "ALTER TABLE t ALTER COLUMN b TYPE VARCHAR(255)",
        // ALTER COLUMN 扩展（SET DATA TYPE / VISIBLE / INVISIBLE）
        "ALTER TABLE t ALTER COLUMN b SET DATA TYPE INT",
        "ALTER TABLE t ALTER COLUMN b SET VISIBLE",
        "ALTER TABLE t ALTER COLUMN b SET INVISIBLE",
        // CONVERT / CHARACTER SET（MySQL 字符集转换）
        "ALTER TABLE t CONVERT TO CHARACTER SET utf8",
        "ALTER TABLE t CONVERT TO CHARACTER SET utf8 COLLATE utf8_bin",
        "ALTER TABLE t CHARACTER SET utf8",
    };

    [Theory]
    [MemberData(nameof(RoundTripCases))]
    public void ShouldRoundTrip(string sql)
    {
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void DropUnique_ParsesConstraintSymbol()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t DROP UNIQUE uk1")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.DROP_UNIQUE, expr.Operation);
        Assert.Equal("uk1", expr.ConstraintSymbol);
    }

    [Fact]
    public void DropForeignKey_ParsesConstraintSymbol()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t DROP FOREIGN KEY fk1")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.DROP_FOREIGN_KEY, expr.Operation);
        Assert.Equal("fk1", expr.ConstraintSymbol);
    }

    [Fact]
    public void RenameIndex_ParsesOldAndNew()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t RENAME INDEX i1 TO i2")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.RENAME_INDEX, expr.Operation);
        Assert.Equal("i1", expr.ColumnOldName);
        Assert.Equal("i2", expr.ColumnName);
    }

    [Fact]
    public void EngineEquals_RecordsUseEquals()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t ENGINE = InnoDB")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.ENGINE, expr.Operation);
        Assert.True(expr.UseEqualsForEngine);
        Assert.Equal("InnoDB", expr.OptionalSpecifier);
    }

    [Fact]
    public void CommentEquals_UsesEqualSignOperation()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t COMMENT = 'hi'")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.COMMENT_WITH_EQUAL_SIGN, expr.Operation);
    }

    [Fact]
    public void AddPartition_ParsesPartitionDefinition()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t ADD PARTITION (PARTITION p0 VALUES LESS THAN (10))")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.ADD_PARTITION, expr.Operation);
        var def = Assert.Single(expr.PartitionDefinitions!);
        Assert.Equal("p0", def.Name);
        Assert.NotNull(def.ValuesLessThan);
    }

    [Fact]
    public void DropPartitions_ParsesPartitionNames()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t DROP PARTITION p0, p1")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.DROP_PARTITION, expr.Operation);
        Assert.Equal(new[] { "p0", "p1" }, expr.PartitionNames!);
    }

    [Fact]
    public void CoalescePartition_ParsesCount()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t COALESCE PARTITION 3")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.COALESCE_PARTITION, expr.Operation);
        Assert.Equal(3, expr.CoalescePartitionNumber);
    }

    [Fact]
    public void ExchangePartition_ParsesTargetTable()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t EXCHANGE PARTITION p0 WITH TABLE other")!;
        var expr = Assert.Single(stmt.AlterExpressions);
        Assert.Equal(AlterOperation.EXCHANGE_PARTITION, expr.Operation);
        Assert.Equal("other", expr.ExchangePartitionTable);
    }

    [Fact]
    public void AlterSequence_ParsesStructuredParameters()
    {
        var stmt = (AlterSequence)CCJSqlParserUtil.Parse(
            "ALTER SEQUENCE seq RESTART WITH 100 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999 CACHE 20 CYCLE")!;
        Assert.NotNull(stmt.Sequence);
        Assert.Equal("seq", stmt.Sequence!.Name);
        Assert.NotNull(stmt.Sequence.Parameters);
        Assert.True(stmt.Sequence.Parameters!.Count >= 6);
    }

    // ── T093: ALTER COLUMN 子句结构化断言（round-trip 由 RoundTripCases 覆盖） ──

    [Fact]
    public void AlterColumn_SetDefault_ShouldPopulateAction()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t ALTER COLUMN b SET DEFAULT 100")!;
        var expr = Assert.Single(stmt.AlterExpressions!);
        Assert.Equal(AlterColumnAction.SetDefault, expr.ColumnAlterAction);
        Assert.Equal("100", expr.AlterColumnDefaultExpression);
    }

    [Fact]
    public void AlterColumn_Type_ShouldPopulateAction()
    {
        var stmt = (Alter)CCJSqlParserUtil.Parse("ALTER TABLE t ALTER COLUMN b TYPE INT")!;
        var expr = Assert.Single(stmt.AlterExpressions!);
        Assert.Equal(AlterColumnAction.Type, expr.ColumnAlterAction);
        Assert.Equal("INT", expr.AlterColumnType);
    }
}
