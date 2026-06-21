# Azrng.NMaxCompute

阿里云 MaxCompute（ODPS）C# 直连客户端——[PyODPS](https://github.com/aliyun/aliyun-odps-python-sdk)（阿里云官方 ODPS Python SDK）核心**读 / 写链路**的 C#/.NET 端口：内置 V1/V4 签名、REST 客户端、SQL 执行链路与 ADO.NET Provider，**无需 Python 中转**。

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

> 当前版本：**2.0.0-alpha**（Tunnel 读/写 + Arrow 读已接入运行时调用链；STS / Hints / TunnelEndpoint / 时区开关已支持）

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
| Tunnel 基础类型 decoder | S1 | bigint/int/double/float/bool/string/binary/decimal/datetime/date |
| Tunnel 单条记录解码 + CRC32C 校验 | S1 | `Tunnel/TunnelRecordReader`（可 dispose） |
| DataReader 类型化返回 | S1 | `QueryResult.ColumnTypes` 透传 |
| TIMESTAMP / TIMESTAMP_NTZ / JSON decoder | S2 | `Tunnel/Types/TimestampJsonDecoder` |
| ARRAY / MAP / STRUCT 递归 decoder | S2 | `Tunnel/Types/CompositeDecoders`，size/null marker 不计入 CRC |
| 复合类型字符串解析 | S2 | `Tunnel/Types/TypeStringParser` |
| NULL 处理 | S2 | record 级缺失 field index 即 NULL；复合类型内部前置 null marker |
| Tunnel 运行时接入（默认取数路径） | S1收尾 | `DirectOdpsQueryExecutor` 默认走 Tunnel，无行数限制 |
| 流式 HTTP 读取 | S1收尾 | `OdpsRestClient.SendStreamingAsync` + `OdpsStreamResponse` |
| Tunnel → Result API 自动回退 | S1收尾 | 命中 `InvalidProjectTable/InvalidArgument/NoSuchProject/InstanceTypeNotSupported/NoDownload` 时回退 |
| **`MaxComputeCommand.Hints` 注入** | S3 | 单命令级 hints 覆盖 config.Hints（同名 key 以命令为准），透传到 SQLTask settings |
| **连接字符串 Hints 键** | S3 | `Hints=a=1,b=2`（逗号分隔 kv），`GetHintsDictionary()` 解析 |
| **STS 临时凭证（`StsAccount`）** | S4 | `config.SecurityToken` 非空时自动用 StsAccount，签名后注入 `authorization-sts-token` 头 |
| **自定义 `TunnelEndpoint`** | S4 | `config.TunnelEndpoint` 非空时，Tunnel 请求走独立端点 |

### 未开始（仅剩需真实集群的部分）

| 能力 | 阶段 | 说明 |
|---|---|---|
| zlib raw deflate 压缩传输 | S4 | 当前未请求压缩，多数场景可跑通；服务端强制压缩时会失败 |
| PyODPS 对照基准测试套件 | S5 | 需真实集群 |

### 集成测试（已就绪，待填凭据）

`test/Azrng.NMaxCompute.Test/Integration/MaxComputeIntegrationTest.cs` 提供端到端集成测试脚手架，默认无操作跳过，设置以下环境变量后运行：
`MAXCOMPUTE_TEST_ENDPOINT` / `MAXCOMPUTE_TEST_ACCESS_ID` / `MAXCOMPUTE_TEST_SECRET_KEY` / `MAXCOMPUTE_TEST_PROJECT` / `MAXCOMPUTE_TEST_REGION`（可选 `SCHEMA` / `TUNNEL_ENDPOINT` / `SECURITY_TOKEN`）。

```bash
dotnet test --filter "Category=Integration"
```

覆盖：`SELECT 1` 烟测、混合类型（Tunnel 路径 + 类型解析）、超 1 万行（验证不截断）。

### 待真实集群验证

S0-S5 的代码已全部通过 fixture 单元测试（**186 项**），但 **Tunnel 端到端联调尚未用真实 MaxCompute 集群验证**。
建议先跑 `SELECT 1 AS a, 2 AS b` 验证签名/会话/解码链路，再逐步覆盖各类型列、STS、超 1 万行场景。

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
| `UseLocalTimeZone` | ❌ | datetime / timestamp 按本地时区返回（默认 `true`，对齐 PyODPS `local_timezone`）；设 `false` 返回 UTC |
| `Hints` | ❌ | SQL hints，如 `odps.sql.mapper.split.size=256` |

## 连接字符串格式

```
Endpoint=http://service.cn-hangzhou.maxcompute.aliyun.com/api;
Project=my_proj;AccessId=...;SecretAccessKey=...;
Region=cn-hangzhou;Schema=...;
Hints=odps.sql.mapper.split.size=256,odps.sql.mapper.cpu=100
```

兼容旧键名：`Url` 等价于 `Endpoint`，`SecretKey` 等价于 `SecretAccessKey`。`UseLocalTimeZone=false` 可让 datetime/timestamp 返回 UTC。

## 与 Dapper 配合

```csharp
using Dapper;

using var conn = MaxComputeConnectionFactory.CreateConnection(executor, config);
await conn.OpenAsync();

var rows = await conn.QueryAsync<(int A, int B)>("SELECT 1 AS A, 2 AS B");
```

## Arrow 格式读取（可选包 `Azrng.NMaxCompute.Arrow`）

核心库默认以 **record/CSV** 路径读取结果（`TunnelRecordReader` + ADO.NET）。
如果你需要以 **Apache Arrow 列式格式**消费结果（更适合大数据分析、与 DataFrame / Arrow 生态互操作），
可额外引用独立的 opt-in 包：

