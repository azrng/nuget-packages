using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// 表示 MySQL FULLTEXT 搜索表达式 MATCH(...) AGAINST(...)。
/// 移植自上游 JSqlParser FullTextSearch，AGAINST 值支持任意表达式（commit 5788ca06）。
/// </summary>
/// <remarks>
/// <see cref="MatchColumns"/> 为 <see cref="List{T}"/>（T=<see cref="Column"/>），
/// 对齐上游 <c>ExpressionList&lt;Column&gt;</c> 的列元信息承载能力（含表名前缀、列限定等）。
/// 此前为 <c>List&lt;string&gt;</c>，丢失 Column 元信息，无法回写带表名前缀的列（如 t.col）。
/// </remarks>
public class FullTextSearch : ASTNodeAccessImpl, IExpression
{
    /// <summary>MATCH 列列表（结构化 Column，对齐上游 ExpressionList&lt;Column&gt;）。</summary>
    public List<Column> MatchColumns { get; set; } = new();

    /// <summary>AGAINST 的匹配表达式（字符串字面量、参数或 concat 等复合表达式）。</summary>
    public required IExpression MatchExpression { get; set; }

    /// <summary>搜索修饰符原文（如 "IN BOOLEAN MODE"），未指定时为 null。</summary>
    public string? SearchModifier { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("MATCH (").Append(string.Join(", ", MatchColumns)).Append(") AGAINST (");
        sb.Append(MatchExpression);
        if (SearchModifier != null) sb.Append(' ').Append(SearchModifier);
        sb.Append(')');
        return sb.ToString();
    }
}
