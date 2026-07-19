# JSqlParser Issue 分类清单

> 数据来源：`collect.json`（GitHub `JSQLParser/JSqlParser` 仓库，共 **88** 条 open issue）
> 分类主轴：按「**修复区域 / 解析器触碰点**」——同区域的 issue 改同一处文法/AST，知识可复用、适合打包批次推进；数据库方言作为每条副标签。
> 生成日期：2026-07-18　|　图例：`[B]`=BUG　`[F]`=FEATURE

---

## 总览

| 类别 | 条数 | 说明 / 修复特点 |
|---|---:|---|
| ① DDL 解析：CREATE / ALTER / DROP / INDEX / CONSTRAINT | 20 | 跨方言，最大批；模式重复，适合集中清。子主题：(a) 约束/索引 #1570 #1893 #1589 #823 #538 #1060 #652 #1927 #1295；(b) ALTER IF 系列 #2112 #1875 #599 #2039；(c) 分区/视图/库 #1668 #1735 #2070 #2353；(d) 整体失败 #1567 #2020 |
| ② 过程化 SQL 与例程：PROCEDURE / FUNCTION / PL-SQL / DO 块 | 9 | 难度最高（块语法/控制流），建议单独排期、靠后做 |
| ③ 词法 / Token / 字面量 / 类型 / 查询子句 | 7 | 多为词法规则补丁，独立、风险低，适合先做。ClickHouse #2442/.N 与 #2436/?: 是词法歧义，可一起做 |
| ④ PostgreSQL 专项 | 12 | 特性多但上游支持成熟，可对照参考实现。窗口帧 #2431 #2430；JSON #2412 #1511；字符串 #2233；interval #1728 |
| ⑤ Oracle 专项 | 4 | XML / JSON / 外连接 |
| ⑥ SQL Server / T-SQL 专项 | 6 | 含全文搜索、FOR XML、hints 等独有语法 |
| ⑦ MySQL 专项 | 5 | #2427 与 #2006 是同一特性（_utf8mb4 introducer），可合并修 |
| ⑧ 其他方言 + 跨方言特性 | 17 | Hive/BigQuery/Snowflake/DuckDB/Teradata/Informix/Informatica/KsqlDB/CockroachDB/Druid/Spark/Interval/ODBC，多数小众，优先级低 |
| ⑨ AST 语义正确性：能解析但树错 / NPE / 父节点错 | 5 | 影响所有用户，正确性问题，建议中等优先 |
| ⑩ 工程 / 架构 / 非解析 | 3 | #2438 是提问可直接回复关闭；#467 marker 接口建议配合当前接口 I 前缀治理一起做 |
| **合计** | **88** | |

---

## 修复推进顺序建议

1. **#467**（marker 接口）—— 与 `Azrng.JSqlParser` 当前「接口加 I 前缀」治理同源，顺手做
2. **③ 词法**（7 条）—— 独立、风险低、见效快
3. **① DDL**（20 条，按 4 个子主题分批）—— 量大利好清债
4. **⑨ AST 正确性**（5 条）—— 影响面广
5. **④→⑥→⑦→⑤ 方言专项** —— PG 最值得先做（条数多、参考实现全）
6. **② 过程化**（9 条）—— 难度高，单独排期
7. **⑧ 小众方言**（17 条）—— 按需，多数可暂缓
8. **#2438** 提问类 —— 直接回复关闭

---

## 修复价值评估（Azrng 视角，2026-07-19）

> 评估原则：**用户覆盖面 × 语法常见度 − 成本/风险**。
> 主流方言（PG/MySQL/SQL Server/Oracle）+ 常见语法 + 正确性 bug → 优先；
> 废弃语法 / 超小众方言（Informix/Informatica/Druid/KsqlDB/CockroachDB）/ 无迫切需求的 FEATURE → 不修。
> 图例：⭐ 值得修（建议优先）｜🕐 暂缓（小众/高成本/需复现，按需）｜❌ 不修（废弃/不适用/提问）｜🔄 T114 在做｜✅ 已完成

### ⭐ 值得修（约 28 条，建议优先）

**① DDL（16）** —— 跨方言常见 DDL/索引/约束，集中清债收益最大：
- #2070 CREATE DATABASE、#2065 DROP 多表 IF EXISTS、#2112 ALTER MODIFY/DROP IF EXISTS、#1875 ADD COLUMN IF NOT EXISTS、#599 MODIFY NULL/NOT NULL —— 标准常见 DDL
- #1668 MySQL 分区 create/alter、#1927 MySQL 函数索引、#1295 ALTER ADD INDEX、#1570/#1893/#823/#538 MySQL 索引名/USING/COMMENT/unique index（同族，一起清）
- #1060 索引类型错误、#652 多参数索引 —— 正确性
- #2020 SQL Server `WITH(index options)`、#2039 Oracle ADD CONSTRAINT tablespace —— 主流方言索引/约束选项

