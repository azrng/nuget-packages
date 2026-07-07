using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Comment;

/// <summary>
/// COMMENT ON 语句，对齐上游 Comment。
/// 形式：<c>COMMENT ON TABLE t IS 'xxx'</c> / <c>COMMENT ON COLUMN c IS 'xxx'</c>。
/// </summary>
public class Comment : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public Column? Column { get; set; }
    public StringValue? CommentText { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("COMMENT ON ");
        if (Column != null) sb.Append("COLUMN ").Append(Column);
        else if (Table != null) sb.Append("TABLE ").Append(Table);
        if (CommentText != null) sb.Append(" IS ").Append(CommentText);
        return sb.ToString();
    }
}
