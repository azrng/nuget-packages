using System.Collections.ObjectModel;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 可添加到 Endpoint 的权限元数据。
/// </summary>
public sealed class PermissionMetadata : IPermissionMetadata
{
    private readonly IReadOnlyList<string> _permissions;

    /// <summary>
    /// 初始化 <see cref="PermissionMetadata"/> 的新实例。
    /// </summary>
    /// <param name="permissions">权限码。</param>
    public PermissionMetadata(params string[] permissions)
        : this(PermissionMatchMode.All, permissions)
    {
    }

    /// <summary>
    /// 初始化 <see cref="PermissionMetadata"/> 的新实例。
    /// </summary>
    /// <param name="matchMode">多个权限码的匹配方式。</param>
    /// <param name="permissions">权限码。</param>
    public PermissionMetadata(PermissionMatchMode matchMode, params string[] permissions)
    {
        _permissions = new ReadOnlyCollection<string>(Normalize(permissions));
        MatchMode = matchMode;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Permissions => _permissions;

    /// <inheritdoc />
    public PermissionMatchMode MatchMode { get; }

    internal static string[] Normalize(IEnumerable<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        var normalized = permissions
            .Select(permission => permission?.Trim())
            .ToArray();

        if (normalized.Length == 0 || normalized.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("至少需要一个非空权限码。", nameof(permissions));
        }

        return normalized!;
    }
}
