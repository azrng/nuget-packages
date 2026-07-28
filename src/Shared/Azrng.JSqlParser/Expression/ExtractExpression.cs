using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents an EXTRACT expression (e.g., EXTRACT(YEAR FROM date_col)).
/// </summary>
public class ExtractExpression : ASTNodeAccessImpl, IExpression
{
    public string Name { get; set; } = "";
    public required IExpression Expression { get; set; }

    /// <summary>
    /// Oracle 区间限定（#673），如 <c>DAY TO SECOND</c>。
    /// 出现在 <c>EXTRACT(DAY FROM expr DAY TO SECOND)</c> 中。未指定时为 null。
    /// </summary>
    public string? IntervalQualifier { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var q = IntervalQualifier != null ? $" {IntervalQualifier}" : "";
        return $"EXTRACT({Name} FROM {Expression}{q})";
    }
}
