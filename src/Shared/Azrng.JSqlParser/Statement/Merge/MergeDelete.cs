namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Represents WHEN MATCHED THEN DELETE in MERGE.
/// </summary>
public class MergeDelete : MergeOperation
{
    public override string ToString()
    {
        var cond = Condition != null ? $" AND {Condition}" : "";
        return $"WHEN {(Not ? "NOT " : "")}MATCHED{cond} THEN DELETE";
    }
}