**③ 词法（2）**：#2435 MySQL `0x` 十六进制字面量、#2359 LIMIT 含子查询（正确性）

**② 过程化（1）**：#1994 解析 FUNCTION 后无法续解析（语句边界，影响多语句批解析，核心）

**⑤ Oracle（1）**：#672 外连接 `(+)`（大量老 Oracle 库在用）

**⑥ SQL Server（2）**：#2033 BULK INSERT、#386 `FOR XML PATH`/STUFF（ETL/拼串常见）

**⑨ AST 正确性（5）** —— 影响所有用户，最高优先：#2440 WHERE `IN(...) AND`、#2195 Lambda 参数、#2194 Parent 节点、#2163 PG JSON+关系运算符混用 AST 错、#1170 NotExpression

**⑩ 工程（1）**：#467 marker 接口（与当前「I 前缀」治理同源）

### 🕐 暂缓（约 35 条，按需推进）

- **② 过程化（8）**：#2358/#1946 PG DO 块、#1786 PL/SQL DECLARE、#715 T-SQL TVF、#2007 Oracle 存储过程、#1978 ALTER FUNCTION/PROCEDURE、#268 OUTPUT 变量、#2192 占位符 —— 块语法/控制流，成本高、解析器定位外，单独排期
- **③ ClickHouse 词法（3）**：#2442 `.N`、#2441 `Nullable(Decimal)`、#2436 `?:` —— 一起做（词法歧义）
- **⑤ Oracle XML/JSON（3）**：#2146 xmlparse、#1825 JSON_VALUE、#1564 XMLSERIALIZE
- **⑥（2）**：#1563 整份 sakila 太泛需逐项拆、#397 `%%` FTS 非标准写法
- **①（3）**：#2353 ClickHouse `CREATE TABLE ORDER BY`、#1735 Redshift BACKUP NO、#1567 SQL Server typed XML（bracket/schema 已支持，typed xml 小众）
- **⑧ 跨方言/有支持基础（~11）**：#2433 Hive LATERAL VIEW 正确性、#2429 Snowflake `IDENTIFIER`、#2423 DuckDB MAP/PIVOT、#2421 BigQuery MERGE、#2350 MATCH_RECOGNIZE（巨型）、#2119/#1846 Hive INSERT OVERWRITE、#1620 Spark `[shuffle]`、#1139 ODBC `{fn ...}`、#891 Teradata UPDATE FROM、#673 DAY TO SECOND interval

### ❌ 不修（8 条，拒绝并说明）

- **⑧ 超小众方言、无长期价值**：#1752 KsqlDB、#1743 CockroachDB（近 PG，专项支持成本高无需求）、#1625 Druid、#1161/#271 Informix、#297 Informatica `{ }` 转义 —— 用户基数≈0，做一次没人用、对照维护成本持续
- **⑩**：#2438「下个版本计划」是提问，直接关；#2403 JavaCC 警告，本库用 ANTLR 不适用
- （注：#2428 PROCEDURE ANALYSE 虽 MySQL 8.0 已废弃，T114 已顺手做，不在此列）

### 🔄 T114 在做 / ✅ 已完成

- ✅ 已完成：④ 全 12 条；① #1589；③ #1169/#1314
- 🔄 T114 进行中：#161 OPTION hint、#911 `@table`、#854 `INTO @var`、#2298 CAST CHARACTER SET、#2427+#2006 `_utf8mb4`、#2428 PROCEDURE ANALYSE

---

## 分类明细

### ① DDL 解析：CREATE / ALTER / DROP / INDEX / CONSTRAINT  [20 条]

> 跨方言，最大批；模式重复，适合集中清。子主题：(a) 约束/索引 #1570 #1893 #1589 #823 #538 #1060 #652 #1927 #1295；(b) ALTER IF 系列 #2112 #1875 #599 #2039；(c) 分区/视图/库 #1668 #1735 #2070 #2353；(d) 整体失败 #1567 #2020
>
> **Azrng 移植版验证状态**（2026-07-19，T114 探针 + round-trip）：
> - 🔧 本次已修复（探针转绿 + round-trip 通过）：#1589（实为 PRIMARY KEY NONCLUSTERED 缺失，已修）
> - ⛔ 复现且未修复：#1570 #1893 #823 #538 #1295（MySQL 索引细节，后续批次）

