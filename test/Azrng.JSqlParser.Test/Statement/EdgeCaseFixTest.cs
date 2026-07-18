using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 边界异常修复测试：M3 JsonFunction null + L8 ParenthesedSelect 非 PlainSelect。
/// </summary>
public class EdgeCaseFixTest
{
    // ===== M3: JsonFunction null path 抛 InvalidOperationException =====

    /// <summary>程序化构造的 JSON_VALUE 类型 JsonFunction 缺 JsonPathExpression 时 ToString 抛异常，防非法 SQL。
    /// 注：JSON_OBJECT 默认类型不调用 AppendInputAndPath，只有 VALUE/QUERY 类型需要 path。</summary>
    [Fact]
    public void M3_JsonFunction_NullPath_Throws()
    {
        var jf = new JsonFunction(JsonFunction.FunctionType.VALUE);
        Assert.Throws<InvalidOperationException>(() => jf.ToString());
    }

    /// <summary>解析路径产生的 JsonFunction 有 path，ToString 正常（回归保护）。</summary>
    [Fact]
    public void M3_JsonFunction_WithPath_DoesNotThrow()
    {
        var stmt = SqlParser.Parse("SELECT JSON_VALUE(t.col, '$.path') FROM t");
        Assert.NotNull(stmt);
        Assert.Contains("JSON_VALUE", stmt!.ToString());
    }

    // ===== L8: ParenthesedSelect.GetPlainSelect 非 PlainSelect 抛 JSqlParserException =====

    /// <summary>子查询含 UNION（内部是 SetOperationList）时 GetPlainSelect 抛 JSqlParserException（非裸 InvalidCastException）。</summary>
    [Fact]
    public void L8_ParenthesedSelect_SetOperationList_ThrowsJSqlParserException()
    {
        var plainSelect = (PlainSelect)SqlParser.Parse(
            "SELECT * FROM (SELECT 1 UNION SELECT 2) t")!;
        var subquery = (ParenthesedSelect)plainSelect.FromItem!;

        var ex = Assert.Throws<JSqlParserException>(() => subquery.GetPlainSelect());
        Assert.Contains("PlainSelect", ex.Message);
    }

    /// <summary>子查询是纯 PlainSelect 时 GetPlainSelect 正常返回（回归保护）。</summary>
    [Fact]
    public void L8_ParenthesedSelect_PlainSelect_ReturnsNormally()
    {
        var plainSelect = (PlainSelect)SqlParser.Parse(
            "SELECT * FROM (SELECT 1) t")!;
        var subquery = (ParenthesedSelect)plainSelect.FromItem!;

        var inner = subquery.GetPlainSelect();
        Assert.NotNull(inner);
    }
}
