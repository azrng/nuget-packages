using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a string value in SQL.
/// </summary>
public class StringValue : ASTNodeAccessImpl, Expression
{
    public string Value { get; set; } = "";

    public StringValue() { }
    public StringValue(string value) => Value = value;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"'{Value}'";
}