```bash
dotnet add package Azrng.NMaxCompute.Arrow
```

- **设计**：核心库 `Azrng.NMaxCompute` **不依赖** Arrow；Arrow 包额外引用 `Apache.Arrow`，按需引入，避免把重型依赖强加给所有消费者。
- **原理**：核心库的 `InstanceDownloadSession.OpenArrowStreamAsync(...)` 用 `?arrow` 拉取原始分帧流；
  Arrow 包负责 **MaxCompute 分帧解码（chunk + CRC32C）→ ODPS→Arrow schema 转换 + 前置 → Apache.Arrow IPC 解析**，逐个产出 `RecordBatch`。
- **目标框架**：仅 net8.0+（受 `Apache.Arrow` 约束）。

```csharp
using Azrng.NMaxCompute.Arrow;
using Azrng.NMaxCompute.Tunnel;

// session 为 InstanceDownloadSession（由 InstanceTunnel 创建）
using var arrow = await session.OpenArrowReaderAsync(start: 0, count: session.RecordCount);

Apache.Arrow.Schema schema = arrow.Schema;          // Arrow 列定义
while (arrow.ReadNext() is { } batch)                // 逐个 RecordBatch
{
    var ids = (Apache.Arrow.Arrays.Int64Array)batch.Column(0);
    // ...
}
```

> 当前状态：分帧 / schema 转换 / 前置 / 合成往返已单元验证；真实集群 RecordBatch 的 buffer 布局兼容仍在校准（见各包内 MIGRATION 说明）。


## 注意事项

1. **事务**：MaxCompute REST API 不支持事务，相关操作抛 `NotSupportedException`
2. **ChangeDatabase**：不支持，请创建新连接
3. **多结果集**：`NextResult()` 始终返回 false
4. **异步**：所有异步方法支持 `CancellationToken`

## 迁移来源与迁移范围

本库是阿里云 MaxCompute（ODPS）官方 Python SDK **PyODPS** 的 C#/.NET 直连端口——不通过 Python 中转，直接用 HTTP + 签名对接 MaxCompute REST / Tunnel。行为以 PyODPS 基线 commit 为准。

### 来源

| 项 | 值 |
|----|----|
| 源项目 | PyODPS（aliyun-odps-python-sdk） |
| 源仓库 | https://github.com/aliyun/aliyun-odps-python-sdk |
| 迁移基线 commit | `558462b8d61b43c73016837f32c68b2ddfad2cdf`（`558462b` / 2026-06-12） |

### 已迁移（PyODPS → C#）

| PyODPS 模块 | C# 对应 | 说明 |
|---|---|---|
| `accounts/`（签名 V1/V4/STS + canonical） | `Accounts/` + `Signers/` | V4 默认、失败自动降级 V1 |
| `rest.py`（REST 客户端） | `Rest/OdpsRestClient.cs` | 重试 + 指数退避 + 错误解析 + 流式 |
| `models/instance.py`（SQL 提交 / 轮询 / 取结果） | `Core/Odps.cs` + `Instance.cs` + `JobXmlBuilder.cs` | XML 协议 + 状态轮询 |
| Result API（`?result` CSV） | `Core/CsvResultParser.cs` | Tunnel 不可用时的回退路径（10000 行限制） |
| `tunnel/instancetunnel.py`（下载会话） | `Tunnel/InstanceDownloadSession.cs` + `TableSchema.cs` | session 创建 / 重载 + JSON schema 解析 |
| `tunnel/io/reader.py`（记录 / 字段解码） | `Tunnel/TunnelRecordReader.cs` + `Types/*Decoder.cs` | 标量全部 + array/map/struct/vector/interval 递归 + CRC32C |
| `tunnel/pb/`（wire 原语 + CRC32C） | `Tunnel/Wire/*`（`ProtobufWireReader` / `Checksum` / `Crc32C`） | |
| `types.py`（类型校验 / 复合解析） | `Tunnel/Types/TypeStringParser.cs` + `TypeDecoderFactory.cs` | |
| `tunnel/io/writer.py` + `tabletunnel.py::TableUploadSession` | `Tunnel/TunnelRecordWriter.cs` + `TableUploadSession.cs` + `Types/Encoders.cs` + `Wire/ProtobufWireWriter.cs` | **写链路**：上传 + 块编码 + 全类型 encoder |
| `tunnel/io/reader.py::ArrowStreamReader`（Arrow 分帧 + IPC） | 独立包 [`Azrng.NMaxCompute.Arrow`](../Azrng.NMaxCompute.Arrow) | opt-in；含 timestamp-as-struct 转换、decimal128 |
| —（C# 独有）ADO.NET Provider | `MaxCompute*.cs` | `Connection` / `Command` / `DataReader` + Dapper，PyODPS 无对应 |

### 未迁移（超出读 / 写链路范围）

- **管理 / 元数据 API**：表、分区、资源、函数、UDF、安全（用户 / 角色 / 策略 / 包）、XFlow、Quota、Schema 2.0 管理。
- **实例增强**：`get_task_summary/detail/quota/workers`、`get/put_task_info`、`stop`。
- **其他任务类型**：SQLCost / SQLRT / Cupid / MaxFrame / Merge / Copy。
- **DataFrame（`df/`）/ ML（`models/ml/`）/ MCQA 会话 / Volume Tunnel / 表血缘**。
- **读链路增强**：多批次分页 reopen（当前单流单请求）、时区开关、legacy-decimal-bytes；Arrow 写。

> 完整的进度表、逐模块映射与未迁移优先级见 [`MIGRATION.md`](./MIGRATION.md)。

## License

MIT
