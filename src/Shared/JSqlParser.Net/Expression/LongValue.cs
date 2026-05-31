using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Represents a long integer value in SQL.
/// </summary>
public class LongValue : ASTNodeAccessImpl, Expression
{
    public long Value { get; set; }

    public LongValue() { }

    public LongValue(long value)
    {
        Value = value;
    }

    public LongValue(string value)
    {
        Value = long.Parse(value);
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Value.ToString();
}
