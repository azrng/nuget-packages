using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 非 PostgreSQL 专项修复的 round-trip 验证（补强探针的"能解析"为"解析且语义结构正确"）。
/// 对照 issue 分类清单已修复项（T114 批次），断言 ToString 保留关键语法结构 + AST 关键字段。
/// </summary>
public class NonPgFixRoundTripTest
{
    // ===== Commit 1：通用 + SQL Server =====

    #region #1169 GROUP BY ASC/DESC

    [Fact]
    public void GroupByDesc_RoundTrips()
    {
        var sql = "SELECT a FROM b GROUP BY c DESC";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("GROUP BY c DESC", output);
        // round-trip：再次解析不抛异常
        SqlParser.Parse(output);
    }

    [Fact]
    public void GroupByMultipleDirections_RoundTrips()
    {
        var sql = "SELECT a FROM b GROUP BY c ASC, d DESC";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("c ASC", output);
        Assert.Contains("d DESC", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void GroupByDesc_TransportedAsOriginalText()
    {
        // 方向不结构化为 IsAsc/IsDesc（避免鼓励已弃用语义），但保留原文供 round-trip
        var sql = "SELECT a FROM b GROUP BY c DESC";
        var stmt = SqlParser.Parse(sql) as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.GroupBy);
        Assert.NotNull(stmt.GroupBy!.GroupByColumnReferences);
        Assert.Single(stmt.GroupBy.GroupByColumnReferences!);
        Assert.Equal("c DESC", stmt.GroupBy.GroupByColumnReferences![0].OriginalText);
    }

    [Fact]
    public void GroupByNoDirection_KeepsLegacyField()
    {
        // 无 ASC/DESC 时沿用旧 GroupByExpressions 字段，保持向后兼容
        var stmt = SqlParser.Parse("SELECT a FROM b GROUP BY c, d") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.GroupBy);
        Assert.Null(stmt.GroupBy!.GroupByColumnReferences);
        Assert.Equal(2, stmt.GroupBy.GroupByExpressions.Count);
    }

    #endregion

    #region #911 SQL Server @table 表变量

