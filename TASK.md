# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| _无活跃任务_ | | | | | | |

> 当前无活跃任务。下一个任务待用户指定。

## 待业务驱动 Backlog

> 以下事项已识别为未做或差异，当前无业务需求驱动，**不进入活跃任务**。出现具体业务场景时按触发条件择项启动，新建对应 `T-` 编号任务并引用本表 `BL-` 编号。

| BL 编号 | 待办 | 类别 | 出处 | 触发条件 | 备注 |
|---------|------|------|------|----------|------|
| BL-01 | JSON_QUERY Legacy 额外 path 参数（`additionalQueryPathArguments`） | 功能补全 | T076 | 业务需解析 MySQL/Oracle legacy `JSON_QUERY(expr, path1, path2...)` 多 path 参数 | 上游用 `additionalQueryPathArguments` 字段承载额外 path，Azrng 当前未实现 |
| BL-02 | JSON_TABLE Oracle/Trino 方言子句（PLAN/WRAPPER/QUOTES/SCALARS/ON EMPTY） | 方言补全 | T076 | 业务需解析 Oracle/Trino 的 JSON_TABLE 方言高级子句 | T076 已迁 PASSING/ON ERROR/NESTED PATH，剩余 PLAN/WRAPPER/QUOTES/SCALARS/ON EMPTY 待补 |
| BL-03 | 聚合函数 OVER 窗口子句 | 架构差异 | T076 | 业务需 `SUM(x) OVER(...)` 等聚合函数直接挂 OVER 子句 | Azrng OVER 走 `AnalyticExpression` 独立路径，需另设计 Function ↔ 窗口接线，影响范围较大 |
| BL-04 | 上游 `MYSQL_OBJECT` 类型（OBJECTAGG 逗号分隔输出） | 输出差异 | T076 F7 | 业务需 OBJECTAGG 按逗号分隔输出（与上游一致） | 当前按冒号分隔输出；上游 `MYSQL_OBJECT` 用逗号，需新增类型或开关 |
| BL-05 | JSON_OBJECT/OBJECTAGG 冒号分隔 lexer 歧义 | 词法限制 | T076 F7 | 业务需支持无空格 `:bar` 形式的键值对 | ANTLR 无上下文 lexer 与上游 JavaCC LOOKAHEAD 本质差异：无空格 `:bar` 会被识别为命名参数，需 lexer 层改造 |
| BL-06 | 方言专项 CREATE TABLE 等方言特性（ClickHouse/DuckDB/Trino/Snowflake/Databricks/BigQuery） | 方言补全 | T075（子项 69-77 除 72/73） | 业务出现上述方言的 CREATE TABLE 或专属语法场景 | 工作量最大，建议按出现的具体方言逐项迁移，不一次性铺开 |

### 测试规模差异说明（非缺陷，仅供参考）

- **Azrng**：845 测试
- **上游 JSqlParser**：2309 测试
- **差距来源**：主要来自方言专项测试（ClickHouse/Snowflake/BigQuery 等），这些是 Azrng 明确不在迁移范围内的（对应 BL-06）
- **核心 SQL 路径**：Azrng 测试独立设计，非上游测试逐条移植

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T079 | Azrng.AspNetCore.Job.Quartz 代码审查 P0 缺陷修复（scope 泄漏/监听器未注册/时区/暂停/扫描割裂 + 历史清理，全量 25 测试通过） | DONE | 2026-07-07 |
| T078 | Azrng.JSqlParser 修复嵌套块注释词法（对齐上游 /* /* */ */ 嵌套支持，全量 845 测试通过） | DONE | 2026-07-07 |
| T077 | Azrng.JSqlParser 客户反馈问题核查与修复（CASE WHEN 序列化 Bug + 6 项现状固化测试，全量 830 测试通过） | DONE | 2026-07-07 |
| T076 | Azrng.JSqlParser 继续迁移上游 5.4-SNAPSHOT 剩余缺口 F1-F8（8 项特性 + 全量 799 测试通过，净增 54） | DONE | 2026-07-07 |
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（77 子项全部处理闭环，全量 745 测试通过） | DONE | 2026-07-06 |

### T079 归档说明

- **目标仓库**：`src/Shared/Azrng.AspNetCore.Job.Quartz`，测试：`test/Azrng.AspNetCore.Job.Quartz.Test`，示例：`src/Services/QuartzApi`
- **任务目标**：修复代码审查发现的调度主链路 5 个 P0 缺陷及必需配套
- **完成情况**：5 个 P0 全部修复（DI scope 泄漏 / QuartzJobListener 未注册 / Cron 时区 -8 偏移 / 暂停仅限 CronTrigger / DI 与调度扫描割裂），补 `JobHistoryCleanupHostedService` 周期清理、`QuartzOptions.SchedulerName`/`JobHistoryRetentionDays`、README/ARCHITECTURE 文档对齐；新增 3 个测试文件（DependencyInjectionJobFactoryTests / TriggerServiceTests / AssemblyResolverTests）+ 2 处回归用例，全量 25 测试通过（net8/net9），包与 QuartzApi 示例构建 0 警告 0 错误
- **阻塞项**：无
- **未做的事**：未删除 `ITriggerService`/`TriggerService`/`ScheduleViewModel`（作为 public API 保留，仅删除 `JobService` 内部未使用的私有 `GetTrigger` 死方法）；未引入 `explicitAssemblies` 参数与 entry 含 JobConfig 边缘场景的程序集共享（当前满足 DI 注册范围 ⊇ 调度范围，常见配置场景一致）

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
