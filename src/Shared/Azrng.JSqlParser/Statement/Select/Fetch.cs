namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a FETCH clause in a SQL statement (SQL:2008 standard).
/// </summary>
public class Fetch
{
    public Expression.IExpression? FetchExpression { get; set; }
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
        if (Percent) sb.Append(" PERCENT");
        sb.Append(RowOrRows ? " ROWS" : " ROW");
        sb.Append(WithTies ? " WITH TIES" : " ONLY");
        return sb.ToString();
    }
}
