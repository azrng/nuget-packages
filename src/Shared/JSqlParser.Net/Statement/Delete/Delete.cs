using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;

namespace JSqlParser.Net.Statement.Delete;

/// <summary>
/// Represents a DELETE statement in SQL.
/// </summary>
public class Delete : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public JSqlParser.Net.Expression.Expression? Where { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("DELETE FROM ").Append(Table);
        if (Where != null) sb.Append(" WHERE ").Append(Where);
        return sb.ToString();
    }
}
