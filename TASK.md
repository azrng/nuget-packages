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
| T076 | Azrng.JSqlParser 继续迁移上游 5.4-SNAPSHOT 剩余缺口 F1-F8（8 项特性 + 全量 799 测试通过，净增 54） | DONE | 2026-07-07 |
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（77 子项全部处理闭环，全量 745 测试通过） | DONE | 2026-07-06 |
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |

### T076 归档说明

- 目标仓库：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- 上游对照：`C:\Work\SourceCode\sqlparser\JSqlParser`，HEAD `2b141568`（5.4-SNAPSHOT）
- 完成情况：8 项特性全部迁移，11 次独立提交（F7 拆 4 批：OBJECT/ARRAY、VALUE/EXISTS、QUERY、OBJECTAGG/ARRAYAGG），全量 799 测试通过（745 → 799，净增 54）。逐项实现记录见 `git log --grep="T076"`。
- 迁移项（8 项）：
  - F1 GeometryDistance（`<->`/`<#>` PostGIS 距离算子）
  - F2 RangeExpression（`start : end` 数组范围）
  - F3 TimeKeyExpression（CURRENT_DATE/TIMESTAMP/TIMEZONE/LOCALTIME/LOCALTIMESTAMP）
  - F4 RawFunction（原样保留参数体的函数 API 对齐，上游未接文法）
  - F5 TranscodingFunction（CONVERT 双风格 + TRY_CONVERT/SAFE_CONVERT）
  - F6 INTO OUTFILE 格式化子句（CHARACTER SET/FIELDS/LINES 9 字段）
  - F7 JSON 表达式族（JsonFunction/JsonKeyValuePair/JsonFunctionExpression/JsonAggregateFunction 4 类 + 7 种函数 OBJECT/ARRAY/VALUE/EXISTS/QUERY/OBJECTAGG/ARRAYAGG）
  - F8 JSON_TABLE 高级子句（PASSING/ON ERROR/NESTED PATH）
- 已核实**无需迁移**：窗口函数族（PartitionByClause 等 5 类，Azrng 已扁平化覆盖且更强）、集合运算 op 类（UnionOp 等 4 类，不同建模功能等价，CORRESPONDING 处理更强）
- 暂未迁移（按需再补）：JSON_QUERY 的 Legacy 额外 path 参数、JSON_TABLE 的 PLAN/WRAPPER/QUOTES/SCALARS/ON EMPTY 等 Oracle/Trino 方言子句、聚合函数的 OVER 窗口子句

### T075 归档说明

- 目标仓库：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- 上游对照：`C:\Work\SourceCode\sqlparser\JSqlParser`，范围 commit `7d2e6b65`(5.4) → `2b141568`(HEAD)
- 完成情况：77 个子项全部处理（迁移实现 + 评估结论），全量 745 测试通过（净增 197）。逐项实现记录见 `git log --grep="T075"`。
- 真实功能迁移（28 项）：Oracle 外连接(+)、ALTER USING INDEX、MySQL 索引 ASC/DESC、CREATE SCHEMA、CONNECT_BY_ROOT、SessionStatement options 等
- 评估不适用/已支持（19 项）：Azrng 架构天然规避或前序子项已实现
- 方言专项暂不迁移（7 项，子项 69-77 除 72/73）：ClickHouse/DuckDB CREATE TABLE/Trino/Snowflake/Databricks/BigQuery，保留按需取用，需具体业务场景驱动时再单独迁移
- 顺带修复的既有缺陷：ALTER alterOperation 未解析、CREATE TABLE 约束未输出、VisitPrimaryExpr 遗漏 connectBy 分派
