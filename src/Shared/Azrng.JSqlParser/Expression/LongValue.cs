using System.Globalization;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a long integer value in SQL.
/// </summary>
public sealed class LongValue : ASTNodeAccessImpl, Expression
{
    public long Value { get; set; }

    public LongValue() { }

    public LongValue(long value)
    {
        Value = value;
    }

    public LongValue(string value)
    {
        Value = long.Parse(value, CultureInfo.InvariantCulture);
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
