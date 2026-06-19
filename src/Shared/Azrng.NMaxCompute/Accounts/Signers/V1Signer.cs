using System.Security.Cryptography;
using System.Text;

namespace Azrng.NMaxCompute.Accounts.Signers;

/// <summary>
/// ODPS V1（legacy）签名器：HMAC-SHA1 + Base64
/// </summary>
public sealed class V1Signer : ISigner
{
    private readonly string _accessId;
    private readonly byte[] _secretAccessKey;

    public V1Signer(string accessId, string secretAccessKey)
    {
        _accessId = accessId ?? throw new ArgumentNullException(nameof(accessId));
        _secretAccessKey = Encoding.UTF8.GetBytes(secretAccessKey ?? throw new ArgumentNullException(nameof(secretAccessKey)));
    }

    public string BuildAuthorization(string canonicalString)
    {
        var canonicalBytes = Encoding.UTF8.GetBytes(canonicalString ?? string.Empty);
        using var hmac = new HMACSHA1(_secretAccessKey);
        var hash = hmac.ComputeHash(canonicalBytes);
        var signature = Convert.ToBase64String(hash);
        return $"ODPS {_accessId}:{signature}";
    }
}
