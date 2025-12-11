namespace Common.SecurityCrypto
{
    /// <summary>
    /// 三统一常量
    /// </summary>
    public class SecurityConstParam
    {
        //sm2参数
        public static readonly string[] sm2_param = {
                "fffffffeffffffffffffffffffffffffffffffff00000000ffffffffffffffff",// p,0
                "fffffffeffffffffffffffffffffffffffffffff00000000fffffffffffffffc",// a,1
                "28e9fa9e9d9f5e344d5a9e4bcf6509a7f39789f515ab8f92ddbcbd414d940e93",// b,2
                "fffffffeffffffffffffffffffffffff7203df6b21c6052b53bbf40939d54123",// n,3
                "32c4ae2c1f1981195f9904466a39c9948fe30bbff2660be1715a4589334c74c7",// gx,4
                "bc3736a2f4f6779c59bdcee36b692153d0a9877cc62a474002df32e52139f0a0" // gy,5
                };

        /// <summary>
        /// sm4
        /// </summary>
        public static readonly bool sm4hexString;

        /// <summary>
        /// SM2加密key
        /// </summary>
        public const string SM2SecurityKey = "04F6E0C3345AE42B51E06BF50B98834988D54EBC7460FE135A48171BC0629EAE205EEDE253A530608178A98F1E19BB737302813BA39ED3FA3C51639D7A20C7391A";

        /// <summary>
        /// SM3加密key
        /// </summary>
        public const string SM3SecurityKey = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        /// <summary>
        /// SM4
        /// </summary>
        public const string SM4SecurityKey = "BMxA9xjQVLsJGEhD";

        /// <summary>
        /// AES
        /// </summary>
        public const string AesSecurityKey = "BMxA9xjQVLsJGEhF";

        /// <summary>
        /// RSA
        /// </summary>
        public const string RSASecurityKey = "<RSAKeyValue><Modulus>3sv82pzcLzRQL8Fju7mqeWNqg9RyyZ2yN4hQIcP5EJIh+NfgQnyRpAu0UEuu5cffCoyQI3rckFaWSiT/Dr6szReWKAC34UH3NCfbscP3PfuqHBiGzErzvZZ85DYaCc+ZuiAggh+/sGY4MtvqMIvO0owkl00s66+BG8VrDYreX50=</Modulus><Exponent>AQAB</Exponent><P>/NU4dnrG1HpKC+pZ9SWf84+StEXPDL8MjoYO6no2uDhwEUOnK9A+xylSSTXRBnRnSBKTuIva2ryanPVS5m+MCw==</P><Q>4ZZytrUJ9avRo1PXBvH1rErV5XGK3tjTZeawHbw/X62cqd3LUm/fIWQxCHLWglTzwp+Um+xvqeZCOvqhLgJj9w==</Q><DP>GTUQ6g8Xn7uJgmKdEWns5pWb5MlI+VZa5CLNfectaXSHB9Gc6ytZ9vVRtOberiwQ2AiyHaYj7cb8C0YSO9NHPQ==</DP><DQ>DZmTWt55Nj1gixcv3HRT2ko8sPNyatLpk7gfn/tMWslNq5P6gQLLkejHZ/n8YqkadP5H6EqNxNFj5shbVTnBqw==</DQ><InverseQ>NnpzWzNgoXjFWc5qWOLUM6iH20Kr5f0V8ER9sC67RykKaBOvLhJW4OUWUEkDGd9d3twPsKms7sk+m5eXP35hLA==</InverseQ><D>E4btcnutELYVERpyE1ICjwEXpNZJ+UHJDPT1kQAMJFeqgpTpIuqoGSitdRwtCBashdAsEfACxOPR6E21zSUJIAayop4nnsnhmbN4moDqKWdinOjQZcXV12cM4zVSbO/834WqlUQiWkSJYTlM5SVOpVrDHNbPfhCzyfAmieTHPEE=</D></RSAKeyValue>";
    }
}