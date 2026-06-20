using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Accounts;

/// <summary>
/// STS 临时账号：在 CloudAccount 基础上，签名后追加 <c>authorization-sts-token</c> 头。
/// <para>对应 PyODPS <c>odps/accounts.py::StsAccount</c>：</para>
/// <para><c>super().sign_request(req, ...)；req.headers["authorization-sts-token"] = self.sts_token</c></para>
/// </summary>
public sealed class StsAccount : IAccount
{
    private readonly CloudAccount _inner;
    private readonly string _securityToken;

    public StsAccount(string accessId, string secretAccessKey, string securityToken, string? region = null, bool useV4Signature = true)
    {
        if (string.IsNullOrWhiteSpace(securityToken))
            throw new ArgumentException("securityToken required", nameof(securityToken));

        _inner = new CloudAccount(accessId, secretAccessKey, region, useV4Signature);
        _securityToken = securityToken;
    }

    public string AccessId => _inner.AccessId;

    /// <summary>
    /// 内层 CloudAccount（用于 V4→V1 降级等能力复用）
    /// </summary>
    public CloudAccount Inner => _inner;

    public void Sign(OdpsRequest request)
    {
        _inner.Sign(request);
        request.Headers["authorization-sts-token"] = _securityToken;
    }
}
