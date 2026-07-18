using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// 表示 MySQL FULLTEXT 搜索表达式 MATCH(...) AGAINST(...)。
/// 移植自上游 JSqlParser FullTextSearch，AGAINST 值支持任意表达式（commit 5788ca06）。
/// </summary>
public class FullTextSearch : ASTNodeAccessImpl, IExpression
{
    /// <summary>MATCH 列列表。</summary>
    public System.Collections.Generic.List<string> Columns { get; set; } = new();

    /// <summary>AGAINST 的匹配表达式（字符串字面量、参数或 concat 等复合表达式）。</summary>
    public required IExpression MatchExpression { get; set; }

    /// <summary>搜索修饰符原文（如 "IN BOOLEAN MODE"），未指定时为 null。</summary>
    public string? SearchModifier { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("MATCH (").Append(string.Join(", ", Columns)).Append(") AGAINST (");
        sb.Append(MatchExpression);
        if (SearchModifier != null) sb.Append(' ').Append(SearchModifier);
        sb.Append(')');
        return sb.ToString();
    }
}

