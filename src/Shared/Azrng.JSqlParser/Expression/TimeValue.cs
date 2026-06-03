using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

public class TimeValue : ASTNodeAccessImpl, Expression
{
    public TimeSpan Value { get; set; }
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => Value.ToString();
}
