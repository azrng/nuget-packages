using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Represents an EXTRACT expression (e.g., EXTRACT(YEAR FROM date_col)).
/// </summary>
public class ExtractExpression : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public Expression Expression { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"EXTRACT({Name} FROM {Expression})";
}