| # | 类型 | 标题 | 要点 |
|---:|:--:|---|---|
| [#2353](https://github.com/JSQLParser/JSqlParser/issues/2353) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : Clickhouse : ORDER BY after CREATE TABLE unsupported | CREATE TABLE ORDER BY (ClickHouse) |
| [#2112](https://github.com/JSQLParser/JSqlParser/issues/2112) | [B] | [BUG] `ALTER TABLE ... MODIFY/DROP` should support the `IF EXIST` option | ALTER TABLE MODIFY/DROP 的 IF EXIST 选项 |
| [#2070](https://github.com/JSQLParser/JSqlParser/issues/2070) | [B] | How to parse CREATE DATABASE DATABASE_NAME  ? | CREATE DATABASE 语句 |
| [#2065](https://github.com/JSQLParser/JSqlParser/issues/2065) | [F] | [FEATURE] Dropping multiple tables IF EXISTS is not supported | DROP 多表 IF EXISTS (MySQL) [FEATURE] |
| [#2039](https://github.com/JSQLParser/JSqlParser/issues/2039) | [B] | ORACLE: ALTER TABLE ... ADD CONSTRAINT ... with tablespace option unsupported | ALTER ADD CONSTRAINT 带 tablespace (Oracle) |
| [#2020](https://github.com/JSQLParser/JSqlParser/issues/2020) | [B] | [BUG] SQLServer  Validation fail  jsqlparser Version :4.9 | SQLServer 校验失败 |
| [#1927](https://github.com/JSQLParser/JSqlParser/issues/1927) | [B] | [BUG] JSQLParser 4.7 : MySQL 8 : Cannot parse functional indices in table creation DDL | 建表 DDL 函数索引 (MySQL 8) |
| [#1893](https://github.com/JSQLParser/JSqlParser/issues/1893) | [B] | Caused by: net.sf.jsqlparser.parser.ParseException: Encountered unexpected token: "UNIQUE" "UNIQUE" | UNIQUE token |
| [#1875](https://github.com/JSQLParser/JSqlParser/issues/1875) | [B] | [BUG] JSQLParser 4.7: `ADD COLUMN IF NOT EXISTS` not supported | ADD COLUMN IF NOT EXISTS (PG) |
| [#1735](https://github.com/JSQLParser/JSqlParser/issues/1735) | [B] | [BUG] JSQLParser 4.6 : Redshift : CREATE MATERIALIZED VIEW with BACKUP NO is not supported | CREATE MATERIALIZED VIEW BACKUP NO (Redshift) |
| [#1668](https://github.com/JSQLParser/JSqlParser/issues/1668) | [B] | parser sql of MySQL create or alter table with partition occur  exception | 分区表 create/alter (MySQL) |
| [#1589](https://github.com/JSQLParser/JSqlParser/issues/1589) | [B] | Error in parsing create statement: Encountered unexpected token: "KEY" "KEY" | KEY token |
| [#1570](https://github.com/JSQLParser/JSqlParser/issues/1570) | [B] | MySQL (optional) Index Names | CONSTRAINT UNIQUE KEY 索引名 (MySQL) |
| [#1567](https://github.com/JSQLParser/JSqlParser/issues/1567) | [B] | Failed to parse SQL Server ddl | SQL Server DDL 解析失败 |
| [#1295](https://github.com/JSQLParser/JSqlParser/issues/1295) | [B] | Failed to parse MySQL statement and add common index statement with alter | ALTER ADD INDEX (MySQL) |
| [#1060](https://github.com/JSQLParser/JSqlParser/issues/1060) | [B] | Incorrect index type for indices parsed from create table statement | 索引类型解析错误 |
| [#823](https://github.com/JSQLParser/JSqlParser/issues/823) | [B] | Failed to parse when creating unique index in creation DDL | 建表 DDL 内 unique index |
| [#652](https://github.com/JSQLParser/JSqlParser/issues/652) | [B] | Indices with multiple parameters are not parsed correctly | 多参数索引解析错误 |
| [#599](https://github.com/JSQLParser/JSqlParser/issues/599) | [B] | Modify column with "NULL" or "NOT NULL" is not getting parsed. | MODIFY column NULL/NOT NULL |
| [#538](https://github.com/JSQLParser/JSqlParser/issues/538) | [B] | Format create table sql error when encounter unique key with comment | unique key 带 comment |

### ② 过程化 SQL 与例程：PROCEDURE / FUNCTION / PL-SQL / DO 块  [9 条]

> 难度最高（块语法/控制流），建议单独排期、靠后做

| # | 类型 | 标题 | 要点 |
|---:|:--:|---|---|
| [#2358](https://github.com/JSQLParser/JSqlParser/issues/2358) | [B] | [BUG] JSQLParser 5.3 Encountered unexpected token K_DO "DO". | DO token |
| [#2192](https://github.com/JSQLParser/JSqlParser/issues/2192) | [B] | [BUG] JSQLParser 5.0-5.1: SQL Server: Stored procedure with placeholders being parsed wrong when it was working in v1 | 存储过程占位符解析退步 (SQL Server) |
| [#2007](https://github.com/JSQLParser/JSqlParser/issues/2007) | [F] | [FEATURE] Stored procedure support Oracle | 存储过程 (Oracle) [FEATURE] |
| [#1994](https://github.com/JSQLParser/JSqlParser/issues/1994) | [B] | [BUG] JSQLParser 4.9 fails to parse subsequent statements after parsing FUNCTION statement | 解析 FUNCTION 后无法继续解析后续语句 |
| [#1978](https://github.com/JSQLParser/JSqlParser/issues/1978) | [B] | [BUG] JSQLParser Version : 4.8: Does not support "ALTER FUNCTION" or "ALTER PROCEDURE". | ALTER FUNCTION / ALTER PROCEDURE (SQL Server) |
| [#1946](https://github.com/JSQLParser/JSqlParser/issues/1946) | [F] | [Feature] Prostgres Procedural `DO $$BEGIN ... END$$` | DO $$BEGIN...END$$ 过程化块 (PG) [FEATURE] |
| [#1786](https://github.com/JSQLParser/JSqlParser/issues/1786) | [F] | [FEATURE] missing feature on parsing PL/SQL with "DECLARE" cluase | PL/SQL DECLARE 块 (Oracle) [FEATURE] |
| [#715](https://github.com/JSQLParser/JSqlParser/issues/715) | [B] | Is it feasible to add support in jsqlparser for "table-valued" functions as defined by t-sql? | table-valued CREATE FUNCTION (T-SQL) |
| [#268](https://github.com/JSQLParser/JSqlParser/issues/268) | [B] | Support for OUTPUT variable arguments to procedure calls | 过程调用 OUTPUT 变量参数 |

### ③ 词法 / Token / 字面量 / 类型 / 查询子句  [7 条]

> 多为词法规则补丁，独立、风险低，适合先做。ClickHouse #2442/.N 与 #2436/?: 是词法歧义，可一起做
>
> **Azrng 移植版验证状态**（2026-07-19，T114 探针 + round-trip）：
> - 🔧 本次已修复（探针转绿 + round-trip 通过）：#1169（方向不结构化为字段，整体透传原文）/ #1314（仅 INSERT SET 主体，AS 行别名不修）
> - ⛔ 复现且未修复：#2435 #2359（后续批次评估）

| # | 类型 | 标题 | 要点 | Azrng 状态 |
|---:|:--:|---|---|---|
| [#2442](https://github.com/JSQLParser/JSqlParser/issues/2442) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : ClickHouse : Tuple positional access via .N not supported in SELECT | .N tuple 位置访问，.2 被当浮点 (ClickHouse) | — |
| [#2441](https://github.com/JSQLParser/JSqlParser/issues/2441) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : ClickHouse : Parametric type Nullable(Decimal(p, s)) not supported as CAST target | Nullable(Decimal(p,s)) 参数化类型作 CAST 目标 (ClickHouse) | — |
| [#2436](https://github.com/JSQLParser/JSqlParser/issues/2436) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : ClickHouse : C-style ternary operator (? :) not supported in SELECT | C 风格三元运算符 ?: (ClickHouse) | — |
| [#2435](https://github.com/JSQLParser/JSqlParser/issues/2435) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : MySQL : 0x hexadecimal literal (0xFF) not supported as select item | 0x 十六进制字面量作 select 项 (MySQL) | ⛔ 复现（探针记录） |
| [#2359](https://github.com/JSQLParser/JSqlParser/issues/2359) | [B] | [BUG] JSQLParser Version 5.3: LIMIT with subquery fails: Was expecting: "BY" | LIMIT 含子查询解析失败 | ⛔ 复现（探针记录） |
| [#1314](https://github.com/JSQLParser/JSqlParser/issues/1314) | [B] | SET clause with alias not parsed | SET 子句带别名未解析 | 🔧 部分修复（仅 INSERT INTO t SET a=1,b=2 主体；AS new(m,n,p) 行别名极冷门不修） |
| [#1169](https://github.com/JSQLParser/JSqlParser/issues/1169) | [B] | net.sf.jsqlparser.JSQLParserException: Encountered unexpected token: "desc" "DESC" | desc / DESC token 意外 | 🔧 已修复（groupByColumn 子规则 + GroupByColumnReference 模型） |

### ④ PostgreSQL 专项  [12 条]

> 特性多但上游支持成熟，可对照参考实现。窗口帧 #2431 #2430；JSON #2412 #1511；字符串 #2233；interval #1728
>
> **Azrng 移植版验证状态**（2026-07-18，探针 + round-trip 测试，测试文件
> `test/Azrng.JSqlParser.Test/Statement/PostgreSqlUpstreamIssuesProbeTest.cs`
> + `PostgreSqlFixRoundTripTest.cs`）：
> - ✅ 已支持（移植版不存在上游缺陷）：#2233 #2342 #2430 #2431
> - 🔧 本次已修复（探针转绿 + round-trip 通过）：#187 #1416 #1511 #1728 #2326 #2411 #2412 #2432
> - ⛔ 复现且未修复：无
> - 🟨 已知保留限制（评估后**主动不修**，理由见各行）：#187 @@@、#1416 ANALYZE 前导、#2411 ROWS FROM 逐项别名

| # | 类型 | 标题 | 要点 | Azrng 状态 |
|---:|:--:|---|---|---|
| [#2432](https://github.com/JSQLParser/JSqlParser/issues/2432) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : LIKE ANY (ARRAY[...]) / LIKE ALL (ARRAY[...]) fails to parse | LIKE ANY/ALL (ARRAY[...]) | 🔧 已修复（predicateSuffix 加 `(ANY\|ALL)?` + LikeExpression.LikeQuantifier） |
| [#2431](https://github.com/JSQLParser/JSqlParser/issues/2431) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : GROUPS not supported in window function frame clause | 窗口帧 GROUPS 子句 [Missing Standard Feature] | ✅ 已支持（windowFrame 早含 GROUPS） |
| [#2430](https://github.com/JSQLParser/JSqlParser/issues/2430) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : EXCLUDE TIES not supported in window function frame clause | 窗口帧 EXCLUDE TIES [Missing Standard Feature] | ✅ 已支持（windowFrame 早含 EXCLUDE TIES） |
| [#2412](https://github.com/JSQLParser/JSqlParser/issues/2412) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : json_populate_record row expansion with (…). * not supported | json_populate_record 行展开 (...).*  | 🔧 已修复（selectItem 加 `(expr).*` → RowGetExpression+Parenthesis 保括号） |
| [#2411](https://github.com/JSQLParser/JSqlParser/issues/2411) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : ROWS FROM not supported | ROWS FROM 语法 | 🔧 已修复（tableOrSubquery 加 ROWS FROM(...) → 新 RowsFrom 模型）。🟨 保留限制：子项各自别名 `foo() AS (a,b)` 未支持——小众用法（仅 SRF 吐复合/无名列时需要），原始 issue SQL 未用，按 YAGNI 延后到需求驱动 |
| [#2342](https://github.com/JSQLParser/JSqlParser/issues/2342) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : Nesting can cause NullPointerException | 嵌套导致 NullPointerException | ✅ 已支持（ANTLR 无栈深 NPE） |
| [#2326](https://github.com/JSQLParser/JSqlParser/issues/2326) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : PostgreSQL : XMLTable function not supported | XMLTable 函数 | 🔧 已修复（新 XMLTABLE/XMLNAMESPACES 词法 token + xmlTable 规则含 XMLNAMESPACES 前缀 + XmlTable 模型） |
| [#2233](https://github.com/JSQLParser/JSqlParser/issues/2233) | [B] | [BUG] JSQLParser 5.1: PostgreSQL: fail to parse dollar-quoted string constants with tags | $tag$ dollar-quoted 带标签字符串 | ✅ 已支持（S_DOLLAR_QUOTED_STRING 早支持带标签） |
| [#1728](https://github.com/JSQLParser/JSqlParser/issues/1728) | [B] | [BUG] JSQLParser 4.5 : Postgres : fails to parse `interval hour to minute` | interval hour to minute | 🔧 已修复（dataType 加 `INTERVAL intervalField (TO intervalField)?` 分支） |
| [#1511](https://github.com/JSQLParser/JSqlParser/issues/1511) | [B] | Cannot parse PGSQL JSONB_ARRAY_ELEMENTS() WITH ORDINALITY ARR() | JSONB_ARRAY_ELEMENTS() WITH ORDINALITY ARR() | 🔧 已修复（tableFunction 加 `[WITH ORDINALITY] alias? (cols)?` + TableFunction.WithOrdinality/ColumnAliases） |
| [#1416](https://github.com/JSQLParser/JSqlParser/issues/1416) | [B] | Postgres EXPLAIN parsing incorrect and missing new flags | EXPLAIN 解析缺新 flag | 🔧 已修复（explainStatement 加 `explainOptionList` + Analyze/Verbose/Options 字段；新增 VERBOSE/BUFFERS/TIMING/SUMMARY/WAL/YAML token）。🟨 保留限制：`ANALYZE SELECT 1` 前导关键字形式 ToString 仍输出 `EXPLAIN`——PG 中 `ANALYZE` 是独立统计语句、非 EXPLAIN 同义词，`(EXPLAIN\|ANALYZE)` 合并是上游建模选择，不为非标准形式加 LeadingKeyword 字段 |
| [#187](https://github.com/JSQLParser/JSqlParser/issues/187) | [B] | Postgresql's FTS queries and function-based indexes are not supported | FTS 全文查询与函数索引 | 🔧 已修复（新增 @@/@@@ token + comparisonOperator + Matches 节点；createIndex 加 `USING method` 并补全索引列/WHERE round-trip）。🟨 保留限制：`@@@`（pg_trgm 历史相似度算子）round-trip 印成 `@@`——现代 PG 已用 `%`/距离算子取代、属废弃语法，能解析（难点已做）即可，不为死语法给 Matches 加可变符号 |

### ⑤ Oracle 专项  [4 条]

> XML / JSON / 外连接

| # | 类型 | 标题 | 要点 |
|---:|:--:|---|---|
| [#2146](https://github.com/JSQLParser/JSqlParser/issues/2146) | [B] | Oracle `xmlparse(content  ...)` not supported | xmlparse(content ...) (Oracle) |
| [#1825](https://github.com/JSQLParser/JSqlParser/issues/1825) | [F] | [FEATURE] missing JSON_VALUE function for Oracle | JSON_VALUE 函数 (Oracle) [FEATURE] |
| [#1564](https://github.com/JSQLParser/JSqlParser/issues/1564) | [B] | Oracle SQL XMLSERIALIZE syntax not supported | XMLSERIALIZE 语法 (Oracle) |
| [#672](https://github.com/JSQLParser/JSqlParser/issues/672) | [B] | parse error between with oracle outer join(+) | 外连接 (+) 语法 |

### ⑥ SQL Server / T-SQL 专项  [6 条]

> 含全文搜索、FOR XML、hints 等独有语法
>
> **Azrng 移植版验证状态**（2026-07-19，T114 探针 + round-trip）：
> - 🔧 本次已修复（探针转绿 + round-trip 通过）：#911 #161
> - ⛔ 复现且未修复：#2033 #397（后续批次评估）

| # | 类型 | 标题 | 要点 | Azrng 状态 |
|---:|:--:|---|---|---|
| [#2033](https://github.com/JSQLParser/JSqlParser/issues/2033) | [B] | [BUG] JSQLParser Version : 4.7 : sqlserver insert bulk  sql failed! | insert bulk (SQL Server) | ⛔ 复现（探针记录） |
| [#1563](https://github.com/JSQLParser/JSqlParser/issues/1563) | [B] | TSQL/MS SQL Server statements/syntax not supported. | TSQL 语法不支持 | — |
| [#911](https://github.com/JSQLParser/JSqlParser/issues/911) | [B] | SQL Server table variables not supported \| SELECT columnName FROM @table | 表变量 @table | 🔧 已修复（table 规则加 SINGLE_AT_IDENTIFIER/S_AT_IDENTIFIER 分支，@name 整段保留到 Table.Name） |
| [#397](https://github.com/JSQLParser/JSqlParser/issues/397) | [B] | SqlServer full text search %% | 全文搜索 %% | ⛔ 复现（探针记录） |
| [#386](https://github.com/JSQLParser/JSqlParser/issues/386) | [B] | Support for TSQL "STUFF" and "FOR XML PATH" instructions | STUFF / FOR XML PATH | ✅ 已支持（ForXmlPath 字段） |
| [#161](https://github.com/JSQLParser/JSqlParser/issues/161) | [B] | add support for SQL Server Query Hints | Query Hints | 🔧 已修复（plainSelect 末尾加 optionClause?，PlainSelect.OptionHints 透传文本） |

### ⑦ MySQL 专项  [5 条]

> #2427 与 #2006 是同一特性（_utf8mb4 introducer），可合并修
>
> **Azrng 移植版验证状态**（2026-07-19，T114 探针 + round-trip）：
> - 🔧 本次已修复（探针转绿 + round-trip 通过）：#2427 #2006 #2298 #854
> - ⛔ 复现且未修复：无
> - 🚫 主动不修：#2428（MySQL 5.7 弃用、8.0 移除的已死语法，扩 grammar/模型/测试是长期负债）

| # | 类型 | 标题 | 要点 | Azrng 状态 |
|---:|:--:|---|---|---|
| [#2428](https://github.com/JSQLParser/JSqlParser/issues/2428) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : MySQL : PROCEDURE ANALYSE() not supported in SELECT statements | PROCEDURE ANALYSE() | 🚫 不修（MySQL 5.7 弃用、8.0 移除，已死语法） |
| [#2427](https://github.com/JSQLParser/JSqlParser/issues/2427) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : MySQL : charset introducer (_utf8mb4) with COLLATE not supported | _utf8mb4 introducer + COLLATE | 🔧 已修复（StringPrefix 扩展为 '_' [a-zA-Z0-9]+，S_CHAR_LITERAL 加 introducer+空格分支，StringValue 支持 _xxx 前缀） |
| [#2298](https://github.com/JSQLParser/JSqlParser/issues/2298) | [B] | [BUG] JSQLParser 5.4-SNAPSHOT : MySQL : Failed to parse CAST as CHAR with CHARACTER SET | CAST as CHAR with CHARACTER SET | 🔧 已修复（castExpr 加 castCharacterSetClause，CastExpression.CharacterSet/Collation 字段） |
| [#2006](https://github.com/JSQLParser/JSqlParser/issues/2006) | [B] | [BUG] JSQLParser 4.8 : MYSQL : not able to parse _utf8mb4 dialects | _utf8mb4 方言解析 | 🔧 已修复（与 #2427 同源修复，覆盖 _utf8mb4 'text' 带空格形式） |
| [#854](https://github.com/JSQLParser/JSqlParser/issues/854) | [B] | Cannot use MySQL user variables after INTO clause | INTO 后用户变量 | 🔧 已修复（intoClause 加 INTO parameter 分支前置，PlainSelect.IntoVariables 字段） |

### ⑧ 其他方言 + 跨方言特性  [17 条]

> Hive/BigQuery/Snowflake/DuckDB/Teradata/Informix/Informatica/KsqlDB/CockroachDB/Druid/Spark/Interval/ODBC，多数小众，优先级低

| # | 类型 | 标题 | 要点 |
|---:|:--:|---|---|
| [#2433](https://github.com/JSQLParser/JSqlParser/issues/2433) | [B] | [BUG] JSQLParser Version : 5.3 : LATERAL VIEW with three or more column aliases silently mis-parses extras as cross-join tables | LATERAL VIEW 三列及以上别名误解析 (Hive) |
| [#2429](https://github.com/JSQLParser/JSqlParser/issues/2429) | [F] | [FEATURE] missing feature description, IDENTIFIER from Snowflake | IDENTIFIER() (Snowflake) [FEATURE] |
| [#2423](https://github.com/JSQLParser/JSqlParser/issues/2423) | [B] | [BUG] JSQLParser Version : RDBMS : failing feature description | MAP / PIVOT (DuckDB) |
| [#2421](https://github.com/JSQLParser/JSqlParser/issues/2421) | [B] | [BUG] BigQuery statement MERGE ... WHEN NOT MATCHED BY TARGET | MERGE ... WHEN NOT MATCHED BY TARGET (BigQuery) |
| [#2350](https://github.com/JSQLParser/JSqlParser/issues/2350) | [F] | [FEATURE] Add support for MATCH_RECOGNIZE clause (BigQuery) to JSQLParser | MATCH_RECOGNIZE 子句 (BigQuery) [FEATURE] |
| [#2119](https://github.com/JSQLParser/JSqlParser/issues/2119) | [B] | [BUG] JSQLParser Version : 5.0 RDBMS : Hive syntax is not supported INSERT OVERWRITE PARTITION (dtime = '20220403')CASE WHEN condition THEN END ELSE ... | INSERT OVERWRITE PARTITION ... CASE WHEN (Hive) |
| [#1846](https://github.com/JSQLParser/JSqlParser/issues/1846) | [B] | [BUG] JSQLParser Version : 4.6 hive  overwrite | INSERT OVERWRITE (Hive) |
| [#1752](https://github.com/JSQLParser/JSqlParser/issues/1752) | [B] | [BUG] JSQLParser 4.6 : KsqlDB : Not able to parse Ksqldb queries | KsqlDB 查询 |
| [#1743](https://github.com/JSQLParser/JSqlParser/issues/1743) | [F] | [FEATURE] Support for CockroachDB | CockroachDB 支持 [FEATURE] |
| [#1625](https://github.com/JSQLParser/JSqlParser/issues/1625) | [B] | Cannot parse druid sql | FLOOR(__time TO HOUR) 时间函数 (Druid) |
| [#1620](https://github.com/JSQLParser/JSqlParser/issues/1620) | [B] | sql contain  join [shuffle], so error | [shuffle] join hint (Spark) |
| [#1161](https://github.com/JSQLParser/JSqlParser/issues/1161) | [B] | support for informix db fnc | CURRENT YEAR TO DAY 等函数 (Informix) |
| [#1139](https://github.com/JSQLParser/JSqlParser/issues/1139) | [B] | support for (date({fn timestampadd(SQL_TSI_YEAR, 2, date("travel_date"))})) | {fn timestampadd(...)} ODBC 转义 |
| [#891](https://github.com/JSQLParser/JSqlParser/issues/891) | [B] | JSqlParser failed to parse Teradata "UPDATE" statement with "FROM" clause | UPDATE ... FROM (Teradata) |
| [#673](https://github.com/JSQLParser/JSqlParser/issues/673) | [B] | `DAY TO SECOND` is not supported | DAY TO SECOND interval |
| [#297](https://github.com/JSQLParser/JSqlParser/issues/297) | [B] | JSQLParser not able to parse the informatica sql query. | { } 转义语法 (Informatica) |
| [#271](https://github.com/JSQLParser/JSqlParser/issues/271) | [B] | Parse errors for ALTER TABLE with Informix syntax | ALTER TABLE Informix 语法 |

### ⑨ AST 语义正确性：能解析但树错 / NPE / 父节点错  [5 条]

> 影响所有用户，正确性问题，建议中等优先

| # | 类型 | 标题 | 要点 |
|---:|:--:|---|---|
| [#2440](https://github.com/JSQLParser/JSqlParser/issues/2440) | [B] | [BUG] JSQLParser 5.3 : Incorrect parse of WHERE column IN ('CONFIRMED') AND .... | WHERE col IN (...) AND ... 解析错误 |
| [#2195](https://github.com/JSQLParser/JSqlParser/issues/2195) | [B] | JSQLParser 5.1: LambdaExpression parameters error | LambdaExpression 参数错误 |
| [#2194](https://github.com/JSQLParser/JSqlParser/issues/2194) | [B] | JSQLParser 5.1: Incorrect Parent node | Parent 节点错误 |
| [#2163](https://github.com/JSQLParser/JSqlParser/issues/2163) | [B] | [BUG] JSQLParser Version : 5.1   RDBMS : PostgreSQL 10   mix the JSON and relational operators, it outputs the wrong AST and sql. | JSON 与关系运算符混用，AST 输出错 (PG) |
| [#1170](https://github.com/JSQLParser/JSqlParser/issues/1170) | [B] | NotExpression parsing error | NotExpression 解析错误 |

### ⑩ 工程 / 架构 / 非解析  [3 条]

> #2438 是提问可直接回复关闭；#467 marker 接口建议配合当前接口 I 前缀治理一起做

| # | 类型 | 标题 | 要点 |
|---:|:--:|---|---|
| [#2438](https://github.com/JSQLParser/JSqlParser/issues/2438) | [B] | Any plan for the next release? | 下个版本计划（提问，非 bug） |
| [#2403](https://github.com/JSQLParser/JSqlParser/issues/2403) | [B] | compileJavacc emits JavaCC warnings for shadowed literals and unreachable DATA_TYPE branches | compileJavacc 产生 JavaCC 警告（遮蔽字面量/不可达分支） |
| [#467](https://github.com/JSQLParser/JSqlParser/issues/467) | [B] | Add marker interfaces to allow OOP classification of expressions | 为表达式加 marker 接口以支持 OOP 分类 [improvement] |
