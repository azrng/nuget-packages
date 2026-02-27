# AzrngCommon.Security

`AzrngCommon.Security` 是一个常用加密工具库，覆盖国际通用算法与国密算法，提供统一的编码输入输出（`Base64`/`Hex`）。

- 对称加密：AES、AES-GCM、DES、3DES、SM4
- 非对称加密：RSA、SM2
- 哈希与消息认证：MD5、SHA1/SHA256/SHA512、SM3、HMAC
- 目标框架：`netstandard2.1; net6.0; net7.0; net8.0; net9.0; net10.0`

## 安装

```powershell
Install-Package AzrngCommon.Security
```

```bash
dotnet add package AzrngCommon.Security
```

## 快速开始

### 1) AES-GCM（推荐，具备完整性校验）

```csharp
using Common.Security;
using Common.Security.Enums;

var plain = "hello aes-gcm";
var (key, _) = AesHelper.ExportSecretAndIv(256, OutType.Base64);

// 推荐使用命名参数，明确调用 GCM 重载
var cipher = AesGcmHelper.Encrypt(
    plainText: plain,
    secretKey: key,
    secretType: SecretType.Base64,
    outType: OutType.Base64);

var restored = AesGcmHelper.Decrypt(
    cipherCombined: cipher,
    secretKey: key,
    secretType: SecretType.Base64,
    cipherTextType: OutType.Base64);
```

### 2) AES-CBC（兼容场景）

```csharp
using Common.Security;
using Common.Security.Enums;

var plain = "hello aes-cbc";
var (key, iv) = AesHelper.ExportSecretAndIv(256, OutType.Base64);

// 安全默认包装：CBC + PKCS7
var cipher = AesHelper.EncryptCbcPkcs7(plain, key, iv);
var restored = AesHelper.DecryptCbcPkcs7(cipher, key, iv);
```

### 3) RSA（推荐 OAEP-SHA256 + PSS）

```csharp
using Common.Security;
using Common.Security.Enums;
using System.Security.Cryptography;

var source = "hello rsa";
var (publicKey, privateKey) = RsaHelper.ExportBase64RsaKey();

// 加解密（推荐 OAEP-SHA256）
var cipher = RsaHelper.EncryptOaepSha256(source, publicKey, keyType: RSAKeyType.PEM);
var restored = RsaHelper.DecryptOaepSha256(cipher, privateKey, keyType: RSAKeyType.PEM, privateKeyFormat: RsaKeyFormat.PKCS8);

// 签名验签（推荐 PSS）
var sign = RsaHelper.SignDataPss(source, privateKey, HashAlgorithmName.SHA256, privateKeyType: OutType.Base64);
var ok = RsaHelper.VerifyDataPss(source, sign, publicKey, HashAlgorithmName.SHA256, publicKeyType: OutType.Base64);
```

## 详细用法

### 哈希与 HMAC

```csharp
using Common.Security;
using Common.Security.Enums;

var md5 = Md5Helper.GetMd5Hash("abc");
var md5_16 = Md5Helper.GetMd5Hash("abc", is16: true);
var fileMd5 = Md5Helper.GetFileMd5Hash("appsettings.json");

var sha256 = ShaHelper.GetSha256Hash("abc");
var sha512 = ShaHelper.GetSha512Hash("abc");
var fileSha256 = ShaHelper.GetFileSha256Hash("appsettings.json", OutType.Hex);
var fileSha512 = ShaHelper.GetFileSha512Hash("appsettings.json", OutType.Hex);

var hmac = ShaHelper.GetHmacSha256Hash("payload", "secret", OutType.Base64);
var verified = ShaHelper.VerifyHmacSha256Hash("payload", "secret", hmac, OutType.Base64);
```

### AES-GCM 分段输出

适合将 `Cipher/Nonce/Tag` 分开存储或跨系统传输。

```csharp
using Common.Security;
using Common.Security.Enums;

var (key, _) = AesHelper.ExportSecretAndIv(256, OutType.Base64);
var (cipher, nonce, tag) = AesGcmHelper.EncryptToParts("hello", key, outType: OutType.Base64);
var plain = AesGcmHelper.DecryptFromParts(cipher, nonce, tag, key, cipherTextType: OutType.Base64);
```

### RSA 兼容接口（历史系统）

```csharp
using Common.Security;
using Common.Security.Enums;
using System.Security.Cryptography;

var (pub, pri) = RsaHelper.ExportPemRsaKey(RsaKeyFormat.PKCS1);

// 兼容模式：PKCS#1 v1.5
var cipher = RsaHelper.Encrypt("legacy-data", pub);
var plain = RsaHelper.Decrypt(cipher, pri, privateKeyFormat: RsaKeyFormat.PKCS1);

var sign = RsaHelper.SignData("legacy-data", pri, HashAlgorithmName.SHA256);
var ok = RsaHelper.VerifyData("legacy-data", sign, pub, HashAlgorithmName.SHA256);
```

