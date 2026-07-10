# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| _无活跃任务_ | | | | | | |

> 当前无活跃任务。P0+P1 缺口已由 T091 修复完毕，BL-18 a/b/c(部分) 已完成（1118 测试通过）。BL-18c 剩余 ALTER 字段结构化与 BL-19 小众方言按需启动。

## 待业务驱动 Backlog

> 以下事项已识别为未做或差异，当前无业务需求驱动，**不进入活跃任务**。出现具体业务场景时按触发条件择项启动，新建对应 `T-` 编号任务并引用本表 `BL-` 编号。

> 全量对比已完成（见 BL-15 对齐说明），上游 HEAD `2b141568`（5.4-SNAPSHOT，2026-04-12）无新提交。BL-01~14 + BL-16/17 已完成（P0+P1 缺口）。**BL-18 部分字段扩展、BL-19 小众方言按需启动。**

### P1 真缺口（经验证确认，非架构差异）

| BL 编号 | 待办 | 类别 | 出处 | 触发条件 | 备注 |
|---------|------|------|------|----------|------|
| BL-16 | P0 静默丢弃：WINDOW 命名窗口 + QUALIFY 子句（已完成） | 静默丢弃缺陷 | T091 三维核查 | — | **已完成（T091）**。PlainSelect 新增 WindowDefinitions/Qualify 字段，visitor 接线 windowClause/qualifyClause |
| BL-17 | P1 功能缺口：GROUP BY ROLLUP/CUBE/GROUPING SETS、CONNECT BY/START WITH、SUBSTRING FROM-FOR/POSITION IN/OVERLAY、MSSQL OUTPUT、REFRESH MATERIALIZED VIEW、UPSERT/REPLACE（已完成） | 功能缺口 | T091 三维核查 | — | **已完成（T091）**。GROUP BY 三分支+GroupByElement 扩展、OracleHierarchicalExpression、NamedExpressionList+Function.NamedParameters、Insert.OutputClause、RefreshMaterializedViewStatement、UpsertStatement。**取消**：JSON_TRANSFORM/CURRVAL（核查确认上游 main 不支持，非缺口） |
| BL-18 | P1 部分：AnalyticType 字段、COMMENT ON 扩展对象、ALTER 5.4→HEAD +694 行字段、Table/Column/Sequence 字段扩展 | 部分缺口 | T091 三维核查 | — | **a/b 已完成**：COMMENT ON VIEW+COLUMN多段列名、AnalyticType 四态（OVER/WITHIN_GROUP/WITHIN_GROUP_OVER/FILTER_ONLY，破坏性：WITHIN GROUP/FILTER 返回类型 Function→AnalyticExpression）。**c 部分完成**：Column.CommentText/ArrayConstructor、Table.TimeTravelAfterAlias 已补。**c 剩余**：ALTER 字段结构化（Index/OldIndex/ColumnDropNotNull/ConvertType/ConstraintState 等，数量多需重构 visitor，当前透传字符串已能 round-trip 留后续） |
| BL-19 | P2 小众方言（按需启动，经核查无硬缺口） | 小众方言 | T091 三维核查 | 业务出现上述方言 | **经核查修正**：(1) Oracle MODEL 子句**上游根本不存在**（grammar 零命中），从缺口剔除；(2) 子 visitor 适配层是**架构差异非缺口**（Azrng 扁平 visitor 已覆盖，强行移植是负优化）；(3) EXPORT/IMPORT 是 Exasol 专用极小众（10类+174行grammar），投入产出比差；(4) KSQL 窗口是 ksqlDB 专用（改动可控，2类+65行grammar）。**仅 KSQL/EXPORT 在业务出现时按需启动** |

### 已完成 P1 真缺口（历史记录）

