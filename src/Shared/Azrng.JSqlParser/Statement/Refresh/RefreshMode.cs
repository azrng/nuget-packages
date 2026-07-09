namespace Azrng.JSqlParser.Statement.Refresh;

/// <summary>
/// REFRESH MATERIALIZED VIEW 的刷新模式，对齐上游 RefreshMode。
/// </summary>
public enum RefreshMode
{
    /// <summary>未指定（默认）。</summary>
    Default,

    /// <summary>WITH DATA。</summary>
    WithData,

    /// <summary>WITH NO DATA。</summary>
    WithNoData,
}
