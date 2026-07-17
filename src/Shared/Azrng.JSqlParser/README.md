# Azrng.JSqlParser

原生 .NET SQL 解析器 — 从 [JSqlParser](https://github.com/JSQLParser/JSqlParser) **5.4** 版本移植而来，基于 ANTLR4 驱动。

将 SQL 解析为强类型 AST，支持 Visitor 模式遍历、表名提取、CNF 转换、SQL 校验以及 AST 反序列化为 SQL 文本。

## 快速开始

```csharp
using Azrng.JSqlParser;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

// 解析 SQL
var stmt = CCJSqlParserUtil.Parse("SELECT id, name FROM users WHERE age > 18");

if (stmt is PlainSelect select)
{
    Console.WriteLine(select.FromItem);   // users
    Console.WriteLine(select.Where);      // age > 18
}

// 提取表名
var tables = stmt.GetTableNames();
// => IReadOnlyCollection<string> { "users" }

// 反序列化为 SQL 文本
Console.WriteLine(stmt.ToString());
// => SELECT id, name FROM users WHERE age > 18
```

## 安装

直接引用项目或通过项目引用：

```xml

<PackageReference Include="Azrng.JSqlParser" Version="1.0.0-beta10" />
```

**依赖项：**
- `Antlr4.Runtime.Standard` 4.13.1

**目标框架：** `net10.0`

## API 概览

### 解析入口

所有入口均为 `CCJSqlParserUtil` 上的静态方法：

```csharp
using Azrng.JSqlParser.Parser;

// 单条语句
Statement? stmt = CCJSqlParserUtil.Parse("SELECT * FROM users");

// 多条语句
Statements? stmts = CCJSqlParserUtil.ParseStatements(
    "INSERT INTO users (name) VALUES ('Alice'); UPDATE users SET active = 1");

// 独立表达式
Expression? expr = CCJSqlParserUtil.ParseExpression("age BETWEEN 18 AND 65");

// 安全解析（失败返回 null，不抛异常）
Statement? stmt = CCJSqlParserUtil.ParseNullable("INVALID SQL");
```

### 错误处理

解析失败时抛出 `JSqlParserException`（继承自 `System.Exception`）：

```csharp
using Azrng.JSqlParser.Parser;

try
{
    var stmt = CCJSqlParserUtil.Parse("SELCT * FORM users");
}
catch (JSqlParserException ex)
{
    Console.WriteLine(ex.Message);
    // => "Syntax error: Line 1:0 - mismatched input 'SELCT' expecting ..."
}
```

如果不想处理异常，使用 `ParseNullable`：

```csharp
var stmt = CCJSqlParserUtil.ParseNullable("INVALID SQL");
if (stmt == null)
    Console.WriteLine("解析失败");
```

### 支持的 SQL 语句

| 分类 | 语句 |
|------|------|
| 查询 | `SELECT`（JOIN、CTE、UNION/INTERSECT/EXCEPT、子查询、窗口函数） |
| DML | `INSERT`、`UPDATE`、`DELETE`、`MERGE` |
| DDL | `CREATE TABLE/VIEW/INDEX`、`ALTER TABLE`、`DROP TABLE/VIEW/INDEX`、`TRUNCATE` |
| 事务 | `COMMIT`、`ROLLBACK`、`SAVEPOINT` |
| 会话 | `SET`、`USE`、`SHOW`、`DESCRIBE`、`EXPLAIN`、`SESSION START/APPLY/DROP/SHOW/DESCRIBE` |
| 管道查询 | BigQuery 风格 `\|>` 管道操作符（SELECT、WHERE、AGGREGATE、JOIN 等 17 种） |



### 提取表名

```csharp
using Azrng.JSqlParser;

var stmt = CCJSqlParserUtil.Parse("SELECT u.id FROM users u JOIN orders o ON u.id = o.uid")!;
IReadOnlyCollection<string> tables = stmt.GetTableNames();
// => { "users", "orders" }
```

> 旧的 `new TablesNamesFinder().GetTables(stmt)` 已标记 `[Obsolete]`，请改用 `stmt.GetTableNames()`。
> 动词统一为 `Get`，与 `GetTableReferences`/`GetSelectColumns`/`GetWhereConditions` 一致；
> 旧扩展方法 `ExtractTableNames` 已标记 `[Obsolete]`，转发到 `GetTableNames`。

### 遍历与收集 AST（C# 风格）

收集表达式中某类节点，无需再「定义一个 visitor 类 + new + Accept + 从字段掏结果」，直接用扩展方法：

```csharp
using Azrng.JSqlParser;

var stmt = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM t WHERE name = 'x' AND age > 18 AND status IN (:p1, :p2)")!;

// 收集 WHERE 中的所有列引用
var columns = stmt.Where!.Descendants<Column>().Select(c => c.ColumnName).ToList();
// => [ "name", "age", "status" ]

// 收集命名参数（替代自定义 ParameterCollector）
var paramNames = stmt.Where!.Descendants<JdbcNamedParameter>().Select(p => p.Name).ToList();
// => [ "p1", "p2" ]

// 就地遍历（推送委托）
stmt.Where!.Walk<Column>(c => Console.WriteLine(c.ColumnName));
```

底层遍历复用已验证的 visitor 递归逻辑；复杂自定义遍历仍可直接实现 visitor 接口（见 ARCHITECTURE.md）。

### 结构化提取（C# 风格）

把常用 AST 提取直接封装成扩展方法，返回中性 DTO，业务方负责套用自己的产品规则与 DTO 装配：

```csharp
using Azrng.JSqlParser;
using Azrng.JSqlParser.Models;

var select = (PlainSelect)CCJSqlParserUtil.Parse(
    "SELECT u.id, COUNT(*) AS cnt FROM users u JOIN orders o ON u.id = o.uid WHERE u.age > 18 AND o.status IN (1, 2)")!;

// 1. 表引用（含别名、全名）——仅 FROM/JOIN/CTE，不含 WHERE 子查询
IReadOnlyList<TableReference> tables = select.GetTableReferences();
// => [ {Name:users,Alias:u,Key:u}, {Name:orders,Alias:o,Key:o} ]

// 2. SELECT 列结构化（区分 * / t.* / 列 / 表达式）
IReadOnlyList<SelectColumn> columns = select.GetSelectColumns();
// => [ {Kind:Column,ColumnName:id,TableAlias:u}, {Kind:Expression,Alias:cnt} ]

// 3. WHERE 条件拍平（AND/OR 链 → 条件列表）
IReadOnlyList<WhereCondition> conds = select.Where!.GetWhereConditions();
// => [ {LinkType:"",Op:">",Left:u.age,Right:18}, {LinkType:"AND",Op:"IN",Left:o.status,Right:(1,2)} ]
```

> 三组方法返回的中性 DTO（`TableReference`/`SelectColumn`/`WhereCondition`）只描述 AST 事实，
> 不含产品业务字段（如虚拟列必填别名校验、列归属启发式、前端契约 DTO）——这些由业务方按需处理。
> `GetTableReferences` 仅遍历 FROM/JOIN/CTE；需要含 WHERE 子查询的全部表名时用 `GetTableNames`。
> `GetSelectColumns` 对 UNION/INTERSECT/EXCEPT 集合运算**仅取首个分支的列**（集合运算的输出列由第一个分支决定）；
> 若需各分支的列，请对 `SetOperationList.Selects` 逐个 `PlainSelect.GetSelectColumns()`。

### SQL 校验

```csharp
using Azrng.JSqlParser.Util.Validation;

var validation = new Validation(
    new List<FeaturesAllowed> { FeaturesAllowed.SELECT, FeaturesAllowed.JOIN },
    "DROP TABLE users");

List<ValidationError> errors = validation.Validate();
// => errors 包含 "DROP is not allowed"
```

### CNF 转换

```csharp
using Azrng.JSqlParser.Expression.Cnf;

var expr = CCJSqlParserUtil.ParseExpression("a = 1 AND (b = 2 OR c = 3)");
var cnf = CNFConverter.ConvertToCNF(expr);
// => (a = 1 OR b = 2) AND (a = 1 OR c = 3)
```

### AST 转 SQL（反序列化）

每个 AST 节点都重写了 `ToString()` 以生成 SQL 文本：

```csharp
var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users WHERE active = 1");
Console.WriteLine(stmt.ToString());
// => SELECT id FROM users WHERE active = 1
```

## SQL 特性覆盖

### 表达式

- 字面量：整数、浮点数、字符串、十六进制、布尔值、null
- 运算符：算术（`+`、`-`、`*`、`/`、`%`）、比较（`=`、`<>`、`>`、`<`、`>=`、`<=`）、逻辑（`AND`、`OR`、`NOT`、`XOR`）、字符串（`||`、`CONCAT`）、位运算
- 谓词：`LIKE`、`ILIKE`、`RLIKE`、`REGEXP`、`IN`、`BETWEEN`、`IS NULL`、`IS UNKNOWN`、`EXISTS`、`MEMBER OF`、`OVERLAPS`
- 高级：`CASE WHEN`、`CAST`、`EXTRACT`、`INTERVAL`、`COALESCE`、`NULLIF`、`LAMBDA`、`STRUCT`、`CONNECT BY PRIOR`、`HIGH`/`LOW`/`INVERSE`（Exasol）
- 函数：聚合（`COUNT`、`SUM`、`AVG`、`MIN`、`MAX`）、字符串、数学、窗口/分析函数
- 参数：`?`（位置参数）、`$1`（编号参数）、`:name` / `@name`（命名参数，`JdbcNamedParameter.Name` 均返回不含前缀的名称）

### 语句

- `SELECT` — DISTINCT/ALL、TOP（PERCENT/WITH TIES）、JOIN（INNER/LEFT/RIGHT/FULL/CROSS/NATURAL/SEMI）、CTE（WITH RECURSIVE，支持 DML）、UNION/INTERSECT/EXCEPT、子查询、GROUP BY、HAVING、WINDOW、PREFERRING（Exasol Skyline）、ORDER BY、LIMIT/OFFSET、FETCH、FOR UPDATE/SHARE（OF 多表、WAIT/NOWAIT/SKIP LOCKED）、`OVERLAPS`、`MEMBER OF`
- 管道查询 — `FROM table \|> WHERE ... \|> SELECT ...`（BigQuery 风格，17 种操作符）
- `INSERT` — 列列表、VALUES、INSERT...SELECT、INSERT OVERWRITE、PARTITION、ON DUPLICATE KEY、RETURNING
- `UPDATE` — SET、JOIN、FROM、WHERE、RETURNING
- `DELETE` — FROM、别名（DELETE u FROM ...）、USING、WHERE、RETURNING
- `MERGE` — WHEN MATCHED/NOT MATCHED、UPDATE/INSERT/DELETE
- `CREATE TABLE` — 列、约束、外键、LIKE、AS SELECT
- `CREATE VIEW/INDEX` — 带选项
- `ALTER TABLE` — ADD/DROP/MODIFY/CHANGE/RENAME COLUMN、ADD/DROP/TRUNCATE/COALESCE/REORGANIZE/EXCHANGE PARTITION、ADD CONSTRAINT、ENGINE/LOCK/ALGORITHM 选项
- `DROP TABLE/VIEW/INDEX` — IF EXISTS、CASCADE/RESTRICT
- `TRUNCATE`、`COMMIT`、`ROLLBACK`、`SAVEPOINT`、`SET`、`USE`、`SHOW`、`DESCRIBE`、`EXPLAIN`、`SESSION START/APPLY/DROP/SHOW/DESCRIBE`

## 版本历史

### 1.0.0-beta10

GetWhereConditions 通用化（方案 B：结构折叠 + 兜底）。

- **二元运算符统一覆盖**：所有继承 `BinaryExpression` 的运算符（=、>、<、LIKE、!=、加减乘除、位运算等）统一提取，新增二元运算符自动覆盖，不再逐个加 case。
- **未识别叶子兜底提取**：IS NULL/EXISTS 等单目运算符不再静默丢弃，作为单目条件提取（`RightExpression` 为 null，`Operator` 取类型名），不随方言/运算符枚举膨胀。
- **DTO 语义澄清**：`WhereCondition.RightExpression` 改为可空，单目条件时为 null。
- **测试**：新增 IS NULL/EXISTS 兜底提取与 AND 链不丢失验证，全量 1436 项通过。

### 1.0.0-beta9

Descendants 覆盖完整性修复。

- **根因修复**：`ExpressionDescendantsWalker` 此前继承 `ExpressionVisitorAdapter`，对约 12 个仅有接口默认实现的节点类型（TrimFunction/CollateExpression/ArrayConstructor 等）静默漏覆盖——`Descendants<TrimFunction>` 会错误返回空。
- **编译期保证**：walker 改为直接实现 `ExpressionVisitor<T>` 接口，所有节点类型的 Visit 方法显式实现；上游新增节点类型时若漏实现会编译失败，强制补全，杜绝静默漏覆盖。
- **回归测试**：新增 TrimFunction/CollateExpression 收集验证，全量 1433 项通过。

### 1.0.0-beta8

API 命名收敛：表名提取动词统一为 `Get`。

- **改名**：`ExtractTableNames` → `GetTableNames`，与 `GetTableReferences`/`GetSelectColumns`/`GetWhereConditions` 动词一致，消除"Extract 与 Get 并存"的分裂困惑。
- **向后兼容**：旧 `ExtractTableNames` 保留为 `[Obsolete]` 转发，存量代码零破坏。
- **文档同步**：README/ARCHITECTURE 当前描述统一为 `GetTableNames`；版本历史段落保留对各版本当时状态的真实记录。

### 1.0.0-beta7

结构化提取扩展方法，下沉下游常用的纯 AST 提取。

- **新增扩展方法**：`GetTableReferences()`（FROM/JOIN/CTE 表引用，含别名与全名）、`GetSelectColumns()`（SELECT 列结构化，区分 * / t.* / 列 / 表达式）、`GetWhereConditions()`（WHERE AND/OR 树拍平为条件列表）。
- **中性 DTO**：新增 `Models/TableReference`、`SelectColumn`、`WhereCondition`，只描述 AST 事实，不含产品业务约定与前端契约字段，业务方自行映射。
- **边界明确**：业务约定（别名优先、虚拟列必填、单表启发式）与 DTO 装配留业务方；`GetTableReferences` 仅 FROM/JOIN/CTE，含 WHERE 子查询的全部表名仍用 `ExtractTableNames`。
- **WHERE 增强**：递归穿透 `Parenthesis`（比下游 LocalSqlParser 原逻辑更完整，原逻辑会漏掉括号内复合条件）；`LikeExpression` 作为二元运算符被提取。
- **测试**：新增 32 项结构化提取测试，全量 1431 项通过。

### 1.0.0-beta6

C# 风格遍历扩展方法，消除 visitor 副作用返回写法。

- **新增扩展方法**：`ExpressionExtension`（`Descendants<T>()` / `Walk<T>()`）与 `StatementExtension`（`ExtractTableNames()` / `Descendants<T>()` / `Walk<T>()`），底层复用已验证的 visitor 递归逻辑，AST 结构与 visitor 接口零改动。
- **消除 Java 味写法**：收集 AST 中某类节点无需再「定义 visitor 类 + new + Accept + 从字段掏结果」，直接 `expr.Descendants<Column>().ToList()`，有返回值、可接 LINQ。
- **Obsolete 标记**：`TablesNamesFinder.GetTables()` 标记 `[Obsolete]`，改用 `stmt.ExtractTableNames()`。
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

## 许可证

本项目基于 [Apache License 2.0](LICENSE) 发布。

Azrng.JSqlParser 是从 [JSqlParser](https://github.com/JSQLParser/JSqlParser)（`Copyright (C) 2004-2024 JSQLParser`，上游双重许可 Apache 2.0 / LGPL 2.1）移植而来的 .NET 衍生作品。本库在发布时从上游双重许可中选择采用 **Apache License 2.0**。详见 [NOTICE](NOTICE)。
