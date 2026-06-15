# 任务清单（TASK）

> 本文件只维护当前活跃任务与少量最近完成任务的最小记录。
> 当任务总数超过 30 条时，应将最早完成的 `DONE` 任务迁移到 `TaskHistory.md`，直到当前文件任务数回落到 20 条以内。
> 详细说明、过程记录与变更背景统一写入 `doc/devlog/`，规则解释与归档阈值仍以 `AGENTS.md` 为准。

---

## 任务总表

| 任务 ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 任务状态 | 优先级 | 最近更新时间 |
| ------- | -------- | -------- | -------- | --------- | -------- | ------ | ------------ |
| T009 | 核查 Azrng.AspNetCore.Core 代码审查 | 核查 `Azrng.AspNetCore.Core-review-2026-05-26.md` 中未提交审查意见的合理性，修复成立问题并补充验证与开发记录 | 收口 | Codex | REVIEW | P1 | 2026-05-27 |
| T010 | 修复 Azrng.NTika 代码审查问题 | 根据 `Azrng.NTika-review-2026-05-29.md` 修复 NTika 安全与解析正确性问题，并补充测试、验证与开发记录 | 收口 | Codex | REVIEW | P1 | 2026-05-29 12:57 |
| T011 | 修复 Azrng.JSqlParser 构建警告 | 核查 `Azrng.JSqlParser` 类库构建警告，修复可低风险处理的问题并补充验证与开发记录 | 收口 | Codex | REVIEW | P1 | 2026-06-01 10:26 |
| T008 | 修复 Common.HttpClients 日志脱敏结构问题 | 将 HTTP 日志脱敏抽象为可替换策略，修复默认脱敏破坏 JSON 格式的问题，并补充测试、文档与开发记录 | 收口 | Codex | REVIEW | P1 | 2026-05-25 15:25 |
| T007 | 修复 LocalLogHelper 日志可靠性问题 | 修复 `Azrng.Core` 本地日志写入的并发保护、显式 flush、错误日志目录与保留天数配置问题，并补充回归测试与开发记录 | 收口 | Codex | REVIEW | P1 | 2026-05-15 |
| T012 | 完善 Azrng.NmcWeather 文档与测试 | 生成 README、补充全量中文 XML 注释、补全集成测试（13 个） | 收口 | Codex | REVIEW | P1 | 2026-06-01 14:30 |
| T013 | 改进 Azrng.DataAccess 连接安全 | 根据连接安全建议核查改动合理性，统一 DataSourceConfig 连接字符串构造并补充脱敏能力与测试 | 收口 | Codex | REVIEW | P1 | 2026-06-02 |
| T014 | 优化 Azrng.Core 结果与异常工具 | 根据结果包装与异常体系建议新增通用 ResultModel 工厂、扩展和异常转换能力并补充测试 | 收口 | Codex | REVIEW | P1 | 2026-06-02 |
| T015 | 修复 Snowflake 短 ID 问题 | 修复 `Azrng.Core.Helpers.Snowflake.NewId()` 可能返回非雪花格式短 ID 的问题，收敛公开 API 并补充时间戳位校验测试与开发记录 | 收口 | Codex | REVIEW | P1 | 2026-06-15 |
| T006 | 新增 Azrng.NmcWeather 天气包 | 在 `src/Shared` 下新增 `Azrng.NmcWeather`，基于 `Common.HttpClients` 封装中央气象台省份、城市与天气查询能力，并接入 `ThirdNugetStudy.slnx` 与基础测试 | 收口 | Codex | DONE | P1 | 2026-06-01 14:30 |
| T005 | 统一 Shared 类库 XML 文档输出配置 | 批量补齐 src/Shared 下类库项目的 XML 文档生成配置，并统一使用按目标框架区分的输出文件格式，避免并行构建写入冲突 | 收口 | Codex | REVIEW | P1 | 2026-04-15 17:31 |
| T004 | 修复待提交内容乱码 | 排查当前待提交改动中的乱码内容并修复文件编码或文本异常，确保提交内容可正常评审 | 收口 | Codex | DONE | P1 | 2026-04-15 |
| T003 | 清理根目录误生成依赖包 | 排查并删除误生成到仓库根目录的 NuGet 依赖包目录，避免污染项目结构并补充开发记录 | 收口 | Codex | DONE | P1 | 2026-04-15 |
| T002 | 修复 DevLogDashboard 核心缺陷 | 继续核查并修复 DevLogDashboard 剩余安全、时间解析、队列丢弃可观测性、服务注册重复与测试生命周期问题，并补充验证与开发记录 | 开发 | Codex | REVIEW | P1 | 2026-04-15 17:01 |
| T001 | 维护仓库规范文件 | 统一仓库规范文件结构，明确协作规则、任务机制与文档目录约定 | 治理 | 通用 | DONE | P1 | 2026-04-07 |

---
