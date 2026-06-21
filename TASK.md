# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 当前活跃

| ID | 任务名称 | 目标 | 阶段 | 状态 | 更新时间 |
|----|----------|------|------|------|----------|
| T041 | Arrow timestamp(ns) struct wire schema + 转换 | 服务端把 timestamp(ns) 按 struct(sec:int64,nano:int32) 发送，原 TimestampType 前置导致 batch index out of range。改 wire schema 按 struct 声明 + reader 读出后转回 TimestampArray（对齐 PyODPS _convert_struct_timestamps），公共 schema 仍为 TimestampType | 阶段 1（实现+离线单测完成，集群 e2e 待用户凭据验证） | REVIEW | 2026-06-21 |
| T022 | Azrng.Security 合并与改名 | 将 Common.Security 全层统一改名为 Azrng.Security，吸收 Common.SecurityCrypto 独有能力（RSA JSON、RandomString），丢弃其 Provider/Factory 抽象与手写 SM 实现 | 阶段 1（9 任务全部完成，待用户确认） | REVIEW | 2026-06-20 |
| T042 | Common.HttpClients.Next 补充 Apifox Echo 集成测试 | 参考 DevLogDashboard.Test 的 Startup（Xunit.DependencyInjection）写法，对 https://echo.apifox.com 补充 IHttpHelper 集成测试，已覆盖 IHttpHelper 全部 17 个成员（GET/POST/PUT/PATCH/DELETE 回显、Query/JSON/Form/文件上传 multipart/Soap、自定义 Header、GetStreamAsync、SendAsync 枚举与原始、DownloadFileAsync 下载 PNG）+ /delay 超时（Fail 降级 503 / FailThrow 抛 TimeoutRejectedException） | 阶段 1（实现完成，全量 164/164 通过，含 22 个集成测试） | REVIEW | 2026-06-21 |
| T043 | Azrng.DuckDB.Quack benchmark 反馈优化 | DuckDBQuackCompareBenchmarks 切本地项目引用、扩 smoke correctness、补 reader/pool lease/batchSize benchmark；smoke 发现并修复 Quack DATE 解码 +1 天 bug，补 DateOnly roundtrip | 阶段 1（实现+验证完成） | DONE | 2026-06-21 |
| T046 | Azrng.DuckDB.Quack 大结果集 Fetch 终止修复 | 修复 Quack result_uuid 非规范 LEB128 编码被重编码导致 benchmark 100k Fetch 报 Result has been closed 的问题，并补无 Catalog 大结果集回归验证 | 阶段 1（实现+验证完成） | DONE | 2026-06-21 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T045 | 流式/分块写 WriteRowsChunkedAsync（BufferedRecordWriter.Batch 自动分块）— P1 部分 | DONE | 2026-06-21 |
| T044 | TableTunnel 表级下载（TableDownloadSession + CreateDownloadSessionAsync，复用 TunnelRecordReader/分片/时区）— P1 | DONE | 2026-06-21 |
| T043 | 多批次分页读 BufferedRecordReader（按 sliceSize 分片 reopen + IAsyncEnumerable 流式）— P0 | DONE | 2026-06-21 |
| T042 | datetime/timestamp 时区开关（UseLocalTimeZone，对齐 PyODPS local_timezone）— P0 | DONE | 2026-06-21 |
| T040 | TunnelRecordReader 补 count 校验（zigzag）+ 回归单测，防止 writer count 编码回归 | DONE | 2026-06-21 |




