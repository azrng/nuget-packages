# Common.Security 项目架构与原理说明

## 1. 项目概述

**Common.Security** 是一个全面的 .NET 加密工具库，提供了常用的加密、解密、签名和哈希算法实现。该项目封装了国际通用算法（AES、DES、RSA、MD5、SHA 等）以及中国国家密码算法（国密 SM2、SM3、SM4）。

### 1.1 设计目标

- 提供简洁易用的静态 API 接口
- 支持多种加密输出格式（Base64、Hex）
- 兼容多个 .NET 框架版本
- 统一的错误处理和参数验证
- 支持国密算法以适应国内安全合规要求

### 1.2 目标框架

```
netstandard2.1; net6.0; net7.0; net8.0; net9.0; net10.0
```

---

## 2. 项目结构

```
Common.Security/
├── Enums/                          # 枚举定义
│   ├── OutType.cs                 # 输出类型（Base64/Hex）
│   ├── SecretType.cs              # 密钥类型（Text/Base64/Hex）
│   ├── Sm4CryptoEnum.cs           # SM4加密模式（ECB/CBC）
│   └── RSAKeyType.cs              # RSA密钥格式（Xml/PEM/PKCS1/PKCS8）
├── Extensions/                     # 扩展方法
│   └── BytesAndStringExtensions.cs # 字节数组与字符串转换扩展
├── Model/                          # 数据模型
│   └── SymmetricEncryptionBase.cs # 对称加密基类
├── AESHelper.cs                    # AES对称加密
├── AESGCMHelper.cs                 # AES-GCM认证加密
├── DES3Helper.cs                   # 3DES对称加密
├── DesHelper.cs                    # DES对称加密
├── Md5Helper.cs                    # MD5哈希
├── SHAHelper.cs                    # SHA哈希系列
├── Sm3Helper.cs                    # SM3哈希（国密）
├── SM4Helper.cs                    # SM4对称加密（国密）
├── Sm2Helper.cs                    # SM2非对称加密（国密）
├── RsaHelper.cs                    # RSA非对称加密
└── MurmurHashHelper.cs             # MurmurHash非加密哈希
```

---

## 3. 核心组件设计

### 3.1 枚举层 (Enums/)

#### 3.1.1 OutType - 输出类型枚举

```csharp
public enum OutType
{
    Base64,  // Base64编码输出
    Hex,     // 十六进制字符串输出
}
```

**设计原理**: 统一管理加密结果的输出格式，便于在不同场景下选择合适的编码方式。

#### 3.1.2 SecretType - 密钥类型枚举

```csharp
public enum SecretType
{
    Text,    // 明文密钥
    Base64,  // Base64编码密钥
    Hex,     // 十六进制编码密钥
}
```

**设计原理**: 支持多种密钥输入格式，提高 API 的灵活性。

#### 3.1.3 Sm4CryptoEnum - SM4加密模式

```csharp
public enum Sm4CryptoEnum
{
    ECB = 0,  // 电码本模式
    CBC = 1   // 密码分组链接模式
}
```

**设计原理**: SM4 国密算法支持的模式选择。

#### 3.1.4 RSAKeyType / RsaKeyFormat - RSA密钥格式

```csharp
public enum RSAKeyType { Xml, PEM }
public enum RsaKeyFormat { PKCS8, PKCS1 }
```

**设计原理**: 支持多种 RSA 密钥格式以兼容不同系统。

---

### 3.2 扩展层 (Extensions/)

#### BytesAndStringExtensions 类

**核心功能**: 提供字节数组与字符串之间的转换能力。

```csharp
// 核心扩展方法
byte[] GetBytes(this string data, OutType outType)
string GetString(this byte[] data, OutType outType)
byte[] GetBytes(this string data, SecretType secretType)
string ToHexString(this byte[] bytes)
```

**设计原理**:
1. **格式转换统一入口**: 所有 Helper 类通过此扩展方法进行格式转换
2. **性能优化**: .NET 6.0+ 使用 `Convert.ToHexString`，否则使用 BouncyCastle 的 `Hex.ToHexString`
3. **内部使用**: `internal` 修饰符确保扩展仅供库内部使用

---

### 3.3 基类层 (Model/)

#### SymmetricEncryptionBase 对称加密基类

**设计模式**: 模板方法模式 + 泛型约束

