# Azrng.JSqlParser 变更记录

完整版本历史按版本号倒序维护。

## 版本历史

### 1.0.0-rc2

同步上游 JSqlParser 5.4 正式版（tag `jsqlparser-5.4`，commit `e847e94b`）第一档缺口 7 项。**无破坏性 API 变更**（仅新增字段/枚举与文法能力，全部向后兼容）。

**新增能力**：
- SQL Server `SET IDENTITY_INSERT t ON|OFF`（#2605）— `SetStatement.IdentityInsertTable` / `SwitchValue`，表名可被 `GetTableNames()` 提取
- SQL Server 布尔开关 `SET NOCOUNT ON` / `SET ANSI_NULLS OFF` 等（#2604）— `SetStatement.SwitchValue`
- `MERGE ... WHEN NOT MATCHED [BY TARGET|BY SOURCE]`（#2421/#2480）— `MergeOperation.Side`（新枚举 `MergeSide`）；配对校验对齐上游（BY TARGET 仅配 INSERT、BY SOURCE 仅配 UPDATE/DELETE，违规抛 `JSqlParserException`）
- `MERGE ... RETURNING`（#2569）— `Merge.Returning`
- `INSERT ... OVERRIDING [USER|SYSTEM] VALUE`（#2569）— `Insert.Overriding`（既有字段补 grammar 接线，输出位置修正到列清单之后对齐上游）
- 递归 CTE 环检测 `WITH RECURSIVE ... CYCLE cols SET mark [TO x DEFAULT y] USING path`（#2566）— `WithItem.CycleClause`（新类 `WithCycleClause`，USING 必填）
- `CREATE INDEX ... INCLUDE (cols)`（#2462）— `CreateIndex.IncludedColumns`
- `GROUPS` 降为非保留字，可作列名/表名（#2473；窗口帧 `GROUPS BETWEEN` 不受影响）

**测试**：新增 `Upstream54SyncRoundTripTest`（27 项），#2421 探针转正；全量 **1781** 通过 / 2 Skip × 3 TFM（net8/9/10）。

### 1.0.0-rc1

版本号从 `1.0.0-beta12` 调整为 `1.0.0-rc1`，功能内容与 beta12 一致，解决预发布版本按字符串排序导致最新版本无法正确显示的问题。

### 1.0.0-beta12

支持 PostgreSQL `ANY`/`ALL`/`SOME` 使用数组表达式、命名数组参数或子查询，并补充 `@name` 参数及否定条件回归测试。

### 1.0.0-beta11

上游 issue 高价值清仓收口（T123–T126）+ 单元测试补全。**无破坏性 API 变更**（仅新增字段/类型与文法能力；相对 beta9 的公共面扩展均向后兼容）。

**相对 beta9 累计能力**（详见下列 beta10 条目与 `MIGRATION.md` 第十六～十九节）：
- 常见 DDL：`CREATE DATABASE`、多表 DROP、`ADD/DROP/MODIFY IF [NOT] EXISTS`、`MODIFY NULL/NOT NULL`、MySQL 分区定义列表
- SQL Server：`INSERT BULK`、`CREATE INDEX WITH`、`ON PRIMARY`、`IDENTITY`、`DEFAULT ... FOR`、`EXEC ... OUTPUT`、`CREATE OR ALTER`
- Oracle：`USING INDEX TABLESPACE`、`XMLPARSE`/`XMLSERIALIZE`、`DAY TO SECOND`
- 其他：ODBC `{fn}/{d|t|ts}`、Hive `INSERT OVERWRITE`、`unsigned`/`zerofill`、前缀索引、`[quoted]` 词法修复
- 测试：全量 **1750** 通过 / 3 Skip；补齐探针 + round-trip + `CALL()` 空括号回归

### 1.0.0-beta10

上游 issue 常见 DDL / SQL Server·Oracle 专项修复（T123 + T124）+ 探针核实。**无破坏性 API 变更**（仅新增字段/类型与文法能力）。

**新增 / 修复能力（T123 常见 DDL）**：
- `CREATE DATABASE [IF NOT EXISTS] name`（#2070）— 新语句类型 `CreateDatabase`
- `DROP TABLE/VIEW` 多对象列表完整 round-trip（#2065）— `Drop.NameList` / `DropBehavior` / `On`
- `ALTER TABLE ... ADD [COLUMN] IF NOT EXISTS ...`（#1875）
- `ALTER TABLE ... DROP/MODIFY [COLUMN] IF EXISTS ...`（#2112）
- `ALTER TABLE ... MODIFY col NULL | NOT NULL`（#599，仅改可空性）
- `ALTER` 列定义保留类型后规格（如 `MODIFY c VARCHAR(10) NOT NULL`）

**新增 / 修复能力（T124 方言专项）**：
- SQL Server `INSERT BULK table[(col type,...)] [WITH(...)]`（#2033）— `Insert.Bulk` / `BulkColumnDefinitions` / `BulkWithOptions`
- Oracle `USING INDEX [name] TABLESPACE ts`（#2039）— `UsingIndexTablespace`
- SQL Server `CREATE INDEX ... WITH (PAD_INDEX=OFF, ...)`（#2020）— `CreateIndex.WithOptions`
- MySQL 前缀索引 `KEY idx (col(10))`
- 词法修复：SQL Server `[quoted]` 标识符不再吞掉中间 `]`（修复 `[a] int,[b] tinyint` 被并成一个 token）

