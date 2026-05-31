using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;

namespace JSqlParser.Net.Statement.CreateView;

/// <summary>
/// Represents a CREATE VIEW statement in SQL.
/// </summary>
public class CreateView : ASTNodeAccessImpl, Statement
{
    public Table? View { get; set; }
    public Select.Select? Select { get; set; }
    public bool OrReplace { get; set; }
    public bool IfNotExists { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE ");
        if (OrReplace) sb.Append("OR REPLACE ");
        sb.Append("VIEW ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(View);
        if (Select != null) sb.Append(" AS ").Append(Select);
        return sb.ToString();
    }
}
