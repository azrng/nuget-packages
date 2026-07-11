using Azrng.JSqlParser.Parser;
using MergeStatement = Azrng.JSqlParser.Statement.Merge.Merge;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// MERGE 语句测试（H1 修复回归保护）。
/// 覆盖 USING 源表、WHEN AND 条件、MergeInsert VALUES、多 WHEN 混合。
/// </summary>
public class MergeTest
{
    // ===== USING 源表 round-trip =====

    [Fact]
    public void Merge_UsingSourceTable_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN MATCHED THEN UPDATE SET t.a = src.a";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    [Fact]
    public void Merge_UsingSubquery_RoundTrip()
    {
        var sql = "MERGE INTO t USING (SELECT id FROM src) s ON t.id = s.id WHEN MATCHED THEN DELETE";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    [Fact]
    public void Merge_WithAlias_RoundTrip()
    {
        var sql = "MERGE INTO t tgt USING src ON t.id = src.id WHEN MATCHED THEN DELETE";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    // ===== WHEN AND 条件 =====

    [Fact]
    public void Merge_WhenMatchedAndCondition_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN MATCHED AND x > 0 THEN DELETE";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    [Fact]
    public void Merge_WhenNotMatchedAndCondition_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN NOT MATCHED AND y = 1 THEN INSERT (a) VALUES (1)";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    // ===== MergeInsert VALUES =====

    [Fact]
    public void Merge_InsertWithValues_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN NOT MATCHED THEN INSERT (a, b) VALUES (1, 2)";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    [Fact]
    public void Merge_InsertWithoutColumns_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN NOT MATCHED THEN INSERT VALUES (1)";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    // ===== 多 WHEN 混合 =====

    [Fact]
    public void Merge_MultipleWhenClauses_RoundTrip()
    {
        var sql = "MERGE INTO t USING s ON t.id = s.id WHEN MATCHED THEN UPDATE SET t.a = s.a WHEN NOT MATCHED THEN INSERT (a) VALUES (1)";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    [Fact]
    public void Merge_MultipleWhenDeleteAndUpdate_RoundTrip()
    {
        var sql = "MERGE INTO t USING s ON t.id = s.id WHEN MATCHED AND x > 0 THEN DELETE WHEN MATCHED AND x <= 0 THEN UPDATE SET t.a = s.a";
        Assert.Equal(sql, CCJSqlParserUtil.Parse(sql)!.ToString());
    }

    // ===== 模型结构断言 =====

    [Fact]
    public void Merge_SourceTable_PopulatedInModel()
    {
        var merge = (MergeStatement)CCJSqlParserUtil.Parse(
            "MERGE INTO t USING src ON t.id = src.id WHEN MATCHED THEN DELETE")!;
        Assert.NotNull(merge.SourceTable);
        Assert.NotNull(merge.OnCondition);
        Assert.Single(merge.Operations);
    }

    [Fact]
    public void Merge_WhenAndCondition_PopulatedInModel()
    {
        var merge = (MergeStatement)CCJSqlParserUtil.Parse(
            "MERGE INTO t USING src ON t.id = src.id WHEN MATCHED AND x > 0 THEN DELETE")!;
        var deleteOp = Assert.IsType<Azrng.JSqlParser.Statement.Merge.MergeDelete>(merge.Operations[0]);
        Assert.NotNull(deleteOp.Condition);
    }

    [Fact]
    public void Merge_InsertValues_PopulatedInModel()
    {
        var merge = (MergeStatement)CCJSqlParserUtil.Parse(
            "MERGE INTO t USING src ON t.id = src.id WHEN NOT MATCHED THEN INSERT (a, b) VALUES (1, 2)")!;
        var insertOp = Assert.IsType<Azrng.JSqlParser.Statement.Merge.MergeInsert>(merge.Operations[0]);
        Assert.NotNull(insertOp.Columns);
        Assert.Equal(2, insertOp.Columns!.Count);
        Assert.NotNull(insertOp.Values);
        Assert.Equal(2, insertOp.Values!.Count);
    }
}
