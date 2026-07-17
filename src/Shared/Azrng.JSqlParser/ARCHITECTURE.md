# 架构说明

## 迁移排除项（明确不迁移，非缺陷）

> 以下项经多轮核查（T088~T092）确认为**非缺口**，不计入未迁移范围。属于上游根本不存在、架构差异或等价实现，强行移植属负优化。

| 项 | 排除原因 |
|----|----------|
| Oracle MODEL 子句 | **上游根本不存在**（grammar 零命中），非 Azrng 缺口 |
| 子 visitor 适配层（FromItemVisitor/OrderByVisitor 等 7 套） | **架构差异**：Azrng 扁平 visitor（StatementVisitor/ExpressionVisitor）已覆盖功能，强行移植是负优化 |
| Skip/First/OptimizeFor/SampleClause | **等价实现**：Azrng 内联为 PlainSelect 字段/Table.TableSample，功能等价 |
| MySQLGroupConcat/UserVariable/VariableAssignment/AllValue/JsonExpression/XMLSerializeExpr | **等价合并**：已合入 Function.cs/SetStatement/AnyType.All 等 |
| CURRVAL / JSON_TRANSFORM | **上游不支持**：核查确认上游 main/ 无此特性 |
| Index/NamedConstraint 等枚举伴生类拆分 | **风格差异**：Azrng 用扁平 Constraint 类，功能等价 |
| Descendants 扩展方法 + GetTableNames | **C# 风格封装**：visitor 体系（Azrng 自有）之上的 LINQ 式外壳，上游无对应物；上游同步只对照 visitor 接口签名与 AST 节点结构，扩展方法无需对照 |
| 结构化提取（GetTableReferences/GetSelectColumns/GetWhereConditions + 中性 DTO） | **C# 风格封装**：把下游常用的纯 AST 提取下沉为库扩展方法，返回中性 DTO（TableReference/SelectColumn/WhereCondition）；不含产品业务约定与前端契约 DTO，业务方自行映射 |

---

## Visitor 模式

