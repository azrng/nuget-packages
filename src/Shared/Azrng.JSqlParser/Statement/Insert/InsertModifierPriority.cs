namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// MySQL INSERT 优先级修饰符。
/// 例如：<code>INSERT LOW_PRIORITY INTO t ...</code>、<code>INSERT DELAYED INTO t ...</code>、
/// <code>INSERT HIGH_PRIORITY INTO t ...</code>。
/// 与上游 InsertModifierPriority 对齐。
/// </summary>
public enum InsertModifierPriority
{
    /// <summary>未指定修饰符。</summary>
    None,

    /// <summary>LOW_PRIORITY：延迟到无其他客户端读取该表时执行。</summary>
    LowPriority,

    /// <summary>DELAYED：将 INSERT 缓冲后立即返回（MySQL 8.0 已废弃）。</summary>
    Delayed,

    /// <summary>HIGH_PRIORITY：覆盖默认的低优先级行为。</summary>
    HighPriority
}
