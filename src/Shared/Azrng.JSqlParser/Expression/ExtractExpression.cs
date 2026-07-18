using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents an EXTRACT expression (e.g., EXTRACT(YEAR FROM date_col)).
/// </summary>
public class ExtractExpression : ASTNodeAccessImpl, IExpression
{
    public string Name { get; set; } = "";
    public required IExpression Expression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"EXTRACT({Name} FROM {Expression})";
}
