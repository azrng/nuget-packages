using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Represents a BETWEEN expression in SQL.
/// </summary>
public class Between : ASTNodeAccessImpl, Expression
{
    public required Expression LeftExpression { get; set; }
    public required Expression BetweenExpressionStart { get; set; }
    public required Expression BetweenExpressionEnd { get; set; }
    public bool Not { get; set; }

    /// <summary>SQL:2016 BETWEEN SYMMETRIC。</summary>
    public bool UsingSymmetric { get; set; }

    /// <summary>SQL:2016 BETWEEN ASYMMETRIC。</summary>
    public bool UsingAsymmetric { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var modifier = UsingSymmetric ? "SYMMETRIC " : (UsingAsymmetric ? "ASYMMETRIC " : "");
        return $"{LeftExpression} {(Not ? "NOT " : "")}BETWEEN {modifier}{BetweenExpressionStart} AND {BetweenExpressionEnd}";
    }
}
