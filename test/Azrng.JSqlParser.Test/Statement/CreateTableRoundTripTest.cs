using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.CreateTable;
using CreateTbl = Azrng.JSqlParser.Statement.CreateTable.CreateTable;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// CREATE TABLE 方言与约束结构化 round-trip 测试，覆盖 BL-06 全量移植能力。
/// 对齐上游 CreateTableTest 代表性用例。
/// </summary>
public class CreateTableRoundTripTest
{
    /// <summary>断言 SQL 解析后 ToString 与原文一致（round-trip）。方言选项用透传字符串策略。</summary>
    private static void AssertRoundTrip(string sql)
    {
        var stmt = SqlParser.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    // ── 表级选项（ENGINE / CHARSET / COLLATE / COMMENT） ──────────────

    [Fact]
    public void TableOptions_EngineEquals_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT) ENGINE = InnoDB");

    [Fact]
    public void TableOptions_MultipleOptions_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT) ENGINE = InnoDB CHARSET = utf8");

    [Fact]
    public void TableOptions_EngineAutoIncrementCharset_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARSET = utf8");

    // ── ClickHouse 方言 ──────────────────────────────────────────────

    [Fact]
    public void ClickHouse_EngineOrderBy_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id UInt64) ENGINE = MergeTree() ORDER BY id");

    [Fact]
    public void ClickHouse_SampleBy_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE tmp (id UInt64) ENGINE = MergeTree() ORDER BY id SAMPLE BY id");

    // ── 分区 ─────────────────────────────────────────────────────────

    [Fact]
    public void Partition_HashPartitions_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (col VARCHAR(32)) PARTITION BY HASH (col) PARTITIONS 4");

    // ── Oracle RowMovement ───────────────────────────────────────────

    [Fact]
    public void Oracle_EnableRowMovement_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (d DATE) ENABLE ROW MOVEMENT");

    [Fact]
    public void Oracle_DisableRowMovement_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (d DATE) DISABLE ROW MOVEMENT");

    // ── CTAS / LIKE ──────────────────────────────────────────────────

    [Fact]
    public void CreateTable_AsSelect_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE a AS SELECT col1 FROM b");

    [Fact]
    public void CreateTable_LikeTable_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE a LIKE b");

    // ── 约束结构化 ───────────────────────────────────────────────────

    [Fact]
    public void ForeignKey_WithOnDeleteCascade_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, FOREIGN KEY (uid) REFERENCES u(id) ON DELETE CASCADE)");

    [Fact]
    public void ForeignKey_WithOnDeleteOnUpdate_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, FOREIGN KEY (uid) REFERENCES u(id) ON DELETE CASCADE ON UPDATE SET NULL)");

    [Fact]
    public void CheckConstraint_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, CHECK (id > 0))");

    [Fact]
    public void ExcludeConstraint_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, EXCLUDE WHERE (id > 0))");

    // ── CREATE 子句选项 ──────────────────────────────────────────────

    [Fact]
    public void Create_Temporary_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TEMPORARY TABLE t (id INT)");

    [Fact]
    public void Create_IfNotExists_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE IF NOT EXISTS t (id INT)");

    [Fact]
    public void Create_Unlogged_ShouldRoundTrip()
        => AssertRoundTrip("CREATE UNLOGGED TABLE t (id INT)");

    // ── 更多上游代表性用例 ───────────────────────────────────────────

    [Fact]
    public void TableOptions_CollateComment_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT) COLLATE = utf8_bin COMMENT = 'test table'");

    [Fact]
    public void ClickHouse_OrderByTuple_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (url String) ENGINE = MergeTree() ORDER BY tuple()");

    [Fact]
    public void ForeignKey_Named_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, CONSTRAINT fk1 FOREIGN KEY (uid) REFERENCES u(id))");

    [Fact]
    public void Check_Named_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, CONSTRAINT ck1 CHECK (id > 0))");

    [Fact]
    public void Create_OrReplace_ShouldRoundTrip()
        => AssertRoundTrip("CREATE OR REPLACE TABLE t (id INT)");

    // ── 结构化断言（验证 AST 字段正确填充，非仅 round-trip） ──────────

    [Fact]
    public void TableOptions_ShouldPopulateTableOptionsList()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT) ENGINE = InnoDB CHARSET = utf8")!;
        Assert.NotNull(stmt.TableOptions);
        // 表级选项透传为字符串列表（ANTLR 贪婪匹配可能合并相邻选项，验证拼接内容而非数量）
        var combined = string.Join(" ", stmt.TableOptions!);
        Assert.Contains("ENGINE = InnoDB", combined);
        Assert.Contains("CHARSET = utf8", combined);
    }

    [Fact]
    public void ForeignKey_ShouldPopulateReferencedTableAndActions()
    {
        var stmt = (CreateTbl)SqlParser.Parse(
            "CREATE TABLE t (id INT, FOREIGN KEY (uid) REFERENCES u(id) ON DELETE CASCADE ON UPDATE SET NULL)")!;
        var fk = Assert.Single(stmt.Constraints!.OfType<ForeignKeyIndex>());
        Assert.NotNull(fk.ReferencedTable);
        Assert.Equal("u", fk.ReferencedTable!.Name);
        Assert.Equal("id", Assert.Single(fk.ReferencedColumnNames!));
        Assert.NotNull(fk.OnDelete);
        Assert.Equal(ReferentialActionMode.Cascade, fk.OnDelete!.Action);
        Assert.NotNull(fk.OnUpdate);
        Assert.Equal(ReferentialActionMode.SetNull, fk.OnUpdate!.Action);
    }

    [Fact]
    public void CheckConstraint_ShouldPopulateExpression()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT, CHECK (id > 0))")!;
        var check = Assert.Single(stmt.Constraints!.OfType<CheckConstraint>());
        Assert.NotNull(check.Expression);
    }

    [Fact]
    public void ExcludeConstraint_ShouldPopulateExpression()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT, EXCLUDE WHERE (id > 0))")!;
        var exclude = Assert.Single(stmt.Constraints!.OfType<ExcludeConstraint>());
        Assert.NotNull(exclude.Expression);
    }

    [Fact]
    public void RowMovement_ShouldPopulateMode()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (d DATE) ENABLE ROW MOVEMENT")!;
        Assert.NotNull(stmt.RowMovement);
        Assert.Equal(RowMovementMode.Enable, stmt.RowMovement!.Mode);
    }

    [Fact]
    public void ColumnDefinition_ShouldUseColDataType()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT NOT NULL)")!;
        var col = Assert.Single(stmt.ColumnDefinitions!);
        Assert.Equal("INT", col.ColDataType.DataType);
        Assert.Contains("NOT NULL", col.ColumnSpecs);
    }

    [Fact]
    public void CreateTable_AsSelect_ShouldPopulateSelect()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE a AS SELECT col1 FROM b")!;
        Assert.NotNull(stmt.Select);
    }

    [Fact]
    public void CreateTable_LikeTable_ShouldPopulateLikeTable()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE a LIKE b")!;
        Assert.NotNull(stmt.LikeTable);
        Assert.Equal("b", stmt.LikeTable!.Name);
    }

    [Fact]
    public void CreateOptions_ShouldPopulateCreateOptions()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TEMPORARY TABLE t (id INT)")!;
        Assert.NotNull(stmt.CreateOptions);
        Assert.Contains("TEMPORARY", stmt.CreateOptions!);
    }

    // ── STRUCT/ARRAY 复合列类型（T089） ──────────────────────────────

    [Fact]
    public void ArrayColumnType_Basic_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, tags ARRAY<INT>)");

    [Fact]
    public void ArrayColumnType_WithLength_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, tags ARRAY<VARCHAR(100)>)");

    [Fact]
    public void ArrayColumnType_StringMax_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, tags ARRAY<STRING(MAX)>)");

    [Fact]
    public void ArrayColumnType_Nested_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, matrix ARRAY<ARRAY<INT>>)");

    [Fact]
    public void StructColumnType_Basic_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, addr STRUCT(street VARCHAR(100), city VARCHAR(50)))");

    [Fact]
    public void StructColumnType_SingleField_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, val STRUCT(x INT))");

    [Fact]
    public void StructColumnType_NestedArray_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, data STRUCT(x INT, y ARRAY<INT>))");

    [Fact]
    public void Spanner_MultipleArrayColumns_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE cmd (id INT64, arr_bool ARRAY<BOOL>, arr_bytes ARRAY<BYTES(1024)>, arr_string ARRAY<STRING(MAX)>)");

    // ── STRUCT/ARRAY 结构化断言 ──────────────────────────────────────

    [Fact]
    public void ArrayColumnType_ShouldFlattenIntoDataType()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT, tags ARRAY<INT>)")!;
        var col = stmt.ColumnDefinitions![1];
        Assert.Equal("ARRAY<INT>", col.ColDataType.DataType);
        // ARRAY 扁平化存储，不拆 ArgumentsStringList
        Assert.Null(col.ColDataType.ArgumentsStringList);
    }

    [Fact]
    public void ArrayColumnType_Nested_ShouldFlattenEntireType()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT, matrix ARRAY<ARRAY<INT>>)")!;
        var col = stmt.ColumnDefinitions![1];
        Assert.Equal("ARRAY<ARRAY<INT>>", col.ColDataType.DataType);
    }

    [Fact]
    public void StructColumnType_ShouldPopulateArgumentsStringList()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT, addr STRUCT(street VARCHAR(100), city VARCHAR(50)))")!;
        var col = stmt.ColumnDefinitions![1];
        Assert.Equal("STRUCT", col.ColDataType.DataType);
        Assert.NotNull(col.ColDataType.ArgumentsStringList);
        Assert.Equal(2, col.ColDataType.ArgumentsStringList!.Count);
        Assert.Contains("street VARCHAR(100)", col.ColDataType.ArgumentsStringList);
        Assert.Contains("city VARCHAR(50)", col.ColDataType.ArgumentsStringList);
    }

    [Fact]
    public void StructColumnType_NestedArray_ShouldKeepFieldTypeInArguments()
    {
        var stmt = (CreateTbl)SqlParser.Parse("CREATE TABLE t (id INT, data STRUCT(x INT, y ARRAY<INT>))")!;
        var col = stmt.ColumnDefinitions![1];
        Assert.Equal("STRUCT", col.ColDataType.DataType);
        Assert.Contains("y ARRAY<INT>", col.ColDataType.ArgumentsStringList!);
    }

    // ── 边缘遗留项（T090） ───────────────────────────────────────────

    // 缺口1: character varying / character 列类型
    [Fact]
    public void CharacterVaring_ColumnType_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, name CHARACTER VARYING(255))");

    [Fact]
    public void CharacterVaring_NoLength_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, name CHARACTER VARYING)");

    // 缺口2: TIMESTAMP WITH/WITHOUT TIME ZONE
    [Fact]
    public void Timestamp_WithTimeZone_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (d TIMESTAMP WITH TIME ZONE)");

    [Fact]
    public void Timestamp_WithoutTimeZone_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (d TIMESTAMP WITHOUT TIME ZONE)");

    [Fact]
    public void Timestamp_WithLocalTimeZone_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (d TIMESTAMP WITH LOCAL TIME ZONE)");

    // 缺口3: MySQL USING BTREE/HASH 索引选项
    [Fact]
    public void Index_UsingBtree_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, KEY idx (id) USING BTREE)");

    [Fact]
    public void PrimaryKey_UsingBtree_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, PRIMARY KEY (id) USING BTREE)");

    [Fact]
    public void Index_CommentOption_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, KEY idx (id) COMMENT 'test')");

    // 缺口4: 功能性/表达式索引
    [Fact]
    public void Index_FunctionalExpression_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (PK INT, b INT, INDEX fAdd ((b + 1)))");

    [Fact]
    public void Index_FunctionalExpressionWithDesc_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (PK INT, b INT, INDEX fAdd ((b + 1) DESC))");

    // 缺口5: set('a','b') 类型
    [Fact]
    public void SetType_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (priv set('Select', 'Insert'))");

    // 缺口6: 数组带尺寸 int[5]
    [Fact]
    public void ArrayType_WithSize_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, arr INT[5])");

    [Fact]
    public void ArrayType_MultiDimensionWithSize_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT, grid TEXT[3][2])");

    // 缺口7: :: text[] 数组类型 cast（既有 :: cast 行为无空格）
    [Fact]
    public void Cast_ArrayType_ShouldRoundTrip()
        => AssertRoundTrip("SELECT ARRAY[]::TEXT[] AS empty_arr");

    // 缺口8: 表级 WITH (fillfactor=70)
    [Fact]
    public void TableLevel_WithOption_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (id INT) WITH (fillfactor = 70)");

    // 缺口9: Spanner 列级 OPTIONS (k = v)
    [Fact]
    public void Spanner_ColumnOptions_ShouldRoundTrip()
        => AssertRoundTrip("CREATE TABLE t (ts TIMESTAMP OPTIONS (allow_commit_timestamp = true))");
}



