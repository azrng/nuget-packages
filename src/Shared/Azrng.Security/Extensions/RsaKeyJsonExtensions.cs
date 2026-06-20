using System;
using System.Security.Cryptography;
using System.Text.Json;
using Azrng.Security.Model;

namespace Azrng.Security.Extensions
{
    /// <summary>
    /// RSA 参数 JSON 序列化扩展，吸收自 Common.SecurityCrypto（原依赖 Newtonsoft.Json，
    /// 此处改用 System.Text.Json 以避免引入额外依赖）。
    /// </summary>
    public static class RsaKeyJsonExtensions
    {
        public static string ToJsonString(this RSA rsa, bool includePrivateParameters)
        {
            var p = rsa.ExportParameters(includePrivateParameters);
            var model = new RSAParametersJson
            {
                Modulus = p.Modulus != null ? Convert.ToBase64String(p.Modulus) : null,
                Exponent = p.Exponent != null ? Convert.ToBase64String(p.Exponent) : null,
                P = p.P != null ? Convert.ToBase64String(p.P) : null,
                Q = p.Q != null ? Convert.ToBase64String(p.Q) : null,
                DP = p.DP != null ? Convert.ToBase64String(p.DP) : null,
                DQ = p.DQ != null ? Convert.ToBase64String(p.DQ) : null,
                InverseQ = p.InverseQ != null ? Convert.ToBase64String(p.InverseQ) : null,
                D = p.D != null ? Convert.ToBase64String(p.D) : null,
            };
            return JsonSerializer.Serialize(model);
        }

        public static void FromJsonString(this RSA rsa, string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                throw new ArgumentNullException(nameof(jsonString));

            var model = JsonSerializer.Deserialize<RSAParametersJson>(jsonString)
                ?? throw new InvalidOperationException("Invalid Json RSA key.");

            rsa.ImportParameters(new RSAParameters
            {
                Modulus = model.Modulus != null ? Convert.FromBase64String(model.Modulus) : null,
                Exponent = model.Exponent != null ? Convert.FromBase64String(model.Exponent) : null,
                P = model.P != null ? Convert.FromBase64String(model.P) : null,
                Q = model.Q != null ? Convert.FromBase64String(model.Q) : null,
                DP = model.DP != null ? Convert.FromBase64String(model.DP) : null,
                DQ = model.DQ != null ? Convert.FromBase64String(model.DQ) : null,
                InverseQ = model.InverseQ != null ? Convert.FromBase64String(model.InverseQ) : null,
                D = model.D != null ? Convert.FromBase64String(model.D) : null,
            });
        }
    }
}
