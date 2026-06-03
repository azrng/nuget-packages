using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Represents a CREATE TABLE statement in SQL.
/// </summary>
public class CreateTable : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public System.Collections.Generic.List<ColumnDefinition>? ColumnDefinitions { get; set; }
    public System.Collections.Generic.List<Constraint>? Constraints { get; set; }
    public bool IfNotExists { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE TABLE ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Table);
        if (ColumnDefinitions != null)
        {
            sb.Append(" (");
            sb.Append(string.Join(", ", ColumnDefinitions));
            sb.Append(')');
        }
        return sb.ToString();
    }
}
