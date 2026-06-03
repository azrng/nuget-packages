using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// 算术运算符测试
/// </summary>
public class ArithmeticOperatorTest
{
    [Fact]
    public void Addition_InSelect_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a + b FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Addition>(item.Expression);
    }

    [Fact]
    public void Subtraction_InSelect_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a - b FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Subtraction>(item.Expression);
    }

    [Fact]
    public void Multiplication_InSelect_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a * b FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Multiplication>(item.Expression);
    }

    [Fact]
    public void Division_InSelect_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a / b FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Division>(item.Expression);
    }

    [Fact]
    public void Modulo_InSelect_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a % b FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Modulo>(item.Expression);
    }

    [Fact]
    public void Concat_InSelect_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a || b FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Concat>(item.Expression);
    }

    [Fact]
    public void Addition_InWhere_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM t WHERE a + b > 10")!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Multiplication_Precedence_ShouldBindTighter()
    {
        // a + b * c 应该解析为 a + (b * c)
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT a + b * c FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Addition>(item.Expression);
        var add = (Addition)item.Expression;
        Assert.IsType<Multiplication>(add.RightExpression);
    }

    [Fact]
    public void ParenthesizedArithmetic_ShouldRespectParens()
    {
        // (a + b) * c
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT (a + b) * c FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Multiplication>(item.Expression);
        var mul = (Multiplication)item.Expression;
        Assert.IsType<Parenthesis>(mul.LeftExpression);
    }

    [Fact]
    public void Arithmetic_WithConstants_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT 1 + 2 * 3 FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<Addition>(item.Expression);
    }

    [Fact]
    public void UnaryMinus_ShouldBeSignedExpression()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT -id FROM t")!;
        var item = select.SelectItems![0];
        Assert.IsType<SignedExpression>(item.Expression);
    }
}
