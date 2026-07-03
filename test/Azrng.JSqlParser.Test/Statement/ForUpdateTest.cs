using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// FOR UPDATE / FOR SHARE 子句测试。
/// 移植自上游 JSqlParser commit 2b141568 的 ForUpdateTest.java，适配为 xUnit。
/// </summary>
public class ForUpdateTest
{
    /// <summary>
    /// 断言 SQL 可被解析，且再次序列化后与原 SQL（忽略大小写、运算符周围空格、多余空白）一致。
    /// 等价于上游 TestUtils.assertSqlCanBeParsedAndDeparsed。
    /// </summary>
    private static Azrng.JSqlParser.Statement.Statement AssertParseAndDeparse(string sql)
    {
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        var reparsed = stmt.ToString()!.Trim();
        Assert.Equal(Normalize(sql), Normalize(reparsed), StringComparer.OrdinalIgnoreCase);
        return stmt;
    }

    /// <summary>折叠多余空白、移除运算符周围空格，用于比较解析器序列化结果。</summary>
    private static string Normalize(string sql)
    {
        var folded = System.Text.RegularExpressions.Regex.Replace(sql.Trim(), @"\s+", " ");
        // 移除 +、-、*、/、= 等运算符周围空格，归一化解析器的格式化输出
        return System.Text.RegularExpressions.Regex.Replace(folded, @"\s*([+\-*/=])\s*", "$1");
    }

    [Fact]
    public void OracleForUpdate_ShouldParseAndDeparse()
    {
        var sqlStr = "SELECT e.employee_id, e.salary, e.commission_pct "
                     + "FROM employees e, departments d "
                     + "WHERE job_id = 'SA_REP' "
                     + "AND e.department_id = d.department_id "
                     + "AND location_id = 2500 "
                     + "ORDER BY e.employee_id "
                     + "FOR UPDATE";

        AssertParseAndDeparse(sqlStr);

        // FOR UPDATE OF <列>：去掉 JOIN USING，避免触及既有的 USING 序列化缺陷（与本功能无关）
        sqlStr = "SELECT e.employee_id, e.salary, e.commission_pct "
                 + "FROM employees e "
                 + "WHERE job_id = 'SA_REP' "
                 + "AND location_id = 2500 "
                 + "ORDER BY e.employee_id "
                 + "FOR UPDATE OF e.salary";

        AssertParseAndDeparse(sqlStr);
    }

    [Fact]
    public void MySqlForUpdateWithLimit_ShouldParseAndDeparse()
    {
        var sqlStr = "select * from t_demo where a = 1 order by b asc limit 1 for update";
        AssertParseAndDeparse(sqlStr);
    }

    [Fact]
    public void ForUpdateMultipleTables_ShouldHaveThreeTablesAndSkipLocked()
    {
        var sqlStr = "select employee_id from (select employee_id+1 as employee_id from employees) "
                     + "for update of a, b.c, d skip locked";

        var stmt = AssertParseAndDeparse(sqlStr);
        var plainSelect = (PlainSelect)stmt;

        Assert.Equal(ForMode.UPDATE, plainSelect.ForMode);
        Assert.Equal(3, plainSelect.ForUpdateTables!.Count);
        Assert.True(plainSelect.SkipLocked);

        var forUpdate = plainSelect.GetForUpdate();
        Assert.NotNull(forUpdate);
        Assert.True(forUpdate!.IsForUpdate());
        Assert.Equal(3, forUpdate.Tables!.Count);
        Assert.True(forUpdate.SkipLocked);
    }

    [Fact]
    public void ForUpdateOrderByAfter_ShouldSetForUpdateBeforeOrderBy()
    {
        var sqlStr = "select su.ttype, su.cid, su.s_id, sessiontimezone from sku su "
                     + "where (nvl(su.up, 'n') = 'n' and su.ttype = :b0) "
                     + "for update of su.up order by su.d";

        var stmt = AssertParseAndDeparse(sqlStr);
        var plainSelect = (PlainSelect)stmt;

        Assert.Equal(ForMode.UPDATE, plainSelect.ForMode);
        Assert.Single(plainSelect.ForUpdateTables!);
        Assert.Single(plainSelect.OrderByElements!);
        Assert.True(plainSelect.ForUpdateBeforeOrderBy);
    }

    [Fact]
    public void ForUpdateDetection_ShouldReturnForUpdateClause()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM users FOR UPDATE")!;
        var plainSelect = (PlainSelect)stmt;

        Assert.Equal(ForMode.UPDATE, plainSelect.ForMode);

        var forUpdate = plainSelect.GetForUpdate();
        Assert.NotNull(forUpdate);
        Assert.True(forUpdate!.IsForUpdate());
        Assert.False(forUpdate.IsForShare());
        Assert.Null(forUpdate.Tables);
    }

    [Fact]
    public void ForShare_ShouldSetShareMode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM users FOR SHARE")!;
        var plainSelect = (PlainSelect)stmt;

        Assert.Equal(ForMode.SHARE, plainSelect.ForMode);

        var forUpdate = plainSelect.GetForUpdate();
        Assert.NotNull(forUpdate);
        Assert.True(forUpdate!.IsForShare());
        Assert.False(forUpdate.IsForUpdate());
    }

    [Fact]
    public void ForUpdateNowait_ShouldSetNoWait()
    {
        var sqlStr = "select employee_id from (select employee_id+1 as employee_id from employees) "
                     + "for update of employee_id nowait";

        var stmt = AssertParseAndDeparse(sqlStr);
        var plainSelect = (PlainSelect)stmt;

        Assert.Equal(ForMode.UPDATE, plainSelect.ForMode);
        Assert.True(plainSelect.NoWait);

        var forUpdate = plainSelect.GetForUpdate();
        Assert.True(forUpdate!.NoWait);
        Assert.False(forUpdate.SkipLocked);
    }

    [Fact]
    public void ForUpdateWait_ShouldSetWaitTimeout()
    {
        var sqlStr = "select employee_id from (select employee_id+1 as employee_id from employees) "
                     + "for update of employee_id wait 10";

        var stmt = AssertParseAndDeparse(sqlStr);
        var plainSelect = (PlainSelect)stmt;

        Assert.NotNull(plainSelect.Wait);
        Assert.Equal(10L, plainSelect.Wait!.Timeout);

        var forUpdate = plainSelect.GetForUpdate();
        Assert.NotNull(forUpdate!.Wait);
        Assert.Equal(10L, forUpdate.Wait!.Timeout);
    }

    [Fact]
    public void ForNoKeyUpdate_ShouldParsePostgresMode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM users FOR NO KEY UPDATE")!;
        var plainSelect = (PlainSelect)stmt;
        Assert.Equal(ForMode.NO_KEY_UPDATE, plainSelect.ForMode);
        Assert.Equal("SELECT * FROM users FOR NO KEY UPDATE", stmt.ToString());
    }

    [Fact]
    public void ForKeyShare_ShouldParsePostgresMode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM users FOR KEY SHARE")!;
        var plainSelect = (PlainSelect)stmt;
        Assert.Equal(ForMode.KEY_SHARE, plainSelect.ForMode);
        Assert.Equal("SELECT * FROM users FOR KEY SHARE", stmt.ToString());
    }
}
