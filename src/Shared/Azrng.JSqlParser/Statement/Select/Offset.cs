namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents an OFFSET clause in a SQL statement.
/// </summary>
public class Offset
{
    public Expression.IExpression? OffsetExpression { get; set; }
    public string? OffsetParam { get; set; }

    public Offset() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(" OFFSET ");
        if (OffsetExpression != null) sb.Append(OffsetExpression);
        if (OffsetParam != null) sb.Append(' ').Append(OffsetParam);
        return sb.ToString();
    }
}
