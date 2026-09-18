namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Represents WHEN MATCHED THEN DELETE in MERGE.
/// </summary>
public class MergeDelete : MergeOperation
{
    public override string ToString() => $"{MatchedHeaderText} THEN DELETE";
}
