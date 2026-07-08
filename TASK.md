# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| _无活跃任务_ | | | | | | |

> 当前无活跃任务。下一个任务待用户指定。

## 待业务驱动 Backlog

> 以下事项已识别为未做或差异，当前无业务需求驱动，**不进入活跃任务**。出现具体业务场景时按触发条件择项启动，新建对应 `T-` 编号任务并引用本表 `BL-` 编号。

> 全量对比已完成（见 BL-15 对齐说明），上游 HEAD `2b141568`（5.4-SNAPSHOT，2026-04-12）无新提交。下表为识别出的缺口，按优先级分组。`BL-07/08/09` 静默丢弃缺陷已由 T080 修复并移出本表。

### P1 真缺口（经验证确认，非架构差异）

| BL 编号 | 待办 | 类别 | 出处 | 触发条件 | 备注 |
|---------|------|------|------|----------|------|
| BL-10 | `LATERAL` 子查询（FROM `LATERAL (subq)`） | 半成品 | BL-15 全量对比 | 业务需 LATERAL JOIN | grammar `g4:151` 接受但 AST 退化为普通子查询，LATERAL 标志丢失；需新增标志字段或 `LateralSubSelect` 类 |
| BL-11 | `DATE`/`TIMESTAMP`/`TIME` 类型前缀字面量（`DATE '2024-01-01'`） | 功能缺口 | BL-15 全量对比 | 业务需 SQL 标准日期字面量 | `DateValue`/`TimestampValue`/`TimeValue.cs` 是死代码（零实例化），`literal` 规则不含类型前缀分支 |
| BL-12 | 上游缺失的语句类型（按需迁移） | 语句缺口 | BL-15 全量对比 | 业务需其中任一语句 | 涵盖：`COMMENT ON`、独立 `ANALYZE`、`RENAME TABLE` 顶层语句、`ALTER VIEW`、`ALTER SESSION/SYSTEM`、`CREATE SYNONYM`、`CREATE FUNCTION/PROCEDURE`、`EXECUTE/CALL` 过程调用、`PURGE`、`RESET`、`SHOW COLUMNS/INDEX/TABLES` 子类、`BLOCK`/`DECLARE`/`IF`（PL/SQL）。详见 BL-15 对齐记录 |
| BL-13 | 上游缺失的 Select/Schema 子特性（已完成） | 特性缺口 | BL-15 全量对比 | — | **已完成（T081 批次）**。逐项实现：FROM 子句 `PIVOT/UNPIVOT`、`TABLESAMPLE`、通用表函数作 FromItem、`OPTIMIZE FOR n ROWS`、Informix `FIRST`/`SKIP`、Join 多 ON、ClickHouse `GLOBAL/ANY/ALL`（含 STRAIGHT_JOIN）、Table 时间旅行（Snowflake AT/BEFORE，已接线）、`Table` 多部分名/Pivot/SampleClause/TimeTravel 字段、`SAFE_CAST`、`SIMILAR TO`、`NumericBind`。**未做**：`PIVOT XML` 变体（边缘方言，PIVOT/UNPIVOT 核心已覆盖，按需再补） |
| BL-14 | `ALTER` 操作覆盖度（已完成 round-trip 收口） | 特性缺口 | BL-15 全量对比 | — | **已完成（T081 批次 A）**。经 Explore 逐值核对：`AlterOperation` 枚举已 **47/47 全量对齐**上游（原 backlog "11/47" 记载已过时）；修复 grammar 已接受但 AST 静默丢失语义的 14 处 round-trip 缺陷（DROP PRIMARY/UNIQUE/FOREIGN KEY/CONSTRAINT、RENAME INDEX/KEY/CONSTRAINT、ENGINE/COMMENT 等号、分区操作族 ADD/DROP/TRUNCATE/COALESCE/REORGANIZE/EXCHANGE PARTITION 填充结构化字段、ALTER SEQUENCE 复用 SequenceParameter）。**澄清**：上游不存在 `ALTER INDEX`/`ALTER SCHEMA` 语句类（用 UnsupportedStatement 兜底），原 backlog 该项非对齐缺口；`ALTER VIEW` 已在 BL-12 完成，`ALTER SEQUENCE` 已在本批次接线 |