> **定位**：visitor 接口与 Adapter 是底层遍历机制（保留用于复杂自定义遍历与上游对照）；下文
> 「C# 风格遍历（推荐）」中的 `Descendants`/`GetTableNames` 是日常推荐使用的 LINQ 式封装。
> 日常收集/遍历需求请优先用扩展方法，避免「new visitor + Accept + 掏字段」的副作用式写法。
>
> **完整的"上游 Java → C#"API 对照**见 [MIGRATION.md](https://github.com/azrng/nuget-packages/blob/master/src/Shared/Azrng.JSqlParser/MIGRATION.md)，涵盖解析入口、遍历收集、
> 结构化提取、visitor 体系、类名差异。后续从上游同步新功能或改造旧 visitor 代码时，按该文档对照定位。

### C# 风格遍历（推荐）

扩展方法是 visitor 体系之上的 LINQ 式外壳，底层复用 Adapter 已验证的递归逻辑：

| 扩展方法 | 替代的旧写法 | 用途 |
|----------|------------|------|
| `expr.Descendants<T>()` | 自定义 visitor + Accept + 掏字段 | 拉取式收集表达式中某类节点，直接接 LINQ |
| `expr.GetWhereConditions()` | 手写 AND/OR 递归 + 运算符分类 | WHERE 树拍平为条件列表（返回中性 `WhereCondition`） |
| `stmt.GetTableNames()` | `new TablesNamesFinder().GetTables(stmt)` | 提取全部表名（含 WHERE 子查询），返回只读集合 |
| `stmt.GetTableReferences()` | 手写 FROM/JOIN 遍历 + 别名映射 | FROM/JOIN/CTE 的表引用（返回中性 `TableReference`，含别名/全名） |
| `select.GetSelectColumns()` | 手写 SELECT 项 switch 分类 | SELECT 列结构化（返回中性 `SelectColumn`，区分 * / t.* / 列 / 表达式） |
| `stmt.Descendants<T>()` | 自定义 StatementVisitor | 语句层收集（含嵌套子语句） |

```csharp
using Azrng.JSqlParser;

// 旧写法：定义 ColumnCollector 类 + new + Accept + 掏 Columns（无返回值、副作用式）
// 新写法：一行，有返回值
var columns = whereClause.Descendants<Column>().Select(c => c.ColumnName).ToList();
var paramNames = expr.Descendants<JdbcNamedParameter>().Select(p => p.Name).ToList();

// 提取表名
var tables = stmt.GetTableNames();
```

> **上游同步说明**：`Descendants`/`GetTableNames` 是 Azrng 自有封装，JSqlParser 上游无对应物。
> 与上游同步时，**只需对照 visitor 接口签名与 AST 节点结构**；扩展方法无需对照。
> `ExpressionDescendantsWalker` 直接实现 `ExpressionVisitor<T>` 接口，**编译期保证完整覆盖**所有节点类型——
> 上游新增节点类型时若 walker 漏实现会编译失败，强制补全，杜绝"某类节点静默不进 Descendants 结果"。

### 底层 Visitor 接口（复杂自定义遍历 / 上游对照）

Azrng.JSqlParser 提供三种 Visitor 接口，均采用双泛型签名 `T Visit<S>(X node, S context)`：

| Visitor | 用途 | 适配器基类 |
|---------|------|-----------|
| `ExpressionVisitor<T>` | 遍历表达式节点 | `ExpressionVisitorAdapter<T>` |
| `StatementVisitor<T>` | 遍历语句节点 | `StatementVisitorAdapter<T>` |
| `SelectVisitor<T>` | 遍历 SELECT body | 无（只有 3 个方法，直接实现即可） |

#### ExpressionVisitor — 收集表达式中的列名

```csharp
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

class ColumnCollector : ExpressionVisitorAdapter<object?>
{
    public List<string> Columns { get; } = new();

    public override object? Visit<S>(Column column, S context)
    {
        Columns.Add(column.ColumnName);
        return null;
    }
}

// 使用
var select = (PlainSelect)CCJSqlParserUtil.Parse(
    "SELECT id FROM users WHERE name = 'test' AND age > 18")!;

var collector = new ColumnCollector();
select.Where!.Accept(collector);

Console.WriteLine(string.Join(", ", collector.Columns));
// => "name, age"
```

#### StatementVisitor — 按语句类型分派处理

```csharp
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Update;
using Azrng.JSqlParser.Statement.Delete;
using Azrng.JSqlParser.Statement.Select;

class DmlRouter : StatementVisitorAdapter<object?>
{
    public override object? Visit<S>(Select select, S context)
    {
        Console.WriteLine("处理 SELECT");
        return null;
    }

    public override object? Visit<S>(Insert insert, S context)
    {
        Console.WriteLine($"INSERT INTO {insert.Table?.Name}");
        return null;
    }

    public override object? Visit<S>(Update update, S context)
    {
        Console.WriteLine($"UPDATE {update.Table?.Name}");
        return null;
    }

    public override object? Visit<S>(Delete delete, S context)
    {
        Console.WriteLine($"DELETE FROM {delete.Table?.Name}");
        return null;
    }
}

// 使用
var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (name) VALUES ('Alice')")!;
stmt.Accept(new DmlRouter());
// => "INSERT INTO users"
```

#### SelectVisitor — 处理不同 SELECT body 类型

```csharp
using Azrng.JSqlParser.Statement.Select;

class SelectInfo : SelectVisitor<object?>
{
    public object? Visit<S>(PlainSelect plainSelect, S context)
    {
        // 普通 SELECT：访问 plainSelect.SelectItems, .FromItem, .Where, .Joins 等
        return null;
    }

    public object? Visit<S>(SetOperationList setOpList, S context)
    {
        // UNION / INTERSECT / EXCEPT：遍历 setOpList.Selects
        return null;
    }

    public object? Visit<S>(WithItem withItem, S context)
    {
        // CTE 定义：访问 withItem.Name, withItem.Select
        return null;
    }

    public object? Visit<S>(Piped.FromQuery fromQuery, S context)
    {
        // BigQuery 管道查询：访问 fromQuery.FromItem, .PipeOperators
        return null;
    }
}
```

### 常用 AST 访问

#### SELECT — 获取列、表、WHERE、JOIN

```csharp
var select = (PlainSelect)CCJSqlParserUtil.Parse(
    "SELECT u.id, u.name FROM users u INNER JOIN orders o ON u.id = o.user_id WHERE u.active = 1")!;

// 列
foreach (var item in select.SelectItems!)
    Console.WriteLine(item.Expression); // u.id, u.name

// 主表
var table = (Table)select.FromItem!;
Console.WriteLine(table.Name);          // users
Console.WriteLine(table.Alias?.Name);   // u

// JOIN
foreach (var join in select.Joins!)
{
    var joinTable = (Table)join.RightItem;
    Console.WriteLine($"JOIN {joinTable.Name} ON {join.OnExpression}");
}

// WHERE
Console.WriteLine(select.Where);        // u.active = 1
```

#### INSERT — 获取目标表和列

```csharp
var insert = (Insert)CCJSqlParserUtil.Parse(
    "INSERT INTO users (name, email) VALUES ('Alice', 'alice@example.com')")!;

Console.WriteLine(insert.Table?.Name);  // users
foreach (var col in insert.Columns!)
    Console.WriteLine(col.ColumnName);   // name, email
```

#### UPDATE — 获取 SET 子句

```csharp
var update = (Update)CCJSqlParserUtil.Parse(
    "UPDATE users SET name = 'Bob', active = 0 WHERE id = 1")!;

Console.WriteLine(update.Table?.Name);  // users
foreach (var set in update.UpdateSets)
{
    foreach (var col in set.Columns)
        Console.Write($"{col.ColumnName} = ");
    Console.WriteLine(string.Join(", ", set.Values));
}
Console.WriteLine(update.Where);        // id = 1
```

#### DELETE — 获取表和条件

```csharp
var delete = (Delete)CCJSqlParserUtil.Parse(
    "DELETE FROM users WHERE active = 0")!;

Console.WriteLine(delete.Table?.Name);  // users
Console.WriteLine(delete.Where);        // active = 0
```

#### 修改 WHERE 后生成新 SQL

```csharp
var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users WHERE active = 1")!;

// 修改 WHERE 子句
select.Where = CCJSqlParserUtil.ParseExpression("active = 1 AND role = 'admin'")!;

Console.WriteLine(select.ToString());
// => SELECT id FROM users WHERE active = 1 AND role = 'admin'
```
## 架构

```
CCJSqlParserUtil.Parse(sql)
        |
        v
  ANTLR4 Lexer/Parser  (JSqlParserGrammar.g4)
        |
        v
  AstBuilderVisitor    (解析树 → 强类型 AST)
        |
        v
  Statement / Expression  (强类型 C# 对象)
        |
        +-- Accept(StatementVisitor)   — 遍历语句（底层机制）
        +-- Accept(ExpressionVisitor)  — 遍历表达式（底层机制）
        +-- Descendants/GetTableNames — LINQ 式遍历扩展（C# 风格，推荐）
        +-- ToString()                 — 反序列化为 SQL
        +-- TablesNamesFinder          — 提取表名（内部，对外用 GetTableNames）
        +-- CNFConverter               — WHERE 子句规范化
        +-- Validation                 — 校验允许的 SQL 特性
```

### 命名空间结构

| 命名空间 | 用途 |
|----------|------|
| `Azrng.JSqlParser` | C# 风格遍历扩展方法（`ExpressionExtension`/`StatementExtension`：Descendants/GetTableNames/GetTableReferences/GetSelectColumns/GetWhereConditions） |
| `Azrng.JSqlParser.Models` | 结构化提取的中性 DTO（`TableReference`/`SelectColumn`/`WhereCondition` 等） |
| `Azrng.JSqlParser.Parser` | `CCJSqlParserUtil` 入口、AST 节点基类 |
| `Azrng.JSqlParser.Expression` | 所有表达式类型（字面量、运算符、函数、参数） |
| `Azrng.JSqlParser.Expression.Operators.*` | 算术、条件、关系运算符 |
| `Azrng.JSqlParser.Expression.Cnf` | CNF 转换 |
| `Azrng.JSqlParser.Statement` | 所有语句类型、`StatementVisitor` |
| `Azrng.JSqlParser.Statement.Select` | SELECT 层级结构（`PlainSelect`、`Join`、`WithItem` 等） |
| `Azrng.JSqlParser.Statement.Piped` | BigQuery 风格管道查询（`FromQuery`、`PipeOperator` 及 17 种操作符） |
| `Azrng.JSqlParser.Schema` | `Table`、`Column`、`Database`、`Index`、`Sequence` 等 |
| `Azrng.JSqlParser.Util` | `TablesNamesFinder` |
| `Azrng.JSqlParser.Util.Validation` | SQL 校验框架 |