| BL 编号 | 待办 | 类别 | 出处 | 触发条件 | 备注 |
|---------|------|------|------|----------|------|
| BL-10 | `LATERAL` 子查询（已完成） | 特性缺口 | BL-15 全量对比 | — | **已完成（commit 6f753c3）**。新增 `LateralSubSelect` 类（继承 `ParenthesedSelect`，Prefix 字段默认 LATERAL），visitor `VisitTableOrSubquery` LATERAL 分支前置接线，保留前缀语义不再退化为普通子查询；6 项 round-trip 测试覆盖（`LateralSubSelectTest`） |
| BL-11 | `DATE`/`TIMESTAMP`/`TIME` 类型前缀字面量（已完成） | 功能缺口 | BL-15 全量对比 | — | **已完成（commit 358906a）**。新增 `DateTimeLiteralExpression` 类（保留字符串原值 + `DateTimeType` 枚举，对齐上游序列化行为），grammar `literal` 增 `dateTimeLiteral` 分支，visitor `VisitLiteral` 接线。**死代码清理（T083）**：删除遗留的 `DateValue`/`TimestampValue`/`TimeValue` 三个类（零实例化，强类型 DateTime 设计与上游不符），同步移除 `ExpressionVisitor`/`ExpressionVisitorAdapter`/`TablesNamesFinder` 中的 visitor 签名——**破坏性 API 变更**：外部实现 `ExpressionVisitor<T>` 接口的代码需移除对应 Visit 方法 |
| BL-12 | 上游缺失的语句类型（已完成） | 语句缺口 | BL-15 全量对比 | — | **已完成（commit 85d9218~3e8a82e 分批）**。14 个语句类型全部实现并接线：`COMMENT ON`、`ANALYZE`、`RENAME TABLE`、`ALTER VIEW`、`ALTER SESSION/SYSTEM`、`CREATE SYNONYM`、`CREATE FUNCTION/PROCEDURE`、`EXECUTE/CALL`、`PURGE`、`RESET`、`SHOW COLUMNS/INDEX/TABLES`、`BLOCK`/`DECLARE`/`IF`。注：`CREATE PROCEDURE` 与 `CREATE FUNCTION` 共用一条 grammar 规则按 token 分流；3 个 SHOW 子类共用一条 `showStatement` 规则在 visitor 分流 |
| BL-13 | 上游缺失的 Select/Schema 子特性（已完成） | 特性缺口 | BL-15 全量对比 | — | **已完成（T081 批次）**。逐项实现：FROM 子句 `PIVOT/UNPIVOT`、`TABLESAMPLE`、通用表函数作 FromItem、`OPTIMIZE FOR n ROWS`、Informix `FIRST`/`SKIP`、Join 多 ON、ClickHouse `GLOBAL/ANY/ALL`（含 STRAIGHT_JOIN）、Table 时间旅行（Snowflake AT/BEFORE，已接线）、`Table` 多部分名/Pivot/SampleClause/TimeTravel 字段、`SAFE_CAST`、`SIMILAR TO`、`NumericBind`。**未做**：`PIVOT XML` 变体（边缘方言，PIVOT/UNPIVOT 核心已覆盖，按需再补） |
| BL-14 | `ALTER` 操作覆盖度（已完成 round-trip 收口） | 特性缺口 | BL-15 全量对比 | — | **已完成（T081 批次 A）**。经 Explore 逐值核对：`AlterOperation` 枚举已 **47/47 全量对齐**上游（原 backlog "11/47" 记载已过时）；修复 grammar 已接受但 AST 静默丢失语义的 14 处 round-trip 缺陷（DROP PRIMARY/UNIQUE/FOREIGN KEY/CONSTRAINT、RENAME INDEX/KEY/CONSTRAINT、ENGINE/COMMENT 等号、分区操作族 ADD/DROP/TRUNCATE/COALESCE/REORGANIZE/EXCHANGE PARTITION 填充结构化字段、ALTER SEQUENCE 复用 SequenceParameter）。**澄清**：上游不存在 `ALTER INDEX`/`ALTER SCHEMA` 语句类（用 UnsupportedStatement 兜底），原 backlog 该项非对齐缺口；`ALTER VIEW` 已在 BL-12 完成，`ALTER SEQUENCE` 已在本批次接线 |

### P2 已知差异（保持现状，按需启动）