**探针核实已支持（零源码改动）**：
- MySQL `0xFF` 十六进制字面量（#2435）
- `LIMIT (SELECT ...)` 子查询（#2359）
- Oracle 外连接 `(+)`（#672）
- MySQL 函数索引 `KEY idx ((LOWER(name)))`（#1927）
- `FOR XML PATH` / STUFF（#386）

**新增 / 修复能力（T125 分区 / interval / filegroup）**：
- MySQL `PARTITION BY RANGE/HASH/... (PARTITION ...)` 完整分区定义列表（#1668）
- `ALTER TABLE ... ADD PARTITION` 多分区一次添加（#1668）
- `INTERVAL expr DAY TO SECOND` / `EXTRACT(... FROM expr DAY TO SECOND)`（#673）
- SQL Server `CREATE TABLE ... ON PRIMARY` / `ON [filegroup]`（#2020 剩余）
- Hive/Spark `LATERAL VIEW ... AS c1,c2,c3,...` 多列别名（#2433，核实已支持）

**新增 / 修复能力（T126 高价值清仓）**：
- ODBC `{fn ...}` / `{d|t|ts '...'}`（#1139）
- MySQL `unsigned` / `signed` / `zerofill` 类型修饰
- SQL Server `IDENTITY(seed,inc)`、`ADD CONSTRAINT ... DEFAULT ... FOR col`、`EXEC ... @out OUTPUT`（#268）
- `CREATE OR ALTER FUNCTION/PROCEDURE`（#1978）、`DROP FUNCTION/PROCEDURE`
- Hive `INSERT OVERWRITE TABLE ... PARTITION (...)`（#1846/#2119）
- Oracle `XMLPARSE` / `XMLSERIALIZE`（#2146/#1564）

**测试**：`DdlUpstream/Batch2/3` + `HighValueCleanupRoundTripTest`；相关探针与回归通过。

### 1.0.0-beta9

正则 / 模式匹配模型对齐上游（JSqlParser 5.4）。**本版本含破坏性 API 变更**，从 beta8 升级需按下表同步代码。

**背景**：beta8 周期为两段真实业务 SQL 补结构化提取测试时，发现 PostgreSQL 正则符号 `~`/`!~` 被误解析为 `=`。深入对比上游 `JSqlParserCC.jjt:6833-6837` + `LikeExpression.java` 后定位到**迁移时模型走样 5 处**，本次彻底纠正。

**破坏性变更（编译期需同步）**：
- **`RegExpMatchOperator` 字段重构**：`string Operator` + `bool Not` 两字段删除，改为 `RegExpMatchOperatorType OperatorType` 枚举（`MatchCaseSensitive`/`MatchCaseInsensitive`/`NotMatchCaseSensitive`/`NotMatchCaseInsensitive` 四态）；构造从无参改为 `new RegExpMatchOperator(RegExpMatchOperatorType)` 必填。**外部访问 `.Operator`/`.Not` 的代码需改为 `.OperatorType`**。
- **`SimilarToExpression` 类删除**：合并到 `LikeExpression(KeyWord.SimilarTo)`。**外部 `new SimilarToExpression()` / `is SimilarToExpression` 需改为 `LikeExpression`**（`SIMILAR TO` 的 round-trip 行为不变）。
- **`Matches.OperatorSymbol`**：从 `"~"` 改为 `"@@"`（对齐上游，`Matches` 类对应 PostgreSQL 全文匹配 `@@`，此前误写为 `~`）。**外部若依赖旧值需改**（实际无 grammar 产出该类，影响面极小）。
- **`LikeExpression` 字段扩展**：新增 `KeyWord` 枚举（`Like`/`Ilike`/`Rlike`/`Regexp`/`RegexpLike`/`SimilarTo`/`MatchAny`/`MatchAll`/`MatchPhrase`/`MatchPhrasePrefix`/`MatchRegexp` 共 11 态）+ `LikeKeyWord` 字段（默认 `Like`）+ `Escape` 字段。**外部 `is LikeExpression` 后按关键字判断的代码需改**：此前 `ILIKE`/`REGEXP` 关键字信息丢失（统一记成 LIKE），现在通过 `LikeKeyWord` 区分。
- **REGEXP / RLIKE / REGEXP_LIKE 的 AST 类型变更**：从 `RegExpMatchOperator` 改为 `LikeExpression`（对齐上游，关键字形式归 `LikeExpression`）。**外部 `is RegExpMatchOperator` 断言 REGEXP 的代码需改为 `is LikeExpression && LikeKeyWord==Regexp`**。

**非破坏性变更（向后兼容）**：
- PostgreSQL 正则符号 `~`/`~*`/`!~`/`!~*` 现正确解析为 `RegExpMatchOperator`（此前被误解析为 `EqualsTo`，是本次治理的起点）。
- `LIKE`/`ILIKE`/`SIMILAR TO` 现支持 `ESCAPE` 子句（`LikeExpression.Escape` 字段，grammar 已支持、visitor 此前丢弃）。
- `ExpressionVisitorAdapter` 修复 `Matches`/`RegExpMatchOperator` 的 Visit 空实现 bug（此前 `=> default!` 不递归子节点，现改为 `VisitBinary` 正确下钻）。
- `ExpressionVisitor`/`ExpressionVisitorAdapter`/`ExpressionDescendantsWalker`/`TablesNamesFinder` 同步移除 `Visit(SimilarToExpression)`（类已删）。

