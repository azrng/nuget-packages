namespace Azrng.JSqlParser.Models;

/// <summary>
/// SQL 语句中引用的一张表（含别名映射）。
/// </summary>
/// <remarks>
/// 中性 DTO，只描述 AST 事实，不含任何产品业务规则。
/// 业务方自行决定别名用法、去重策略与列归属规则。
/// </remarks>
public sealed record TableReference
{
    /// <summary>表名（不含 schema，如 <c>users</c>）。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>表别名（如 <c>u</c>），未指定别名时为 null。</summary>
    public string? Alias { get; init; }

    /// <summary>含 schema 的全限定名（如 <c>dbo.users</c>），无 schema 时与 <see cref="Name"/> 相同。</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// 业务方常用的"别名优先键"：有别名取别名，否则取表名。
    /// 仅提供取值，不强制用法；调用方可按 <see cref="Key"/> 聚合或自行定义键策略。
    /// </summary>
    public string Key => !string.IsNullOrWhiteSpace(Alias) ? Alias : Name;
}
