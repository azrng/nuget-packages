namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents the DISTINCT clause in a SELECT statement.
/// </summary>
public class Distinct
{
    public List<SelectItem>? OnSelectItems { get; set; }
    public bool UseUnique { get; set; }

    public Distinct() { }

    public Distinct(bool useUnique)
    {
        UseUnique = useUnique;
    }

    public override string ToString()
    {
        var sql = UseUnique ? "UNIQUE" : "DISTINCT";
        if (OnSelectItems != null && OnSelectItems.Count > 0)
        {
            sql += " ON (" + string.Join(", ", OnSelectItems) + ")";
        }
        return sql;
    }
}
