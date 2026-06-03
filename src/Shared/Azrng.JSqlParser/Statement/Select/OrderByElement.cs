namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents an ORDER BY element in a SQL statement.
/// </summary>
public class OrderByElement
{
    public enum NullOrdering
    {
        NULLS_FIRST,
        NULLS_LAST
    }

    public Expression.Expression Expression { get; set; } = null!;
    public bool Asc { get; set; } = true;
    public bool AscDescPresent { get; set; }
    public NullOrdering? NullOrder { get; set; }

    public OrderByElement() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(Expression.ToString());
        if (!Asc) sb.Append(" DESC");
        else if (AscDescPresent) sb.Append(" ASC");
        if (NullOrder.HasValue)
            sb.Append(' ').Append(NullOrder == NullOrdering.NULLS_FIRST ? "NULLS FIRST" : "NULLS LAST");
        return sb.ToString();
    }
}
