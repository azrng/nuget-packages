namespace Azrng.JSqlParser.Statement.Lock;

/// <summary>
/// LOCK TABLE 语句的锁模式。
/// 移植自上游 JSqlParser commit 6697c063 的 LockMode 枚举。
/// </summary>
public enum LockMode
{
    /// <summary>SHARE — 共享锁</summary>
    Share,

    /// <summary>EXCLUSIVE — 排他锁</summary>
    Exclusive,

    /// <summary>ROW SHARE — 行共享锁（Oracle）</summary>
    RowShare,

    /// <summary>ROW EXCLUSIVE — 行排他锁（Oracle）</summary>
    RowExclusive,

    /// <summary>SHARE UPDATE — 共享更新锁（Oracle）</summary>
    ShareUpdate,

    /// <summary>SHARE ROW EXCLUSIVE — 共享行排他锁（Oracle）</summary>
    ShareRowExclusive
}

/// <summary>
/// LockMode 扩展方法，提供与上游一致的字符串值。
/// </summary>
public static class LockModeExtensions
{
    /// <summary>返回锁模式对应的 SQL 文本（如 "ROW SHARE"）。</summary>
    public static string GetValue(this LockMode mode) => mode switch
    {
        LockMode.Share => "SHARE",
        LockMode.Exclusive => "EXCLUSIVE",
        LockMode.RowShare => "ROW SHARE",
        LockMode.RowExclusive => "ROW EXCLUSIVE",
        LockMode.ShareUpdate => "SHARE UPDATE",
        LockMode.ShareRowExclusive => "SHARE ROW EXCLUSIVE",
        _ => mode.ToString()
    };
}
