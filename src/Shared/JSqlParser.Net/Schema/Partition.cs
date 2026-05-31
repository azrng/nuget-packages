using System.Text;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Schema;

/// <summary>
/// Represents a partition reference in SQL.
/// </summary>
public class Partition : ASTNodeAccessImpl
{
    public string Name { get; set; } = "";
    public Column? Column { get; set; }
    public Expression.Expression? Value { get; set; }

    public Partition() { }

    public Partition(Column column, Expression.Expression value)
    {
        Column = column;
        Value = value;
    }

    public static void AppendPartitionsTo(StringBuilder builder, IEnumerable<Partition> partitions)
    {
        int j = 0;
        foreach (var partition in partitions)
        {
            if (j > 0) builder.Append(", ");
            builder.Append(partition);
            j++;
        }
    }

    public override string ToString()
    {
        if (Column != null && Value != null)
            return $"{Column} = {Value}";
        return Name;
    }
}
