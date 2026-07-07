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
| T078 | Azrng.JSqlParser 修复嵌套块注释词法（对齐上游 /* /* */ */ 嵌套支持，全量 845 测试通过） | DONE | 2026-07-07 |
| T077 | Azrng.JSqlParser 客户反馈问题核查与修复（CASE WHEN 序列化 Bug + 6 项现状固化测试，全量 830 测试通过） | DONE | 2026-07-07 |
| T076 | Azrng.JSqlParser 继续迁移上游 5.4-SNAPSHOT 剩余缺口 F1-F8（8 项特性 + 全量 799 测试通过，净增 54） | DONE | 2026-07-07 |
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（77 子项全部处理闭环，全量 745 测试通过） | DONE | 2026-07-06 |
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |

### T078 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：T077 核查注释对齐时发现嵌套块注释缺口——上游用 `commentNesting` 计数器 + lexer 状态机支持嵌套（`/* 外 /* 内 */ 外 */`），Azrng 词法规则 `BLOCK_COMMENT: '/*' .*? '*/'` 非贪婪，遇第一个 `*/` 即结束，剩余文本导致 `JSqlParserException`
- **完成情况**：改用 ANTLR4 lexer 模式（`IN_BLOCK_COMMENT`）+ `commentNesting` 深度计数器替代非贪婪规则，模拟上游 DEFAULT ↔ IN_BLOCK_COMMENT 状态机。全量 845 测试通过（830 → 845，净增 15）。
- **方案演进**：尝试过「实例字段 + 循环内语义谓词」单规则方案，但 ANTLR4 词法预测器对成员变量谓词不稳定（flat/嵌套用例都出错）；最终改用 MORE + lexer 模式切换，state 机清晰可靠。关键约束：`mode` 声明会使其后所有规则归属该模式，因此 `IN_BLOCK_COMMENT` 段必须放在文件最后。
- **对齐情况（对齐上游 NestedCommentTest 全部语义）**：
  - 扁平/一层/两层嵌套、WHERE 子句内嵌套、注释内含 `*`/`/`/`--`/`//`、空嵌套、多注释连续、多行嵌套：全部解析通过
  - `/*+ ... */` Oracle Hint：由 ORACLE_HINT_ML 规则保留（不走嵌套路径），与上游一致
  - 未闭合 `/*`：仍抛错（含嵌套未闭合），与上游一致
  - round-trip 丢弃注释：与上游一致（上游 toString 也不保留注释）
- **阻塞项**：无
- **未做的事**：不实现注释保留（上游也不保留，属新特性）；不改 parser grammar

### T077 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：逐项核查客户迁移反馈的 8 个问题，真 Bug 修复，非 Bug 项补测试固化现状
- **完成情况**：8 项反馈逐项核查，1 项真 Bug 修复（CASE WHEN），6 项补 round-trip / AST 测试固化现状，1 项（#1 格式化）按用户明确指示忽略。全量 830 测试通过（799 → 830，净增 31）。
- **核查结论（8 项）**：
  - #1 `!=`/`||` 格式化（全角/空格）：用户已明确为格式化问题，**忽略**
  - #3 CASE WHEN：**真 Bug 已修复**。searched 形式 `CASE WHEN a>1 THEN 'big' ELSE 'small' END` round-trip 错成 `CASE 'small' WHEN ... END`。根因 `AstBuilderVisitor.VisitCaseExpr` 误用 `context.expression()`（ANTLR 递归收集 whenExpr/ELSE 内嵌表达式）+ `GetChild<ExpressionContext>(0)` 在 searched 形式下返回 ELSE 表达式。改为判断 `child[1]`（CASE 后首个直接子节点）类型。
  - #4 `--` 行注释：实测不抛错，与 `/* */` 一致地被 lexer `-> skip` 丢弃，**与上游一致**，补测试固化
  - #5 NULL AS 字段名：round-trip 正常，补测试固化
  - #6 `'0' || 字段` 拼接：round-trip 正常（Concat 节点），补测试固化
  - #7 UNION ALL：round-trip 正常（SetOperationList），补测试固化
  - #8 `a.qty::varchar(20)`：round-trip 正常（CastExpression UseCastKeyword=false），补测试固化
- **新增测试**：`CaseExpressionTest`（10）、`CustomerReportedRegressionTest`（21），覆盖 searched/switch/嵌套 CASE、-- 注释、/* */、NULL AS、||、UNION ALL/UNION、:: 与 CAST、客户综合场景
- **阻塞项**：无
- **未做的事**：不实现注释保留（上游也不保留，属新特性）；不改 `NotEqualsTo` 的 `<>` 标准化（#1 是格式化问题）；不改 lexer/grammar

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
