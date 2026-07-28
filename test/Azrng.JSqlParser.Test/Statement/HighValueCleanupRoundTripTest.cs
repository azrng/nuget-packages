using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
// ExpressionVisitorAdapter
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Create.Function;
using Azrng.JSqlParser.Statement.Create.Procedure;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Drop;
using Azrng.JSqlParser.Statement.Execute;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Select;
using AlterStatement = Azrng.JSqlParser.Statement.Alter.Alter;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T126：高价值剩余 issue 清仓 round-trip。
/// #1139 ODBC、unsigned/IDENTITY、#268 OUTPUT、#1978 OR ALTER、DROP FUNCTION、
/// #2020 DEFAULT FOR、#1846 INSERT OVERWRITE、#2146/#1564 XML。
/// </summary>
public class HighValueCleanupRoundTripTest
{
    #region #1139 ODBC

    [Fact]
    public void OdbcFn_RoundTrips()
    {
        var sql = "SELECT {fn timestampadd(SQL_TSI_YEAR, 2, travel_date)} FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var item = Assert.IsType<SelectItem>(stmt.SelectItems![0]);
        Assert.IsType<PassthroughExpression>(item.Expression);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void OdbcDateLiteral_RoundTrips()
    {
        var sql = "SELECT d FROM t WHERE d = {d '2020-01-01'}";
        var stmt = SqlParser.Parse(sql);
        Assert.Contains("{d '2020-01-01'}", stmt!.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void OdbcTimeAndTimestampLiterals_RoundTrips()
    {
        foreach (var sql in new[]
                 {
                     "SELECT t FROM x WHERE t = {t '12:00:00'}",
                     "SELECT ts FROM x WHERE ts = {ts '2020-01-01 12:00:00'}"
                 })
        {
            var stmt = SqlParser.Parse(sql);
            Assert.NotNull(stmt);
            SqlParser.Parse(stmt!.ToString()!);
        }
    }

    [Fact]
    public void PassthroughExpression_AcceptsVisitor()
    {
        var expr = new PassthroughExpression { Text = "{fn now()}" };
        var adapter = new ExpressionVisitorAdapter<object?>();
        // 默认 adapter 不抛；直接 Accept 验证接口接线
        expr.Accept<object?, object?>(adapter, null);
        Assert.Equal("{fn now()}", expr.ToString());
    }

    #endregion

    #region MySQL unsigned / SQL Server IDENTITY

    [Fact]
    public void MysqlUnsigned_RoundTrips()
    {
        var sql = "CREATE TABLE t (id bigint unsigned NOT NULL)";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.Contains("unsigned", stmt.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NOT NULL", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void MysqlSignedAndZerofill_RoundTrips()
    {
        var sql = "CREATE TABLE t (a INT signed, b INT zerofill)";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.Contains("signed", stmt.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("zerofill", stmt.ToString(), StringComparison.OrdinalIgnoreCase);
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void SqlServerIdentity_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT IDENTITY(1,1) PRIMARY KEY)";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.Contains("IDENTITY(1,1)", stmt.ToString());
        Assert.Contains("PRIMARY KEY", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void SqlServerIdentity_Bare_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT IDENTITY NOT NULL)";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.Contains("IDENTITY", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #268 EXECUTE OUTPUT

    [Fact]
    public void Execute_OutputArg_RoundTrips()
    {
        var sql = "EXECUTE myProc 'foo', @outputVar OUTPUT";
        var stmt = Assert.IsType<Execute>(SqlParser.Parse(sql));
        Assert.Equal(ExecType.EXECUTE, stmt.ExecType);
        Assert.NotNull(stmt.PlainArguments);
        Assert.Equal(2, stmt.PlainArguments!.Count);
        Assert.Contains("OUTPUT", stmt.PlainArguments[1], StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void Exec_NamedArgsWithOutput_RoundTrips()
    {
        var sql = "EXEC myProc @p1 = 1, @p2 = @out OUTPUT";
        var stmt = Assert.IsType<Execute>(SqlParser.Parse(sql));
        Assert.Equal(ExecType.EXEC, stmt.ExecType);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void Call_EmptyParens_StillParses()
    {
        // 回归：T126 改 execute 文法后曾拒绝 CALL p()
        var stmt = Assert.IsType<Execute>(SqlParser.Parse("CALL my_proc()"));
        Assert.Equal(ExecType.CALL, stmt.ExecType);
        Assert.True(stmt.HasParentheses);
        Assert.Equal("CALL my_proc", stmt.ToString());
    }

    [Fact]
    public void Call_WithParenArgs_PreservesParentheses()
    {
        var sql = "CALL my_proc(1, 'hello')";
        var stmt = Assert.IsType<Execute>(SqlParser.Parse(sql));
        Assert.True(stmt.HasParentheses);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void Execute_SchemaQualifiedName_RoundTrips()
    {
        var sql = "EXECUTE dbo.myProc 1";
        var stmt = Assert.IsType<Execute>(SqlParser.Parse(sql));
        Assert.Contains("dbo.myProc", stmt.Name);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region #1978 CREATE OR ALTER / DROP FUNCTION

    [Fact]
    public void CreateOrAlterFunction_RoundTrips()
    {
        var sql = "CREATE OR ALTER FUNCTION getPayments() RETURNS int RETURN 1";
        var stmt = Assert.IsType<CreateFunction>(SqlParser.Parse(sql));
        Assert.True(stmt.OrAlter);
        Assert.False(stmt.OrReplace);
        Assert.Contains("OR ALTER FUNCTION", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateOrReplaceFunction_StillWorks()
    {
        var sql = "CREATE OR REPLACE FUNCTION foo() RETURNS int RETURN 1";
        var stmt = Assert.IsType<CreateFunction>(SqlParser.Parse(sql));
        Assert.True(stmt.OrReplace);
        Assert.False(stmt.OrAlter);
        Assert.Contains("OR REPLACE FUNCTION", stmt.ToString());
    }

    [Fact]
    public void CreateOrAlterProcedure_RoundTrips()
    {
        var sql = "CREATE OR ALTER PROCEDURE SPPayment AS SELECT 1";
        var stmt = Assert.IsType<CreateProcedure>(SqlParser.Parse(sql));
        Assert.True(stmt.OrAlter);
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void DropFunction_IfExists_RoundTrips()
    {
        var sql = "DROP FUNCTION IF EXISTS fin.f";
        var stmt = Assert.IsType<Drop>(SqlParser.Parse(sql));
        Assert.Equal("FUNCTION", stmt.Type);
        Assert.True(stmt.IfExists);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void DropProcedure_RoundTrips()
    {
        var sql = "DROP PROCEDURE IF EXISTS dbo.sp_x";
        var stmt = Assert.IsType<Drop>(SqlParser.Parse(sql));
        Assert.Equal("PROCEDURE", stmt.Type);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void DropFunction_ThenSelect_MultiStatement()
    {
        var stmts = SqlParser.ParseStatements("DROP FUNCTION IF EXISTS fin.f(int8); SELECT 1");
        Assert.NotNull(stmts);
        Assert.Equal(2, stmts!.StatementList.Count);
        Assert.IsType<Drop>(stmts.StatementList[0]);
        Assert.IsType<PlainSelectType>(stmts.StatementList[1]);
    }

    [Fact]
    public void CreateOrReplaceFunction_ThenSelect_MultiStatement()
    {
        // #1994 简化场景：FUNCTION 后可续解析
        var stmts = SqlParser.ParseStatements(
            "CREATE OR REPLACE FUNCTION foo() RETURNS int AS $$ SELECT 1 $$ LANGUAGE sql; SELECT 2");
        Assert.NotNull(stmts);
        Assert.True(stmts!.StatementList.Count >= 2);
        Assert.IsType<CreateFunction>(stmts.StatementList[0]);
    }

    #endregion

    #region #2020 DEFAULT FOR / #1846 OVERWRITE

    [Fact]
    public void AlterAddDefaultFor_RoundTrips()
    {
        var sql = "ALTER TABLE dbo.t ADD CONSTRAINT DF_t_d DEFAULT ((0)) FOR _d";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void InsertOverwriteTable_RoundTrips()
    {
        var sql = "INSERT OVERWRITE TABLE t PARTITION (d='2020') SELECT 1";
        var stmt = Assert.IsType<Insert>(SqlParser.Parse(sql));
        Assert.True(stmt.Overwrite);
        Assert.True(stmt.TableKeyword);
        Assert.NotNull(stmt.Partitions);
        Assert.Contains("OVERWRITE TABLE", stmt.ToString());
        Assert.Contains("PARTITION", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void InsertOverwrite_WithoutTableKeyword_RoundTrips()
    {
        var sql = "INSERT OVERWRITE t SELECT 1";
        var stmt = Assert.IsType<Insert>(SqlParser.Parse(sql));
        Assert.True(stmt.Overwrite);
        Assert.False(stmt.TableKeyword);
        Assert.Contains("OVERWRITE", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2146 / #1564 Oracle XML

    [Fact]
    public void XmlParse_RoundTrips()
    {
        var sql = "SELECT xmlparse(content '<a>1</a>') FROM dual";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var item = Assert.IsType<SelectItem>(stmt.SelectItems![0]);
        Assert.IsType<PassthroughExpression>(item.Expression);
        Assert.Contains("xmlparse", stmt.ToString(), StringComparison.OrdinalIgnoreCase);
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void XmlParse_Document_RoundTrips()
    {
        var sql = "SELECT XMLPARSE(DOCUMENT '<r/>') FROM dual";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        Assert.Contains("XMLPARSE", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void XmlSerialize_RoundTrips()
    {
        var sql = "SELECT XMLSERIALIZE(CONTENT xmlcol AS CLOB) FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        Assert.Contains("XMLSERIALIZE", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion
}
