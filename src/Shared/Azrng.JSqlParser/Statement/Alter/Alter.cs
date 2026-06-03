using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// Represents an ALTER TABLE statement in SQL.
/// </summary>
public class Alter : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public System.Collections.Generic.List<AlterExpression> AlterExpressions { get; set; } = new();

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("ALTER TABLE ");
        sb.Append(Table);
        foreach (var expr in AlterExpressions) sb.Append(' ').Append(expr);
        return sb.ToString();
    }
}
