using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Base class for MERGE operations (WHEN MATCHED/NOT MATCHED).
/// </summary>
public abstract class MergeOperation : ASTNodeAccessImpl
{
    public bool Not { get; set; }
    public Expression.IExpression? Condition { get; set; }

    /// <summary>WHEN NOT MATCHED 的 BY TARGET/BY SOURCE 限定（#2421）。None 表示未指定。</summary>
    public MergeSide Side { get; set; } = MergeSide.None;

    /// <summary>WHEN 子句公共头部：WHEN [NOT] MATCHED [BY TARGET|BY SOURCE] [AND cond]。</summary>
    protected string MatchedHeaderText
    {
        get
        {
            var side = Side switch
            {
                MergeSide.Target => " BY TARGET",
                MergeSide.Source => " BY SOURCE",
                _ => ""
            };
            var cond = Condition != null ? $" AND {Condition}" : "";
            return $"WHEN {(Not ? "NOT " : "")}MATCHED{side}{cond}";
        }
    }
}
