using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Update;

/// <summary>
/// Represents a SET clause entry in UPDATE (column = value).
/// </summary>
public class UpdateSet
{
    public System.Collections.Generic.List<Column> Columns { get; set; } = new();
    public System.Collections.Generic.List<Azrng.JSqlParser.Expression.Expression> Values { get; set; } = new();

    public UpdateSet() { }

    public UpdateSet(Column column, Azrng.JSqlParser.Expression.Expression value)
    {
        Columns.Add(column);
        Values.Add(value);
    }

    public override string ToString()
    {
        var cols = string.Join(", ", Columns);
        var vals = string.Join(", ", Values);
        return $"{cols} = {vals}";
    }
}