### P2 已知差异（保持现状，按需启动）

| BL 编号 | 待办 | 类别 | 出处 | 触发条件 | 备注 |
|---------|------|------|------|----------|------|
| BL-01 | JSON_QUERY Legacy 额外 path 参数（`additionalQueryPathArguments`） | 功能补全 | T076 | 业务需解析 MySQL/Oracle legacy `JSON_QUERY(expr, path1, path2...)` 多 path 参数 | 上游用 `additionalQueryPathArguments` 字段承载额外 path，Azrng 当前未实现 |
| BL-02 | JSON_TABLE Oracle/Trino 方言子句（PLAN/WRAPPER/QUOTES/SCALARS/ON EMPTY） | 方言补全 | T076 | 业务需解析 Oracle/Trino 的 JSON_TABLE 方言高级子句 | T076 已迁 PASSING/ON ERROR/NESTED PATH，剩余 PLAN/WRAPPER/QUOTES/SCALARS/ON EMPTY 待补 |
| BL-03 | 聚合函数 OVER 窗口子句 | 架构差异 | T076 | 业务需 `SUM(x) OVER(...)` 等聚合函数直接挂 OVER 子句 | Azrng OVER 走 `AnalyticExpression` 独立路径，需另设计 Function ↔ 窗口接线，影响范围较大 |
| BL-04 | 上游 `MYSQL_OBJECT` 类型（OBJECTAGG 逗号分隔输出） | 输出差异 | T076 F7 | 业务需 OBJECTAGG 按逗号分隔输出（与上游一致） | 当前按冒号分隔输出；上游 `MYSQL_OBJECT` 用逗号，需新增类型或开关 |
| BL-05 | JSON_OBJECT/OBJECTAGG 冒号分隔 lexer 歧义 | 词法限制 | T076 F7 | 业务需支持无空格 `:bar` 形式的键值对 | ANTLR 无上下文 lexer 与上游 JavaCC LOOKAHEAD 本质差异：无空格 `:bar` 会被识别为命名参数，需 lexer 层改造 |
| BL-06 | 方言专项 CREATE TABLE 等方言特性（ClickHouse/DuckDB/Trino/Snowflake/Databricks/BigQuery） | 方言补全 | T075（子项 69-77 除 72/73） | 业务出现上述方言的 CREATE TABLE 或专属语法场景 | 工作量最大，建议按出现的具体方言逐项迁移，不一次性铺开 |

### BL-15 对齐基线说明

- **对比时间**：2026-07-07
- **上游仓库**：`C:/Work/SourceCode/sqlparser/JSqlParser`
- **上游对照点**：commit `2b141568`（5.4-SNAPSHOT，2026-04-12），`feat: add ForUpdateClause class with multi-table and ORDER BY support (#2426)`
- **上游无新提交**：`2b141568` 即上游 HEAD，无后续 commit 需追赶
- **对比维度**：Expression 类、Statement 类、Select/Schema 子类、grammar 关键字、ALTER 操作
- **已验证为等价实现（不计入缺口）**：`ArrayExpression`（合入 ArrayConstructor.cs）、`MySQLGroupConcat`（合入 Function.cs）、`UserVariable`（合入 JdbcNamedParameter + S_AT_IDENTIFIER）、`VariableAssignment`（由 SetStatement 承载）、`AllValue`（合入 AnyType.All 枚举）、`ON CONFLICT`（由 Insert.cs + InsertConflictAction/Target 承载）、`ForUpdateClause`（已完整移植）、命名参数三合一（Oracle/Postgres/统一 NamedFunctionParameter）
- **上游本身不支持（非 Azrng 缺口）**：`REVOKE`、`ROLE`、`VACUUM`、`FLASHBACK`、`ATTACH/DETACH`（SQLite）、`BULK COLLECT/FORALL`、`TIMESTAMPLTZ`

