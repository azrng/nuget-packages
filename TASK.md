# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| _无活跃任务_ | | | | | | |

> 当前无活跃任务。下一个任务待用户指定。

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（77 子项全部处理闭环，全量 745 测试通过） | DONE | 2026-07-06 |
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |

### T075 归档说明

- 目标仓库：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- 上游对照：`C:\Work\SourceCode\sqlparser\JSqlParser`，范围 commit `7d2e6b65`(5.4) → `2b141568`(HEAD)
- 完成情况：77 个子项全部处理（迁移实现 + 评估结论），全量 745 测试通过（净增 197）。逐项实现记录见 `git log --grep="T075"`。
- 真实功能迁移（28 项）：Oracle 外连接(+)、ALTER USING INDEX、MySQL 索引 ASC/DESC、CREATE SCHEMA、CONNECT_BY_ROOT、SessionStatement options 等
- 评估不适用/已支持（19 项）：Azrng 架构天然规避或前序子项已实现
- 方言专项暂不迁移（7 项，子项 69-77 除 72/73）：ClickHouse/DuckDB CREATE TABLE/Trino/Snowflake/Databricks/BigQuery，保留按需取用，需具体业务场景驱动时再单独迁移
- 顺带修复的既有缺陷：ALTER alterOperation 未解析、CREATE TABLE 约束未输出、VisitPrimaryExpr 遗漏 connectBy 分派
