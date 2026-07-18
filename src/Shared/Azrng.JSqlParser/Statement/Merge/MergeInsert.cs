using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Represents WHEN NOT MATCHED THEN INSERT in MERGE.
/// </summary>
public class MergeInsert : MergeOperation
{
    public System.Collections.Generic.List<Column>? Columns { get; set; }
    public System.Collections.Generic.List<Expression.IExpression>? Values { get; set; }

    public override string ToString()
    {
        var cols = Columns != null ? $" ({string.Join(", ", Columns)})" : "";
        var vals = Values != null ? $" VALUES ({string.Join(", ", Values)})" : "";
        var cond = Condition != null ? $" AND {Condition}" : "";
        return $"WHEN {(Not ? "NOT " : "")}MATCHED{cond} THEN INSERT{cols}{vals}";
    }
}
