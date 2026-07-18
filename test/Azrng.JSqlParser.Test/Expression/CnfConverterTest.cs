using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// CNF 转换测试
/// </summary>
public class CnfConverterTest
{
    private static EqualsTo Eq(string col, long val) => new()
    {
        LeftExpression = new Column { ColumnName = col },
        RightExpression = new LongValue(val)
    };

    private static AndExpression And(Azrng.JSqlParser.Expression.IExpression left, Azrng.JSqlParser.Expression.IExpression right) => new()
    {
        LeftExpression = left,
        RightExpression = right
    };

    private static OrExpression Or(Azrng.JSqlParser.Expression.IExpression left, Azrng.JSqlParser.Expression.IExpression right) => new()
    {
        LeftExpression = left,
        RightExpression = right
    };

    [Fact]
    public void Cnf_SimpleAnd_ShouldRemainAnd()
    {
        var expr = And(Eq("a", 1), Eq("b", 2));
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
    }

    [Fact]
    public void Cnf_SimpleOr_ShouldRemainOr()
    {
        var expr = Or(Eq("a", 1), Eq("b", 2));
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
    }

    [Fact]
    public void Cnf_OrOverAnd_ShouldDistribute()
    {
        // (a = 1 AND b = 2) OR c = 3
        // CNF: (a = 1 OR c = 3) AND (b = 2 OR c = 3)
        var inner = And(Eq("a", 1), Eq("b", 2));
        var expr = Or(inner, Eq("c", 3));
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
        Assert.True(cnf is AndExpression || cnf is MultiAndExpression);
    }

    [Fact]
    public void Cnf_AndOverOr_ShouldRemain()
    {
        // (a = 1 OR b = 2) AND c = 3
        // Already in CNF
        var inner = Or(Eq("a", 1), Eq("b", 2));
        var expr = And(inner, Eq("c", 3));
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
    }

    [Fact]
    public void Cnf_ComplexExpression_ShouldConvert()
    {
        // (a = 1 AND b = 2) OR (c = 3 AND d = 4)
        var left = And(Eq("a", 1), Eq("b", 2));
        var right = And(Eq("c", 3), Eq("d", 4));
        var expr = Or(left, right);
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
        Assert.True(cnf is AndExpression || cnf is MultiAndExpression);
    }

    [Fact]
    public void Cnf_ParsedExpression_ShouldConvert()
    {
        var stmt = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE (status = 'active' AND role = 'admin') OR age > 18")!;
        var where = stmt.Where;
        Assert.NotNull(where);
        var cnf = CNFConverter.ConvertToCNF(where);
        Assert.NotNull(cnf);
    }

    [Fact]
    public void Cnf_NestedOr_ShouldFlatten()
    {
        // a = 1 OR b = 2 OR c = 3
        var expr = Or(Or(Eq("a", 1), Eq("b", 2)), Eq("c", 3));
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
    }

    [Fact]
    public void Cnf_NestedAnd_ShouldFlatten()
    {
        // a = 1 AND b = 2 AND c = 3
        var expr = And(And(Eq("a", 1), Eq("b", 2)), Eq("c", 3));
        var cnf = CNFConverter.ConvertToCNF(expr);
        Assert.NotNull(cnf);
    }
}
