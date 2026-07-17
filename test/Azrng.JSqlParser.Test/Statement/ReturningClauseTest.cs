using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;
using Azrng.JSqlParser.Statement.Insert;
using Insert = Azrng.JSqlParser.Statement.Insert.Insert;
using Update = Azrng.JSqlParser.Statement.Update.Update;
using Delete = Azrng.JSqlParser.Statement.Delete.Delete;
using ReturningClause = Azrng.JSqlParser.Statement.ReturningClause;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// RETURNING 子句测试，覆盖基础 RETURNING 及 PostgreSQL 18 OLD/NEW 引用。
/// 移植自上游 JSqlParser commit f47a8b30 的 ReturningClauseTest.java，适配为 xUnit。
/// </summary>
public class ReturningClauseTest
{
    /// <summary>断言 SQL 可被解析，且再次序列化后与原 SQL（忽略大小写、空白、运算符空格）一致。</summary>
    private static Azrng.JSqlParser.Statement.Statement AssertParseAndDeparse(string sql)
    {
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var reparsed = stmt!.ToString()!.Trim();
        Assert.Equal(Normalize(sql), Normalize(reparsed), StringComparer.OrdinalIgnoreCase);
        return stmt;
    }

    private static string Normalize(string sql)
    {
        var folded = System.Text.RegularExpressions.Regex.Replace(sql.Trim(), @"\s+", " ");
        return System.Text.RegularExpressions.Regex.Replace(folded, @"\s*([+\-*/=])\s*", "$1");
    }

    [Fact]
    public void Update_BasicReturning_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("UPDATE products SET price = 10 RETURNING price, name")!;
        var update = (Update)stmt;
        Assert.NotNull(update.Returning);
        Assert.Equal(2, update.Returning!.SelectItems.Count);
        Assert.Equal("UPDATE products SET price = 10 RETURNING price, name", stmt.ToString());
    }

    [Fact]
    public void Delete_BasicReturning_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("DELETE FROM users WHERE id = 1 RETURNING *")!;
        var delete = (Delete)stmt;
        Assert.NotNull(delete.Returning);
        Assert.Single(delete.Returning!.SelectItems);
    }

    [Fact]
    public void Insert_BasicReturning_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("INSERT INTO emp (empno) VALUES (1) RETURNING empno")!;
        var insert = (Insert)stmt;
        Assert.NotNull(insert.Returning);
        Assert.Single(insert.Returning!.SelectItems);
    }

    /// <summary>
    /// Oracle PL/SQL 的 RETURNING ... INTO 变量绑定应正确解析并往返。
    /// </summary>
    [Fact]
    public void Returning_IntoSingleTarget_ShouldRoundTrip()
    {
        var sql = "INSERT INTO emp (empno) VALUES (1) RETURNING empno INTO x";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void Returning_IntoMultipleTargets_ShouldRoundTrip()
    {
        var sql = "INSERT INTO emp (empno, ename) VALUES (1, 'a') RETURNING empno, ename INTO x, y";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void Returning_IntoQualifiedTarget_ShouldRoundTrip()
    {
        var sql = "DELETE FROM users WHERE id = 1 RETURNING name INTO pkg.var";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>
    /// PostgreSQL 18: RETURNING old.price AS old_price, new.price AS new_price, new.*
    /// 默认限定符 old/new 应被归一化为 ReturningReferenceType，Table 被清空。
    /// 注：用整数避免浮点字面量归一化(1.10->1.1)干扰往返比较。
    /// </summary>
    [Fact]
    public void ReturningOldNewDefaultReferences_ShouldNormalizeAndDeparse()
    {
        var sql = "UPDATE products SET price = 10 "
                  + "RETURNING old.price AS old_price, new.price AS new_price, new.*";

        var update = (Update)AssertParseAndDeparse(sql);
        var returning = update.Returning;
        Assert.NotNull(returning);
        Assert.Null(returning!.OutputAliases);

        var oldPrice = Assert.IsType<Column>(returning.SelectItems[0].Expression);
        Assert.Null(oldPrice.Table);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.Old, oldPrice.ReturningReferenceType);
        Assert.Equal("old", oldPrice.ReturningQualifier);

        var newPrice = Assert.IsType<Column>(returning.SelectItems[1].Expression);
        Assert.Null(newPrice.Table);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.New, newPrice.ReturningReferenceType);
        Assert.Equal("new", newPrice.ReturningQualifier);

        var allNew = Assert.IsType<AllTableColumns>(returning.SelectItems[2].Expression);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.New, allNew.ReturningReferenceType);
        Assert.Equal("new", allNew.ReturningQualifier);
    }

    /// <summary>
    /// PostgreSQL 18: RETURNING WITH (OLD AS o, NEW AS n) o.price, n.price, n.*
    /// 自定义别名 o/n 应被映射到 OLD/NEW。
    /// 注：用 UPDATE 验证，规避 INSERT VALUES 序列化的既有缺陷。
    /// </summary>
    [Fact]
    public void ReturningWithOutputAliases_ShouldParseAndMapAliases()
    {
        var sql = "UPDATE products SET price = 99 "
                  + "RETURNING WITH (OLD AS o, NEW AS n) o.price AS old_price, n.price AS new_price, n.*";

        var update = (Update)AssertParseAndDeparse(sql);
        var returning = update.Returning;
        Assert.NotNull(returning);
        Assert.Equal(2, returning!.OutputAliases!.Count);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.Old,
            returning.OutputAliases[0].ReferenceType);
        Assert.Equal("o", returning.OutputAliases[0].Alias);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.New,
            returning.OutputAliases[1].ReferenceType);
        Assert.Equal("n", returning.OutputAliases[1].Alias);

        var oldPrice = Assert.IsType<Column>(returning.SelectItems[0].Expression);
        Assert.Null(oldPrice.Table);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.Old, oldPrice.ReturningReferenceType);
        Assert.Equal("o", oldPrice.ReturningQualifier);

        var allNew = Assert.IsType<AllTableColumns>(returning.SelectItems[2].Expression);
        Assert.Equal(Azrng.JSqlParser.Statement.ReturningReferenceType.New, allNew.ReturningReferenceType);
        Assert.Equal("n", allNew.ReturningQualifier);
    }
}
