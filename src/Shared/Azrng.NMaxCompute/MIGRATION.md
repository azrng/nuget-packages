# Azrng.NMaxCompute 迁移任务追踪

本库是阿里云 MaxCompute（ODPS）官方 Python SDK（**PyODPS**）核心**读链路**的 C#/.NET 直连端口，
无需 Python 中转。本文档记录迁移来源、基线 hash、进度与未迁移范围。

---

## 1. 迁移来源

| 项 | 值 |
|----|----|
| 源项目 | aliyun-odps-python-sdk（PyODPS） |
| 源仓库 | https://github.com/aliyun/aliyun-odps-python-sdk |
| 迁移基线 commit（hash） | `558462b8d61b43c73016837f32c68b2ddfad2cdf` |
| 基线短 hash / 时间 | `558462b` / 2026-06-12（"Update docs (#327)"） |
| 本地对照副本 | `C:\Work\SourceCode\database\aliyun-odps-python-sdk` |
| 目标语言/运行时 | C# / .NET（net6.0–net10.0） |

> 对照时如需更新基线，请同步更新上面的 commit hash 并重新核对"未迁移"清单。

---

## 2. 迁移进度（按链路）

| 链路 | 进度 | 说明 |
|------|:----:|------|
| 账号与签名（V1/V4/STS + canonical） | ✅ 100% | 含 V4→V1 自动降级 |
| REST 客户端（签名注入/重试/错误解析/流式） | ✅ 100% | |
| SQL 提交 + 实例生命周期（submit/wait/status） | ✅ 100% | |
| Result API（`?result` CSV 回退） | ✅ 100% | |
| Instance Tunnel **读**（session + record reader + wire + CRC32C） | ✅ 100% | 单流单请求 |
| 类型解码（标量全部 + array/map/struct/vector/interval + 嵌套） | ✅ 100% | 对齐 PyODPS `_read_field` 全分支 |
| ADO.NET Provider（Connection/Command/DataReader/参数/连接串/Dapper） | ✅ 100% | C# 独有，PyODPS 无对应 |
| Tunnel **写**（upload/encoder/block） | ✅ 100% | TableUploadSession + TunnelRecordWriter + 全类型 encoder；**真实集群端到端已验证**（user_info 上传+读回），含 schema(2.0) 支持 |
| Arrow 格式读（ArrowReader/IPC/Stream） | ✅ 读 100% | 独立包 Azrng.NMaxCompute.Arrow；分帧解码 + schema 前置 + Apache.Arrow IPC，**集群端到端已验证**（?arrow 读 RecordBatch）；timestamp(ns) 按 struct(sec,nano) 解码后转回 TimestampArray、decimal 按 Decimal128Type（见 P2） |
| 多批次分页读（BufferedRecordReader/reopen） | ⚠️ 部分 | 单流单请求，无自动跨请求分页 |
| 表/分区/资源/函数/安全/XFlow/Quota/Session/df/ml 管理 API | ❌ 0% | 超出"查询执行+读取"范围 |

**总体**：核心**读（含 Arrow）+ 写链路 100%**；PyODPS 全功能面（含管理/df/ml）按广度约 **45%**。

---

## 3. 已迁移模块映射（PyODPS → C#）

| PyODPS 模块 | C# 对应 | 状态 |
|---|---|:--:|
| `accounts/`（CloudAccount/StsAccount/Signers/Canonical） | `Accounts/` | ✅ |
| `rest.py`（rest client） | `Rest/OdpsRestClient.cs` + `OdpsRequest/Response/Stream/Error/Exception` | ✅ |
| `models/instance.py`（submit/wait/get_result） | `Core/Odps.cs` + `Instance.cs` + `InstanceResult.cs` | ✅ |
| `models/tasks/sql.py` + `tasks/core.py`（Job/SQLTask XML） | `Core/JobXmlBuilder.cs` | ✅ |
| `tunnel/instancetunnel.py`（InstanceDownloadSession） | `Tunnel/InstanceDownloadSession.cs` | ✅ |
| `tunnel/io/reader.py`（`_read_field`/`_read_single_record`/`_read_array`/`_read_struct`/`_read_vector`） | `Tunnel/TunnelRecordReader.cs` + `Types/*Decoder.cs` | ✅ |
| `tunnel/pb/decoder.py` + `input_stream.py` | `Tunnel/Wire/ProtobufWireReader.cs` | ✅ |
| `tunnel/checksum.py`（CRC32C） | `Tunnel/Wire/Checksum.cs` + `Crc32C.cs` | ✅ |
| `tunnel/wireconstants.py` | `Tunnel/Wire/TunnelWireConstants.cs` | ✅ |
| `types.py`（validate_data_type / 复合解析 / 各类型） | `Tunnel/Types/TypeStringParser.cs` + `TypeDecoderFactory.cs` | ✅ |
| `tunnel/io/writer.py`（`BaseRecordWriter` 记录/块编码） | `Tunnel/TunnelRecordWriter.cs` + `Types/Encoders.cs` + `Wire/ProtobufWireWriter.cs` | ✅ |
| `tunnel/pb/output_stream.py`（编码原语） | `Tunnel/Wire/ProtobufWireWriter.cs` | ✅ |
| `tunnel/tabletunnel.py::TableUploadSession` | `Tunnel/TableUploadSession.cs` + `TableTunnel.cs` | ✅ |
| `tunnel/io/reader.py::ArrowStreamReader`/`TunnelArrowReader`（Arrow 分帧 + IPC） | 独立包 `Azrng.NMaxCompute.Arrow`（MaxComputeArrowFramedStream + MaxComputeArrowReader） | ✅ 读 |
| —（C# 独有）ADO.NET Provider | `MaxCompute*.cs` | ✅ |

