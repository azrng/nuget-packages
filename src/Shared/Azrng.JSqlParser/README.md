# Azrng.JSqlParser

原生 .NET SQL 解析器 — 从 [JSqlParser](https://github.com/JSQLParser/JSqlParser) **5.4** 版本移植而来，基于 ANTLR4 驱动。

将 SQL 解析为强类型 AST，支持 Visitor 模式遍历、表名提取、CNF 转换、SQL 校验以及 AST 反序列化为 SQL 文本。

## 快速开始

```csharp
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
var tables = new TablesNamesFinder().GetTables(stmt);
// => HashSet<string> { "users" }

// 反序列化为 SQL 文本
Console.WriteLine(stmt.ToString());
// => SELECT id, name FROM users WHERE age > 18
```

## 安装

直接引用项目或通过项目引用：

```xml

<PackageReference Include="Azrng.JSqlParser" Version="1.0.0-beta1" />
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
using Azrng.JSqlParser.Util;

var finder = new TablesNamesFinder();
HashSet<string> tables = finder.GetTables(
    CCJSqlParserUtil.Parse("SELECT u.id FROM users u JOIN orders o ON u.id = o.uid")!);
// => { "users", "orders" }
```

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
- 参数：`?`（位置参数）、`$1`（编号参数）、`:name`（命名参数）、`@var`（会话参数）

### 语句

- `SELECT` — DISTINCT/ALL、TOP、JOIN（INNER/LEFT/RIGHT/FULL/CROSS/NATURAL/SEMI）、CTE（WITH RECURSIVE，支持 DML）、UNION/INTERSECT/EXCEPT、子查询、GROUP BY、HAVING、WINDOW、PREFERRING（Exasol Skyline）、ORDER BY、LIMIT/OFFSET、FETCH
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

### 1.0.0-beta1

从 JSqlParser 5.4 移植。

- 上游版本：JSqlParser 5.4
- 上游 Tag：`jsqlparser-5.4`
- 上游 Commit：`7d2e6b65324ce5770681115202c47b6cb5412c1b`
- 上游提交时间：2025-05-25
- 上游 Commit 说明：`feat: Session Statement`

## 许可证

移植自 [JSqlParser](https://github.com/JSQLParser/JSqlParser)（Apache License 2.0）。
