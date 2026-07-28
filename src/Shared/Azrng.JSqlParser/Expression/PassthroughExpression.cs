using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 原文透传表达式：用于 ODBC 转义、Oracle XMLPARSE/XMLSERIALIZE 等
/// 结构化收益低、但 round-trip 必须保留的方言形式。
/// </summary>
public class PassthroughExpression : ASTNodeAccessImpl, IExpression
{
    /// <summary>完整原文（含关键字/括号/花括号）。</summary>
    public string Text { get; set; } = "";

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Text;
}
