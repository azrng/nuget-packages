using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a plain SELECT statement (without UNION/INTERSECT/EXCEPT).
/// </summary>
public class PlainSelect : Select
{
    public Distinct? Distinct { get; set; }
    public bool All { get; set; }
    public List<SelectItem>? SelectItems { get; set; }
    public FromItem? FromItem { get; set; }
    public List<Join>? Joins { get; set; }
    public Expression.Expression? Where { get; set; }
    public PreferringClause? Preferring { get; set; }
    public GroupByElement? GroupBy { get; set; }
    public Expression.Expression? Having { get; set; }

    public override T Accept<T, S>(SelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        builder.Append("SELECT ");
        if (Distinct != null) builder.Append(Distinct).Append(' ');
        else if (All) builder.Append("ALL ");
        if (SelectItems != null) builder.Append(string.Join(", ", SelectItems));

        if (FromItem != null)
        {
            builder.Append(" FROM ").Append(FromItem);
        }

        if (Joins != null)
        {
            foreach (var join in Joins)
            {
                if (join.Simple) builder.Append(join);
                else builder.Append(' ').Append(join);
            }
        }

        if (Where != null) builder.Append(" WHERE ").Append(Where);
        if (GroupBy != null) builder.Append(' ').Append(GroupBy);
        if (Having != null) builder.Append(" HAVING ").Append(Having);

        return builder;
    }

    public static string OrderByToString(List<OrderByElement> orderByElements)
    {
        if (orderByElements == null || orderByElements.Count == 0) return "";
        return " ORDER BY " + string.Join(", ", orderByElements);
    }

    public static string GetStringList<T>(IEnumerable<T> list, bool useComma = true, bool useBrackets = false)
    {
        if (list == null) return "";
        var items = list.Select(x => x?.ToString() ?? "");
        var result = useComma ? string.Join(", ", items) : string.Join(" ", items);
        if (useBrackets) result = $"({result})";
        return result;
    }
}
