using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

public class BooleanValue : ASTNodeAccessImpl, Expression
{
    public bool Value { get; set; }

    public BooleanValue() { }
    public BooleanValue(bool value) => Value = value;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value ? "TRUE" : "FALSE";
}
