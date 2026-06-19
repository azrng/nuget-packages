using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Azrng.NMaxCompute.Accounts.Signers;

/// <summary>
/// ODPS V4 签名器：4 层 HMAC-SHA256 派生 + HMAC-SHA1 终签
/// </summary>
public sealed class V4Signer : ISigner
{
    public const string DefaultSignaturePrefix = "aliyun_v4";

    private readonly string _accessId;
    private readonly string _secretAccessKey;
    private readonly string _region;
    private readonly string _signaturePrefix;

    public V4Signer(string accessId, string secretAccessKey, string region, string? signaturePrefix = null)
    {
        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("V4 signature requires region name.", nameof(region));

        _accessId = accessId ?? throw new ArgumentNullException(nameof(accessId));
        _secretAccessKey = secretAccessKey ?? throw new ArgumentNullException(nameof(secretAccessKey));
        _region = region;
        _signaturePrefix = signaturePrefix ?? DefaultSignaturePrefix;
    }

    public string BuildAuthorization(string canonicalString)
    {
        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var signingKey = DeriveSigningKey(dateStr);

        var canonicalBytes = Encoding.UTF8.GetBytes(canonicalString ?? string.Empty);
        using var hmac = new HMACSHA1(signingKey);
        var hash = hmac.ComputeHash(canonicalBytes);
        var signature = Convert.ToBase64String(hash);

        var credential = string.Join("/",
            _accessId,
            dateStr,
            _region,
            $"odps/{_signaturePrefix}_request");

        return $"ODPS {credential}:{signature}";
    }

    private byte[] DeriveSigningKey(string dateStr)
    {
        var kSecret = _signaturePrefix + _secretAccessKey;
        var kSecretBytes = Encoding.UTF8.GetBytes(kSecret);

        var kDate = HmacSha256(kSecretBytes, Encoding.UTF8.GetBytes(dateStr));
        var kRegion = HmacSha256(kDate, Encoding.UTF8.GetBytes(_region));
        var kService = HmacSha256(kRegion, Encoding.UTF8.GetBytes("odps"));
        var kSigning = HmacSha256(kService, Encoding.UTF8.GetBytes($"{_signaturePrefix}_request"));
        return kSigning;
    }

    private static byte[] HmacSha256(byte[] key, byte[] data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }
}