    [Fact]
    public void TableVariableAtPrefix_RoundTrips()
    {
        var sql = "SELECT columnName FROM @table";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("@table", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void TableVariable_AtName_Structured()
    {
        var stmt = SqlParser.Parse("SELECT * FROM @myTableVar") as PlainSelectType;
        Assert.NotNull(stmt);
        var table = stmt!.FromItem as Table;
        Assert.NotNull(table);
        Assert.Equal("@myTableVar", table!.Name);
    }

    [Fact]
    public void ParameterAtName_StillWorks()
    {
        // 确保 @x 在 WHERE 中仍作为命名参数，不被 table 规则抢匹配
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE id = @p") as PlainSelectType;
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("@p", output);
    }

    #endregion

    #region #1589 PRIMARY KEY NONCLUSTERED

    [Fact]
    public void PrimaryKeyNonclustered_TableLevel_RoundTrips()
    {
        var sql = "CREATE TABLE actor (actor_id INT NOT NULL, first_name VARCHAR(45) NOT NULL, PRIMARY KEY NONCLUSTERED (actor_id))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("NONCLUSTERED", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void PrimaryKeyClustered_ColumnLevel_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT PRIMARY KEY CLUSTERED)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("CLUSTERED", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void UniqueNonclustered_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT, name VARCHAR(50), UNIQUE NONCLUSTERED (name))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("NONCLUSTERED", output);
        SqlParser.Parse(output);
    }

    #endregion

    #region #161 OPTION hint

    [Fact]
    public void OptionMaxRecursion_RoundTrips()
    {
        var sql = "SELECT CustomerID FROM cte OPTION (MAXRECURSION 2)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("OPTION", output);
        Assert.Contains("MAXRECURSION 2", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void OptionMultipleHints_RoundTrips()
    {
        var sql = "SELECT * FROM t OPTION (HASH JOIN, MAXDOP 4)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("HASH JOIN", output);
        Assert.Contains("MAXDOP 4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void OptionHint_StructuredField()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t OPTION (MAXRECURSION 2)") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.OptionHints);
        Assert.Contains("MAXRECURSION", stmt.OptionHints!);
    }

    #endregion

    // ===== Commit 2：MySQL 专项 =====

    #region #854 SELECT INTO @var

    [Fact]
    public void IntoUserVariable_RoundTrips()
    {
        var sql = "SELECT COUNT(*) INTO @countTotal FROM employee";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("INTO @countTotal", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void IntoMultipleUserVariables_RoundTrips()
    {
        var sql = "SELECT a, b INTO @x, @y FROM t";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("INTO @x, @y", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void IntoUserVariable_StructuredField()
    {
        var stmt = SqlParser.Parse("SELECT a INTO @x FROM t") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.IntoVariables);
        Assert.Single(stmt.IntoVariables!);
        Assert.Equal("@x", stmt.IntoVariables![0]);
    }

    #endregion

    #region #1314 INSERT SET AS alias

    [Fact]
    public void InsertSetBasic_RoundTrips()
    {
        var sql = "INSERT INTO t SET a = 1, b = 2";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("SET a = 1, b = 2", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void InsertSetWithOnDuplicate_RoundTrips()
    {
        // MySQL 手册明文的 INSERT SET + ON DUPLICATE 主体；AS new(m,n,p) 行别名极冷门不在本批支持
        var sql = "INSERT INTO t1 SET a = 1, b = 2, c = 3 ON DUPLICATE KEY UPDATE c = m + n";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("SET a = 1, b = 2, c = 3", output);
        Assert.Contains("ON DUPLICATE KEY UPDATE", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void InsertSet_StructuredField()
    {
        var stmt = SqlParser.Parse("INSERT INTO t SET a = 1") as Azrng.JSqlParser.Statement.Insert.Insert;
        Assert.NotNull(stmt);
        Assert.True(stmt!.UseSet);
    }

    #endregion

    #region #2298 CAST CHARACTER SET

    [Fact]
    public void CastCharCharacterSet_RoundTrips()
    {
        var sql = "SELECT CAST('abc' AS CHAR CHARACTER SET utf8mb4)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("CHAR CHARACTER SET utf8mb4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void CastCharCharacterSetCollate_RoundTrips()
    {
        var sql = "SELECT CAST('abc' AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_bin)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("CHARACTER SET utf8mb4", output);
        Assert.Contains("COLLATE utf8mb4_bin", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void CastCharacterSet_StructuredField()
    {
        // 通过 ToString 间接验证结构化字段（CastExpression 是内部 AST）
        var output = SqlParser.Parse("SELECT CAST('x' AS CHAR CHARACTER SET utf8mb4)")!.ToString();
        Assert.Contains("CHARACTER SET utf8mb4", output);
    }

    #endregion

    #region #2427 + #2006 _utf8mb4 introducer

    [Fact]
    public void IntroducerNoSpace_RoundTrips()
    {
        // _utf8mb4'text' 紧贴形式（#2427）
        var sql = "SELECT _utf8mb4'some text' COLLATE utf8mb4_unicode_ci AS custom_string";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("_utf8mb4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void IntroducerWithSpace_RoundTrips()
    {
        // _utf8mb4 'text' 带空格形式（#2006）
        var sql = "SELECT short_name FROM player_table WHERE `short_name` LIKE _utf8mb4 '%Felipe%'";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("_utf8mb4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void Latin1Introducer_RoundTrips()
    {
        // 其他 MySQL introducer：_latin1
        var sql = "SELECT _latin1'some text'";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("_latin1", output);
        SqlParser.Parse(output);
    }

    #endregion

    // ===== Commit 3：MySQL DDL 索引族（T115） =====

    #region #1570 CONSTRAINT name UNIQUE KEY index_name (cols) 双名

    [Fact]
    public void ConstraintUniqueKey_DoubleName_RoundTrips()
    {
        // 上游 #1570：CONSTRAINT 约束名 + UNIQUE KEY 索引名 —— 历史版本吞掉约束名
        var sql = "CREATE TABLE table1 (col1 INT, col2 INT, CONSTRAINT my_constraint UNIQUE KEY index_name (col1))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("CONSTRAINT my_constraint", output);
        Assert.Contains("UNIQUE KEY index_name", output);
        Assert.Contains("(col1)", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void ConstraintUniqueKey_DoubleName_StructuredFields()
    {
        var stmt = SqlParser.Parse("CREATE TABLE table1 (col1 INT, CONSTRAINT c UNIQUE KEY idx (col1))") as Azrng.JSqlParser.Statement.CreateTable.CreateTable;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.Constraints);
        var cons = stmt.Constraints![0];
        // 约束名与索引名分别落字段
        Assert.Equal("c", cons.Name);
        Assert.Equal("idx", cons.IndexName);
        Assert.Equal("UNIQUE KEY", cons.Type);
        Assert.Single(cons.Columns);
        Assert.Equal("col1", cons.Columns[0]);
    }

    [Fact]
    public void ConstraintUniqueKey_SingleName_KeepsLegacyField()
    {
        // 无 CONSTRAINT 前缀的单名场景：Name=索引名，IndexName=null（保持历史行为）
        var stmt = SqlParser.Parse("CREATE TABLE t (id INT, UNIQUE KEY idx (id))") as Azrng.JSqlParser.Statement.CreateTable.CreateTable;
        Assert.NotNull(stmt);
        var cons = stmt!.Constraints![0];
        Assert.Equal("idx", cons.Name);
        Assert.Null(cons.IndexName);
    }

    #endregion

    #region #538 UNIQUE name USING BTREE (cols) COMMENT '...'

    [Fact]
    public void UniqueWithIndexNameAndUsing_RoundTrips()
    {
        // 上游 #538：UNIQUE 后直接跟索引名（无 KEY/INDEX 关键字）+ USING BTREE + COMMENT
        var sql = "CREATE TABLE `t` (`id` int NOT NULL AUTO_INCREMENT, PRIMARY KEY (`id`), UNIQUE `uniq` USING BTREE (`id`) COMMENT 'unique')";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("UNIQUE `uniq`", output);
        Assert.Contains("USING BTREE", output);
        Assert.Contains("(`id`)", output);
        Assert.Contains("COMMENT 'unique'", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void UniqueWithIndexName_StructuredFields()
    {
        var stmt = SqlParser.Parse("CREATE TABLE t (id INT, UNIQUE uniq (id))") as Azrng.JSqlParser.Statement.CreateTable.CreateTable;
        Assert.NotNull(stmt);
        var cons = stmt!.Constraints![0];
        Assert.Equal("UNIQUE", cons.Type);
        Assert.Equal("uniq", cons.IndexName);
        Assert.Null(cons.Name); // 无 CONSTRAINT 前缀，Name 为 null
    }

    [Fact]
    public void UniquePlain_NoIndexName_StillWorks()
    {
        // 普通 UNIQUE (cols) 无索引名 —— 保持历史行为，走简单约束分支
        var stmt = SqlParser.Parse("CREATE TABLE t (id INT, UNIQUE (id))") as Azrng.JSqlParser.Statement.CreateTable.CreateTable;
        Assert.NotNull(stmt);
        var cons = stmt!.Constraints![0];
        Assert.Equal("UNIQUE", cons.Type);
        Assert.Null(cons.IndexName);
        var output = stmt!.ToString()!;
        Assert.Contains("UNIQUE (id)", output);
    }

    #endregion

    #region #1893/#823/#1295 已支持回归

    [Fact]
    public void UniqueIndexWithUsingComment_RoundTrips()
    {
        // #1893: UNIQUE INDEX name (cols) USING BTREE COMMENT '...' —— 移植版已支持
        var sql = "CREATE TABLE `sys_user` (`id` bigint NOT NULL, UNIQUE INDEX `ina_index` (`id`,`name`) USING BTREE COMMENT 'Unique')";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("UNIQUE INDEX `ina_index`", output);
        Assert.Contains("USING BTREE", output);
        Assert.Contains("COMMENT 'Unique'", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void UniqueIndexInCreateTable_RoundTrips()
    {
        // #823: 建表 DDL 内 UNIQUE INDEX（注：原始 SQL 含 bigint unsigned 数据类型修饰符，
        // 属独立数据类型问题不在本批；此处用 core 形式验证 UNIQUE INDEX 部分）
        var sql = "CREATE TABLE `test3` (`NAME` varchar(255) NOT NULL, `ID` bigint NOT NULL, PRIMARY KEY (`NAME`), UNIQUE INDEX idx(`id`))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("UNIQUE INDEX idx", output);
        Assert.Contains("(`id`)", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void AlterAddIndex_RoundTrips()
    {
        // #1295: ALTER TABLE t ADD INDEX (col) —— 移植版已支持
        var sql = "ALTER TABLE table_name8 ADD INDEX (column_1)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("ADD INDEX", output);
        Assert.Contains("(column_1)", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void AlterAddUniqueWithIndexName_RoundTrips()
    {
        // #538 ALTER 变体：ALTER TABLE t ADD UNIQUE idx (col) —— 索引名不能丢
        var sql = "ALTER TABLE t ADD UNIQUE idx (col)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("UNIQUE idx", output);
        Assert.Contains("(col)", output);
        SqlParser.Parse(output);
    }

    #endregion
}
