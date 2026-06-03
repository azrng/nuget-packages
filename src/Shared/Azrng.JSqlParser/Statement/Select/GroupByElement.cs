namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a GROUP BY clause in a SQL statement.
/// </summary>
public class GroupByElement
{
    public List<Expression.Expression> GroupByExpressions { get; set; } = new();
    public bool MySqlWithRollup { get; set; }

    public GroupByElement() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("GROUP BY ");
        sb.Append(string.Join(", ", GroupByExpressions));
        if (MySqlWithRollup) sb.Append(" WITH ROLLUP");
        return sb.ToString();
    }
}
