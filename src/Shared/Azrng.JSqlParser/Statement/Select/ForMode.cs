namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// FOR UPDATE / FOR SHARE / FOR READ ONLY 锁模式。
/// 移植自 JSqlParser 5.4 ForMode 枚举及 commit e4004444 (FOR READ ONLY/FETCH ONLY)。
/// </summary>
public enum ForMode
{
    /// <summary>FOR UPDATE — 标准行级排他锁</summary>
    UPDATE,

    /// <summary>FOR SHARE — 共享行锁（PostgreSQL）</summary>
    SHARE,

    /// <summary>FOR NO KEY UPDATE — 非键排他锁（PostgreSQL）</summary>
    NO_KEY_UPDATE,

    /// <summary>FOR KEY SHARE — 键级共享锁（PostgreSQL）</summary>
    KEY_SHARE,

    /// <summary>FOR READ ONLY — 只读声明（DB2）</summary>
    READ_ONLY,

    /// <summary>FOR FETCH ONLY — 只读声明（DB2）</summary>
    FETCH_ONLY
}

/// <summary>
/// FOR 模式扩展方法，提供与上游一致的字符串值。
/// </summary>
public static class ForModeExtensions
{
    /// <summary>返回 FOR 模式对应的 SQL 文本（如 "NO KEY UPDATE"）。</summary>
    public static string GetValue(this ForMode mode) => mode switch
    {
        ForMode.UPDATE => "UPDATE",
        ForMode.SHARE => "SHARE",
        ForMode.NO_KEY_UPDATE => "NO KEY UPDATE",
        ForMode.KEY_SHARE => "KEY SHARE",
        ForMode.READ_ONLY => "READ ONLY",
        ForMode.FETCH_ONLY => "FETCH ONLY",
        _ => mode.ToString()
    };
}
