using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

public sealed class HexValue : ASTNodeAccessImpl, Expression
{
    public string Value { get; set; } = "";
    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value;
}
