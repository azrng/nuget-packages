using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Accounts;

/// <summary>
/// ODPS 账号抽象
/// </summary>
public interface IAccount
{
    /// <summary>
    /// AccessKey ID
    /// </summary>
    string AccessId { get; }

    /// <summary>
    /// 对请求注入 Authorization 头（同时可能注入 Date / x-odps-security-token 等头）
    /// </summary>
    void Sign(OdpsRequest request);
}
