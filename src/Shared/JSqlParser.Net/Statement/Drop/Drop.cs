using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;

namespace JSqlParser.Net.Statement.Drop;

/// <summary>
/// Represents a DROP statement in SQL (DROP TABLE/VIEW/INDEX, etc.).
/// </summary>
public class Drop : ASTNodeAccessImpl, Statement
{
    public string Type { get; set; } = "";
    public Table? Name { get; set; }
    public bool IfExists { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("DROP ");
        sb.Append(Type).Append(' ');
        if (IfExists) sb.Append("IF EXISTS ");
        sb.Append(Name);
        return sb.ToString();
    }
}
