using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a LIMIT clause in a SQL statement.
/// </summary>
public class Limit : ASTNodeAccessImpl
{
    public Expression.IExpression? RowCount { get; set; }
    public Expression.IExpression? Offset { get; set; }

    public Limit() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(" LIMIT ");
        if (RowCount != null) sb.Append(RowCount);
        if (Offset != null) sb.Append(" OFFSET ").Append(Offset);
        return sb.ToString();
    }
}
