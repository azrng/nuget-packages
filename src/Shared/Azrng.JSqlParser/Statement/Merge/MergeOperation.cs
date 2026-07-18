using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Base class for MERGE operations (WHEN MATCHED/NOT MATCHED).
/// </summary>
public abstract class MergeOperation : ASTNodeAccessImpl
{
    public bool Not { get; set; }
    public Expression.IExpression? Condition { get; set; }
}
