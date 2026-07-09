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

<PackageReference Include="Azrng.JSqlParser" Version="1.0.0-beta3" />
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

### 1.0.0-beta3

完成 BL-13/BL-14 剩余子特性收口，同步核实并归档 BL-10/11/12，清理 BL-11 遗留死代码，修复 JSON 方言系列缺陷（BL-01/02/04/05），核实关闭 BL-03。

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

移植自 [JSqlParser](https://github.com/JSQLParser/JSqlParser)（Apache License 2.0）。
