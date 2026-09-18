# Azrng.JSqlParser

原生 .NET SQL 解析器 — 从 [JSqlParser](https://github.com/JSQLParser/JSqlParser) **5.4** 版本移植而来，基于 ANTLR4 驱动。

将 SQL 解析为强类型 AST，支持 Visitor 模式遍历、表名提取、CNF 转换、SQL 校验以及 AST 反序列化为 SQL 文本。

> 从 JSqlParser（Java 上游）迁移、或改造旧 visitor 写法？见 [MIGRATION.md](https://github.com/azrng/nuget-packages/blob/master/src/Shared/Azrng.JSqlParser/MIGRATION.md)（上游 → C# API 对照）。
> 架构与 visitor 体系说明见 [ARCHITECTURE.md](https://github.com/azrng/nuget-packages/blob/master/src/Shared/Azrng.JSqlParser/ARCHITECTURE.md)。

## 快速开始

```csharp
using Azrng.JSqlParser;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

// 解析 SQL
var stmt = SqlParser.Parse("SELECT id, name FROM users WHERE age > 18");

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

```xml
<PackageReference Include="Azrng.JSqlParser" Version="1.0.0-rc2" />
```

或 `dotnet add package Azrng.JSqlParser --version 1.0.0-rc2`

**依赖项：**
- `Antlr4.Runtime.Standard` 4.13.1

**目标框架：** `net8.0` / `net9.0` / `net10.0`

## API 概览

### 解析入口

所有入口均为 `SqlParser` 上的静态方法：

```csharp
using Azrng.JSqlParser.Parser;

// 单条语句
Statement? stmt = SqlParser.Parse("SELECT * FROM users");

// 多条语句
Statements? stmts = SqlParser.ParseStatements(
    "INSERT INTO users (name) VALUES ('Alice'); UPDATE users SET active = 1");

// 独立表达式
Expression? expr = SqlParser.ParseExpression("age BETWEEN 18 AND 65");

// 安全解析（失败返回 null，不抛异常）
Statement? stmt = SqlParser.ParseNullable("INVALID SQL");
```

### 错误处理

解析失败时抛出 `JSqlParserException`（继承自 `System.Exception`）：

```csharp
using Azrng.JSqlParser.Parser;

try
{
    var stmt = SqlParser.Parse("SELCT * FORM users");
}
catch (JSqlParserException ex)
{
    Console.WriteLine(ex.Message);
    // => Syntax error: Line 1:6 near '*' - mismatched input '*' expecting {ALL, ANY, CROSS, ...}
}
```

如果不想处理异常，使用 `ParseNullable`：

```csharp
var stmt = SqlParser.ParseNullable("INVALID SQL");
if (stmt == null)
    Console.WriteLine("解析失败");
```

### 支持的 SQL 语句

| 分类 | 语句 |
|------|------|
| 查询 | `SELECT`（JOIN、CTE、UNION/INTERSECT/EXCEPT、子查询、窗口函数） |
| DML | `INSERT`、`UPDATE`、`DELETE`、`MERGE` |
| DDL | `CREATE TABLE/VIEW/INDEX/DATABASE/SCHEMA`、`ALTER TABLE`、`DROP TABLE/VIEW/INDEX`（多对象）、`TRUNCATE` |
| 事务 | `COMMIT`、`ROLLBACK`、`SAVEPOINT` |
| 会话 | `SET`、`USE`、`SHOW`、`DESCRIBE`、`EXPLAIN`、`SESSION START/APPLY/DROP/SHOW/DESCRIBE` |
| 管道查询 | BigQuery 风格 `\|>` 管道操作符（SELECT、WHERE、AGGREGATE、JOIN 等 17 种） |

### 提取表名

```csharp
using Azrng.JSqlParser;

var stmt = SqlParser.Parse("SELECT u.id FROM users u JOIN orders o ON u.id = o.uid")!;
IReadOnlyCollection<string> tables = stmt.GetTableNames();
// => { "users", "orders" }
```

> 动词统一为 `Get`，与 `GetTableReferences`/`GetSelectColumns`/`GetWhereConditions` 一致。
> 早期版本曾存在 `new TablesNamesFinder().GetTables(stmt)` / `stmt.ExtractTableNames()` 公开 API，已于后续版本删除，统一收敛到 `stmt.GetTableNames()`。

### 遍历与收集 AST（C# 风格）

收集表达式中某类节点，无需再「定义一个 visitor 类 + new + Accept + 从字段掏结果」，直接用扩展方法：

```csharp
using Azrng.JSqlParser;

var stmt = (PlainSelect)SqlParser.Parse("SELECT id FROM t WHERE name = 'x' AND age > 18 AND status IN (:p1, :p2)")!;

// 收集 WHERE 中的所有列引用
var columns = stmt.Where!.Descendants<Column>().Select(c => c.ColumnName).ToList();
// => [ "name", "age", "status" ]

// 收集命名参数（替代自定义 ParameterCollector）
var paramNames = stmt.Where!.Descendants<JdbcNamedParameter>().Select(p => p.Name).ToList();
// => [ "p1", "p2" ]

// 就地遍历：直接 foreach LINQ 结果即可，无需额外 API
foreach (var c in stmt.Where!.Descendants<Column>())
    Console.WriteLine(c.ColumnName);
```

底层遍历复用已验证的 visitor 递归逻辑；复杂自定义遍历仍可直接实现 visitor 接口（见 ARCHITECTURE.md）。

### 结构化提取（C# 风格）

把常用 AST 提取直接封装成扩展方法，返回中性 DTO，业务方负责套用自己的产品规则与 DTO 装配：

```csharp
using Azrng.JSqlParser;
using Azrng.JSqlParser.Models;

var select = (PlainSelect)SqlParser.Parse(
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

var expr = SqlParser.ParseExpression("a = 1 AND (b = 2 OR c = 3)");
var cnf = CNFConverter.ConvertToCNF(expr);
// => (a = 1 OR b = 2) AND (a = 1 OR c = 3)
```

### AST 转 SQL（反序列化）

每个 AST 节点都重写了 `ToString()` 以生成 SQL 文本：

```csharp
var stmt = SqlParser.Parse("SELECT id FROM users WHERE active = 1");
Console.WriteLine(stmt.ToString());
// => SELECT id FROM users WHERE active = 1
```

## SQL 特性覆盖

### 表达式

- 字面量：整数、浮点数、字符串、十六进制、布尔值、null
- 运算符：算术（`+`、`-`、`*`、`/`、`%`）、比较（`=`、`<>`、`>`、`<`、`>=`、`<=`）、逻辑（`AND`、`OR`、`NOT`、`XOR`）、字符串（`||`、`CONCAT`）、位运算
- 谓词：`LIKE`、`ILIKE`、`RLIKE`、`REGEXP`、`REGEXP_LIKE`、`SIMILAR TO`、PostgreSQL 正则符号（`~`、`~*`、`!~`、`!~*`）、`IN`、`ANY`/`ALL`/`SOME`（支持子查询、数组表达式和命名数组参数）、`BETWEEN`、`IS NULL`、`IS UNKNOWN`、`EXISTS`、`MEMBER OF`、`OVERLAPS`
- 高级：`CASE WHEN`、`CAST`、`EXTRACT`（含 `DAY TO SECOND` 限定）、`INTERVAL`（含 `DAY TO SECOND`）、`COALESCE`、`NULLIF`、`LAMBDA`、`STRUCT`、`CONNECT BY PRIOR`、`HIGH`/`LOW`/`INVERSE`（Exasol）
- 函数：聚合（`COUNT`、`SUM`、`AVG`、`MIN`、`MAX`）、字符串、数学、窗口/分析函数；Oracle `XMLPARSE` / `XMLSERIALIZE`；ODBC `{fn ...}` / `{d|t|ts '...'}`
- 参数：`?`（位置参数）、`$1`（编号参数）、`:name` / `@name`（命名参数，`JdbcNamedParameter.Name` 返回不含前缀的名称，`Prefix` 字段保留原始前缀 `:`/`@`）

### 语句

- `SELECT` — DISTINCT/ALL、TOP（PERCENT/WITH TIES）、JOIN（INNER/LEFT/RIGHT/FULL/CROSS/NATURAL/SEMI）、CTE（WITH RECURSIVE，支持 DML，递归 `SEARCH`/`CYCLE` 子句 #2566）、UNION/INTERSECT/EXCEPT、子查询、GROUP BY、HAVING、WINDOW、PREFERRING（Exasol Skyline）、ORDER BY、LIMIT/OFFSET、FETCH、FOR UPDATE/SHARE（OF 多表、WAIT/NOWAIT/SKIP LOCKED）、`OVERLAPS`、`MEMBER OF`
- 管道查询 — `FROM table |> WHERE ... |> SELECT ...`（BigQuery 风格，17 种操作符）
- `INSERT` — 列列表、VALUES、INSERT...SELECT、`INSERT OVERWRITE [TABLE]`（Hive #1846）、PARTITION、ON DUPLICATE KEY、RETURNING、SQL Server `INSERT BULK`（#2033）、`OVERRIDING [USER|SYSTEM] VALUE`（#2569）
- `UPDATE` — SET、JOIN、FROM、WHERE、RETURNING
- `DELETE` — FROM、别名（DELETE u FROM ...）、USING、WHERE、RETURNING
- `MERGE` — WHEN MATCHED/NOT MATCHED、UPDATE/INSERT/DELETE、`NOT MATCHED BY TARGET/SOURCE`（#2421/#2480，含配对校验）、`RETURNING`（#2569）
- `CREATE TABLE` — 列、约束、外键、LIKE、AS SELECT、前缀索引 `col(n)`、函数索引、`unsigned`/`IDENTITY`、`PARTITION BY ... (PARTITION ...)`（#1668）、SQL Server `ON PRIMARY`
- `CREATE VIEW/INDEX` — 带选项；SQL Server `WITH (index_option=...)`（#2020）、`INCLUDE (cols)` 覆盖列（#2462）
- `CREATE [OR REPLACE|OR ALTER] FUNCTION/PROCEDURE`（#1978）
- `CREATE DATABASE` — `IF NOT EXISTS`（#2070）
- `CREATE SCHEMA` — `IF NOT EXISTS`、catalog.schema、AUTHORIZATION
- `ALTER TABLE` — ADD/DROP/MODIFY/CHANGE/RENAME COLUMN、ADD CONSTRAINT（含 `DEFAULT ... FOR col`、Oracle `USING INDEX TABLESPACE`）、分区操作、ENGINE/LOCK/ALGORITHM
- `DROP TABLE/VIEW/INDEX/FUNCTION/PROCEDURE` — 多对象、IF EXISTS、CASCADE/RESTRICT
- `EXECUTE`/`EXEC`/`CALL` — 括号或无括号参数、`OUTPUT`（#268）
- `TRUNCATE`、`COMMIT`、`ROLLBACK`、`SAVEPOINT`、`SET`（赋值、SQL Server `SET IDENTITY_INSERT t ON` #2605、`SET NOCOUNT ON` 布尔开关 #2604）、`USE`、`SHOW`、`DESCRIBE`、`EXPLAIN`、`SESSION START/APPLY/DROP/SHOW/DESCRIBE`

## 版本历史

当前版本为 `1.0.0-rc2`。

完整版本变更记录请查看 [CHANGELOG.md](https://github.com/azrng/nuget-packages/blob/master/src/Shared/Azrng.JSqlParser/CHANGELOG.md)。

## 许可证

本项目基于 [Apache License 2.0](LICENSE) 发布。

Azrng.JSqlParser 是从 [JSqlParser](https://github.com/JSQLParser/JSqlParser)（`Copyright (C) 2004-2024 JSQLParser`，上游双重许可 Apache 2.0 / LGPL 2.1）移植而来的 .NET 衍生作品。本库在发布时从上游双重许可中选择采用 **Apache License 2.0**。详见 [NOTICE](NOTICE)。
