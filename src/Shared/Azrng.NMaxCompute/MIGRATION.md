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
| Tunnel **写**（upload/encoder/block） | ✅ 100% | TableUploadSession + TunnelRecordWriter + 全类型 encoder，往返单测验证 |
| Arrow 格式（ArrowReader/IPC/Stream） | ❌ 0% | 仅 record，无 arrow |
| 多批次分页读（BufferedRecordReader/reopen） | ⚠️ 部分 | 单流单请求，无自动跨请求分页 |
| 表/分区/资源/函数/安全/XFlow/Quota/Session/df/ml 管理 API | ❌ 0% | 超出"查询执行+读取"范围 |

**总体**：核心**读链路 + 写链路 100%**；PyODPS 全功能面（含管理/Arrow/df/ml）按广度约 **40%**。

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
| —（C# 独有）ADO.NET Provider | `MaxCompute*.cs` | ✅ |

---

## 4. 尚未迁移（按优先级）

### P0 — 读链路增强（贴近已实现范围）
- **多批次分页读**：PyODPS `BufferedRecordReader` 按批 reopen（多 HTTP 请求）+ `call_with_retry` 失败重开。当前 C# 单流单请求；超大结果集若服务端按批返回需补 reopen 逻辑。
- **时区选项**：PyODPS `options.local_timezone` / `MillisecondsConverter`。当前 `DateTimeDecoder` 固定本地时区，未暴露开关。
- **Legacy decimal 字节解码**：PyODPS `convert_legacy_decimal_bytes`（旧服务端定点小数内存布局）。

### P1 — 表级下载 / 流式写
- **TableTunnel 下载**：表级下载 session（`create_download_session` + 表数据读取）。当前仅有 InstanceTunnel（查询结果读取），非表级。
- **流式 / 分块写**：当前 `TableUploadSession` 支持整块上传（`WriteBlock`）；PyODPS 的 `BufferedRecordWriter` 自动分块/压缩/重试未迁移。

### P2 — 其他格式
- **Arrow 读取**：`tunnel/io/reader.py::TunnelArrowReader` + `ArrowStreamReader`（IPC 流），含 timestamp struct 转换。

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