```csharp
public class SymmetricEncryptionBase
{
    protected static byte[] EncryptCore<TCryptoServiceProvider>(
        byte[] sourceBytes, byte[] keyBytes,
        CipherMode cipherMode = CipherMode.ECB, byte[] ivBytes = null)
        where TCryptoServiceProvider : SymmetricAlgorithm, new()

    public static byte[] DecryptCore<TCryptoServiceProvider>(
        byte[] encryptBytes, byte[] keyBytes,
        CipherMode cipherMode = CipherMode.ECB, byte[] ivBytes = null)
        where TCryptoServiceProvider : SymmetricAlgorithm, new()
}
```

**设计原理**:
1. **泛型约束**: `where TCryptoServiceProvider : SymmetricAlgorithm, new()` 确保类型安全
2. **代码复用**: DES、AES 等对称加密算法共享相同的核心加密/解密逻辑
3. **资源管理**: 使用 `try-finally` 确保加密算法提供者被正确释放
4. **模式支持**: 支持 ECB 和 CBC 等加密模式，CBC 模式需要 IV 参数

**使用示例**: [DesHelper.cs:34](src/Shared/Common.Security/DesHelper.cs#L34) 继承此基类实现 DES 加密。

---

## 4. 算法实现原理

### 4.1 哈希算法层

#### 4.1.1 Md5Helper - MD5 哈希

**核心方法**:
- `GetMd5Hash()`: 字符串 MD5 哈希，支持 16 位和 32 位输出
- `GetHmacMd5Hash()`: HMAC-MD5 带密钥哈希
- `GetFileMd5Hash()`: 文件 MD5 计算

**实现特点**:
```csharp
#if NET6_0_OR_GREATER
    var hashResultNew = MD5.HashData(inputBytes);
#else
    using var md5 = MD5.Create();
    var hashResult = md5.ComputeHash(buffer);
#endif
```

- .NET 6.0+ 使用静态方法 `MD5.HashData()` 提高性能
- 早期版本使用 `MD5.Create()` 创建实例

#### 4.1.2 ShaHelper - SHA 哈希系列

**支持算法**: SHA1、SHA256、SHA512 及其 HMAC 变体

**设计模式**:
- `ValidateInput()`: 参数验证提取为私有方法
- `ComputeHash()`: 统一的哈希计算入口

**代码结构**:
```csharp
public static string GetSha256Hash(string str, OutType outputType = OutType.Hex)
{
    ValidateInput(str);
    return ComputeHash(str, SHA256.Create(), outputType);
}
```

#### 4.1.3 Sm3Helper - SM3 国密哈希

**实现依赖**: BouncyCastle.Cryptography

```csharp
var digest = new Org.BouncyCastle.Crypto.Digests.SM3Digest();
var hashBytes = new byte[digest.GetDigestSize()];
digest.BlockUpdate(plaintextBytes, 0, plaintextBytes.Length);
digest.DoFinal(hashBytes, 0);
```

**与网站互认**: https://lzltool.cn/SM3

#### 4.1.4 MurmurHashHelper - 非加密哈希

**算法特点**:
- Google 开发的非加密型哈希函数
- 性能是 MD5 等加密算法的 10 倍以上
- 适用于哈希检索操作（非安全场景）

**核心方法**: `MakeHash64BValue()` - 64 位哈希值计算

---

### 4.2 对称加密算法层

#### 4.2.1 DesHelper - DES 加密

**继承关系**: 继承 `SymmetricEncryptionBase`

**密钥处理**:
```csharp
private static byte[] ComputeRealValue(string originString, Encoding encoding)
{
    const int length = 8;
    var destinationArray = new byte[length];
    Array.Copy(encoding.GetBytes(originString.PadRight(length)), destinationArray, length);
    return destinationArray;
}
```

**设计原理**:
- DES 要求 8 字节密钥
- 使用 `PadRight()` 填充不足的密钥
- 截取过长的密钥

#### 4.2.2 AESHelper / AESGCMHelper - AES 加密

**AESHelper 特点**:
- 支持多种加密模式 (ECB/CBC)
- 支持多种填充模式 (None/PKCS7 等)
- 支持多种密钥格式

**AESGCMHelper 特点**:
- 认证加密（Authenticated Encryption）
- 支持 nonce、cipher、tag 三段式处理
- 可配置 tag 大小（12-16 字节）

**GCM 模式优势**:
- 同时提供机密性和完整性校验
- 适合网络传输等安全敏感场景

**数据组合方式**:
```
Combined = Nonce (12B) | Cipher (var) | Tag (12-16B)
```

#### 4.2.3 SM4Helper - SM4 国密对称加密

**对标算法**: AES（国际标准）

**实现依赖**: BouncyCastle.Cryptography

**核心组件**:
```csharp
var engine = new SM4Engine();
var cipher = new PaddedBufferedBlockCipher(
    sm4CryptoEnum == Sm4CryptoEnum.ECB
        ? new EcbBlockCipher(engine)
        : new CbcBlockCipher(engine)
);
```

**模式支持**:
- ECB 模式: 不需要 IV
- CBC 模式: 需要 16 字节 IV

#### 4.2.4 DES3Helper - 3DES 加密

**实现特点**:
- 使用 BouncyCastle 的 `CipherUtilities`
- 默认模式: `DESede/ECB/PKCS5Padding`
- 使用 `CipherStream` 进行流式加解密

---

### 4.3 非对称加密算法层

#### 4.3.1 RsaHelper - RSA 非对称加密

**密钥生成**:
```csharp
public static (string PublicKey, string PrivateKey) ExportBase64RsaKey()
{
    using var rsa = RSA.Create(DefaultKeySize); // 默认 2048 位
    return (
        Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
        Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())
    );
}
```

**支持的密钥格式**:
- XML 格式（仅限 .NET）
- PEM 格式（跨平台）
  - PKCS1
  - PKCS8

**分块加解密**:
```csharp
var maxBlockSize = rsa.KeySize / 8; // 2048位 => 256字节
// 明文分块加密，每块 <= maxBlockSize - 11
// 密文分块解密，每块 <= maxBlockSize
```

**设计原理**: RSA 加密有长度限制，必须分块处理长文本。

**签名验证**:
```csharp
// 签名
public static string SignData(string data, string privateKey, HashAlgorithmName hash, ...)

// 验证
public static bool VerifyData(string data, string sign, string publicKey, HashAlgorithmName hash, ...)
```

#### 4.3.2 Sm2Helper - SM2 国密非对称加密

**对标算法**: RSA（国际标准）

**椭圆曲线**: `sm2p256v1`（国密标准曲线）

**密钥生成**:
```csharp
var curve = CustomNamedCurves.GetByName("sm2p256v1");
var domainParameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

// 随机生成私钥
var secureRandom = new SecureRandom();
var privateKeyValue = new ECPrivateKeyParameters(new BigInteger(1, secureRandom.GenerateSeed(32)), domainParameters);

// 计算公钥 Q = G * d
var publicKeyValue = new ECPublicKeyParameters(domainParameters.G.Multiply(privateKeyValue.D), domainParameters);
```

**加密原理**:
```csharp
// 使用 ParametersWithRandom 引入随机数
var publicKeyParams = new ParametersWithRandom(publicKeyParam, new SecureRandom());

var engine = new SM2Engine();
engine.Init(true, publicKeyParams);
var encryptedData = engine.ProcessBlock(plaintextBytes, 0, plaintextBytes.Length);
```

**随机性说明**: SM2 加密会引入随机临时私钥，导致相同明文每次加密结果不同（这是安全特性）。

---

## 5. 设计模式与架构原则

### 5.1 使用的设计模式

| 模式 | 应用位置 | 说明 |
|------|---------|------|
| **静态工具类** | 所有 Helper 类 | 无状态 API，简化调用 |
| **模板方法** | SymmetricEncryptionBase | 定义加解密流程骨架 |
| **泛型约束** | EncryptCore/DecryptCore | 类型安全的算法选择 |
| **策略模式** | OutType/SecretType | 可插拔的格式转换策略 |
| **工厂方法** | SHA.Create() 等 | .NET 框架提供的算法工厂 |
| **扩展方法** | BytesAndStringExtensions | 非侵入式功能扩展 |

### 5.2 架构原则

1. **单一职责原则**: 每个 Helper 类专注于一种算法
2. **开闭原则**: 通过泛型基类支持新算法添加
3. **依赖倒置**: 依赖抽象（SymmetricAlgorithm）而非具体实现
4. **接口隔离**: 使用枚举限制参数范围，避免参数过度复杂
5. **迪米特法则**: Helper 类直接暴露，无需中间层

---

## 6. 技术亮点

### 6.1 多框架兼容

```csharp
#if NET6_0_OR_GREATER
    return Convert.ToHexString(bytes);
#else
    return Hex.ToHexString(bytes);
#endif
```

使用条件编译实现不同框架的优化路径。

### 6.2 资源管理

```csharp
using var md5 = MD5.Create();  // 自动释放
using var aes = Aes.Create();
```

使用 `using` 声明确保加密算法提供者正确释放。

### 6.3 参数验证

```csharp
if (string.IsNullOrEmpty(plaintext))
    throw new ArgumentNullException(nameof(plaintext));
```

在入口方法中进行参数验证，快速失败。

### 6.4 国密算法支持

通过 BouncyCastle.Cryptography 库实现 SM2/SM3/SM4 国密算法，满足国内安全合规需求。

---

## 7. 依赖关系

```
Common.Security
├── .NET Framework/System.Security.Cryptography (AES/DES/RSA/MD5/SHA)
└── BouncyCastle.Cryptography (SM2/SM3/SM4/Hex工具)
```

**BouncyCastle 版本**: 2.6.2

---

## 8. 安全注意事项

### 8.1 已知弱算法

以下算法仅供参考或兼容性用途，不推荐新项目使用：

| 算法 | 弱点 | 推荐替代 |
|------|------|---------|
| DES | 56位密钥，可暴力破解 | AES |
| 3DES | 2023年后废弃 | AES |
| MD5 | 碰撞漏洞 | SHA256 |
| SHA1 | 碰撞漏洞 | SHA256 |

### 8.2 推荐算法组合

| 场景 | 推荐算法 |
|------|---------|
| 对称加密 | AES-256-GCM |
| 哈希 | SHA256 |
| 数字签名 | RSA-2048/SM2 |
| 消息认证 | HMAC-SHA256 |
| 国内合规 | SM2/SM3/SM4 |

### 8.3 密钥管理

- 密钥应存储在安全位置（Key Vault、环境变量）
- 不要硬编码密钥在源代码中
- 生产环境使用强随机密钥生成器
- 定期轮换密钥

---

## 9. 性能考虑

### 9.1 哈希算法性能

```
MurmurHash > MD5 > SHA1 > SHA256 > SHA512
```

MurmurHash 适用于非安全场景的哈希表、布隆过滤器等。

### 9.2 加密算法性能

```
AES > SM4 > 3DES > DES
```

AES-NI 指令集可显著加速 AES 加解密。

### 9.3 非对称加密性能

RSA/SM2 加密速度远慢于对称加密，通常用于：
- 加密对称密钥（数字信封）
- 数字签名验证
- 密钥交换

---

## 10. 扩展指南

### 10.1 添加新的对称加密算法

```csharp
public static class NewAlgorithmHelper : SymmetricEncryptionBase
{
    public static string Encrypt(string plaintext, string secretKey, ...)
    {
        var sourceBytes = Encoding.UTF8.GetBytes(plaintext);
        var keyBytes = ComputeRealValue(secretKey, Encoding.UTF8);
        var encrypted = EncryptCore<NewAlgorithmProvider>(sourceBytes, keyBytes, ...);
        return encrypted.GetString(OutType.Base64);
    }

    public static string Decrypt(string ciphertext, string secretKey, ...)
    {
        var encryptedBytes = ciphertext.GetBytes(OutType.Base64);
        var keyBytes = ComputeRealValue(secretKey, Encoding.UTF8);
        var decrypted = DecryptCore<NewAlgorithmProvider>(encryptedBytes, keyBytes, ...);
        return Encoding.UTF8.GetString(decrypted);
    }
}
```

### 10.2 添加新的哈希算法

```csharp
public static class NewHashHelper
{
    public static string GetHash(string str, OutType outputType = OutType.Hex)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));

        using var hashAlgorithm = NewHash.Create();
        var buffer = Encoding.UTF8.GetBytes(str);
        var hashResult = hashAlgorithm.ComputeHash(buffer);
        return hashResult.GetString(outputType);
    }
}
```

---

## 11. 在线测试工具

项目中的多个算法提供了在线测试工具链接，用于验证实现正确性：

| 算法 | 测试工具 |
|------|---------|
| AES | https://lzltool.cn/AES |
| DES | https://lzltool.cn/DES |
| SM2 | https://lzltool.cn/SM2 |
| SM3 | https://lzltool.cn/SM3 |
| SM4 | https://lzltool.cn/SM4 |
| RSA | https://www.toolhelper.cn/AsymmetricEncryption/RsaGenerate |
| HMAC | https://www.lddgo.net/encrypt/hmac |

---

## 12. 版本历史

| 版本 | 变更 |
|------|------|
| 1.2.0 | 适配 .NET 10 |
| 1.1.2 | 更新与十六进制互转操作 |
| 1.1.1 | 将扩展方法改为静态方法 |
| 1.1.0 | 升级依赖包，补充文档 |
| 0.0.1-beta6 | 替换依赖包为 BouncyCastle.Cryptography |
| 0.0.1-beta5 | 增加 SHA 的 HMAC 算法 |
| 0.0.1-beta3 | 支持 MD5/SHA/DES/AES/RSA |
