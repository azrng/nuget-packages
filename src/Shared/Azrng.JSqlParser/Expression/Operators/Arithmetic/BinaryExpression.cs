using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Arithmetic;

/// <summary>
/// Base class for binary arithmetic expressions (e.g., a + b, a * b).
/// </summary>
public abstract class BinaryExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public Expression RightExpression { get; set; } = null!;

    public abstract T Accept<T, S>(ExpressionVisitor<T> visitor, S context);

    /// <summary>运算符符号（如 "+"、"AND"），由子类重写。</summary>
    public abstract string OperatorSymbol { get; }

    /// <summary>拼接好的运算表达式文本：左 操作符 右。</summary>
    protected string OperatorExpressionText => $"{LeftExpression} {OperatorSymbol} {RightExpression}";

    /// <summary>返回运算符符号（兼容旧 API，改用 <see cref="OperatorSymbol"/> 属性）。</summary>
    [Obsolete("改用 OperatorSymbol 属性")]
    public string GetStringExpression() => OperatorSymbol;

    public override string ToString() => OperatorExpressionText;
}
