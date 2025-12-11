namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 用户ID提供者接口，用于获取当前用户的唯一标识。
/// </summary>
public interface IUserIdProvider
{
    /// <summary>
    /// 获取当前用户的ID（可能为 null）。
    /// </summary>
    /// <returns>用户ID字符串，或 null。</returns>
    string? GetUserId();
}

/// <summary>
/// 基于环境信息的用户ID提供者，通常用于获取当前登录用户的用户名。
/// </summary>
public class EnvironmentUserIdProvider : IUserIdProvider
{
    /// <summary>
    /// 单例实例，确保全局唯一。
    /// </summary>
    public static EnvironmentUserIdProvider Instance { get; } = new EnvironmentUserIdProvider();

    /// <summary>
    /// 获取当前操作系统的用户名。
    /// </summary>
    /// <returns>系统用户名，例如 "Administrator"。</returns>
    public string? GetUserId()
    {
        return Environment.UserName;
    }
}