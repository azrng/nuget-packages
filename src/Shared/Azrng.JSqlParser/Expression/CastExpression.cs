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

    /// <summary>MySQL CAST 目标类型后的字符集名（如 utf8mb4），未指定时为 null。对齐 #2298。</summary>
    public string? CharacterSet { get; set; }

    /// <summary>MySQL CAST 目标类型后的 COLLATE 排序规则，未指定时为 null。对齐 #2298。</summary>
    public string? Collation { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (UseCastKeyword)
        {
            var charset = string.IsNullOrEmpty(CharacterSet) ? "" : $" CHARACTER SET {CharacterSet}";
            var collation = string.IsNullOrEmpty(Collation) ? "" : $" COLLATE {Collation}";
            return $"{Keyword}({Expression} AS {DataType}{charset}{collation})";
        }
        return $"{Expression}::{DataType}";
    }
}
