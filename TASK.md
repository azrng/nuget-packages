# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 当前活跃

| ID | 任务名称 | 目标 | 阶段 | 状态 | 更新时间 |
|----|----------|------|------|------|----------|
| T058 | Azrng.AspNetCore.DbEnvConfig 代码审查问题修复 | 修复审查报告中的必须项(Dispose 模式缺陷/后台线程不可及时停止/InitTable 静默吞异常/SQL 注入文档警告)与建议项(DB→Db 命名统一、ParamVerify→Normalize、DefaultScriptService→PostgreSqlScriptService、移除冗余 System.Data.Common/System.Text.Json 引用),补充 Dispose 停止后台线程/DbException 回滚/OnReload 仅变化时触发 单测,同步示例项目/README/ARCHITECTURE,版本 1.2.0→2.0.0(破坏性)。本轮继续修复 DbConfigurationSource 复用已释放 Provider 与 Dispose 超时释放锁并发边界,补充重复 Build/慢查询 Dispose 回归测试,完善 README 生命周期与安全说明。Release 全 TFM(net6/7/8/9/10)0 警告 0 错误,测试 net8/9 各 11/11 通过 | 阶段 1(实现+验证完成,待确认) | REVIEW | 2026-06-26 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T045 | 流式/分块写 WriteRowsChunkedAsync（BufferedRecordWriter.Batch 自动分块）— P1 部分 | DONE | 2026-06-21 |
| T044 | TableTunnel 表级下载（TableDownloadSession + CreateDownloadSessionAsync，复用 TunnelRecordReader/分片/时区）— P1 | DONE | 2026-06-21 |
| T043 | 多批次分页读 BufferedRecordReader（按 sliceSize 分片 reopen + IAsyncEnumerable 流式）— P0 | DONE | 2026-06-21 |
| T042 | datetime/timestamp 时区开关（UseLocalTimeZone，对齐 PyODPS local_timezone）— P0 | DONE | 2026-06-21 |
| T040 | TunnelRecordReader 补 count 校验（zigzag）+ 回归单测，防止 writer count 编码回归 | DONE | 2026-06-21 |




