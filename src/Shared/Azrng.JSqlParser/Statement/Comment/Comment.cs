using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Comment;

/// <summary>
/// COMMENT ON 语句，对齐上游 Comment。
/// 形式：<c>COMMENT ON TABLE t IS 'xxx'</c> / <c>COMMENT ON COLUMN c IS 'xxx'</c> / <c>COMMENT ON VIEW v IS 'xxx'</c>。
/// </summary>
public class Comment : ASTNodeAccessImpl, IStatement
{
    public Table? Table { get; set; }
    public Column? Column { get; set; }
    /// <summary>COMMENT ON VIEW 的目标视图，对齐上游 view 字段。</summary>
    public Table? View { get; set; }
    public StringValue? CommentText { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("COMMENT ON ");
        if (Column != null) sb.Append("COLUMN ").Append(Column);
        else if (Table != null) sb.Append("TABLE ").Append(Table);
        else if (View != null) sb.Append("VIEW ").Append(View);
        if (CommentText != null) sb.Append(" IS ").Append(CommentText);
        return sb.ToString();
    }
}