### 测试规模差异说明（非缺陷，仅供参考）

- **Azrng**：845 测试
- **上游 JSqlParser**：2309 测试
- **差距来源**：主要来自方言专项测试（ClickHouse/Snowflake/BigQuery 等），这些是 Azrng 明确不在迁移范围内的（对应 BL-06）
- **核心 SQL 路径**：Azrng 测试独立设计，非上游测试逐条移植

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T082 | EFCore Provider 日志工厂复用修复（四个关系型 provider 复用宿主 ILoggerFactory，移除包内 ConsoleLoggerProvider 重复创建；Postgres 升至 1.7.1，MySQL/SQLServer/SQLite 升至 1.6.2；日志配置文档已补充，新增 2 项回归测试） | DONE | 2026-07-08 |
| T081 | Azrng.JSqlParser BL-13/BL-14 收口（ClickHouse JOIN GLOBAL/ANY/ALL + Snowflake 时间旅行接线 + ALTER round-trip 14 处缺陷修复，全量 1006 测试通过，净增 48） | DONE | 2026-07-08 |
| T080 | Azrng.JSqlParser 修复 3 个静默丢弃缺陷（OVERLAPS / MEMBER OF / SELECT TOP，全量 861 测试通过，净增 16） | DONE | 2026-07-07 |
| T079 | Azrng.AspNetCore.Job.Quartz 代码审查 P0 缺陷修复（scope 泄漏/监听器未注册/时区/暂停/扫描割裂 + 历史清理，全量 25 测试通过） | DONE | 2026-07-07 |
| T078 | Azrng.JSqlParser 修复嵌套块注释词法（对齐上游 /* /* */ */ 嵌套支持，全量 845 测试通过） | DONE | 2026-07-07 |

### T082 归档说明

- **目标仓库**：`src/Shared/Common.EFCore.PostgresSql`、`src/Shared/Common.EFCore.MySQL`、`src/Shared/Common.EFCore.SQLServer`、`src/Shared/Common.EFCore.SQLite`
- **任务目标**：修复四个关系型 EFCore provider 在 `DbContextOptions` 创建时重复 `LoggerFactory.Create(...).AddConsole()` 导致 `ConsoleLoggerProvider` 资源累积的风险，并补充宿主应用 SQL 日志配置文档
- **完成情况**：四个 provider 的 `AddEntityFramework<T>` / `AddEntityFrameworkFactory<T>` 均改为复用宿主 DI 中的 `ILoggerFactory`；包内部不再强制创建 ConsoleLoggerProvider；Postgres 版本升至 `1.7.1`，MySQL/SQLServer/SQLite 版本升至 `1.6.2`；四个 README 补充 `builder.Logging` 和 `appsettings.json` 开启 SQL 日志示例
- **验证**：串行构建四个 provider 通过并生成新版本 nupkg；`ProviderLoggingTests` 2 项通过，验证 `AddEntityFramework` 与 `AddEntityFrameworkFactory` 均复用宿主 `ILoggerFactory`
- **未覆盖项**：完整 PostgreSQL 集成测试未通过，原因是本机 `127.0.0.1:5432` 未启动 PostgreSQL，26 个既有依赖真实数据库的测试连接被拒绝；与本次日志工厂修复无关
- **风险**：默认行为从“包内强制输出 SQL 控制台日志”调整为“由宿主应用 Logging 配置控制”，需要应用按 README 示例显式开启 SQL 日志
- **阻塞项**：无

