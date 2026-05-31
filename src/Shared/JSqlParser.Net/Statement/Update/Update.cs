using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;
using JSqlParser.Net.Statement.Select;

namespace JSqlParser.Net.Statement.Update;

/// <summary>
/// Represents an UPDATE statement in SQL.
/// </summary>
public class Update : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public List<Join>? Joins { get; set; }
    public JSqlParser.Net.Expression.Expression? Where { get; set; }
    public System.Collections.Generic.List<UpdateSet> UpdateSets { get; set; } = new();

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("UPDATE ").Append(Table);
        if (Joins != null)
        {
            foreach (var join in Joins) sb.Append(' ').Append(join);
        }
        sb.Append(" SET ");
        sb.Append(string.Join(", ", UpdateSets));
        if (Where != null) sb.Append(" WHERE ").Append(Where);
        return sb.ToString();
    }
}
