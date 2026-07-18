using System.Globalization;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a double/float value in SQL.
/// </summary>
public sealed class DoubleValue : ASTNodeAccessImpl, IExpression
{
    public double Value { get; set; }

    public DoubleValue() { }

    public DoubleValue(double value) => Value = value;
    public DoubleValue(string value) => Value = double.Parse(value, CultureInfo.InvariantCulture);

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