### T081 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：BL-13/BL-14 剩余项收口（Select 子特性 + ALTER 覆盖度）
- **完成情况**：3 批次独立提交，全量 1006 测试通过（958 → 1006，净增 48）
  - **批次 A（BL-14 round-trip 收口）**：Explore 逐值核对 `AlterOperation` 枚举已 47/47 全量对齐上游（原 backlog "11/47" 过时）；修复 14 处 grammar 已接受但 AST 静默丢失语义的缺陷——DROP PRIMARY/UNIQUE/FOREIGN KEY/CONSTRAINT、RENAME INDEX/KEY/CONSTRAINT、ENGINE/COMMENT（含等号）、分区操作族（ADD/DROP/TRUNCATE/COALESCE/REORGANIZE/EXCHANGE PARTITION 填充结构化字段）；ALTER SEQUENCE 重构为复用 `SequenceParameter`；`partitionDef` 增加可选 PARTITION 前缀支持标准 MySQL 形式。新增 `AlterOperationRoundTripTest`（30 项）
  - **批次 B（BL-13 ClickHouse JOIN）**：Join 新增 Global/Any/All 字段对齐上游；grammar joinClause 增加 `GLOBAL? (ANY|ALL)?` 前缀；从 identifier 兜底列表移除 GLOBAL（消除 alias 贪婪吞掉 GLOBAL 的歧义，与上游保留字一致）。新增 `ClickHouseJoinModifierTest`（11 项）
  - **批次 C（BL-13 Snowflake 时间旅行）**：grammar 新增 `timeTravelClause` 产生式并接线 visitor 填 `Table.TimeTravel`；修复 `TimeTravelClause.ToString` 缺括号缺陷（AT (TIMESTAMP => expr) 标准形式）。新增 `TimeTravelTest`（7 项）
- **阻塞项**：无
- **未做的事**：`PIVOT XML` 变体（边缘方言，PIVOT/UNPIVOT 核心已覆盖，按需再补）；`ALTER INDEX`/`ALTER SCHEMA` 语句类（上游无此建模，用 UnsupportedStatement 兜底，属 Azrng 净新功能非对齐缺口）；Snowflake time-travel 与 alias 同时出现时的排序细节（grammar 解析 alias 在前，round-trip 一致，真实 Snowflake alias 在后属边缘场景）

### T080 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：修复 BL-15 全量对比发现的 3 个 P0 静默丢弃缺陷（grammar 已接受但 `AstBuilderVisitor` 漏接线，导致 round-trip 丢语义）
- **完成情况**：3 项全部修复，新增 2 个测试文件（`OverlapsAndMemberOfTest` 7 项 + `SelectTopTest` 9 项），全量 861 测试通过（845 → 861，净增 16）
- **逐项修复**：
  - **BL-08 MEMBER OF**：`MemberOfExpression.cs` 加 `Not` 字段、`ToString` 去错误括号对齐上游 `MEMBER OF expr` 格式；grammar `g4:773` 加 `NOT?`；`VisitPredicateSuffix` 新增 MEMBER 分支
  - **BL-07 OVERLAPS**：新建 `OverlapsCondition.cs`（左右各一 `ExpressionList`，对齐上游）；`ExpressionVisitor` + `ExpressionVisitorAdapter` + `TablesNamesFinder` 补 Visit 方法；`VisitPredicateSuffix` 新增 OVERLAPS 分支
  - **BL-09 SELECT TOP**：新建 `Top.cs`（`HasParenthesis`/`IsPercentage`/`IsWithTies`/`Expression` 4 字段，对齐上游 `Top.java`）；`PlainSelect` 加 `Top` 字段并在 `AppendSelectBodyTo` 序列化；`VisitPlainSelect` 读取 `context.topClause()`；lexer/grammar 已具备无需改
- **阻塞项**：无
- **未做的事**：OVERLAPS 多元素列表形式 `(a,b) OVERLAPS (c,d)`（需改 grammar 支持括号列表，当前仅支持单元素两侧）；BL-10~14 其它 P1 真缺口（后续批次）

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
