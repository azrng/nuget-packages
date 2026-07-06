# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（逐项迁移 + 单元测试验证） | 逐步迁移上游 JSqlParser commit 7d2e6b65(5.4)..2b141568(HEAD) 的高价值功能与 Bug 修复，每项独立验证独立提交 | 阶段 1 后端实现 | DOING | high | 2026-07-06 |

### T075 概况

- 目标仓库：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- 上游对照：`C:\Work\SourceCode\sqlparser\JSqlParser`，范围 commit `7d2e6b65`(5.4) → `2b141568`(HEAD)
- 已完成：子项 1-60（全量 726 测试通过，净增 178）。逐项实现记录见 `git log --grep="T075"` 与本文件历史版本。
- 下一步：按优先级处理剩余通用功能/修复（子项 57-77），方言专项按需取用。
- 阻塞项：无

### T075 剩余子项清单

> 每条独立迁移或评估，完成即提交。优先级：high > medium > low；方言专项默认 low，按需取用。

#### 通用功能/修复 — medium 优先级

| 子项 | commit | 内容 | 状态 |
|------|--------|------|------|
| 57 | 49958b6b | avoid visiting twice — visitor 重复访问修复（评估：Azrng Adapter 空实现 + HashSet 天然规避，补 1 回归测试） | DONE |
| 58 | eeb04004 | avoid NPE and expose modifier（评估：Azrng 用 bool 属性替代 string modifier，无 NPE 风险，补 1 回归测试） | DONE |
| 59 | 834afe18 | Oracle outer join nvl/coalesce — Column 加 OldOracleJoinSyntax + 文法 columnRef(+) + ToString + 4 测试 | DONE |
| 60 | c7b3bdbd | ALTER TABLE USING INDEX clause — 文法 usingIndexClause + Constraint/AlterExpression 加 UsingIndex 字段 + 修复 ALTER alterOperation 未解析与 CREATE TABLE 约束未输出两处既有缺陷 + 4 测试 | DONE |
| 61 | 763e92d7 | alter table index descending — 索引 DESCENDING | TODO |
| 62 | ac46c434 | CREATE SCHEMA with catalog — CREATE SCHEMA 带目录限定 | TODO |
| 63 | 624a768b | Oracle hierarchical queries — CONNECT BY 中操作符接受 Expression | TODO |
| 64 | 7c52e7fe | legacy Postgres named parameter — 兼容旧式命名参数 | TODO |

#### 通用功能/修复 — low 优先级

| 子项 | commit | 内容 | 状态 |
|------|--------|------|------|
| 65 | 74607624 | Exasol IMPORT/EXPORT — Exasol 导入导出语句 | TODO |
| 66 | c60ff739 | normalised backtick quotes — 反引号标识符规范化输出 | TODO |
| 67 | 6c98f10f | SessionStatement with options — Session 语句带选项 | TODO |
| 68 | 528dd722 | array<double> 函数声明（需评估，Azrng 无 CREATE FUNCTION 文法） | TODO |

#### 方言专项（按需取用）

| 子项 | commit | 内容 | 状态 |
|------|--------|------|------|
| 69 | a34db0ce | ClickHouse SELECT SETTINGS | TODO |
| 70 | 64542c86 | ClickHouse parametric aggregate | TODO |
| 71 | 0e1715e9 | DuckDB CREATE TABLE STRUCT | TODO |
| 72 | 297ef846 | DuckDB STRUCT 数据类型 | TODO |
| 73 | aaebe591 | DuckDB STRUCT 数据类型（解析修复） | TODO |
| 74 | 6ce95d54 | Trino UDF | TODO |
| 75 | 6f4c4fb2 | Snowflake time travel | TODO |
| 76 | df5e6690 | Databricks Temporal spec | TODO |
| 77 | 5fa071ef | BigQuery Historic Version | TODO |

#### 已评估为不适用（JavaCC 特定机制，ANTLR 版无对应项，不再跟踪）

cf5bbc9a、59dfc3b0、08d0bcc9、c5b85abf、93515149、6049fd72、ac175138、7d42ff61、fe860ddd、fff8a081、0f9e4779、5b5fe6c2（注：这些已在前序子项中评估或在 ANTLR 版天然规避，详见 git history）

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |
| T070 | Common.Cache.Redis 审查建议清理（删除死代码 + 清理残留注释 + 简化 SCAN 异常分支 + 补充行为说明） | DONE | 2026-07-03 |