**Obsolete 兼容层清理（beta9 二次迭代，版本号不变）**：
- 移除 beta6-beta8 引入的全部 `[Obsolete]` 转发壳，对外统一使用 C# 风格 API：
  - 解析入口：`CCJSqlParserUtil`（整类删除）→ 统一用 `SqlParser`。
  - `BinaryExpression.GetStringExpression()` → `OperatorSymbol` 属性。
  - `SimpleNode.JjtGetFirstToken/JjtGetLastToken` → `GetFirstToken/GetLastToken`。
  - `FromItem.GetAlias()/SetAlias()`（6 个实现类：Table/JsonTable/LateralView/ParenthesedSelect/TableFunction/Values）→ `Alias` 属性。
  - `Select.GetForUpdateTable()` → `ForUpdateTable`；`Select.GetSelectBody()` 删除（用具体子类型）；`ForUpdateClause.GetFirstTable()` → `FirstTable`。
  - `TablesNamesFinder.GetTables()` 公开方法删除（内部改为 `Traverse`，仅供 `GetTableNames()` 扩展方法复用）→ 统一用 `stmt.GetTableNames()`。
  - `StatementExtension.ExtractTableNames()` → `GetTableNames()`。
  - `Validation.GetParsedStatements()/GetErrors()` → `ParsedStatements`/`Errors` 属性。
- 受影响的外部代码：凡是引用上述 API 的位置都需改写（具体迁移路径见 MIGRATION.md）。
- `IFromItem` 接口签名不变；`TablesNamesFinder` 类本身及所有 visitor 方法保留。

**测试**：全量 1566 项 × 3 TFM（net8/9/10）全部通过，0 失败。

### 1.0.0-beta8

C# 风格化治理收口 + 多目标框架支持。**本版本含多处破坏性 API 变更**，从 beta7 升级需按下表同步代码。

**多目标框架**：
- `TargetFrameworks` 由 `net10.0` 扩展为 `net8.0;net9.0;net10.0`，net8/9 项目可直接引用。
- 显式 `<LangVersion>latest</LangVersion>` 统一三 TFM 语言版本。

**破坏性变更（编译期需同步）**：
- **解析入口改名**：新增 `SqlParser`（推荐入口），`CCJSqlParserUtil` 改为 `[Obsolete]` 转发壳（beta9 二次迭代中删除，仅保留 `SqlParser`）。新代码用 `SqlParser.Parse/ParseStatements/ParseExpression/ParseCondExpression/ParseNullable`。
- **接口加 `I` 前缀**（C# 命名规范）：`ExpressionVisitor<T>`→`IExpressionVisitor<T>`、`StatementVisitor<T>`→`IStatementVisitor<T>`、`SelectVisitor<T>`→`ISelectVisitor<T>`、`Expression`→`IExpression`、`Statement`→`IStatement`、`ASTNodeAccess`→`IASTNodeAccess`、`Model`→`IModel`、`FromItem`→`IFromItem`（共 8 个接口；外部 visitor 实现需同步改接口名）。
- **枚举值 SCREAMING_SNAKE_CASE → PascalCase**（共 60 值）：`ForMode`（UPDATE→Update 等 6 值）、`AlterOperation`（ADD→Add 等 47 值）、`ReturningReferenceType`（OLD→Old/NEW→New）、`DateTimeType`（DATE→Date/DATETIME→Datetime 等 5 值）。外部 switch case 与枚举名引用需全替换。
- **`null!` 字段治理 → `required`**：`BinaryExpression.LeftExpression/RightExpression` + 28 个 AST 类字段 + `WhereCondition.LeftExpression` DTO 改为 `required`，外部 `new X()` 无参构造需改为对象初始化器或带 `[SetsRequiredMembers]` 的构造。

**非破坏性变更（向后兼容）**（其中标注 `[Obsolete]` 的转发壳已于 beta9 二次迭代删除）：
- `BinaryExpression.GetStringExpression()` → `OperatorSymbol` 属性（旧方法 beta8 标 `[Obsolete]`，beta9 二次迭代删除）。
- `JjtGetFirstToken/JjtGetLastToken` → `GetFirstToken/GetLastToken`（旧名 beta8 标 `[Obsolete]`，beta9 二次迭代删除）。
- `OracleJoinSyntax` 从 `static class` 常量改为 `enum { None, Right, Left }`，`Column.OldOracleJoinSyntax` 字段类型同步。
- 6 个字面量叶子类（NullValue/LongValue/DoubleValue/StringValue/HexValue/BooleanValue）加 `sealed`；`Alias` 加 `sealed` + 值相等。
- `FromItem.GetAlias()/SetAlias()` → `Alias` 属性（6 实现类旧方法 beta8 标 `[Obsolete]`，beta9 二次迭代删除）。
- `Select.GetForUpdateTable()` → `ForUpdateTable` 属性；`ForUpdateClause.GetFirstTable()` → `FirstTable` 属性（旧名 beta8 标 `[Obsolete]`，beta9 二次迭代删除）。
- 删除 `ForUpdateClause` 的 5 个 SetXxx builder 方法、`PlainSelect.GetStringList<T>()` 死代码。
- `[NonSerialized]` 删除（本库无二进制序列化路径）。

