using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// MySQL 8.2 TABLE 简写语句，对齐上游 TableStatement。
/// 语法：<c>TABLE table_name [ORDER BY ...] [LIMIT n [OFFSET n]]</c>，等价于 <c>SELECT * FROM table_name</c>。
/// 不支持 UNION（上游注释明确限制）。
/// </summary>
public class TableStatement : Select
{
    /// <summary>目标表。</summary>
    public Table? Table { get; set; }

    public override T Accept<T, S>(ISelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        builder.Append("TABLE ");
        if (Table != null) builder.Append(Table.Name);
        return builder;
    }
}
