using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

public class DateValue : ASTNodeAccessImpl, Expression
{
    public DateTime Value { get; set; }
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value.ToString("yyyy-MM-dd");
}
