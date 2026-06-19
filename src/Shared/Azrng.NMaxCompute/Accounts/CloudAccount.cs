using Azrng.NMaxCompute.Accounts.Signers;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Accounts;

/// <summary>
/// 阿里云主账号：AccessId + AccessKey，支持 V4（默认）和 V1（fallback）
/// </summary>
public sealed class CloudAccount : IAccount
{
    private readonly ISigner _v4Signer;
    private readonly ISigner _v1Signer;
    private readonly bool _preferV4;

    public string AccessId { get; }

    /// <summary>
    /// 创建 CloudAccount
    /// </summary>
    /// <param name="accessId">AccessKey ID</param>
    /// <param name="secretAccessKey">AccessKey Secret</param>
    /// <param name="region">region（如 <c>cn-hangzhou</c>）；为 null 则使用 V1 签名</param>
    /// <param name="useV4Signature">是否优先 V4 签名（默认 true，仅在 region 非空时生效）</param>
    public CloudAccount(string accessId, string secretAccessKey, string? region = null, bool useV4Signature = true)
    {
        if (string.IsNullOrWhiteSpace(accessId))
            throw new ArgumentException("accessId required", nameof(accessId));
        if (string.IsNullOrWhiteSpace(secretAccessKey))
            throw new ArgumentException("secretAccessKey required", nameof(secretAccessKey));

        AccessId = accessId;
        _preferV4 = useV4Signature && !string.IsNullOrWhiteSpace(region);

        _v4Signer = _preferV4
            ? new V4Signer(accessId, secretAccessKey, region!)
            : new V1FallbackShim(accessId, secretAccessKey);

        _v1Signer = new V1Signer(accessId, secretAccessKey);
    }

    /// <summary>
    /// 是否在 V4 失败时降级到 V1
    /// </summary>
    public bool CanDowngradeToV1 => _preferV4;

    public void Sign(OdpsRequest request)
    {
        var canonical = CanonicalStringBuilder.Build(request);
        request.Headers["Authorization"] = _v4Signer.BuildAuthorization(canonical);
    }

    /// <summary>
    /// 在 V4 被服务端拒绝时调用，强制用 V1 重签
    /// </summary>
    public void SignWithV1(OdpsRequest request)
    {
        var canonical = CanonicalStringBuilder.Build(request);
        request.Headers["Authorization"] = _v1Signer.BuildAuthorization(canonical);
    }

    private sealed class V1FallbackShim : ISigner
    {
        private readonly V1Signer _inner;

        public V1FallbackShim(string accessId, string secretAccessKey)
        {
            _inner = new V1Signer(accessId, secretAccessKey);
        }

        public string BuildAuthorization(string canonicalString) => _inner.BuildAuthorization(canonicalString);
    }
}
