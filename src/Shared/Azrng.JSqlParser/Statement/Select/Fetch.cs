namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a FETCH clause in a SQL statement (SQL:2008 standard).
/// </summary>
public class Fetch
{
    public Expression.Expression? FetchExpression { get; set; }
    public bool FetchFirst { get; set; }
    public bool RowOrRows { get; set; } // true = ROWS, false = ROW
    public bool Percent { get; set; }
    public bool WithTies { get; set; }

    public Fetch() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(" FETCH ");
        sb.Append(FetchFirst ? "FIRST" : "NEXT");
        if (FetchExpression != null) sb.Append(' ').Append(FetchExpression);
        sb.Append(Percent ? " PERCENT" : "");
        sb.Append(RowOrRows ? " ROWS" : " ROW");
        if (WithTies) sb.Append(" WITH TIES");
        return sb.ToString();
    }
}
