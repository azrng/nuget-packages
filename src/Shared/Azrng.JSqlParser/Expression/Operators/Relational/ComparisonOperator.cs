using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Base class for comparison operators (=, &lt;&gt;, &gt;, &lt;, etc.)
/// </summary>
public abstract class ComparisonOperator : BinaryExpression
{
    /// <summary>比较运算符文本（如 "="、"&lt;&gt;"）。</summary>
    public string Operator { get; set; } = "";

    protected ComparisonOperator() { }
    protected ComparisonOperator(string op) => Operator = op;

    /// <summary>运算符符号，取自 <see cref="Operator"/> 字段。</summary>
    public override string OperatorSymbol => Operator;
}
