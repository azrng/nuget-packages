using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 上游真实 SQL 解析覆盖度探针：从上游 CreateTableTest/SelectTest 抽取代表性 SQL，
/// 验证 Azrng 解析能力，识别失败模式（非 round-trip 验证，仅解析+ToString 无异常）。
/// </summary>
public class UpstreamCoverageProbeTest
{
    public static IEnumerable<object[]> UpstreamSqlCases
    {
        get
        {
            // CREATE TABLE 基础
            yield return new object[] { "CREATE TABLE mytab (mycol INT PRIMARY KEY, mycol2 INT UNIQUE)" };
            yield return new object[] { "CREATE TABLE mytab (mycol INT NOT NULL, mycol2 INT)" };
            yield return new object[] { "CREATE TABLE mytab (mycol INT DEFAULT 1)" };
            // MySQL 索引
            yield return new object[] { "CREATE TABLE t (id INT, KEY idx_name (name))" };
            yield return new object[] { "CREATE TABLE t (id INT, UNIQUE KEY uk_name (name))" };
            yield return new object[] { "CREATE TABLE t (id INT, FULLTEXT KEY ft_name (body))" };
            // ENGINE/CHARSET
            yield return new object[] { "CREATE TABLE t (id INT) ENGINE=InnoDB DEFAULT CHARSET=utf8" };
            yield return new object[] { "CREATE TABLE t (id INT) ENGINE = MyISAM AUTO_INCREMENT = 4 DEFAULT CHARSET = utf8 COLLATE = utf8_bin" };
            // CTAS
            yield return new object[] { "CREATE TABLE a AS SELECT col1, col2 FROM b" };
            yield return new object[] { "CREATE TEMPORARY TABLE T1 (C1, C2) AS SELECT C3, C4 FROM T2" };
            // IF NOT EXISTS
            yield return new object[] { "CREATE TABLE IF NOT EXISTS t (id INT)" };
            // 外键
            yield return new object[] { "CREATE TABLE orders (id INT, FOREIGN KEY (cust_id) REFERENCES customers(id) ON DELETE CASCADE ON UPDATE SET NULL)" };
            // 数组
            yield return new object[] { "CREATE TABLE sal_emp (name text, pay_by_quarter integer[], schedule text[][])" };
            // ClickHouse
            yield return new object[] { "CREATE TABLE tmp.events (id UInt64) ENGINE = MergeTree() ORDER BY id SAMPLE BY id" };
            // 分区
            yield return new object[] { "CREATE TABLE T_TEST (PART_COLUMN VARCHAR2(32)) PARTITION BY HASH (PART_COLUMN) PARTITIONS 4" };
            // RowMovement
            yield return new object[] { "CREATE TABLE test (startdate DATE) ENABLE ROW MOVEMENT" };
            // Spanner
            yield return new object[] { "CREATE TABLE cmd (id INT64, arr_bool ARRAY<BOOL>, arr_string ARRAY<STRING(MAX)>)" };
            // SELECT 基础
            yield return new object[] { "SELECT * FROM mytable WHERE a = 1 AND b = 2" };
            yield return new object[] { "SELECT a, b, COUNT(*) FROM t GROUP BY a, b" };
            yield return new object[] { "SELECT a, b FROM t ORDER BY a DESC, b ASC" };
            // JOIN
            yield return new object[] { "SELECT * FROM a INNER JOIN b ON a.id = b.id LEFT JOIN c ON b.id = c.id" };
            // 子查询
            yield return new object[] { "SELECT * FROM (SELECT a FROM t) sub" };
            // 聚合 + OVER
            yield return new object[] { "SELECT SUM(salary) OVER (PARTITION BY dept ORDER BY hiredate) FROM emp" };
            yield return new object[] { "SELECT ROW_NUMBER() OVER (PARTITION BY dept ORDER BY salary DESC) FROM emp" };
            // UNION
            yield return new object[] { "SELECT a FROM t1 UNION SELECT a FROM t2 UNION ALL SELECT a FROM t3" };
            // CTE
            yield return new object[] { "WITH cte AS (SELECT a FROM t) SELECT * FROM cte" };
            yield return new object[] { "WITH RECURSIVE r AS (SELECT 1 AS n UNION ALL SELECT n+1 FROM r WHERE n < 10) SELECT * FROM r" };
            // CASE
            yield return new object[] { "SELECT CASE WHEN x > 1 THEN 'big' ELSE 'small' END FROM t" };
            // INSERT
            yield return new object[] { "INSERT INTO t (a, b) VALUES (1, 2)" };
            yield return new object[] { "INSERT INTO t SELECT * FROM src" };
            // UPDATE/DELETE
            yield return new object[] { "UPDATE t SET a = 1, b = 2 WHERE id = 3" };
            yield return new object[] { "DELETE FROM t WHERE id = 3" };
            // JSON
            yield return new object[] { "SELECT JSON_OBJECT('name': name, 'age': age) FROM t" };
            yield return new object[] { "SELECT JSON_EXTRACT(j, '$.name') FROM t" };
            // CAST
            yield return new object[] { "SELECT CAST(a AS VARCHAR(100)) FROM t" };
            yield return new object[] { "SELECT a::varchar(20) FROM t" };

            // ── 复杂边缘场景 ──
            // 窗口帧
            yield return new object[] { "SELECT SUM(x) OVER (PARTITION BY a ORDER BY b ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM t" };
            yield return new object[] { "SELECT SUM(x) OVER (ORDER BY b RANGE BETWEEN 1 PRECEDING AND 1 FOLLOWING) FROM t" };
            // INTERVAL
            yield return new object[] { "SELECT INTERVAL '1' DAY + INTERVAL '2' HOUR FROM t" };
            // IN 列表
            yield return new object[] { "SELECT * FROM t WHERE id IN (1, 2, 3)" };
            yield return new object[] { "SELECT * FROM t WHERE id IN (SELECT id FROM other)" };
            // EXISTS / NOT EXISTS
            yield return new object[] { "SELECT * FROM t WHERE EXISTS (SELECT 1 FROM u WHERE u.id = t.id)" };
            yield return new object[] { "SELECT * FROM t WHERE NOT EXISTS (SELECT 1 FROM u WHERE u.id = t.id)" };
            // BETWEEN / LIKE
            yield return new object[] { "SELECT * FROM t WHERE age BETWEEN 18 AND 65" };
            yield return new object[] { "SELECT * FROM t WHERE name LIKE 'A%'" };
            yield return new object[] { "SELECT * FROM t WHERE name NOT LIKE '%test%'" };
            // ON CONFLICT (PG)
            yield return new object[] { "INSERT INTO t (a) VALUES (1) ON CONFLICT (a) DO NOTHING" };
            yield return new object[] { "INSERT INTO t (a) VALUES (1) ON CONFLICT (a) DO UPDATE SET b = excluded.b" };
            // ON DUPLICATE KEY (MySQL)
            yield return new object[] { "INSERT INTO t (a, b) VALUES (1, 2) ON DUPLICATE KEY UPDATE b = 3" };
            // DISTINCT
            yield return new object[] { "SELECT DISTINCT a, b FROM t" };
            // LIMIT/OFFSET
            yield return new object[] { "SELECT * FROM t LIMIT 10 OFFSET 20" };
            yield return new object[] { "SELECT * FROM t OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY" };
            // FOR UPDATE
            yield return new object[] { "SELECT * FROM t WHERE id = 1 FOR UPDATE" };
            yield return new object[] { "SELECT * FROM t WHERE id = 1 FOR SHARE NOWAIT" };
            // 嵌套子查询
            yield return new object[] { "SELECT * FROM (SELECT * FROM (SELECT a FROM t1) s1 JOIN t2 ON s1.a = t2.a) s2" };
            // 多表 JOIN
            yield return new object[] { "SELECT a.x, b.y, c.z FROM a JOIN b ON a.id = b.id JOIN c ON b.id = c.id WHERE a.val > 100" };
            // 聚合 HAVING
            yield return new object[] { "SELECT dept, COUNT(*) FROM emp GROUP BY dept HAVING COUNT(*) > 5" };
            // DISTINCT COUNT
            yield return new object[] { "SELECT dept, COUNT(DISTINCT emp_id) FROM emp GROUP BY dept" };
            // 字符串函数
            yield return new object[] { "SELECT CONCAT(first, ' ', last) AS full_name FROM users" };
            yield return new object[] { "SELECT UPPER(name), LOWER(name), LENGTH(name) FROM t" };
            // 数学函数
            yield return new object[] { "SELECT ROUND(price, 2), ABS(val), MOD(a, b) FROM t" };
            // 日期函数
            yield return new object[] { "SELECT CURRENT_DATE, CURRENT_TIMESTAMP, NOW() FROM t" };
            // COALESCE / NULLIF
            yield return new object[] { "SELECT COALESCE(a, b, 0), NULLIF(a, 0) FROM t" };
            // DROP / TRUNCATE / ALTER
            yield return new object[] { "DROP TABLE IF EXISTS t" };
            yield return new object[] { "TRUNCATE TABLE t" };
            yield return new object[] { "ALTER TABLE t ADD COLUMN new_col INT NOT NULL DEFAULT 0" };
            yield return new object[] { "ALTER TABLE t DROP COLUMN old_col" };
            yield return new object[] { "ALTER TABLE t MODIFY COLUMN col VARCHAR(200)" };
            // CREATE INDEX
            yield return new object[] { "CREATE INDEX idx_name ON t (name)" };
            yield return new object[] { "CREATE UNIQUE INDEX uk_email ON users (email)" };
            // CREATE VIEW
            yield return new object[] { "CREATE VIEW v AS SELECT a, b FROM t WHERE a > 0" };
            yield return new object[] { "CREATE OR REPLACE VIEW v AS SELECT * FROM t" };
            // MERGE
            yield return new object[] { "MERGE INTO target t USING source s ON t.id = s.id WHEN MATCHED THEN UPDATE SET t.val = s.val WHEN NOT MATCHED THEN INSERT (id, val) VALUES (s.id, s.val)" };
            // 事务
            yield return new object[] { "BEGIN TRANSACTION" };
            yield return new object[] { "COMMIT" };
            yield return new object[] { "ROLLBACK" };
            yield return new object[] { "SAVEPOINT sp1" };
            // GRANT
            yield return new object[] { "GRANT SELECT, INSERT ON t TO user1" };
            // COMMENT
            yield return new object[] { "COMMENT ON TABLE t IS 'my table'" };
            // 数组字面量
            yield return new object[] { "SELECT ARRAY[1, 2, 3]" };
            // 范围表达式
            yield return new object[] { "SELECT arr[1:3] FROM t" };
            // IS NULL / IS NOT NULL
            yield return new object[] { "SELECT * FROM t WHERE a IS NULL" };
            yield return new object[] { "SELECT * FROM t WHERE a IS NOT NULL" };
            // IS DISTINCT FROM
            yield return new object[] { "SELECT * FROM t WHERE a IS DISTINCT FROM b" };
        }
    }

    [Theory]
    [MemberData(nameof(UpstreamSqlCases))]
    public void UpstreamSql_CanParse(string sql)
    {
        // 仅验证解析无异常 + ToString 无异常（不要求 round-trip 完全一致）
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var result = stmt.ToString();
        Assert.False(string.IsNullOrEmpty(result));
    }
}
