namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// MERGE <c>WHEN NOT MATCHED</c> 子句的 BY 限定（#2421）。
/// Target = BY TARGET（默认，只允许配 INSERT）；Source = BY SOURCE（只允许配 UPDATE/DELETE，#2480）。
/// </summary>
public enum MergeSide
{
    /// <summary>未指定（等价 BY TARGET 语义）。</summary>
    None,

    /// <summary>BY TARGET。</summary>
    Target,

    /// <summary>BY SOURCE。</summary>
    Source
}