**测试**：全量 1436 项 × 3 TFM（net8/9/10）全部通过，0 失败。

### 1.0.0-beta7

结构化提取扩展方法 + 中性 DTO，并经一轮库内审查整改（API 命名收敛、Descendants 覆盖完整性、WHERE 通用化）。

**新增扩展方法**（C# 风格，替代 visitor 副作用写法）：
- `GetTableReferences()` — FROM/JOIN/CTE 表引用（含别名与全名），返回中性 `TableReference`
- `GetSelectColumns()` — SELECT 列结构化，区分 * / t.* / 列 / 表达式，返回中性 `SelectColumn`
- `GetWhereConditions()` — WHERE AND/OR 树拍平为条件列表，返回中性 `WhereCondition`
- `GetTableNames()` — 全部表名（含 WHERE 子查询）；旧 `ExtractTableNames` 曾转发到本方法（beta9 二次迭代删除旧名）。

**审查整改**（beta7 合并内容）：
- **命名收敛**：`ExtractTableNames` → `GetTableNames`，动词统一 `Get`（beta9 二次迭代删除旧名转发，仅保留 `GetTableNames`）。
- **Descendants 覆盖完整性**：`ExpressionDescendantsWalker` 改为直接实现 `ExpressionVisitor<T>` 接口，编译期强制覆盖所有节点类型，杜绝约 12 个边缘类型（TrimFunction/CollateExpression/ArrayConstructor 等）静默漏覆盖。
- **WHERE 通用化**：二元运算符（继承 `BinaryExpression`）统一提取、新增自动覆盖；未识别单目运算符（IS NULL/EXISTS）兜底提取不丢弃。`WhereCondition.RightExpression` 改可空。
- **集合运算语义**：`GetSelectColumns` 对 UNION/INTERSECT/EXCEPT 按首个分支取列（文档明确）。

**边界**：业务约定（别名优先、虚拟列必填、单表启发式）与前端契约 DTO 装配留业务方，库只提供中性 DTO。

**测试**：新增结构化提取 + 整改回归共 60+ 项，全量 1438 项通过。

### 1.0.0-beta6

C# 风格遍历扩展方法，消除 visitor 副作用返回写法。

- **新增扩展方法**：`ExpressionExtension`（`Descendants<T>()` / `Walk<T>()`）与 `StatementExtension`（`ExtractTableNames()` / `Descendants<T>()` / `Walk<T>()`），底层复用已验证的 visitor 递归逻辑，AST 结构与 visitor 接口零改动。
- **消除 Java 味写法**：收集 AST 中某类节点无需再「定义 visitor 类 + new + Accept + 从字段掏结果」，直接 `expr.Descendants<Column>().ToList()`，有返回值、可接 LINQ。
- **Obsolete 标记**：`TablesNamesFinder.GetTables()` 在 beta6 标记 `[Obsolete]`，改用 `stmt.ExtractTableNames()`（beta7 进一步改名为 `GetTableNames()`；beta9 二次迭代删除 `GetTables()` 公开方法）。
- **架构同步**：`ARCHITECTURE.md` 新增「C# 风格遍历（推荐）」对照表，明确扩展方法是 Azrng 自有封装、上游同步无需对照。
- **测试**：新增 42 项扩展方法测试（含与旧 ColumnCollector/ParameterCollector/GetTables 的等价验证），全量 1399 项通过。

### 1.0.0-beta5

服务申请 SQL 参数场景修复版。

- **命名参数修复**：`@name` 现在解析为 `JdbcNamedParameter`，`Name` 返回 `name`，不再退化为普通 `JdbcParameter(?)` 导致变量名丢失。
- **参数前缀保真**：`JdbcNamedParameter` 新增 `Prefix` 字段，默认 `":"`；解析 `@name` 时为 `"@"`，`ToString()` 可保留原始前缀。
- **回归测试**：补充 `@name` 独立表达式与 `u.name = @name` 条件表达式测试。

### 1.0.0-beta4

完成 BL-06 方言专项 CREATE TABLE 全量移植（破坏性重构对齐上游 CreateTable 11 类模型），BL-01~05、BL-07~14 已全部完成，全部 backlog 清零。

