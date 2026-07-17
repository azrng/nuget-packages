using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// VALUES 表构造器：<c>VALUES (1, 'a'), (2, 'b')</c>。
/// 对齐上游 <c>net.sf.jsqlparser.statement.select.Values</c>（commit 2b141568）。
/// 既是独立的 SELECT 语句（<see cref="Select"/> 子类），也可作为 FROM 子项（<see cref="IFromItem"/>）。
/// </summary>
public class Values : Select, IFromItem
{
    /// <summary>
    /// 值行集合，每个 <see cref="ExpressionList"/> 代表一组 <c>(expr, expr, ...)</c>。
    /// </summary>
    public List<ExpressionList> Rows { get; set; } = new();

    /// <summary>FROM (VALUES ...) AS alias 场景下的表别名，未指定时为 null。</summary>
    public Alias? Alias { get; set; }

    public override T Accept<T, S>(ISelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        builder.Append("VALUES ");
        for (int i = 0; i < Rows.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append('(').Append(Rows[i]).Append(')');
        }
        return builder;
    }

    /// <summary>FROM 子项输出（带别名），用于 <c>FROM (VALUES ...) AS t</c> 场景。</summary>
    [Obsolete("改用 " + nameof(Alias) + " 属性")]
    public Alias? GetAlias() => Alias;

    [Obsolete("改用 " + nameof(Alias) + " 属性")]
    public void SetAlias(Alias alias) { Alias = alias; }
}
