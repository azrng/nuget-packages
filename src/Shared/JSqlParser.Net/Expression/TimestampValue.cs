using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

public class TimestampValue : ASTNodeAccessImpl, Expression
{
    public DateTime Value { get; set; }
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss");
}
