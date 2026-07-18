using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a CAST expression (e.g., CAST(x AS INT)).
/// </summary>
public class CastExpression : ASTNodeAccessImpl, IExpression
{
    public string Keyword { get; set; } = "CAST";
    public required IExpression Expression { get; set; }
    public string DataType { get; set; } = "";
    public bool UseCastKeyword { get; set; } = true;

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (UseCastKeyword)
            return $"{Keyword}({Expression} AS {DataType})";
        return $"{Expression}::{DataType}";
    }
}