| BL 编号 | 待办 | 类别 | 出处 | 触发条件 | 备注 |
|---------|------|------|------|----------|------|
| BL-01 | JSON_QUERY Legacy 额外 path 参数（已完成） | 功能补全 | T076 | — | **已完成（T085）**。`AdditionalQueryPathArguments` 字段此前是死代码（grammar 不接受多 path、visitor 不填、ToString 不输出）；grammar `jsonQueryFunction` 增 `(COMMA expression)*` 尾部重复；visitor 仅在无 PASSING 时收集额外 path（对齐上游 `additionalQueryPathArguments` 仅无 PASSING 时存在的语义）；`AppendQuery` 循环输出额外 path |
| BL-02 | JSON_TABLE Oracle/Trino 方言子句（已完成） | 方言补全 | T076 | — | **已完成（T087）**。函数级补：`ON EMPTY`（ERROR/NULL/EMPTY/TRUE/FALSE/DEFAULT/EMPTY ARRAY/OBJECT）、更丰富的 `ON ERROR`（同上行为集）、`TYPE (STRICT\|LAX)`、`FORMAT JSON` 输入、`PLAN [DEFAULT] (plan_expr)`；列级补：`EXISTS`、`FORMAT JSON [ENCODING]`、`WRAPPER`（WITHOUT/WITH [CONDITIONAL\|UNCONDITIONAL] [ARRAY]）、`QUOTES [ON SCALAR STRING]`、`(ALLOW\|DISALLOW) SCALARS`、列级 `ON EMPTY`/`ON ERROR`。On*Behavior 字段从 string 升级为结构化 `JsonOnResponseBehavior`（**破坏性**：原 `Assert.Equal("NULL", x.OnErrorBehavior)` 需改为 `.Type` 断言） |
| BL-03 | 聚合函数 OVER 窗口子句（非问题，关闭） | 架构差异 | T076 | — | **已确认非问题（T086 核实）**。原 backlog 判断"Azrng OVER 走 AnalyticExpression 独立路径、需另设计 Function↔窗口接线"不准确：grammar `functionExpr` 已支持任意函数后接 `overClause`，visitor 在检测到 OVER 时构造 `AnalyticExpression`（含 Name/Expression/PartitionExpressionList/OrderByElements/WindowFrame/FilterExpression）。`SUM(x) OVER(...)`、`COUNT(*) OVER(...)`、`RANK() OVER(...)`、窗口帧 ROWS/RANGE/GROUPS、FILTER+OVER 组合均解析并 round-trip（AdvancedExpressionTest 43 项已覆盖）。架构差异仅为风格：上游保留 Function 包裹，Azrng 扁平化到 AnalyticExpression，不影响常用场景 |
| BL-04 | 上游 `MYSQL_OBJECT` 类型（OBJECTAGG 逗号分隔输出，已完成） | 输出差异 | T076 F7 | — | **已完成（T084）**。`JsonAggregateFunction` 新增 `UseCommaSeparator` 字段；visitor `VisitJsonObjectAggFunction` 增加 COMMA 分支（此前逗号静默归入非 VALUE 导致冒号输出）；`AppendObjectAgg` 三路输出（VALUE/逗号/冒号），对齐上游 MYSQL_OBJECT |
| BL-05 | JSON_OBJECT 冒号分隔 lexer 歧义（已完成） | 词法限制 | T076 F7 | — | **已完成（T086）**。原 backlog 判断"ANTLR 与 JavaCC LOOKAHEAD 本质差异、需 lexer 层改造"**不准确**——实为 token 优先级冲突（`:bar` 被 `S_JDBC_NAMED_PARAM` 最大匹配吞掉）。解法：grammar `jsonKeyValuePair` 增加 `S_JDBC_NAMED_PARAM` 分支（把命名参数整体当冒号分隔符+值），visitor 去前导冒号得到值。无需 lexer 层改造 |
| BL-06 | 方言专项 CREATE TABLE 等方言特性（已完成） | 方言补全 | T075（子项 69-77 除 72/73） | — | **已完成（T088）**。破坏性重构对齐上游 11 类模型：新建 ColDataType/ReferentialAction/ForeignKeyIndex/CheckConstraint/ExcludeConstraint/RowMovement/SpannerInterleaveIn；CreateTable 补 OrReplace/Unlogged/CreateOptions/TableOptions/Select(CTAS)/LikeTable/Columns/RowMovement/InterleaveIn；约束结构化（FK→ForeignKeyIndex 含 ReferencedTable/ReferencedColumnNames/OnDelete/OnUpdate、CHECK→CheckConstraint 持有 Expression、EXCLUDE→ExcludeConstraint）；grammar 新增 createParameter 产生式透传表级选项（ENGINE/CHARSET/PARTITION BY/ORDER BY/SAMPLE BY 等）与列规格（NOT NULL/MATERIALIZED/COMMENT 等），用 InputStream 区间获取原始文本保 round-trip。**未做**：STRUCT/ARRAY 复合列类型（改动最大，留独立批次）；PartitionDefinition 不复用于 CREATE TABLE（上游该类仅服务 ALTER，CREATE TABLE 分区走 TableOptions 字符串透传）；功能性索引 Expression 字段（边缘场景）。**破坏性 API 变更**：ColumnDefinition.DataType(string)→ColDataType；Constraint 新增 FK/CHECK/EXCLUDE 子类层次。全量 1052 测试通过（净增 31） |

