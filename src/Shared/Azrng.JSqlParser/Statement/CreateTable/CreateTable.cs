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
        // 合并列定义与表级约束（按列在先、约束在后的顺序），保持括号内逗号分隔
        var items = new List<string>();
        if (ColumnDefinitions != null)
            items.AddRange(ColumnDefinitions.Select(c => c.ToString()));
        if (Constraints != null)
            items.AddRange(Constraints.Select(c => c.ToString()));
        if (items.Count > 0)
        {
            sb.Append(" (");
            sb.Append(string.Join(", ", items));
            sb.Append(')');
        }
        return sb.ToString();
    }
}
