using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Base class for comparison operators (=, &lt;&gt;, &gt;, &lt;, etc.)
/// </summary>
public abstract class ComparisonOperator : BinaryExpression
{
    public string Operator { get; set; } = "";

    protected ComparisonOperator() { }
    protected ComparisonOperator(string op) => Operator = op;

    public override string GetStringExpression() => Operator;
}