### BL-15 对齐基线说明

- **对比时间**：2026-07-07
- **上游仓库**：`C:/Work/SourceCode/sqlparser/JSqlParser`
- **上游对照点**：commit `2b141568`（5.4-SNAPSHOT，2026-04-12），`feat: add ForUpdateClause class with multi-table and ORDER BY support (#2426)`
- **上游无新提交**：`2b141568` 即上游 HEAD，无后续 commit 需追赶
- **对比维度**：Expression 类、Statement 类、Select/Schema 子类、grammar 关键字、ALTER 操作
- **已验证为等价实现（不计入缺口）**：`ArrayExpression`（合入 ArrayConstructor.cs）、`MySQLGroupConcat`（合入 Function.cs）、`UserVariable`（合入 JdbcNamedParameter + S_AT_IDENTIFIER）、`VariableAssignment`（由 SetStatement 承载）、`AllValue`（合入 AnyType.All 枚举）、`ON CONFLICT`（由 Insert.cs + InsertConflictAction/Target 承载）、`ForUpdateClause`（已完整移植）、命名参数三合一（Oracle/Postgres/统一 NamedFunctionParameter）
- **上游本身不支持（非 Azrng 缺口）**：`REVOKE`、`ROLE`、`VACUUM`、`FLASHBACK`、`ATTACH/DETACH`（SQLite）、`BULK COLLECT/FORALL`、`TIMESTAMPLTZ`

### 测试规模差异说明（非缺陷，仅供参考）

- **Azrng**：1118 测试（截至 BL-18 a/b/c 部分字段扩展完成）
- **上游 JSqlParser**：2309 测试
- **差距来源**：主要来自方言专项测试（ClickHouse/Snowflake/BigQuery 等上游 CreateTableTest/SelectTest 方言用例）；BL-06~T091+BL-18 已移植 CREATE TABLE 通用能力、STRUCT/ARRAY 列类型、9 项边缘缺口、P0 静默丢弃修复、P1 功能缺口与部分字段扩展（AnalyticType/COMMENT VIEW/Column-Table 字段），但未逐条移植上游全量方言测试
- **核心 SQL 路径**：Azrng 测试独立设计，非上游测试逐条移植

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T091 | Azrng.JSqlParser 三维核查后 P0+P1 缺口修复（P0: WINDOW/QUALIFY 静默丢弃；P1: GROUP BY ROLLUP/CUBE/GROUPING SETS、CONNECT BY、SUBSTRING FROM-FOR、MSSQL OUTPUT、REFRESH MATERIALIZED VIEW、UPSERT/REPLACE；取消 JSON_TRANSFORM/CURRVAL 上游不支持；全量 1111 测试通过，净增 31） | DONE | 2026-07-09 |
| T090 | Azrng.JSqlParser CREATE TABLE 边缘遗留项一次性清完（9 缺口：character varying 列类型、TIMESTAMP WITH/WITHOUT TIME ZONE、MySQL USING BTREE/HASH 索引选项、功能性索引 (expr)、set('a','b') 类型、数组带尺寸 int[5]、::text[] 数组 cast、表级 WITH(fillfactor=70)、Spanner OPTIONS(k=true)；全量 1080 测试通过，净增 16） | DONE | 2026-07-09 |
| T089 | Azrng.JSqlParser STRUCT/ARRAY 复合列类型移植（CREATE TABLE 列类型支持 ARRAY<T> 尖括号扁平化 + STRUCT(x INT) 圆括号字段进 ArgumentsStringList，含嵌套 ARRAY<ARRAY<T>>/STRUCT(x ARRAY<T>)，Spanner 风格多 ARRAY 列；全量 1064 测试通过，净增 12） | DONE | 2026-07-09 |
| T088 | Azrng.JSqlParser BL-06 全量移植 CREATE TABLE 方言与约束结构化（破坏性重构对齐上游 11 类模型，新建 ColDataType/ReferentialAction/ForeignKeyIndex/CheckConstraint/ExcludeConstraint/RowMovement/SpannerInterleaveIn；CreateTable 补 TableOptions/CTAS/LIKE/OrReplace/Unlocked/CreateOptions；约束结构化 FK/CHECK/EXCLUDE；全量 1052 测试通过，净增 31） | DONE | 2026-07-09 |
| T087 | Azrng.JSqlParser BL-02 补全 JSON_TABLE Oracle/Trino 全量方言子句（函数级 ON EMPTY/TYPE/FORMAT JSON/PLAN + 列级 EXISTS/WRAPPER/QUOTES/SCALARS/ON EMPTY/ON ERROR，全量 1021 测试通过） | DONE | 2026-07-09 |
| T086 | Azrng.JSqlParser BL-05 修复 JSON_OBJECT 无空格冒号 lexer 冲突（grammar 接受 S_JDBC_NAMED_PARAM 作分隔符，visitor 去前导冒号，全量 1011 测试通过） | DONE | 2026-07-08 |

