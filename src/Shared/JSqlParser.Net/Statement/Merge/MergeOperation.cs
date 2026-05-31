using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement.Merge;

/// <summary>
/// Base class for MERGE operations (WHEN MATCHED/NOT MATCHED).
/// </summary>
public abstract class MergeOperation : ASTNodeAccessImpl
{
    public bool Not { get; set; }
    public Expression.Expression? Condition { get; set; }
}
