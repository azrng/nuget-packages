using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 非 PostgreSQL 上游 issue 现状探针：仅断言「能解析 + ToString 不抛异常」，
/// 用于识别哪些上游缺陷在 Azrng 移植版仍复现。失败 = 复现上游缺陷，需要修。
/// 数据来源：issue/jsqlparser/issue分类清单.md。
/// 本批次（T114）已修复的 issue 探针全部改为非 Skip，验证修复；其余暂不修的仍 Skip。
/// </summary>
public class NonPgIssuesProbeTest
{
    private static void Probe(string sql)
    {
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    // ===== 本批次（T114）已修复，探针转绿 =====

    // ③ #1169 GROUP BY c desc
    [Fact]
    public void Issue1169_GroupByDesc() => Probe("SELECT a FROM b GROUP BY c DESC");

    // ⑦ #2428 PROCEDURE ANALYSE()
    [Fact]
    public void Issue2428_MysqlProcedureAnalyse() =>
        Probe("SELECT col1, col2 FROM heavy_table PROCEDURE ANALYSE(10, 256)");

    // ⑦ #2427 _utf8mb4 introducer + COLLATE
    [Fact]
    public void Issue2427_MysqlUtf8mb4Introducer() =>
        Probe("SELECT _utf8mb4'some text' COLLATE utf8mb4_unicode_ci AS custom_string");

    // ⑦ #2006 _utf8mb4 dialects（与 #2427 同源修复）
    [Fact]
    public void Issue2006_MysqlUtf8mb4Dialect() =>
        Probe("SELECT short_name FROM player_table WHERE (`short_name` LIKE _utf8mb4 '%Felipe%')");

    // ⑦ #2298 CAST as CHAR with CHARACTER SET
    [Fact]
    public void Issue2298_MysqlCastCharCharset() =>
        Probe("SELECT CAST('abc' AS CHAR CHARACTER SET utf8mb4)");

    // ⑦ #854 INTO @var
    [Fact]
    public void Issue854_MysqlIntoUserVar() =>
        Probe("SELECT COUNT(*) INTO @countTotal FROM employee");

    // ⑦ #1314 INSERT SET 带 AS 别名
    [Fact]
    public void Issue1314_InsertSetAlias() =>
        Probe("INSERT INTO t1 SET a=1,b=2,c=3 AS new(m,n,p) ON DUPLICATE KEY UPDATE c = m+n");

    // ⑥ #911 表变量 @table
    [Fact]
    public void Issue911_SqlServerTableVariable() =>
        Probe("SELECT columnName FROM @table");

    // ⑥ #161 OPTION (MAXRECURSION 2)
    [Fact]
    public void Issue161_SqlServerQueryHint() =>
        Probe("SELECT CustomerID FROM cte OPTION (MAXRECURSION 2)");

    // ⑥ #1589 PRIMARY KEY NONCLUSTERED (SQL Server)
    [Fact]
    public void Issue1589_SqlServerPrimaryKeyNonclustered() =>
        Probe("CREATE TABLE actor (actor_id INT NOT NULL IDENTITY, first_name VARCHAR(45) NOT NULL, PRIMARY KEY NONCLUSTERED (actor_id))");

    // ⑧ #2421 BigQuery MERGE WHEN NOT MATCHED BY TARGET/SOURCE — 本批次不做（BigQuery 小众语法）
    [Fact(Skip = "本批次暂不修（小众语言）")]
    public void Issue2421_BigQueryMergeNotMatchedByTarget() =>
        Probe(@"MERGE INTO target_table AS tt
USING (SELECT key, field FROM source_table) AS st
ON tt.key = st.key
WHEN NOT MATCHED BY TARGET THEN INSERT (key, field) VALUES (st.key, st.field)
WHEN NOT MATCHED BY SOURCE THEN DELETE");

    // ===== 本批次暂不修，保留探针记录现状（Skip） =====

    // ③ #2435 MySQL 0x 十六进制字面量
    [Fact(Skip = "本批次暂不修")]
    public void Issue2435_MySqlHexLiteral() => Probe("SELECT 0xFF FROM t");

    // ③ #2359 LIMIT 子查询
    [Fact(Skip = "本批次暂不修")]
    public void Issue2359_LimitSubquery() =>
        Probe("SELECT * FROM t LIMIT (SELECT 10)");

    // ⑦ #1295 ALTER ADD INDEX (col)
    [Fact(Skip = "本批次暂不修")]
    public void Issue1295_MysqlAlterAddIndex() =>
        Probe("ALTER TABLE table_name8 ADD INDEX (column_1)");

    // ⑦ #1927 建表 DDL 函数索引
    [Fact(Skip = "本批次暂不修")]
    public void Issue1927_MysqlFunctionalIndex() =>
        Probe("CREATE TABLE t (id INT, KEY idx_lower ((LOWER(name))))");

    // ⑦ #1893 UNIQUE INDEX 名 + USING BTREE COMMENT
    [Fact(Skip = "本批次暂不修")]
    public void Issue1893_MysqlUniqueIndex() =>
        Probe("CREATE TABLE `sys_user` (`id` bigint NOT NULL, UNIQUE INDEX `ina_index` (`id`,`name`) USING BTREE COMMENT 'Unique')");

    // ⑦ #823 建表 DDL unique index
    [Fact(Skip = "本批次暂不修")]
    public void Issue823_MysqlUniqueIndex() =>
        Probe("CREATE TABLE `test3` (`NAME` varchar(255) NOT NULL, `ID` bigint unsigned NOT NULL, PRIMARY KEY (`NAME`), UNIQUE INDEX idx(`id`))");

    // ⑦ #538 unique key 带 comment
    [Fact(Skip = "本批次暂不修")]
    public void Issue538_MysqlUniqueKeyComment() =>
        Probe("CREATE TABLE `t` (`id` int NOT NULL AUTO_INCREMENT, PRIMARY KEY (`id`), UNIQUE `uniq` USING BTREE (`id`) COMMENT 'unique')");

    // ⑦ #1570 CONSTRAINT my_constraint UNIQUE KEY index_name (col1)
    [Fact(Skip = "本批次暂不修")]
    public void Issue1570_MysqlConstraintUniqueKeyWithName() =>
        Probe("CREATE TABLE table1 (col1 INT, col2 INT, CONSTRAINT my_constraint UNIQUE KEY index_name (col1))");

    // ⑥ #397 全文搜索 %%
    [Fact(Skip = "本批次暂不修")]
    public void Issue397_SqlServerFullTextPctPct() =>
        Probe("SELECT * FROM vwdatasearch WHERE ComId = ? AND (Title1 %% ?)");

    // ⑥ #2033 insert bulk
    [Fact(Skip = "本批次暂不修")]
    public void Issue2033_SqlServerInsertBulk() =>
        Probe("INSERT BULK tpch.dbo.order_line([ol_o_id] int,[ol_d_id] tinyint) WITH(ROWS_PER_BATCH=500000)");

    // ⑤ #672 外连接 (+)
    [Fact(Skip = "本批次暂不修")]
    public void Issue672_OracleOuterJoin() =>
        Probe("SELECT * FROM table1 t1, table2 t2 WHERE t1.col1 BETWEEN t2.col2(+) AND t2.col3(+)");

    // ⑤ #2039 ALTER ADD CONSTRAINT ... USING INDEX TABLESPACE
    [Fact(Skip = "本批次暂不修")]
    public void Issue2039_OracleAddConstraintTablespace() =>
        Probe("ALTER TABLE bfmcs.your_table ADD CONSTRAINT your_table_pk PRIMARY KEY (ID) USING INDEX TABLESPACE your_tablespace");

    // ⑨ #2440 WHERE col IN ('X') AND x >= y
    [Fact(Skip = "本批次暂不修")]
    public void Issue2440_WhereInAndPrecedence() =>
        Probe("SELECT * FROM record WHERE status IN ('CONFIRMED') AND start_datetime >= CURRENT_TIMESTAMP");

    // ⑨ #1170 NotExpression 解析（双 not）
    [Fact(Skip = "本批次暂不修")]
    public void Issue1170_NotNotExpression() =>
        SqlParser.ParseCondExpression("not not 1 = 1")?.ToString();

    // ⑧ #2433 LATERAL VIEW 三列及以上别名
    [Fact(Skip = "本批次暂不修")]
    public void Issue2433_HiveLateralViewManyAliases() =>
        Probe("SELECT a FROM t LATERAL VIEW json_tuple(j, 'a', 'b', 'c') x AS c1, c2, c3");
}