> 更早的 T079~T082 已归档，详见下方各任务归档说明章节。

### T090 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：一次性清完 CREATE TABLE 剩余 9 个边缘遗留缺口（全量核查上游 CreateTableTest 后识别）
- **完成情况**：单次提交，全量 1080 测试通过（1064 → 1080，净增 16）。9 缺口逐项：
  - **缺口1 character varying/character 列类型**：dataType 增加 `CHARACTER VARYING [(n)]` 分支（CHARACTER/VARYING 为保留 token）
  - **缺口2 TIMESTAMP WITH/WITHOUT TIME ZONE**：colDataType 新增 `timeZoneSuffix` 产生式（`(WITH|WITHOUT) LOCAL? TIME ZONE`），visitor 并入 DataType 文本
  - **缺口3 MySQL USING BTREE/HASH 索引选项**：tableConstraint PK/UNIQUE/KEY/INDEX 分支追加 `indexOption*`，新增 indexOption 产生式（USING/COMMENT/兜底）；Constraint 新增 IndexOptions 字段，ToString 追加
  - **缺口4 功能性索引 (expr)**：indexColumn 增加 `OPENING_PAREN expression CLOSING_PAREN` 分支；ExtractIndexColumnList 适配输出 `(expr)`
  - **缺口5 set('a','b') 类型**：dataType 增加 `SET (...)` 分支（SET 为保留 token）
  - **缺口6 数组带尺寸 int[5]**：colDataType 数组维度改为 `arrayDimension`（`LBRACKET LONG_VALUE? RBRACKET`），visitor 读尺寸填 ArrayData；ColDataType.ToString 数组括号去空格
  - **缺口7 ::text[] 数组 cast**：postfixExpr 的 `DOUBLE_COLON dataType` 改为 `DOUBLE_COLON colDataType`（含数组维度），visitor 适配
  - **缺口8 表级 WITH(fillfactor=70)**：createParameter 增加 `WITH OPENING_PAREN parameterListItem+ CLOSING_PAREN` 分支；新增 parameterListItem（`atom EQUALS atom`）
  - **缺口9 Spanner OPTIONS(k=v)**：createParameterAtom 括号内改用 parameterListItem 支持 key=value；createParameterAtom 增加 TRUE/FALSE（allow_commit_timestamp = true）
- **阻塞项**：无
- **未做的事**：Spanner 完整 testCreateTableSpanner 的生成列 `SEARCH STRING(MAX) AS (UPPER(AUTHOR)) STORED`（AS(...) 已解析，STORED 走兜底，但 AS 在列级 createParameter 与 DEFAULT 语义可能冲突需专项验证）；`::text[]` cast 的 ToString 空格既有行为（无空格）保持不变，仅测试用例适配

