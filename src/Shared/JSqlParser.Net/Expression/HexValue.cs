using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

public class HexValue : ASTNodeAccessImpl, Expression
{
    public string Value { get; set; } = "";
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value;
}
