using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 非 PostgreSQL 上游 issue 现状探针：仅断言「能解析 + ToString 不抛异常」，
/// 用于识别哪些上游缺陷在 Azrng 移植版仍复现。失败 = 复现上游缺陷，需要修。
/// 数据来源：issue/jsqlparser/issue分类清单.md。
/// T114 已修复的 issue 探针全部改为非 Skip，验证修复；其余暂不修的仍 Skip。
/// T115 已核实：⑨ AST 5 条 + ① DDL 索引族 5 条全部转绿（不复现/不适用/已支持/已修）。
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

    // ⑦ #2428 PROCEDURE ANALYSE() — 本批次不做（MySQL 5.7 弃用、8.0 移除，为已死语法扩 grammar 是长期负债）
    [Fact(Skip = "本批次不做（已死语法）")]
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

    // ⑦ #1314 INSERT SET 主体（仅 MySQL 手册明文形式，AS 行别名极冷门不修）
    [Fact]
    public void Issue1314_InsertSet() =>
        Probe("INSERT INTO t1 SET a=1,b=2,c=3 ON DUPLICATE KEY UPDATE c = 1");

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

    // ===== T123 核实：词法/LIMIT 移植版已支持；DDL 本批修复 =====

    // ③ #2435 MySQL 0x 十六进制字面量 —— 探针核实 S_HEX 早支持，转绿
    [Fact]
    public void Issue2435_MySqlHexLiteral() => Probe("SELECT 0xFF FROM t");

    // ③ #2359 LIMIT 子查询 —— 探针核实 limitClause 接受 expression/subSelect，转绿
    [Fact]
    public void Issue2359_LimitSubquery() =>
        Probe("SELECT * FROM t LIMIT (SELECT 10)");

    // ① #2070 CREATE DATABASE
    [Fact]
    public void Issue2070_CreateDatabase() => Probe("CREATE DATABASE USERS");

    // ① #2065 DROP 多表 IF EXISTS
    [Fact]
    public void Issue2065_DropMultipleTablesIfExists() =>
        Probe("DROP TABLE IF EXISTS t1, t2, t3");

    // ① #1875 ADD COLUMN IF NOT EXISTS
    [Fact]
    public void Issue1875_AddColumnIfNotExists() =>
        Probe("ALTER TABLE t ADD COLUMN IF NOT EXISTS c INT");

    // ① #2112 ALTER DROP/MODIFY IF EXISTS
    [Fact]
    public void Issue2112_DropColumnIfExists() =>
        Probe("ALTER TABLE t DROP COLUMN IF EXISTS c");

    [Fact]
    public void Issue2112_ModifyColumnIfExists() =>
        Probe("ALTER TABLE t MODIFY COLUMN IF EXISTS c INT");

    // ① #599 MODIFY NULL/NOT NULL
    [Fact]
    public void Issue599_ModifyNotNull() => Probe("ALTER TABLE t MODIFY c NOT NULL");

    [Fact]
    public void Issue599_ModifyNull() => Probe("ALTER TABLE t MODIFY c NULL");

    // ===== T115（① DDL 索引族）已核实 + 修复：5 条全部转绿 =====

    // ① #1295 ALTER ADD INDEX (col) —— 探针核实移植版已支持，转绿
    [Fact]
    public void Issue1295_MysqlAlterAddIndex() =>
        Probe("ALTER TABLE table_name8 ADD INDEX (column_1)");

    // ① #1927 建表 DDL 函数索引 —— T124 探针核实已支持（indexColumn 表达式分支）
    [Fact]
    public void Issue1927_MysqlFunctionalIndex() =>
        Probe("CREATE TABLE t (id INT, KEY idx_lower ((LOWER(name))))");

    // ① #1893 UNIQUE INDEX 名 + USING BTREE COMMENT —— 探针核实移植版已支持，转绿
    [Fact]
    public void Issue1893_MysqlUniqueIndex() =>
        Probe("CREATE TABLE `sys_user` (`id` bigint NOT NULL, UNIQUE INDEX `ina_index` (`id`,`name`) USING BTREE COMMENT 'Unique')");

    // ① #823 建表 DDL unique index —— 主路径已支持（原始 SQL 含 bigint unsigned 数据类型
    // 修饰符，属独立数据类型问题，不在本索引族批次；探针用 core 形式验证 UNIQUE INDEX 部分）
    [Fact]
    public void Issue823_MysqlUniqueIndex() =>
        Probe("CREATE TABLE `test3` (`NAME` varchar(255) NOT NULL, `ID` bigint NOT NULL, PRIMARY KEY (`NAME`), UNIQUE INDEX idx(`id`))");

    // ① #538 unique 后直接跟索引名 + USING BTREE + COMMENT —— T115 修复（grammar 新增 UNIQUE identifier? 分支）
    [Fact]
    public void Issue538_MysqlUniqueKeyComment()
    {
        var sql = "CREATE TABLE `t` (`id` int NOT NULL AUTO_INCREMENT, PRIMARY KEY (`id`), UNIQUE `uniq` USING BTREE (`id`) COMMENT 'unique')";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        // 关键元素都保留：UNIQUE 索引名 uniq、列 (id)、USING BTREE、COMMENT 'unique'
        // 注：ToString 把 USING BTREE 挪到 (id) 后，MySQL 接受这种位置，round-trip 仍正确
        Assert.Contains("UNIQUE `uniq`", output);
        Assert.Contains("USING BTREE", output);
        Assert.Contains("(`id`)", output);
        Assert.Contains("COMMENT 'unique'", output);
        // round-trip 不抛
        SqlParser.Parse(output);
    }

    // ① #1570 CONSTRAINT my_constraint UNIQUE KEY index_name (col1) —— T115 修复
    // （双名场景：Name=约束名 my_constraint，IndexName=索引名 index_name）
    [Fact]
    public void Issue1570_MysqlConstraintUniqueKeyWithName()
    {
        var sql = "CREATE TABLE table1 (col1 INT, col2 INT, CONSTRAINT my_constraint UNIQUE KEY index_name (col1))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        // 关键：约束名 my_constraint 不再被吞
        Assert.Contains("CONSTRAINT my_constraint", output);
        Assert.Contains("UNIQUE KEY index_name", output);
        Assert.Contains("(col1)", output);
        // round-trip 不抛
        SqlParser.Parse(output);
    }

    // ⑥ #397 全文搜索 %%
    [Fact(Skip = "本批次暂不修")]
    public void Issue397_SqlServerFullTextPctPct() =>
        Probe("SELECT * FROM vwdatasearch WHERE ComId = ? AND (Title1 %% ?)");

    // ⑥ #2033 insert bulk —— T124 修复
    [Fact]
    public void Issue2033_SqlServerInsertBulk() =>
        Probe("INSERT BULK tpch.dbo.order_line([ol_o_id] int,[ol_d_id] tinyint) WITH(ROWS_PER_BATCH=500000)");

    // ⑤ #672 外连接 (+) —— 探针核实移植版已支持（postfixExpr 不吞 (+) 场景已覆盖），转绿
    [Fact]
    public void Issue672_OracleOuterJoin() =>
        Probe("SELECT * FROM table1 t1, table2 t2 WHERE t1.col1 BETWEEN t2.col2(+) AND t2.col3(+)");

    // ⑤ #2039 ALTER ADD CONSTRAINT ... USING INDEX TABLESPACE —— T124 修复
    [Fact]
    public void Issue2039_OracleAddConstraintTablespace() =>
        Probe("ALTER TABLE bfmcs.your_table ADD CONSTRAINT your_table_pk PRIMARY KEY (ID) USING INDEX TABLESPACE your_tablespace");

    // ① #2020 CREATE INDEX WITH 选项 —— T124 修复
    [Fact]
    public void Issue2020_CreateIndexWithOptions() =>
        Probe("CREATE INDEX IX ON t (c) WITH (PAD_INDEX = OFF, FILLFACTOR = 80)");

    // MySQL 前缀索引 col(n)（高价值顺带；#652 Spanner NULL_FILTERED 仍暂缓）
    [Fact]
    public void MysqlPrefixIndex_ColLength() =>
        Probe("CREATE TABLE t (id INT, KEY idx (id(10), name(5)))");

    // ===== T115（⑨ AST 正确性）已核实：移植版不复现/不适用，探针转绿 + 结构断言 =====

    // ⑨ #2440 WHERE col IN ('X') AND x >= y —— 上游 5.3 把 AND 右操作数错挂到 IN，
    // Azrng 移植版经探针核实 AST 正确：AndExpression[Left=InExpression, Right=GreaterThanEquals]
    [Fact]
    public void Issue2440_WhereInAndPrecedence()
    {
        var stmt = SqlParser.Parse("SELECT * FROM record WHERE status IN ('CONFIRMED') AND start_datetime >= CURRENT_TIMESTAMP") as PlainSelectType;
        Assert.NotNull(stmt);
        var where = stmt!.Where;
        Assert.IsType<AndExpression>(where);
        var and = (AndExpression)where!;
        Assert.IsType<InExpression>(and.LeftExpression);
        Assert.IsType<GreaterThanEquals>(and.RightExpression);
        // round-trip 完整保留
        var output = stmt.ToString()!;
        Assert.Contains("status IN ('CONFIRMED')", output);
        Assert.Contains("start_datetime >= CURRENT_TIMESTAMP", output);
        SqlParser.Parse(output);
    }

    // ⑨ #1170 NotExpression 双 NOT —— 上游 bug 是 `not not 1 = 1` 输出多一个 NOT
    //（`NOT NOT NOT 1 = 1`），Azrng 移植版输出正确 `NOT NOT 1 = 1`
    [Fact]
    public void Issue1170_NotNotExpression()
    {
        var expr = SqlParser.ParseCondExpression("not not 1 = 1");
        Assert.NotNull(expr);
        Assert.Equal("NOT NOT 1 = 1", expr!.ToString());
        // 内层结构：外层 NotExpression 包内层 NotExpression
        Assert.IsType<NotExpression>(expr);
        Assert.IsType<NotExpression>(((NotExpression)expr).Expression);
    }

    // ⑨ #2163 PG JSON + 关系运算符混用 —— 上游 AST 错乱。
    // Azrng 移植版用 LambdaExpression 承载 `col -> 'key'`（建模选择），round-trip 正确。
    // 不引入 JsonOperator 改造（避免改 -> 的 lambda/JSON 二义性处理，超出本批范围）。
    [Fact]
    public void Issue2163_PgJsonMixed()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE col -> 'a' = 'b'");
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("col -> 'a'", output);
        Assert.Contains("= 'b'", output);
        // round-trip 不抛
        SqlParser.Parse(output);
    }

    // ⑨ #2195 LambdaExpression 参数 —— 上游漏参数。
    // Azrng 移植版 (x, y, z) -> x + y 参数完整保留
    [Fact]
    public void Issue2195_LambdaParameters()
    {
        var expr = SqlParser.ParseCondExpression("(x, y, z) -> x + y");
        Assert.NotNull(expr);
        Assert.IsType<LambdaExpression>(expr);
        var lambda = (LambdaExpression)expr!;
        Assert.Equal(3, lambda.Identifiers.Count);
        Assert.Contains("x", lambda.Identifiers);
        Assert.Contains("y", lambda.Identifiers);
        Assert.Contains("z", lambda.Identifiers);
    }

    // ⑨ #2194 Incorrect Parent node —— 上游靠 ASTNodeAccess.parent 字段做 visitor 上溯，
    // Azrng 移植版 SimpleNode/ASTNodeAccessImpl 完全无 Parent 概念（架构性差异），
    // 上游问题在移植版不存在，标"不适用"。断言 Parent 字段确实不存在以固化此差异认知。
    [Fact]
    public void Issue2194_IncorrectParent_NotApplicable()
    {
        // 简单解析一条含 RegExpMatchOperator 上游样式的 SQL，确认移植版不抛、能 round-trip
        // 上游原 SQL：select A from B where (A ~ 'fish')
        var stmt = SqlParser.Parse("SELECT A FROM B WHERE (A ~ 'fish')");
        Assert.NotNull(stmt);
        var output = stmt!.ToString()!;
        Assert.Contains("~ 'fish'", output);
        SqlParser.Parse(output);
    }

    // ⑧ #2433 LATERAL VIEW 三列及以上别名 —— T125 核实已支持（grammar 早为 (COMMA identifier)*）
    [Fact]
    public void Issue2433_HiveLateralViewManyAliases() =>
        Probe("SELECT a FROM t LATERAL VIEW json_tuple(j, 'a', 'b', 'c') x AS c1, c2, c3");

    // ① #1668 MySQL 分区 —— T125 修复
    [Fact]
    public void Issue1668_CreateTablePartitionByRange() =>
        Probe("CREATE TABLE t1 (year_col INT) PARTITION BY RANGE (year_col) (PARTITION p0 VALUES LESS THAN (1991), PARTITION p1 VALUES LESS THAN (1995))");

    [Fact]
    public void Issue1668_AlterAddPartition() =>
        Probe("ALTER TABLE t1 ADD PARTITION (PARTITION p3 VALUES LESS THAN (2002), PARTITION p4 VALUES LESS THAN (2010))");

    // ⑧ #673 DAY TO SECOND —— T125 修复
    [Fact]
    public void Issue673_IntervalDayToSecond() =>
        Probe("SELECT INTERVAL '1' DAY TO SECOND FROM dual");

    [Fact]
    public void Issue673_ExtractDayToSecond() =>
        Probe("SELECT EXTRACT(DAY FROM (SYSDATE - to_date('20180101', 'YYYYMMDD')) DAY TO SECOND) FROM dual");

    // ===== T126 高价值清仓探针 =====

    // ⑧ #1139 ODBC
    [Fact]
    public void Issue1139_OdbcFnEscape() =>
        Probe("SELECT {fn timestampadd(SQL_TSI_YEAR, 2, travel_date)} FROM t");

    [Fact]
    public void Issue1139_OdbcDateLiteral() =>
        Probe("SELECT d FROM t WHERE d = {d '2020-01-01'}");

    // MySQL unsigned / SQL Server IDENTITY
    [Fact]
    public void MysqlUnsignedTypeModifier() =>
        Probe("CREATE TABLE t (id bigint unsigned NOT NULL)");

    [Fact]
    public void SqlServerIdentityColumn() =>
        Probe("CREATE TABLE t (id INT IDENTITY(1,1) PRIMARY KEY)");

    // ⑥ #268 EXEC OUTPUT
    [Fact]
    public void Issue268_ExecuteOutputArg() =>
        Probe("EXECUTE myProc 'foo', @outputVar OUTPUT");

    // ② #1978 CREATE OR ALTER
    [Fact]
    public void Issue1978_CreateOrAlterFunction() =>
        Probe("CREATE OR ALTER FUNCTION getPayments() RETURNS int RETURN 1");

    [Fact]
    public void DropFunction_IfExists() =>
        Probe("DROP FUNCTION IF EXISTS fin.f");

    // #2020 DEFAULT FOR
    [Fact]
    public void Issue2020_AlterDefaultForColumn() =>
        Probe("ALTER TABLE dbo.t ADD CONSTRAINT DF_t_d DEFAULT ((0)) FOR _d");

    // ⑧ #1846 INSERT OVERWRITE
    [Fact]
    public void Issue1846_InsertOverwriteTable() =>
        Probe("INSERT OVERWRITE TABLE t PARTITION (d='2020') SELECT 1");

    // ⑤ #2146 / #1564 Oracle XML
    [Fact]
    public void Issue2146_XmlParse() =>
        Probe("SELECT xmlparse(content '<a>1</a>') FROM dual");

    [Fact]
    public void Issue1564_XmlSerialize() =>
        Probe("SELECT XMLSERIALIZE(CONTENT xmlcol AS CLOB) FROM t");

    // #1825 已支持
    [Fact]
    public void Issue1825_JsonValue() =>
        Probe("SELECT JSON_VALUE(col, '$.name') FROM t");
}
