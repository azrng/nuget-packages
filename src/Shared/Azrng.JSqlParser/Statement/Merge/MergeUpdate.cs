using Azrng.JSqlParser.Statement.Update;

namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Represents WHEN MATCHED THEN UPDATE in MERGE.
/// </summary>
public class MergeUpdate : MergeOperation
{
    public System.Collections.Generic.List<UpdateSet> UpdateSets { get; set; } = new();

    public override string ToString()
    {
        var cond = Condition != null ? $" AND {Condition}" : "";
        return $"WHEN {(Not ? "NOT " : "")}MATCHED{cond} THEN UPDATE SET {string.Join(", ", UpdateSets)}";
    }
}
