using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a CAST expression (e.g., CAST(x AS INT)).
/// </summary>
public class CastExpression : ASTNodeAccessImpl, Expression
{
    public string Keyword { get; set; } = "CAST";
    public Expression Expression { get; set; } = null!;
    public string DataType { get; set; } = "";
    public bool UseCastKeyword { get; set; } = true;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (UseCastKeyword)
            return $"{Keyword}({Expression} AS {DataType})";
        return $"{Expression}::{DataType}";
    }
}
