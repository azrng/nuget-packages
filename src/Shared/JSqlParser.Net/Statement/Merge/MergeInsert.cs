using JSqlParser.Net.Schema;

namespace JSqlParser.Net.Statement.Merge;

/// <summary>
/// Represents WHEN NOT MATCHED THEN INSERT in MERGE.
/// </summary>
public class MergeInsert : MergeOperation
{
    public System.Collections.Generic.List<Column>? Columns { get; set; }
    public System.Collections.Generic.List<Expression.Expression>? Values { get; set; }

    public override string ToString()
    {
        var cols = Columns != null ? $" ({string.Join(", ", Columns)})" : "";
        var vals = Values != null ? $" VALUES ({string.Join(", ", Values)})" : "";
        var cond = Condition != null ? $" AND {Condition}" : "";
        return $"WHEN {(Not ? "NOT " : "")}MATCHED{cond} THEN INSERT{cols}{vals}";
    }
}
