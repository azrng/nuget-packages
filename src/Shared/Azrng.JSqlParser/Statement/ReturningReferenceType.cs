namespace Azrng.JSqlParser.Statement;

/// <summary>
/// RETURNING 子句中 OLD/NEW 引用的类型（PostgreSQL 18）。
/// 移植自上游 JSqlParser commit f47a8b30 的 ReturningReferenceType。
/// </summary>
public enum ReturningReferenceType
{
    /// <summary>OLD — 引用修改前的行数据</summary>
    Old,

    /// <summary>NEW — 引用修改后的行数据</summary>
    New
}

/// <summary>ReturningReferenceType 辅助方法。</summary>
public static class ReturningReferenceTypeExtensions
{
    /// <summary>
    /// 将字符串解析为 ReturningReferenceType，不区分大小写。
    /// 仅识别 "OLD" 和 "NEW"，其它返回 null。
    /// </summary>
    public static ReturningReferenceType? From(string? name)
    {
        if (name == null) return null;
        if ("OLD".Equals(name, StringComparison.OrdinalIgnoreCase)) return ReturningReferenceType.Old;
        if ("NEW".Equals(name, StringComparison.OrdinalIgnoreCase)) return ReturningReferenceType.New;
        return null;
    }
}
