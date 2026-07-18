using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Arithmetic;

/// <summary>
/// Base class for binary arithmetic expressions (e.g., a + b, a * b).
/// </summary>
public abstract class BinaryExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression LeftExpression { get; set; }
    public required IExpression RightExpression { get; set; }

    public abstract T Accept<T, S>(IExpressionVisitor<T> visitor, S context);

    /// <summary>运算符符号（如 "+"、"AND"），由子类重写。</summary>
    public abstract string OperatorSymbol { get; }

    /// <summary>拼接好的运算表达式文本：左 操作符 右。</summary>
    protected string OperatorExpressionText => $"{LeftExpression} {OperatorSymbol} {RightExpression}";

    public override string ToString() => OperatorExpressionText;
}
