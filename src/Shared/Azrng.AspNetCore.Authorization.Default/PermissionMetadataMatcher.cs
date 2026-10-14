namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 按权限元数据中的匹配模式检查已授予的权限码。
/// </summary>
public static class PermissionMetadataMatcher
{
    /// <summary>
    /// 检查每个权限声明是否都满足。声明之间按 AND 组合，单个声明内部按其匹配模式判断。
    /// </summary>
    /// <param name="requiredPermissions">Endpoint 声明的权限元数据。</param>
    /// <param name="grantedPermissions">当前用户已获得的权限码。</param>
    /// <param name="comparer">权限码比较器；默认使用不区分大小写的序号比较。</param>
    /// <returns>所有权限声明都满足时返回 <see langword="true"/>；没有声明时返回 <see langword="false"/>。</returns>
    public static bool IsSatisfiedBy(
        IEnumerable<IPermissionMetadata> requiredPermissions,
        IEnumerable<string> grantedPermissions,
        StringComparer? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(requiredPermissions);
        ArgumentNullException.ThrowIfNull(grantedPermissions);

        var requirements = requiredPermissions.ToArray();
        if (requirements.Length == 0)
        {
            return false;
        }

        var granted = grantedPermissions
            .Where(static permission => !string.IsNullOrWhiteSpace(permission))
            .ToHashSet(comparer ?? StringComparer.OrdinalIgnoreCase);

        foreach (var requirement in requirements)
        {
            if (requirement.Permissions.Count == 0)
            {
                return false;
            }

            var isSatisfied = requirement.MatchMode switch
            {
                PermissionMatchMode.All => requirement.Permissions.All(
                    permission => !string.IsNullOrWhiteSpace(permission) && granted.Contains(permission)),
                PermissionMatchMode.Any => requirement.Permissions.Any(
                    permission => !string.IsNullOrWhiteSpace(permission) && granted.Contains(permission)),
                _ => false
            };

            if (!isSatisfied)
            {
                return false;
            }
        }

        return true;
    }
}