**迁移基线**：上游 [JSqlParser](https://github.com/JSQLParser/JSqlParser) commit `2b141568`（5.4-SNAPSHOT，2026-04-12，`feat: add ForUpdateClause class with multi-table and ORDER BY support (#2426)`）；迁移起点 JSqlParser 5.4（tag `jsqlparser-5.4`，commit `7d2e6b65324ce5770681115202c47b6cb5412c1b`，2025-05-25）。**本版本迁移已完结**，无已知未迁移缺口。明确不迁移的项（经核查非缺口，属架构差异或等价实现）见 `ARCHITECTURE.md` 开头「迁移排除项」。

- **新增特性（BL-06 CREATE TABLE 方言与约束结构化）**：
  - **表级选项透传**：`ENGINE = InnoDB`、`CHARSET`/`COLLATE`/`COMMENT`/`AUTO_INCREMENT`/`ROW_FORMAT`、`PARTITION BY HASH(x) PARTITIONS n`、ClickHouse `ENGINE = MergeTree() ORDER BY id SAMPLE BY id`、`ORDER BY tuple()` 等全部以原始字符串透传到 `CreateTable.TableOptions`（保 round-trip，对齐上游 `tableOptionsStrings`）
  - **CREATE 子句选项**：`CREATE OR REPLACE`/`UNLOGGED`/`TEMPORARY`/`TEMP`/`GLOBAL`/`EXTERNAL`、`IF NOT EXISTS`（`CreateTable.CreateOptions`/`OrReplace`/`Unlogged`/`IfNotExists` 字段）
  - **CTAS / LIKE**：`CREATE TABLE t AS SELECT ...`（`Select` 字段）、`CREATE TABLE t (c1,c2) AS SELECT`（仅列名 `Columns` 字段）、`CREATE TABLE a LIKE b`（`LikeTable` 字段）
  - **约束结构化**：`CHECK (expr)`→`CheckConstraint`（持有 `Expression`）、`FOREIGN KEY ... REFERENCES t(c) ON DELETE CASCADE ON UPDATE SET NULL`→`ForeignKeyIndex`（持有 `ReferencedTable`/`ReferencedColumnNames`/`OnDelete`/`OnUpdate` `ReferentialAction`）、`EXCLUDE WHERE (expr)`→`ExcludeConstraint`
  - **列类型结构化**：`ColumnDefinition.DataType`(string) → `ColDataType`（`DataType`/`ArgumentsStringList`/`ArrayData`/`CharacterSet`，支持 `schema.type` 点号、数组维度 `text[]`、`set('a','b')` 字符串参数）
  - **STRUCT/ARRAY 复合列类型**（T089）：`col ARRAY<INT>`（尖括号，整体扁平化存 `DataType`，对齐上游；递归支持嵌套 `ARRAY<ARRAY<INT>>`）、`col STRUCT(x INT, y VARCHAR(100))`（圆括号，`DataType="STRUCT"`、字段列表进 `ArgumentsStringList`，对齐上游；支持嵌套 `STRUCT(x INT, y ARRAY<INT>)`）；Spanner 风格多 ARRAY 列
  - **列规格透传**：`NOT NULL`/`DEFAULT expr`/`AUTO_INCREMENT`/`GENERATED AS IDENTITY`/`COMMENT '...'`/`MATERIALIZED expr` 等收集到 `ColumnSpecs`（保 round-trip，对齐上游 `columnSpecs`）
  - **Oracle `ENABLE`/`DISABLE ROW MOVEMENT`**（`RowMovement`/`RowMovementMode`）
  - **Spanner `INTERLEAVE IN PARENT t [ON DELETE CASCADE|NO ACTION]`**（`SpannerInterleaveIn`）
- **破坏性 API 变更**：
  - `ColumnDefinition.DataType`(string) → `ColDataType`（结构化对象）——外部访问 `.DataType` 的代码需改为 `.ColDataType`
  - `Constraint` 新增子类层次：`ForeignKeyIndex`/`CheckConstraint`/`ExcludeConstraint` 继承 `Constraint`；`VisitTableConstraint` 对 FK/CHECK/EXCLUDE 返回对应子类（外部按 `Constraint` 类型断言的代码需改为 `OfType<ForeignKeyIndex>()` 等）
  - `CreateTable` 新增 `TableOptions`/`CreateOptions`/`Select`/`LikeTable`/`Columns`/`RowMovement`/`InterleaveIn`/`OrReplace`/`Unlogged`/`SelectParenthesis` 字段
- **新增保留字**：`ORDER`/`BY`/`SAMPLE`/`HASH`/`PARTITION` 在 `createParameterAtom` 上下文作表级选项关键字（这些已是保留 token，本次确认在 CREATE TABLE 表选项中可达）
- **CREATE TABLE 边缘遗留项清完（T090）**：`character varying(n)`/`character varying` 列类型、`TIMESTAMP WITH/WITHOUT [LOCAL] TIME ZONE` 后缀、MySQL 索引 `USING BTREE/HASH`/`COMMENT '...'` 选项（`Constraint.IndexOptions`）、功能性/表达式索引 `(expr)`、`set('a','b')` 类型、数组带尺寸 `int[5]`/`text[3][2]`、`::text[]` 数组类型 cast、表级 `WITH (fillfactor=70)`、Spanner 列级 `OPTIONS (k = true)`
- **SELECT 子句缺口修复（T091）**：
  - P0 静默丢弃修复：`WINDOW w AS (...)` 命名窗口（`PlainSelect.WindowDefinitions`）、`QUALIFY expr` 子句（`PlainSelect.Qualify`）——此前 grammar 已解析但 visitor 丢弃导致 round-trip 丢数据
  - P1 功能缺口：`GROUP BY ROLLUP(a,b)`/`CUBE(a,b)`/`GROUPING SETS(...)`/`WITH ROLLUP`（`GroupByElement` 扩展）、`START WITH ... CONNECT BY [NOCYCLE]` Oracle 层次查询（`OracleHierarchicalExpression`）、`SUBSTRING(x FROM 1 FOR 3)`/`POSITION(a IN b)`/`OVERLAY(x PLACING y FROM 1)` 命名参数（`NamedExpressionList` + `Function.NamedParameters`）、MSSQL `OUTPUT inserted.col [INTO ...]`（`Insert.OutputClause`）、`REFRESH MATERIALIZED VIEW [CONCURRENTLY] mv [WITH [NO] DATA]`（`RefreshMaterializedViewStatement`）、`UPSERT`/`REPLACE INTO`/`INSERT OR REPLACE`（`UpsertStatement`）
- **全量测试**：1217 通过（0 失败 0 跳过，较 beta3 净增 196），新增 `CreateTableRoundTripTest`（59 项）+ `SelectClauseRoundTripTest`（48 项）+ `UpstreamCoverageProbeTest`（84 项）
- **字段补齐（BL-18）**：COMMENT ON VIEW + COLUMN 多段列名、AnalyticType 四态（OVER/WITHIN_GROUP/WITHIN_GROUP_OVER/FILTER_ONLY，修复 WITHIN GROUP/FILTER 退化为 Function）、Column.CommentText/ArrayConstructor、Table.TimeTravelAfterAlias
- **长期对标缺口修复（T092）**：UPDATE/DELETE 修饰符（`LOW_PRIORITY`/`IGNORE`/`QUICK`）、CREATE VIEW 补齐（`TEMPORARY`/`RECURSIVE`/`WITH CHECK OPTION` + 修复 CHECK OPTION 位置 bug）、Hive/Spark `LATERAL VIEW [OUTER] function() AS col`、SQL Server JoinHint（`LOOP`/`HASH`/`MERGE`）、WithSearchClause 模型就绪、BEGIN TRANSACTION 支持
- **ALTER 字段结构化（T093）**：ALTER COLUMN 子句接线修复静默丢弃（`SetOperation`/`DropColumnOperation`），SET DATA TYPE/VISIBLE/INVISIBLE、CONVERT/CHARACTER SET 全方言覆盖
- **WithSearchClause grammar 接线（T094）**：标准递归 CTE 序列化子句 `SEARCH {BREADTH|DEPTH} FIRST BY cols SET seqcol` 从模型就绪升级为完整接线——grammar `withItem` 末尾接 `withSearchClause?`（此前注释称"破坏 LL 预测"经实测为误判，ANTLR4 LL(*) 可正常处理）、新增结构化 `WithSearchClause` 模型类（`SearchOrder`/`SearchColumns`/`SequenceColumnName`，替代原 `string?` 透传）、`AstBuilderVisitor.VisitWithSearchClause` 填充结构化字段、round-trip 保真。全量 1234 测试通过（净增 4 项 SEARCH 子句测试）
- **P4 小众方言批量补齐（T095）**：
  - BL-19b KSQL 窗口（ksqlDB）：新增 `KSQLWindow`（HOPPING/TUMBLING/SESSION）+ `KSQLJoinWindow`（WITHIN 单值/双值）模型类 + `KSQLTimeUnit` 枚举，`PlainSelect.KsqlWindow`/`EmitChanges` + `Join.JoinWindow` 字段，grammar `ksqlWindowClause`/`ksqlJoinWindowClause`/`ksqlEmitClause` 产生式（位置精确：窗口在 FROM/JOIN 后 WHERE 前、EMIT CHANGES 在 ORDER BY 后 LIMIT 前），visitor `VisitKsqlWindowSpec`/`BuildKsqlJoinWindow` 接线
  - BL-19c CREATE VIEW 方言：`FORCE`/`NO FORCE`/`SECURE`/`WITH READ ONLY` 字段扩展（`CreateView.Force?`/`Secure`/`WithReadOnly`），新增 `SECURE` token
  - BL-19e PivotXml（Oracle）：`Pivot.IsXml` 字段 + grammar `PIVOT XML?` 可选关键字 + visitor 接线
  - BL-19f ParenthesedFromItem alias 保真：修正括号 FROM 项兜底路径 `Visit(GetChild(0))` 丢失 alias 的缺陷，改为显式递归 `fromItem()` 并透传 alias
  - BL-19g ON DUPLICATE KEY UPDATE ... WHERE（MySQL 8.0.20+）：grammar `onDuplicateKey` 加可选 `whereClause`，`Insert.DuplicateUpdateWhereExpression` 字段 + visitor 接线
  - 全量 1254 测试通过（净增 20 项：KSQL 12 + CreateView 5 + PivotXml 1 + ParenthesedFromItem 1 + ON DUPLICATE WHERE 1）
- **P4 剩余方言清零（T096）**：全部 backlog 清零，简化透传版策略（与 LateralView/WindowDefinitions 风格一致）
  - BL-19d TableStatement（MySQL 8.2）：新增 `TableStatement`（继承 Select），`TABLE name [ORDER BY] [LIMIT] [OFFSET]`，复用 Select 基类 ORDER BY/LIMIT/OFFSET
  - BL-19a EXPORT/IMPORT（Exasol）：新增 `ExportStatement`/`ImportStatement`，destination/source 透传保 round-trip，EXPORT/IMPORT 提升为保留关键字
  - BL-19h-1 WITH FUNCTION（SQL 标准）：新增 `WithFunctionDeclaration`/`WithFunctionParameter` 模型，withItem 加 FUNCTION 分支（FUNCTION 从 nonReserved 移除避免 CTE 名冲突）
  - BL-19h-2 WITH ISOLATION（DB2）：`Select.Isolation` 字段 + grammar `WITH IDENTIFIER`（UR/RS/RR/CS 透传，保大小写）
  - BL-19h-3 FOR CLAUSE 透传扩展：`PlainSelect.ForClause` 字段（FOR BROWSE / FOR XML RAW|AUTO|EXPLICIT / FOR JSON AUTO|PATH 整体透传），向后兼容 FOR XML PATH 仍填充 ForXmlPath 字段
  - 全量 1275 测试通过（净增 21 项：TableStatement 4 + WITH ISOLATION 3 + FOR CLAUSE 5 + WITH FUNCTION 2 + EXPORT 4 + IMPORT 3）
- **VALUES 表构造器（T097）**：补齐唯一语法层缺口——新增 `Values` 模型类（继承 `Select`+`FromItem`）、grammar `selectBody` 增加 `valuesClause` 分支、`VisitValuesClause`/`VisitSelectBody` 接入、`SelectVisitor`/`TablesNamesFinder` 补 Values 分派、修复 INSERT/UPSERT VALUES 语义冲突。全量 1303 测试通过
- **库代码审查修复（T098）**：全量审查发现的 H1-H4/M1-M10/L1-L7 共 22 项缺陷全部修复——Merge 三连失（SourceTable/WHEN AND/InsertValues）、区域性数值解析静默数据损坏、多语句静默丢弃、TablesNamesFinder 表名提取遗漏、ExpressionVisitorAdapter context 丢失与子树遍历不全、JsonFunction null 非法 SQL、Validation 校验补全、CTE 括号、Offset ROWS、ASTNode 漏末 token、死代码/死变量清理等（M5 误报、M6 边缘跳过）。全量 1318 测试通过
- **补充回归测试（T099）**：为 T098 修复项补 25 项回归保护——ExpressionVisitorAdapter 子树遍历、JsonFunction null 路径、ParenthesedSelect 异常、Merge round-trip、Validation 能力校验。全量 1355 测试通过
- **本轮未做**：`PartitionDefinition` 不复用于 CREATE TABLE（上游该类仅服务 ALTER，CREATE TABLE 分区走 `TableOptions` 字符串透传）；Spanner 生成列 `SEARCH STRING(MAX) AS (UPPER(AUTHOR)) STORED` 的 STORED 后缀专项验证（AS 已解析，STORED 走兜底，列级 AS 与 DEFAULT 语义冲突需专项验证留后续）
- **Backlog 清零**：BL-01~14 全部完成，无已知缺口

### 1.0.0-beta3

完成 BL-13/BL-14 剩余子特性收口，同步核实并归档 BL-10/11/12，清理 BL-11 遗留死代码，修复 JSON 方言系列缺陷（BL-01/02/04/05），核实关闭 BL-03。

- **新增特性**：

- **新增特性**：
  - ClickHouse JOIN 修饰符 `GLOBAL`/`ANY`/`ALL`（`Join` 新增三个布尔字段，对齐上游 isGlobal/isAny/isAll）
  - Snowflake 时间旅行 `AT`/`BEFORE (TIMESTAMP|OFFSET|STATEMENT => expr)` 接线（grammar 新增 `timeTravelClause` 产生式，填 `Table.TimeTravel`）
  - JSON_QUERY Legacy 多 path 参数 `JSON_QUERY(input, path1, path2...)`（BL-01，接线 `AdditionalQueryPathArguments`）
  - JSON_TABLE Oracle/Trino 全量方言子句（BL-02）：函数级 `ON EMPTY`/`TYPE (STRICT|LAX)`/`FORMAT JSON` 输入/`PLAN`；列级 `EXISTS`/`FORMAT JSON`/`WRAPPER`/`QUOTES`/`SCALARS`/列级 `ON EMPTY`/`ON ERROR`
- **修复的静默丢弃缺陷**（grammar 此前已接受但 AST 丢语义，round-trip 会丢数据）：
  - ALTER 操作 14 处 round-trip 缺陷——`DROP PRIMARY/UNIQUE/FOREIGN KEY/CONSTRAINT`、`RENAME INDEX/KEY/CONSTRAINT`、`ENGINE`/`COMMENT`（含等号）、分区操作族（`ADD/DROP/TRUNCATE/COALESCE/REORGANIZE/EXCHANGE PARTITION` 此前只设 Operation 枚举不填结构化字段）、`ALTER SEQUENCE` 此前用 GetText 原样拼接导致空格丢失
  - `TimeTravelClause.ToString` 此前缺括号（`AT TIMESTAMP => x` 现修正为 `AT (TIMESTAMP => x)`）
  - `JSON_OBJECTAGG(foo, bar)` 逗号分隔静默退化为冒号（BL-04，对齐上游 MYSQL_OBJECT）
  - `JSON_OBJECT(foo:bar)` 无空格冒号此前解析失败（BL-05，原 backlog 误判为 JavaCC LOOKAHEAD 差异，实为 token 优先级冲突，grammar 接受 `S_JDBC_NAMED_PARAM` 作分隔符解决）
- **核实归档（状态同步，非新实现）**：经逐项核实，BL-10（`LATERAL`→`LateralSubSelect`）、BL-11（`DATE`/`TIMESTAMP` 字面量→`DateTimeLiteralExpression`）、BL-12（14 个语句类型）均已在 beta2 周期实现，仅 TASK.md backlog 未同步；本版本已将 backlog 状态修正为已完成。BL-03（聚合函数 OVER 窗口）经核实为非问题——`SUM(x) OVER(...)` 等已通过 `AnalyticExpression` 完整工作并有测试覆盖
- **破坏性 API 变更**：
  - 删除零实例化的 `DateValue`/`TimestampValue`/`TimeValue` 三个类及 `ExpressionVisitor<T>` 中对应 `Visit` 方法签名——外部直接实现 `ExpressionVisitor<T>` 接口的代码需移除这三个 Visit 方法（改用 `DateTimeLiteralExpression`）
  - `GLOBAL` 从 identifier 兜底列表移除（保留为关键字），消除 `table alias?` 贪婪吞掉 `GLOBAL JOIN` 的歧义——以 `global` 作列名/表名/别名的 SQL 将无法解析（对齐上游 `K_GLOBAL` 保留字行为）
  - `JsonTable.On*Behavior` 从 `string?` 升级为结构化 `JsonOnResponseBehavior`（BL-02）——外部按字符串断言需改为 `.Type`
  - 新增保留字 `LAX`/`SCALARS`/`ALLOW`/`DISALLOW`（BL-02 JSON_TABLE 方言）
- **ALTER 覆盖度澄清**：经 Explore 逐值核对，`AlterOperation` 枚举已 **47/47 全量对齐**上游（原 backlog "11/47" 记载过时）；上游不存在 `ALTER INDEX`/`ALTER SCHEMA` 语句类（用 `UnsupportedStatement` 兜底），非对齐缺口
- **全量测试**：1021 通过（0 失败 0 跳过，较 beta2 净增 160）
- **已知缺口**：见 `TASK.md`「待业务驱动 Backlog」BL-06（方言专项 CREATE TABLE，经核实确认工作量巨大，按具体方言逐项迁移）。BL-01~05、BL-07~14 已全部完成

### 1.0.0-beta2

增量对齐上游 5.4 → 5.4-SNAPSHOT HEAD，并修复两个影响 round-trip 的行为缺陷。

- **对齐上游 Commit**：`2b141568`（5.4-SNAPSHOT，2026-04-12）
- **对齐日期**：2026-07-07（见 `TASK.md` BL-15 对齐基线说明）
- **新增特性**：
  - 上游 5.4..HEAD 高价值变更 28 项（Oracle 外连接(+)、ALTER USING INDEX、MySQL 索引 ASC/DESC、CREATE SCHEMA、CONNECT_BY_ROOT、SessionStatement options 等）
  - F1-F8：PostGIS 几何距离算子（`<->`/`<#>`）、RangeExpression、TimeKeyExpression（CURRENT_DATE 等）、RawFunction、TranscodingFunction（CONVERT/TRY_CONVERT/SAFE_CONVERT）、INTO OUTFILE 格式化子句、JSON 表达式族（OBJECT/ARRAY/VALUE/EXISTS/QUERY/OBJECTAGG/ARRAYAGG）、JSON_TABLE 高级子句（PASSING/ON ERROR/NESTED PATH）
  - `FOR UPDATE` 多表 + ORDER BY 支持（ForUpdateClause）
  - 嵌套块注释词法（任意深度嵌套）
- **行为变更（破坏性）**：
  - 修复 `CASE WHEN searched` 形式序列化错误（`CASE WHEN a>1 THEN 'big' ELSE 'small' END` 此前被错误输出为 `CASE 'small' WHEN ... END`）
  - 修复嵌套块注释词法支持任意深度嵌套（`/* 外 /* 内 */ 外 */` 此前会抛 `JSqlParserException`）
- **全量测试**：861 通过（0 失败 0 跳过）
- **修复的静默丢弃缺陷**（grammar 此前已接受但 AST 丢语义，round-trip 会丢数据）：
  - `OVERLAPS` 谓词（`a OVERLAPS b`）—— 新增 `OverlapsCondition` 类并接线 visitor
  - `MEMBER OF` 谓词（`val MEMBER OF json_arr`）—— 补齐 visitor 分派、加 `NOT` 支持
  - `SELECT TOP n [PERCENT] [WITH TIES]` —— 新增 `Top` 类、`PlainSelect.Top` 字段并接线 visitor
- **已知缺口**：见 `TASK.md`「待业务驱动 Backlog」BL-01~06、BL-10~14（BL-07~09 已由 T080 修复）

### 1.0.0-beta1

从 JSqlParser 5.4 移植。

- 上游版本：JSqlParser 5.4
- 上游 Tag：`jsqlparser-5.4`
- 上游 Commit：`7d2e6b65324ce5770681115202c47b6cb5412c1b`
- 上游提交时间：2025-05-25
- 上游 Commit 说明：`feat: Session Statement`
