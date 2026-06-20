# Azrng.NMaxCompute S3 + S4：Hints 注入 / STS / TunnelEndpoint

日期：2026-06-20
任务 ID：T020（S3+S4）
关联：S3（Provider 完善）、S4（STS/TunnelEndpoint）；承接 T019（S1 收尾）

## 背景

Tunnel 已接入运行时调用链后，Provider 层还缺三块实际使用中常被需要的：单命令级 SQL hints、STS 临时凭证、独立 Tunnel 端点。本次一并补齐 S3 全部 + S4 的可验证部分（压缩留待真实集群联调时处理）。

## 完成范围

### S3：Hints 注入 + 连接字符串键

- `MaxComputeConnectionStringBuilder`：
  - 新增 `Hints` 属性（原始字符串，逗号分隔的 `key=value`）
  - 新增 `GetHintsDictionary()`：容错解析（容忍空格、空项、缺等号的项）
  - `ToConfig()` 透传 `config.Hints`
  - `ToString()` 回写 `Hints=...`
- `MaxComputeCommand.Hints`：单命令级 hints 字典
  - `MergeConfig()`：命令 hints 覆盖 config hints（同名 key 以命令为准），无命令 hints 时直接复用 config（零拷贝优化）
  - 6 个执行路径（ExecuteNonQuery/Reader/Scalar 同步+异步）全部切到 `MergeConfig()`

### S4：STS + TunnelEndpoint

- `Accounts/StsAccount.cs`：对应 PyODPS `StsAccount`
  - 包装 `CloudAccount`，签名后追加 `authorization-sts-token` 头（PyODPS 实际用此头，非 `x-odps-security-token`）
  - 暴露 `Inner` 复用 V4→V1 降级能力
- `DirectOdpsQueryExecutor`：
  - `BuildAccount`：`config.SecurityToken` 非空 → `StsAccount`，否则 `CloudAccount`
  - `config.TunnelEndpoint` 非空时，Tunnel 请求走独立 `OdpsRestClient`（指向 TunnelEndpoint）

### 未做（明确推迟）

- **zlib raw deflate 压缩传输**：当前不主动请求压缩（不带 `Accept-Encoding`），服务端通常不压缩；若真实集群强制压缩才会暴露。留待联调时按需补 `CompressOption` 解压层。

## 测试

- `ConnectionStringHintsTest`：Hints 解析往返、ToConfig 透传、空值、容错、ToString 回写
- `CommandHintsOverrideTest`（用 `RecordingExecutor` 捕获 config）：命令级覆盖、null 透传、空字典不触发合并
- `StsAccountTest`：sts token 头注入、空 token 拒绝、Inner 暴露、跨请求 token 稳定

全量 **183** 测试通过（+12）。

## 关键设计点

1. **PyODPS STS 头确认**：源码 `accounts.py:504` 用 `authorization-sts-token`（计划文档原写 `x-odps-security-token` 是笔误），按源码实现。
2. **Hints 合并零拷贝**：`Hints` 为 null 或空时不构造新 config，直接复用原引用——避免热路径上无谓分配。
3. **TunnelEndpoint 独立 client**：不与 REST client 共用，因为两者 base URL 不同；按需在 `_preferTunnel` 分支内构造。
4. **连接字符串 Hints 解析容错**：`  a = 1 , ,badnoeq, b = 2 ` 这类脏输入只丢无效项，不抛错。

## 已知缺口

- 压缩传输未实现（见上）
- 全部代码待真实集群联调验证

## 测试结果

`dotnet test` 全绿：183 个测试通过。
