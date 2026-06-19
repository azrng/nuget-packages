# Azrng.NMaxCompute

阿里云 MaxCompute（ODPS）C# 直连客户端：内置 V1/V4 签名、REST 客户端、SQL 执行链路，提供 ADO.NET Provider 能力，**无需 Python 中转**。

## 安装

```bash
dotnet add package Azrng.NMaxCompute
```

## 快速开始

```csharp
using Azrng.NMaxCompute;
using Azrng.NMaxCompute.Provider;

var config = new MaxComputeConfig
{
    Endpoint = "http://service.cn-hangzhou.maxcompute.aliyun.com/api",
    AccessId = "<your-access-id>",
    SecretAccessKey = "<your-secret-access-key>",
    Project = "<your-project>",
    Region = "cn-hangzhou",
    MaxRows = 10000,
    UseV4Signature = true
};

var executor = new DirectOdpsQueryExecutor(
    httpClientFactory: httpClientFactory,
    logger: logger);

using var conn = MaxComputeConnectionFactory.CreateConnection(executor, config);
await conn.OpenAsync();

using var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT 1 AS a, 2 AS b";
using var reader = await cmd.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    Console.WriteLine($"a={reader["a"]}, b={reader["b"]}");
}
```

## 能力范围（v2.0.0 / S0 MVP）

| 能力 | 状态 |
|---|---|
| V1 / V4 签名（默认 V4，失败降级 V1） | ✅ |
| REST 客户端（4 次重试 + 指数退避 + 错误解析） | ✅ |
| SQL 提交（XML 协议）+ 状态轮询 | ✅ |
| Result API 取结果（CSV，10000 行限制） | ✅ |
| ADO.NET Provider（`DbConnection` / `DbCommand` / `DbDataReader`） | ✅ |
| Dapper 扩展（`QueryAsync<T>` 等） | ✅ |
| Instance Tunnel（解除 1 万行限制） | 🔜 后续版本 |
| 复杂类型（ARRAY/MAP/STRUCT） | 🔜 后续版本 |
| STS Token | 🔜 后续版本 |
| 压缩传输 | 🔜 后续版本 |

## 鉴权

默认使用阿里云 V4 签名（推荐）。当服务端返回 V4 不支持的错误时，自动降级到 V1。

可通过 `MaxComputeConfig.UseV4Signature = false` 强制使用 V1。

## 配置项

| 字段 | 必需 | 说明 |
|---|---|---|
| `Endpoint` | ✅ | MaxCompute REST API 地址，如 `http://service.cn-hangzhou.maxcompute.aliyun.com/api` |
| `AccessId` | ✅ | 阿里云 AccessKey ID |
| `SecretAccessKey` | ✅ | 阿里云 AccessKey Secret |
| `Project` | ✅ | MaxCompute 项目名 |
| `Region` | V4 必需 | 地域，如 `cn-hangzhou` |
| `Schema` | ❌ | Schema 名（2.0 模式） |
| `MaxRows` | ❌ | 最大拉取行数，默认 10000 |
| `UseV4Signature` | ❌ | 默认 `true` |
| `Hints` | ❌ | SQL hints，如 `odps.sql.mapper.split.size=256` |

## 连接字符串格式

```
Endpoint=http://service.cn-hangzhou.maxcompute.aliyun.com/api;
Project=my_proj;AccessId=...;SecretAccessKey=...;
Region=cn-hangzhou;Schema=...;
Hints=odps.sql.mapper.split.size=256,odps.sql.mapper.cpu=100
```

兼容旧键名：`Url` 等价于 `Endpoint`，`SecretKey` 等价于 `SecretAccessKey`。

## 与 Dapper 配合

```csharp
using Dapper;

using var conn = MaxComputeConnectionFactory.CreateConnection(executor, config);
await conn.OpenAsync();

var rows = await conn.QueryAsync<(int A, int B)>("SELECT 1 AS A, 2 AS B");
```

## 注意事项

1. **事务**：MaxCompute REST API 不支持事务，相关操作抛 `NotSupportedException`
2. **ChangeDatabase**：不支持，请创建新连接
3. **多结果集**：`NextResult()` 始终返回 false
4. **异步**：所有异步方法支持 `CancellationToken`

## 参考

签名与 SQL 执行链路基于 [PyODPS](https://github.com/aliyun/aliyun-odps-python-sdk) 源码移植。

## License

MIT
