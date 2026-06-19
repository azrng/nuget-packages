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

## 能力范围与实现进度

> 当前版本：**1.0.1**（S2 + S1 收尾交付，Tunnel 已接入运行时调用链）

### 已完成能力

| 能力 | 阶段 | 说明 |
|---|---|---|
| V1 / V4 签名（默认 V4，失败降级 V1） | S0 | `Accounts/CloudAccount` + `Signers/V1Signer` + `Signers/V4Signer` |
| REST 客户端（4 次重试 + 指数退避 + 错误解析） | S0 | `Rest/OdpsRestClient` |
| SQL 提交（XML 协议）+ 状态轮询 | S0 | `Core/Odps` + `Core/Instance` + `Core/JobXmlBuilder` |
| Result API 取结果（CSV，10000 行限制） | S0 | `Core/CsvResultParser`，Tunnel 不可用时的回退路径 |
| ADO.NET Provider | S0 | `MaxComputeConnection` / `MaxComputeCommand` / `MaxComputeDataReader` |
| Dapper 扩展（`QueryAsync<T>` 等） | S0 | `MaxComputeDapperExtensions` |
| Tunnel wire 解码（CRC32 / CRC32C / ProtobufWireReader） | S1 | `Tunnel/Wire/*` |
| Tunnel Session（创建/重载 + JSON schema 解析） | S1 | `Tunnel/InstanceDownloadSession` + `Tunnel/TableSchema` |
| Tunnel 基础类型 decoder | S1 | `Tunnel/Types/*`：bigint/int/double/float/bool/string/binary/decimal/datetime/date |
| Tunnel 单条记录解码 + CRC32C 校验 | S1 | `Tunnel/TunnelRecordReader`（可 dispose） |
| DataReader 类型化返回（`GetFieldType` / `GetDataTypeName`） | S1 | `QueryResult.ColumnTypes` 透传 |
| TIMESTAMP / TIMESTAMP_NTZ / JSON decoder | S2 | `Tunnel/Types/TimestampJsonDecoder` |
| ARRAY / MAP / STRUCT 递归 decoder | S2 | `Tunnel/Types/CompositeDecoders`，size/null marker 不计入 CRC |
| 复合类型字符串解析 | S2 | `Tunnel/Types/TypeStringParser`，支持 `array/map/struct` 嵌套 + 反引号字段名 |
| NULL 处理 | S2 | record 级缺失 field index 即 NULL；复合类型内部前置 null marker |
| **Tunnel 运行时接入（默认取数路径）** | S1收尾 | `DirectOdpsQueryExecutor` 默认走 Tunnel：`RunSql → 等待 → CreateSession → OpenRecordReader → Materialize`，无行数限制 |
| **流式 HTTP 读取** | S1收尾 | `OdpsRestClient.SendStreamingAsync`（`ResponseHeadersRead`）+ `OdpsStreamResponse` |
| **Tunnel → Result API 自动回退** | S1收尾 | 命中 `InvalidProjectTable/InvalidArgument/NoSuchProject/InstanceTypeNotSupported/NoDownload` 时回退 Result API |

### 未开始（S3-S5 计划范围）

| 能力 | 阶段 |
|---|---|
| `MaxComputeCommand.Hints` 注入 SQLTask settings + 连接字符串新键 | S3 |
| `StsAccount`（SecurityToken 头注入） | S4 |
| zlib raw deflate 压缩传输 | S4 |
| 自定义 `TunnelEndpoint` | S4 |
| PyODPS 对照基准测试套件 + 完整集成测试（真实集群） | S5 |

### 待真实集群验证

S0-S2 的代码已全部通过 fixture 单元测试（171 项），但 **Tunnel 端到端联调尚未用真实 MaxCompute 集群验证**。
接入后建议先跑 `SELECT 1 AS a, 2 AS b` 验证签名/会话/解码链路，再逐步覆盖各类型列与超 1 万行场景。



### 后续接手要点

- S1 wire 解码层已完成并通过 fixture 单测，但**未在 `DirectOdpsQueryExecutor` 中接入**，下一步要：
  1. 给 `OdpsRestClient` 增加 `SendStreamingAsync`（返回 `Stream`，不走 `ReadAsByteArrayAsync`）
  2. 在 `InstanceDownloadSession` 上实现 `OpenDataReaderAsync(start, count, columns?, ct)`：拼 `?data&downloadid=...&rowrange=(start,count)` 请求
  3. 在 `DirectOdpsQueryExecutor` 中：默认走 Tunnel；遇到 `InvalidProjectTable / InvalidArgument / NoSuchProject / InstanceTypeNotSupported` 时回退 Result API
- 多目标框架：`net6.0;net7.0;net8.0;net9.0;net10.0`，每个 TFM 单独引用对应版本的 `Microsoft.Extensions.Http` 与 `Microsoft.Extensions.Logging.Abstractions`
- 测试覆盖：`test/Azrng.NMaxCompute.Test/` 共 136 个测试，全部通过

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
