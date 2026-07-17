using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

public sealed class BooleanValue : ASTNodeAccessImpl, Expression
{
    public bool Value { get; set; }

    public BooleanValue() { }
    public BooleanValue(bool value) => Value = value;

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value ? "TRUE" : "FALSE";
}
