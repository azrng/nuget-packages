# AzrngCommon.Security

## 概述
AzrngCommon.Security 是一个全面的加密工具库，提供了常用的加密、解密、签名和哈希算法实现。

该包封装了常见的加密公共类，支持国密算法（SM2、SM3、SM4）以及国际通用算法（AES、DES、RSA、MD5、SHA等）。

## 功能特性

- 支持多种加密算法：AES、DES、RSA、SM2、SM4等
- 支持多种哈希算法：MD5、SHA1/SHA256/SHA512、SM3等
- 支持多种输出格式：Base64、Hex
- 支持国密算法（SM系列）
- 支持对称和非对称加密
- 支持数字签名和验证
- 支持文件哈希计算
- 支持多框架：.NET Standard 2.1、.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package AzrngCommon.Security
```

或通过 .NET CLI:

```
dotnet add package AzrngCommon.Security
```

## 使用方法

### 哈希算法

#### MD5 哈希算法

```c#
// 字符串MD5哈希算法
string md5Hash = Md5Helper.GetMd5Hash("Hello World");

// 获取16位MD5哈希
string md5Hash16 = Md5Helper.GetMd5Hash("Hello World", is16: true);

// 获取HMAC-MD5哈希
string hmacMd5Hash = Md5Helper.GetHmacMd5Hash("Hello World", "secretKey");

// 文件获取MD5
string fileMd5Hash = Md5Helper.GetFileMd5Hash("path/to/file.txt");
```

#### SHA 哈希算法

```c#
// 获取字符串SHA1值
string sha1Hash = ShaHelper.GetSha1Hash("Hello World");

// 获取字符串SHA256值
string sha256Hash = ShaHelper.GetSha256Hash("Hello World");

// 获取字符串SHA512值
string sha512Hash = ShaHelper.GetSha512Hash("Hello World");

// 获取HMAC-SHA系列哈希值
string hmacSha1Hash = ShaHelper.GetHmacSha1Hash("Hello World", "secretKey");
string hmacSha256Hash = ShaHelper.GetHmacSha256Hash("Hello World", "secretKey");
string hmacSha512Hash = ShaHelper.GetHmacSha512Hash("Hello World", "secretKey");
```

#### SM3 哈希算法（国密）

```c#
// 获取字符串SM3哈希值
string sm3Hash = Sm3Helper.GetSm3Hash("Hello World");
```

### 对称加密算法

#### DES 加密算法

```c#
// 加密
string encrypted = DesHelper.Encrypt("Hello World", "12345678");

// 解密
string decrypted = DesHelper.Decrypt(encrypted, "12345678");
```

#### AES 加密算法

```c#
// 加密
string encrypted = AESHelper.Encrypt("Hello World", "mySecretKey12345");

// 解密
string decrypted = AESHelper.Decrypt(encrypted, "mySecretKey12345");
```

#### SM4 加密算法（国密）

```c#
// ECB模式加密
string encryptedEcb = Sm4Helper.Encrypt("Hello World", "1234567890123456");

// CBC模式加密
string encryptedCbc = Sm4Helper.Encrypt("Hello World", "1234567890123456", Sm4CryptoEnum.CBC, "1234567890123456");

// ECB模式解密
string decryptedEcb = Sm4Helper.Decrypt(encryptedEcb, "1234567890123456");

// CBC模式解密
string decryptedCbc = Sm4Helper.Decrypt(encryptedCbc, "1234567890123456", Sm4CryptoEnum.CBC, "1234567890123456");
```

### 非对称加密算法

#### RSA 非对称加密算法

```c#
// 生成RSA密钥对
var (publicKey, privateKey) = RsaHelper.ExportBase64RsaKey();

// 加密
string encrypted = RsaHelper.Encrypt("Hello World", publicKey);

// 解密
string decrypted = RsaHelper.Decrypt(encrypted, privateKey);

// 签名
string signature = RsaHelper.QuickSign("Hello World", privateKey);

// 验证签名
bool isValid = RsaHelper.QuickVerify("Hello World", signature, publicKey);
```

#### SM2 非对称加密算法（国密）

```c#
// 生成SM2密钥对
var (publicKey, privateKey) = Sm2Helper.ExportKey();

// 加密
string encrypted = Sm2Helper.Encrypt("Hello World", publicKey);

// 解密
string decrypted = Sm2Helper.Decrypt(encrypted, privateKey);
```

## API 参考

### 哈希算法类

- `Md5Helper`: MD5 哈希算法
  - `GetMd5Hash`: 获取字符串 MD5 哈希值
  - `GetHmacMd5Hash`: 获取 HMAC-MD5 哈希值
  - `GetFileMd5Hash`: 获取文件 MD5 哈希值

- `ShaHelper`: SHA 哈希算法
  - `GetSha1Hash`: 获取字符串 SHA1 哈希值
  - `GetSha256Hash`: 获取字符串 SHA256 哈希值
  - `GetSha512Hash`: 获取字符串 SHA512 哈希值
  - `GetHmacSha1Hash`: 获取 HMAC-SHA1 哈希值
  - `GetHmacSha256Hash`: 获取 HMAC-SHA256 哈希值
  - `GetHmacSha512Hash`: 获取 HMAC-SHA512 哈希值

- `Sm3Helper`: SM3 哈希算法（国密）
  - `GetSm3Hash`: 获取字符串 SM3 哈希值

### 对称加密类

- `DesHelper`: DES 对称加密算法
  - `Encrypt`: 加密
  - `Decrypt`: 解密

- `AESHelper`: AES 对称加密算法
  - `Encrypt`: 加密
  - `Decrypt`: 解密

- `Sm4Helper`: SM4 对称加密算法（国密）
  - `Encrypt`: 加密
  - `Decrypt`: 解密

### 非对称加密类

- `RsaHelper`: RSA 非对称加密算法
  - `ExportBase64RsaKey`: 生成 RSA 密钥对
  - `Encrypt`: 加密
  - `Decrypt`: 解密
  - `QuickSign`: 快速签名
  - `QuickVerify`: 快速验证签名

- `Sm2Helper`: SM2 非对称加密算法（国密）
  - `ExportKey`: 生成 SM2 密钥对
  - `Encrypt`: 加密
  - `Decrypt`: 解密

## 更新记录

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