---

## 4. 尚未迁移（按优先级）

### P0 — 读链路增强（贴近已实现范围）
- ✅ **时区选项**（已完成）：`MaxComputeConfig.UseLocalTimeZone`（默认 true，对齐 PyODPS `options.local_timezone`）。`DateTimeDecoder` 补 `LocalInstance`/`UtcInstance`；`TypeDecoderFactory`/`TypeStringParser` 透传 `useUtc` 到 datetime/timestamp（含 `array<datetime>` 等嵌套）；`timestamp_ntz` 无时区语义始终 UTC。离线单测覆盖（`TimeZoneOptionTest`）。
- ✅ **多批次分页读**（实现 + 离线单测完成）：`BufferedRecordReader.ReadAllAsync` 按 `sliceSize` 分片 reopen（每片一次 HTTP 请求 + `TunnelRecordReader`），`IAsyncEnumerable` 流式产出；`InstanceDownloadSession.ReadRowsAsync` 暴露，内存受切片大小约束。对应 PyODPS `BufferedRecordReader`。单请求失败重试由 `OdpsRestClient` 4 次退避承担；集群是否强制分批待真机确认（客户端分片与服务端是否分批无关地工作）。离线单测覆盖切片边界/末片部分/0 行/取消（`BufferedRecordReaderTest`）。
- **Legacy decimal 字节解码**：PyODPS `convert_legacy_decimal_bytes`（旧服务端定点小数内存布局）。

### P1 — 表级下载 / 流式写
- ✅ **TableTunnel 下载**（实现 + 离线单测完成）：`TableDownloadSession`（创建 session + `OpenRecordReaderAsync` + `ReadRowsAsync` 分片），`TableTunnel.CreateDownloadSessionAsync`（支持分区 / schema 2.0 / quota / tags）。复用 `TunnelRecordReader` + `BufferedRecordReader` + `UseLocalTimeZone`。请求格式镜像 `TableUploadSession`（写侧）+ `InstanceDownloadSession`（读侧）；集群真机确认后置。离线单测覆盖响应解析（`TableDownloadSessionTest`）。
- **流式 / 分块写**：当前 `TableUploadSession` 支持整块上传（`WriteBlock`）；PyODPS 的 `BufferedRecordWriter` 自动分块/压缩/重试未迁移。

### P2 — Arrow 写 / 列格式转换
- ✅ **timestamp-as-struct 读侧转换**（T041，已完成）：服务端把纳秒 `timestamp` 按 `struct(sec:int64, nano:int32)` 发送，原 TimestampType 前置会导致 batch `index out of range`。`OdpsArrowSchemaConverter` 的 wire schema 把 timestamp(ns) 映射为 struct 以对齐 batch 布局，`MaxComputeArrowReader` 读出后转回 `TimestampArray`（对齐 PyODPS `_convert_struct_timestamps`），公共 schema 仍为 TimestampType。离线单测通过（schema 映射 + struct↔timestamp 转换 + 真实 Arrow IPC struct batch 端到端）；集群端到端待 `ArrowTsProbe` 探针最终确认。
- ✅ **decimal 列**（已完成）：Arrow 端按 `Decimal128Type`（16 字节定点）声明与解码。
- ❌ **Arrow 写**（未迁移）：当前仅支持 Arrow 读；PyODPS 的 Arrow 上传写（`ArrowStreamWriter` 写 block）未迁移。
- ❌ **legacy-decimal-bytes**（未迁移）：PyODPS `convert_legacy_decimal_bytes`（旧服务端定点小数字节内存布局）未处理。

### P3 — 管理 / 元数据 API（PyODPS 庞大体量，按需迁移）
- 表与分区：`models/tables.py` / `partitions.py`（CRUD、列表、分区规格、PartitionSpecCondition）。
- 实例增强：`get_task_summary/detail/quota/workers`、`get/put_task_info`、`stop`。
- 其他任务类型：SQLCost / SQLRT / Cupid / MaxFrame / Merge / Copy。
- 函数 / 资源 / UDF、安全（用户/角色/策略/包）、XFlow、Quota、Schema 2.0 管理。
- 会话 / MCQA：`models/session/`、`odps.inter`。
- DataFrame（`df/`）、ML（`models/ml/`）、Volume Tunnel、表血缘。

---

## 5. 维护约定

- 每次从 PyODPS 引入新模块或修复对齐问题时，更新本文档第 2、3、4 节。
- 若更新迁移基线（pull PyODPS 新 commit），必须更新第 1 节 hash 并复核差异。
- 已迁移模块的行为以 PyODPS 基线 commit 为准；偏差（如 JSON 转义、vector 返回 double[]）在代码注释与测试中固化。
