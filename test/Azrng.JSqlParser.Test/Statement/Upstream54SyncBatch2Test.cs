using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Comment;
using Azrng.JSqlParser.Statement.Create.Domain;
using Azrng.JSqlParser.Statement.Create.Event;
using Azrng.JSqlParser.Statement.Create.Extension;
using Azrng.JSqlParser.Statement.Create.Macro;
using Azrng.JSqlParser.Statement.Create.Publication;
using Azrng.JSqlParser.Statement.Create.Role;
using Azrng.JSqlParser.Statement.Create.Subscription;
using Azrng.JSqlParser.Statement.Create.Trigger;
using Azrng.JSqlParser.Statement.DuckDb;
using Azrng.JSqlParser.Statement.Export;
using Azrng.JSqlParser.Statement.Piped;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Util;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T149 批次：上游 JSqlParser 5.4 第二/三档能力 round-trip。
/// 覆盖 PG/MySQL DDL 族、ClickHouse、BigQuery、DuckDB、三元表达式、MATCH_RECOGNIZE。
/// </summary>
public class Upstream54SyncBatch2Test
{
    private static void AssertRoundTrip(string sql)
    {
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #region 批次A：PG/MySQL DDL 族

    [Theory]
    [InlineData("CREATE USER 'jeffrey'@'localhost' IDENTIFIED BY 'mypass'")]
    [InlineData("CREATE USER IF NOT EXISTS app_user")]
    [InlineData("CREATE ROLE myrole WITH LOGIN PASSWORD 'x' VALID UNTIL '2027-01-01'")]
    [InlineData("CREATE GROUP analysts")]
    public void CreateRole_UserGroup_RoundTrips(string sql)
    {
        var stmt = Assert.IsType<CreateRole>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateDomain_WithCheck_RoundTrips()
    {
        var sql = "CREATE DOMAIN addr AS TEXT DEFAULT 'n/a' CHECK (VALUE IS NOT NULL)";
        var stmt = Assert.IsType<CreateDomain>(SqlParser.Parse(sql));
        Assert.Equal("addr", stmt.Name);
        Assert.Equal("TEXT", stmt.DataType);
        Assert.Contains("DEFAULT 'n/a'", stmt.Tail);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateExtension_RoundTrips()
    {
        var sql = "CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public";
        var stmt = Assert.IsType<CreateExtension>(SqlParser.Parse(sql));
        Assert.Equal("pgcrypto", stmt.Name);
        Assert.Equal(ExtensionOptionKind.Schema, stmt.Options![0].Kind);
        Assert.Equal("public", stmt.Options[0].Value);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("CREATE PUBLICATION pub1 FOR ALL TABLES")]
    [InlineData("CREATE PUBLICATION pub2 FOR TABLE t1, t2 WITH (publish = 'insert')")]
    public void CreatePublication_RoundTrips(string sql)
    {
        var stmt = Assert.IsType<CreatePublication>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateSubscription_RoundTrips()
    {
        var sql = "CREATE SUBSCRIPTION sub1 CONNECTION 'host=127.0.0.1 port=5432' PUBLICATION pub1, pub2 WITH (copy_data = true)";
        var stmt = Assert.IsType<CreateSubscription>(SqlParser.Parse(sql));
        Assert.Equal("sub1", stmt.Name);
        Assert.Equal(new[] { "pub1", "pub2" }, stmt.Publications);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateTrigger_WithInsertBody_RoundTrips()
    {
        var sql = "CREATE TRIGGER trg AFTER DELETE ON orders FOR EACH ROW INSERT INTO audit_log VALUES (1)";
        var stmt = Assert.IsType<CreateTrigger>(SqlParser.Parse(sql));
        Assert.Equal(TriggerTiming.After, stmt.Timing);
        Assert.Equal(TriggerEvent.Delete, stmt.Event);
        // 单语句体已结构化（triggerSimpleStatement → Insert）
        var insert = Assert.IsType<Azrng.JSqlParser.Statement.Insert.Insert>(stmt.Body);
        Assert.Equal("audit_log", insert.Table!.Name);
        Assert.Null(stmt.BodyText);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateTrigger_DefinerAndOrder_RoundTrips()
    {
        var sql = "CREATE DEFINER = 'admin'@'localhost' TRIGGER trg BEFORE UPDATE ON t FOR EACH ROW PRECEDES other SET @x = 1";
        var stmt = Assert.IsType<CreateTrigger>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Definer);
        Assert.NotNull(stmt.Order);
        Assert.True(stmt.Order!.Follows == false);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("CREATE EVENT ev ON SCHEDULE EVERY 1 HOUR DO SELECT 1")]
    [InlineData("CREATE EVENT ev2 ON SCHEDULE AT '2026-01-01 00:00:00' ON COMPLETION PRESERVE ENABLE COMMENT 'c' DO UPDATE t SET x = 1")]
    [InlineData("CREATE EVENT IF NOT EXISTS ev3 ON SCHEDULE EVERY 5 MINUTE STARTS NOW() ENDS NOW() + INTERVAL 1 DAY DISABLE ON SLAVE DO DELETE FROM t")]
    public void CreateEvent_RoundTrips(string sql)
    {
        var stmt = Assert.IsType<CreateEvent>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Body);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void DoStatement_PgAnonymousBlock_RoundTrips()
    {
        var sql = "DO $$ BEGIN RAISE NOTICE 'hi'; END $$;";
        SqlParser.ParseStatements(sql);
    }

    [Fact]
    public void DoStatement_LanguageForm_Parses()
    {
        var sql = "DO LANGUAGE plpgsql $$ BEGIN RETURN 1; END $$";
        AssertRoundTrip(sql);
    }

    [Theory]
    [InlineData("COMMENT ON SCHEMA s1 IS 'comment'")]
    [InlineData("COMMENT ON DATABASE db1 IS 'db comment'")]
    [InlineData("COMMENT ON FUNCTION fn(int) IS 'fn comment'")]
    [InlineData("COMMENT ON TABLE t IS NULL")]
    [InlineData("COMMENT ON COLUMN t.c IS 'col comment'")]
    public void Comment_OnVariousTargets_RoundTrips(string sql)
    {
        var stmt = Assert.IsType<Comment>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region 批次B：ClickHouse

    [Fact]
    public void ArrayJoin_RoundTrips()
    {
        var sql = "SELECT a, b FROM t ARRAY JOIN arr AS x";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var join = Assert.IsType<Join>(stmt.Joins!.Single());
        Assert.True(join.ArrayJoin);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void LeftArrayJoin_RoundTrips()
    {
        var sql = "SELECT * FROM t LEFT ARRAY JOIN arr1, arr2";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var join = Assert.IsType<Join>(stmt.Joins!.Single());
        Assert.True(join.LeftArrayJoin);
        Assert.Equal(2, join.ArrayJoinItems!.Count);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void OrderByWithFill_RoundTrips()
    {
        var sql = "SELECT d, count() FROM t GROUP BY d ORDER BY d WITH FILL FROM 1 TO 10 STEP 2";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var fill = stmt.OrderByElements![0].WithFill;
        Assert.NotNull(fill);
        Assert.NotNull(fill!.From);
        Assert.NotNull(fill.To);
        Assert.NotNull(fill.Step);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Interpolate_RoundTrips()
    {
        var sql = "SELECT d, total FROM t ORDER BY d, total INTERPOLATE (total AS total + 1)";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        Assert.Single(stmt.InterpolateElements!);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void ColumnsApplyExceptReplace_RoundTrips()
    {
        var sql = "SELECT COLUMNS('c') APPLY lower, COLUMNS(*) EXCEPT (id), COLUMNS('m') REPLACE x AS c FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region 批次C：BigQuery

    [Fact]
    public void ExportData_WithOptions_RoundTrips()
    {
        var sql = "EXPORT DATA OPTIONS(uri = 'gs://bucket/*.csv', format = 'CSV') AS SELECT * FROM t";
        var stmt = Assert.IsType<ExportData>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Select);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void ExportData_UriForm_RoundTrips()
    {
        var sql = "EXPORT DATA 'file.csv' AS SELECT 1";
        AssertRoundTrip(sql);
    }

    [Fact]
    public void LoadData_RoundTrips()
    {
        var sql = "LOAD DATA OVERWRITE INTO TABLE t FROM FILES(format = 'CSV', uris = ['gs://b/*'])";
        AssertRoundTrip(sql);
    }

    [Fact]
    public void AssertFunction_AndStatement_Parses()
    {
        AssertRoundTrip("SELECT ASSERT(x > 0, 'must be positive') FROM t");
        AssertRoundTrip("ASSERT x > 0 AS 'must be positive'");
    }

    [Fact]
    public void UnnestWithOffset_RoundTrips()
    {
        var sql = "SELECT * FROM t, UNNEST(arr) AS u WITH OFFSET AS o";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var join = Assert.IsType<Join>(stmt.Joins!.Single());
        var unnest = Assert.IsType<UnnestTable>(join.RightItem);
        Assert.True(unnest.WithOffset);
        Assert.Equal("o", unnest.OffsetAlias);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region 批次D：DuckDB

    [Fact]
    public void AntiJoin_RoundTrips()
    {
        var sql = "SELECT a FROM t ANTI JOIN s ON t.id = s.id";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var join = Assert.IsType<Join>(stmt.Joins!.Single());
        Assert.True(join.Anti);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("COPY t TO 'file.csv' (FORMAT CSV, HEADER)")]
    [InlineData("COPY (SELECT * FROM t WHERE x > 1) TO 'file.parquet' (FORMAT PARQUET)")]
    [InlineData("COPY t FROM 'in.csv'")]
    public void CopyStatement_RoundTrips(string sql)
    {
        var stmt = Assert.IsType<CopyStatement>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("ATTACH 'db.db' AS mydb")]
    [InlineData("PRAGMA memory_limit = '1GB'")]
    [InlineData("CREATE MACRO add(a, b) AS a + b")]
    public void DuckDbSimpleStatements_RoundTrips(string sql)
    {
        AssertRoundTrip(sql);
    }

    [Fact]
    public void AttachStatement_Typed()
    {
        var stmt = Assert.IsType<AttachStatement>(SqlParser.Parse("ATTACH 'db.db' AS mydb"));
        Assert.Contains("mydb", stmt.Text);
    }

    [Fact]
    public void CreateMacro_Typed()
    {
        var stmt = Assert.IsType<CreateMacro>(SqlParser.Parse("CREATE MACRO add(a, b) AS a + b"));
        Assert.Contains("add(a, b)", stmt.Text);
    }

    #endregion

    #region 批次E：三元表达式

    [Fact]
    public void Ternary_Basic_RoundTrips()
    {
        var sql = "SELECT a > 1 ? 'x' : 'y' FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var ternary = Assert.IsType<TernaryExpression>(stmt.SelectItems![0].Expression);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Ternary_Nested_Else_RoundTrips()
    {
        // 右结合：else 分支嵌套三元
        var sql = "SELECT a > 1 ? 'x' : a > 2 ? 'y' : 'z' FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var outer = Assert.IsType<TernaryExpression>(stmt.SelectItems![0].Expression);
        Assert.IsType<TernaryExpression>(outer.ElseExpression);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void PositionalParameter_Regression_StillWorks()
    {
        // 三元引入后，? 占位符与 : 命名参数不受影响
        AssertRoundTrip("SELECT * FROM t WHERE x = ? AND y = :name");
    }

    #endregion

    #region 批次F：MATCH_RECOGNIZE

    [Fact]
    public void MatchRecognize_Full_RoundTrips()
    {
        var sql = "SELECT * FROM trades MATCH_RECOGNIZE (" +
                  "PARTITION BY symbol ORDER BY ts " +
                  "MEASURES A.price AS start_price " +
                  "ONE ROW PER MATCH " +
                  "AFTER MATCH SKIP TO LAST B " +
                  "PATTERN (A B+ C) " +
                  "SUBSET s1 = (A, B) " +
                  "DEFINE A AS A.price > 100, B AS B.price < A.price) AS mr";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var mr = Assert.IsType<MatchRecognize>(stmt.FromItem!);
        Assert.Single(mr.PartitionKeys!);
        Assert.Single(mr.Measures!);
        Assert.True(mr.RowsPerMatch);
        Assert.NotNull(mr.Skip);
        Assert.Equal("A B+ C", mr.PatternText);
        Assert.Equal(2, mr.Defines!.Count);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void MatchRecognize_Minimal_RoundTrips()
    {
        var sql = "SELECT * FROM t MATCH_RECOGNIZE (PATTERN (A))";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var mr = Assert.IsType<MatchRecognize>(stmt.FromItem!);
        Assert.Equal("A", mr.PatternText);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region 结构化升级复核（T149 后续：去透传 + FROM-first）

    [Fact]
    public void CreateDomain_Structured_TypeAndTail()
    {
        var sql = "CREATE DOMAIN addr AS TEXT DEFAULT 'n/a' CHECK (VALUE IS NOT NULL)";
        var stmt = Assert.IsType<CreateDomain>(SqlParser.Parse(sql));
        Assert.True(stmt.UseAs);
        Assert.Equal("TEXT", stmt.DataType);
        Assert.Contains("DEFAULT 'n/a'", stmt.Tail);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateExtension_Structured_Options()
    {
        var sql = "CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public CASCADE";
        var stmt = Assert.IsType<CreateExtension>(SqlParser.Parse(sql));
        Assert.Equal(2, stmt.Options!.Count);
        Assert.Equal(ExtensionOptionKind.Schema, stmt.Options[0].Kind);
        Assert.Equal("public", stmt.Options[0].Value);
        Assert.Equal(ExtensionOptionKind.Cascade, stmt.Options[1].Kind);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("DO $$ BEGIN RETURN 1; END $$", null, false)]
    [InlineData("DO LANGUAGE plpgsql $$ BEGIN RETURN 1; END $$", "plpgsql", true)]
    [InlineData("DO $$ BEGIN RETURN 1; END $$ LANGUAGE plpgsql", "plpgsql", false)]
    public void DoStatement_Structured_CodeAndLanguage(string sql, string? language, bool beforeCode)
    {
        var stmt = Assert.IsType<DoStatement>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Code);
        Assert.Equal(language, stmt.Language);
        Assert.Equal(beforeCode, stmt.LanguageBeforeCode);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("CREATE TRIGGER trg AFTER INSERT ON orders FOR EACH ROW INSERT INTO audit_log VALUES (1)", "audit_log")]
    [InlineData("CREATE TRIGGER trg BEFORE UPDATE ON t FOR EACH ROW DELETE FROM log", "log")]
    public void CreateTrigger_SimpleStatementBody_Structured(string sql, string bodyTable)
    {
        var stmt = Assert.IsType<CreateTrigger>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Body);
        Assert.Null(stmt.BodyText);
        var tables = stmt.GetTableNames();
        Assert.Contains(bodyTable, tables);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateTrigger_SetBody_WithFromFirst()
    {
        // 触发体 SET 语句 + FROM-first 查询
        var stmt = SqlParser.Parse("CREATE TRIGGER trg AFTER INSERT ON t FOR EACH ROW SET @x = 1");
        Assert.IsType<CreateTrigger>(stmt);
        var fromFirst = SqlParser.Parse("FROM trades SELECT symbol, price WHERE price > 10 ORDER BY price LIMIT 5");
        Assert.IsType<FromQuery>(fromFirst);
        Assert.Contains("SELECT symbol, price", fromFirst!.ToString());
    }

    [Theory]
    [InlineData("FROM trades SELECT symbol, price WHERE price > 10 ORDER BY price LIMIT 5")]
    [InlineData("FROM t SELECT *")]
    [InlineData("FROM t")]
    public void DuckDbFromFirst_RoundTrips(string sql)
    {
        var stmt = Assert.IsType<FromQuery>(SqlParser.Parse(sql));
        Assert.True(stmt.UsingFromKeyword);
        Assert.Equal(sql, stmt!.ToString());
    }

    #endregion

    #region 批次G：结构化升级（#1728 / #2539）

    [Fact]
    public void Interval_Qualifier_Structured()
    {
        var expr = SqlParser.ParseExpression("INTERVAL '7' DAY TO SECOND(6)");
        var interval = Assert.IsType<IntervalExpression>(expr);
        var q = interval.Qualifier;
        Assert.NotNull(q);
        Assert.Equal("DAY", q!.Unit);
        Assert.Equal("SECOND", q.ToUnit);
        Assert.Equal(6, q.ToPrecision);
        Assert.Null(q.Precision);
        Assert.Equal("DAY TO SECOND(6)", q.ToString());
    }

    [Fact]
    public void ColDataType_PrecisionScale_Structured()
    {
        var sql = "CREATE TABLE t (a DECIMAL(10,2), b VARCHAR(30), c INT)";
        var stmt = Assert.IsType<Azrng.JSqlParser.Statement.CreateTable.CreateTable>(SqlParser.Parse(sql));
        var cols = stmt.ColumnDefinitions!;
        Assert.Equal(10, cols[0].ColDataType.Precision);
        Assert.Equal(2, cols[0].ColDataType.Scale);
        Assert.Equal(30, cols[1].ColDataType.Precision);
        Assert.Null(cols[1].ColDataType.Scale);
        Assert.Null(cols[2].ColDataType.Precision);
    }

    #endregion

    #region TablesNamesFinder 集成

    [Fact]
    public void TablesNamesFinder_TriggerAndPublication()
    {
        var tables1 = SqlParser.Parse("CREATE TRIGGER trg AFTER DELETE ON orders FOR EACH ROW BEGIN INSERT INTO audit_log VALUES (1); END")!
            .GetTableNames();
        Assert.Contains("orders", tables1);
        Assert.Contains("audit_log", tables1);

        var tables2 = SqlParser.Parse("CREATE PUBLICATION p FOR TABLE t1, t2")!.GetTableNames();
        Assert.Contains("t1", tables2);
        Assert.Contains("t2", tables2);
    }

    #endregion
}