### T089 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：BL-06 留尾项——CREATE TABLE 列类型支持 STRUCT/ARRAY 复合类型
- **完成情况**：单次提交，全量 1064 测试通过（1052 → 1064，净增 12）
  - **grammar**：`colDataType` 增加 ARRAY/STRUCT 分支——`ARRAY MINOR_THAN colDataType GREATER_THAN`（尖括号，递归支持嵌套 `ARRAY<ARRAY<INT>>`）、`STRUCT OPENING_PAREN structColField+ CLOSING_PAREN`（圆括号，新增 `structColField` 产生式）
  - **visitor**：`BuildColDataType` 增加三路——ARRAY 扁平化整体存 `DataType`（对齐上游 `setDataType("ARRAY<" + inner + ">")`）；STRUCT 设 `DataType="STRUCT"`、字段列表进 `ArgumentsStringList`（对齐上游 `argumentsStringList.add(type + " " + colDataType.toString())`）；普通 dataType 保持不变
  - **模型**：`ColDataType.ToString` 修正参数括号空格（`STRUCT(x INT)` 而非 `STRUCT (x INT)`，此前 ArgumentsStringList 从未被填充所以空格问题从未触发）
  - **测试**：`CreateTableRoundTripTest` 追加 12 项——ARRAY 基础/带长度/STRING(MAX)/嵌套、STRUCT 基础/单字段/嵌套 ARRAY、Spanner 风格多 ARRAY 列、结构化断言验证扁平化与字段列表
- **阻塞项**：无
- **未做的事**：Spanner 完整 `testCreateTableSpanner`（含 `OPTIONS(allow_commit_timestamp = true)` 生成列 OPTIONS、`AS (...) STORED` 生成列、表级 `PRIMARY KEY` 作约束——OPTIONS 不是 lexer token 且等号参数需额外处理，属独立 Spanner 方言特性，非 ARRAY/STRUCT 核心留后续）；尖括号 `STRUCT<x INT>` 列类型（上游不支持，仅 SELECT 表达式支持）；Expression 命名空间的 StructType/ArrayConstructor 未改动（语义不同，是表达式字面量）

### T088 归档说明

- **目标仓库**：`src/Shared/Azrng.JSqlParser`，测试：`test/Azrng.JSqlParser.Test`
- **任务目标**：BL-06 方言专项 CREATE TABLE 全量移植，破坏性重构对齐上游 CreateTable 11 类模型
- **完成情况**：5 阶段独立提交，全量 1052 测试通过（1021 → 1052，净增 31）
  - **阶段1（模型类新建）**：ColDataType/ReferentialAction(+Type/Mode 枚举)/ForeignKeyIndex/CheckConstraint/ExcludeConstraint/RowMovement(+Mode 枚举)/SpannerInterleaveIn 共 7 个类
  - **阶段2（模型重构）**：ColumnDefinition.DataType(string)→ColDataType（破坏性）；CreateTable 新增 OrReplace/Unlogged/CreateOptions/TableOptions/Select/LikeTable/SelectParenthesis/RowMovement/InterleaveIn/Columns 共 10 字段；ToString 按上游序列化顺序重写
  - **阶段3+4（grammar+visitor）**：grammar createTable 重构（括号内定义可选支持纯 CTAS、createParameter 透传表级选项、rowMovementClause、LIKE、spannerInterleaveIn）；dataType 支持 schema.type 点号；新增 colDataType/createParameter/createParameterAtom/createOption/simpleColumnNames 产生式；tableConstraint 新增 EXCLUDE WHERE；visitor VisitCreateTable/VisitColumnDefinition/VisitTableConstraint 完全重写
  - **阶段5（测试+收口）**：CreateTableRoundTripTest 31 项（覆盖表选项/ClickHouse/分区/RowMovement/CTAS/LIKE/FK/CHECK/EXCLUDE + 结构化断言）；修复表级选项空格丢失（InputStream 区间取原始文本）、createParameterAtom 函数形式 MergeTree()、ROW 关键字、FK 引用括号空格
- **阻塞项**：无
- **未做的事**：STRUCT/ARRAY 复合列类型（改动最大，需 grammar/模型/visitor 全链路重构，留独立批次）；PartitionDefinition 不复用于 CREATE TABLE（上游该类仅服务 ALTER，CREATE TABLE 分区走 TableOptions 字符串透传）；功能性索引 Expression 字段（Index.ColumnParams.expression，边缘场景）；DeParser 双轨（Azrng 单轨设计，序列化全靠模型 ToString）
- **破坏性 API 变更**：ColumnDefinition.DataType(string)→ColDataType；Constraint 新增 ForeignKeyIndex/CheckConstraint/ExcludeConstraint 子类层次（VisitTableConstraint 对 FK/CHECK/EXCLUDE 返回对应子类）

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
