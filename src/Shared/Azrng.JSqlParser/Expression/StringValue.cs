using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a string value in SQL.
/// </summary>
public class StringValue : ASTNodeAccessImpl, Expression
{
    public string Value { get; set; } = "";

    /// <summary>
    /// 非 0 时表示用 PostgreSQL dollar-quoted 形式（如 <c>$$...$$</c> 或 tag 形式 <c>$tag$...$tag$</c>）。
    /// 对应上游 commit 95ebda5a。ToString 时按原始前缀输出，否则按 'value' 单引号字面量输出。
    /// </summary>
    public string? DollarPrefix { get; set; }

    public StringValue() { }
    public StringValue(string value) => Value = value;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(DollarPrefix))
        {
            return $"{DollarPrefix}{Value}{DollarPrefix}";
        }
        return $"'{Value}'";
    }
}
