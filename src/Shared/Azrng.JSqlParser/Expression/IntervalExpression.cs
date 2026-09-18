using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents an INTERVAL expression (e.g., INTERVAL '7' DAY).
/// </summary>
public class IntervalExpression : ASTNodeAccessImpl, IExpression
{
    public string? Parameter { get; set; }

    /// <summary>限定符原文（如 "DAY TO SECOND(6)"），与 <see cref="Qualifier"/> 同步填充。</summary>
    public string? IntervalType { get; set; }

    /// <summary>限定符结构化模型（#1728），与 <see cref="IntervalType"/> 同步填充。</summary>
    public IntervalQualifier? Qualifier { get; set; }
    public bool IntervalKeyword { get; set; } = true;
    public IExpression? Expression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (IntervalKeyword)
        {
            var param = Expression != null ? Expression.ToString() : $"'{Parameter}'";
            return $"INTERVAL {param}" + (IntervalType != null ? $" {IntervalType}" : "");
        }
        return Expression?.ToString() ?? "";
    }
}
