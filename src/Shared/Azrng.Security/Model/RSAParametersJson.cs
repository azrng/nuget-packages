namespace Azrng.Security.Model
{
    /// <summary>
    /// RSA 参数的 JSON 表示（各字段为 Base64 编码的字节数组）。
    /// 吸收自 Common.SecurityCrypto，用于跨平台/跨语言传递 RSA 密钥参数。
    /// </summary>
    public class RSAParametersJson
    {
        public string Modulus { get; set; }

        public string Exponent { get; set; }

        public string P { get; set; }

        public string Q { get; set; }

        public string DP { get; set; }

        public string DQ { get; set; }

        public string InverseQ { get; set; }

        public string D { get; set; }
    }
}