### SM 系列

```csharp
using Common.Security;
using Common.Security.Enums;

// SM3
var sm3 = Sm3Helper.GetSm3Hash("hello");

// SM4
var key = "1234567890123456";
var iv = "1234567890123456";
var cipherSm4 = Sm4Helper.Encrypt("hello", key, Sm4CryptoEnum.CBC, outType: OutType.Base64, iv: iv);
var plainSm4 = Sm4Helper.Decrypt(cipherSm4, key, Sm4CryptoEnum.CBC, inputType: OutType.Base64, iv: iv);

// SM2
var (publicKey, privateKey) = Sm2Helper.ExportKey(OutType.Hex);
var cipherSm2 = Sm2Helper.Encrypt("hello", publicKey, publicKeyType: OutType.Hex, outType: OutType.Hex);
var plainSm2 = Sm2Helper.Decrypt(cipherSm2, privateKey, privateKeyType: OutType.Hex, inputType: OutType.Hex);
```

## 常用枚举说明

- `OutType`
    - `Base64`：Base64 编码字符串
    - `Hex`：十六进制字符串
- `SecretType`
    - `Text`：原始文本密钥（UTF8）
    - `Base64`：Base64 编码密钥
    - `Hex`：十六进制密钥

## 安全建议

- 新项目优先：
    - 对称加密：`AesGcmHelper`（GCM）
    - 非对称加密：`EncryptOaepSha256/DecryptOaepSha256`
    - 数字签名：`SignDataPss/VerifyDataPss`
- DES/3DES 仅建议兼容历史系统，不建议新系统使用。
- 固定密钥、固定 IV、明文落库都会显著降低安全性。

## 兼容与迁移

- `AesGcmHelper` 中历史非 GCM 重载（`Encrypt/Decrypt` 带 `CipherMode/PaddingMode`）已标记 `[Obsolete]`。
- 迁移建议：
    - 历史 AES 调用迁移到 `AesHelper.Encrypt/Decrypt` 或 `AesHelper.EncryptCbcPkcs7/DecryptCbcPkcs7`
    - 新业务优先使用 `AesGcmHelper` 的 GCM 重载

## 主要 API 一览

- `Md5Helper`
    - `GetMd5Hash`
    - `GetHmacMd5Hash`
    - `GetFileMd5Hash`
- `ShaHelper`
    - `GetSha1Hash` `GetSha256Hash` `GetSha512Hash`
    - `GetHmacSha1Hash` `GetHmacSha256Hash` `GetHmacSha512Hash`
    - `VerifyHmacSha1Hash` `VerifyHmacSha256Hash` `VerifyHmacSha512Hash`
    - `GetFileSha256Hash` `GetFileSha512Hash`
- `AesHelper`
    - `ExportSecretAndIv`
    - `EncryptCbcPkcs7` `DecryptCbcPkcs7`
    - `Encrypt` `Decrypt`
- `AesGcmHelper`
    - `Encrypt` `Decrypt`
    - `EncryptToParts` `DecryptFromParts`
- `RsaHelper`
    - `ExportBase64RsaKey` `ExportPemRsaKey` `ExportXmlRsaKey`
    - `EncryptOaepSha256` `DecryptOaepSha256`
    - `SignDataPss` `VerifyDataPss`
    - `QuickSign` `QuickVerify` `QuickSignPss` `QuickVerifyPss`
- `Sm2Helper` `Sm3Helper` `Sm4Helper` `DesHelper` `Des3Helper` `MurmurHashHelper`

## 版本记录

## 更新记录

* 1.2.1
    * 更新加密方法
* 1.2.0
    * 适配.Net10
* 1.1.2
    * 更新与十六进制互转操作
* 1.1.1
    * 将扩展方法改为静态方法
* 1.1.0
    * 升级依赖包
    * 补充文档
* 1.0.0
    * 升级依赖包
* 0.0.1-beta7
    * 对AES加密算法优化
* 0.0.1-beta6
    * 替换依赖包BouncyCastle.NetCore为BouncyCastle.Cryptography，且将里面的一些源码操作改为使用包的方法，性能更好
* 0.0.1-beta5
    * 增加SHA的HMAC算法
* 0.0.1-beta4
    * 增加sm3、rsa等示例，增加单元测试
* 0.0.1-beta3
    * 支持MD5、SHA等、DES、AES、RSA等
* 0.0.1-beta2
    * fix处理md5加密将16位和32位弄混问题
* 0.0.1-beta1
    * 从common里面移出来一些方法