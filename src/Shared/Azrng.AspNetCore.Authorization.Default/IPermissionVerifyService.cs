namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限验证服务接口
/// </summary>
/// <remarks>
/// 实现此接口以自定义权限验证逻辑
/// 通常结合数据库或缓存来验证当前用户是否有访问指定路径的权限
/// </remarks>
public interface IPermissionVerifyService
{
    /// <summary>
    /// 验证当前用户是否有访问指定路径的权限
    /// </summary>
    /// <param name="path">请求的路径（已转换为小写）</param>
    /// <returns>如果用户有权限返回 true，否则返回 false</returns>
    /// <example>
    /// 示例：从数据库验证权限
    /// <code>
    /// public async Task&lt;bool&gt; HasPermission(string path)
    /// {
    ///     var httpContext = _httpContextAccessor.HttpContext;
    ///     var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    ///
    ///     if (string.IsNullOrEmpty(userId))
    ///         return false;
    ///
    ///     // 从数据库获取用户权限
    ///     var userPermissions = await _userRepository.GetUserPermissionsAsync(userId);
    ///     return userPermissions.Any(p => path.Contains(p.Path.ToLowerInvariant()));
    /// }
    /// </code>
    /// </example>
    Task<bool> HasPermission(string path);
}