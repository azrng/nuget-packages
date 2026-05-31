### Visitor 模式

JSqlParser.Net 提供三种 Visitor 接口，均采用双泛型签名 `T Visit<S>(X node, S context)`：

| Visitor | 用途 | 适配器基类 |
|---------|------|-----------|
| `ExpressionVisitor<T>` | 遍历表达式节点 | `ExpressionVisitorAdapter<T>` |
| `StatementVisitor<T>` | 遍历语句节点 | `StatementVisitorAdapter<T>` |
| `SelectVisitor<T>` | 遍历 SELECT body | 无（只有 3 个方法，直接实现即可） |

#### ExpressionVisitor — 收集表达式中的列名

```csharp
using JSqlParser.Net.Expression;
using JSqlParser.Net.Schema;

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
using JSqlParser.Net.Statement;
using JSqlParser.Net.Statement.Insert;
using JSqlParser.Net.Statement.Update;
using JSqlParser.Net.Statement.Delete;
using JSqlParser.Net.Statement.Select;

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
using JSqlParser.Net.Statement.Select;

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
        +-- Accept(StatementVisitor)   — 遍历语句
        +-- Accept(ExpressionVisitor)  — 遍历表达式
        +-- ToString()                 — 反序列化为 SQL
        +-- TablesNamesFinder          — 提取表名
        +-- CNFConverter               — WHERE 子句规范化
        +-- Validation                 — 校验允许的 SQL 特性
```

### 命名空间结构

| 命名空间 | 用途 |
|----------|------|
| `JSqlParser.Net.Parser` | `CCJSqlParserUtil` 入口、AST 节点基类 |
| `JSqlParser.Net.Expression` | 所有表达式类型（字面量、运算符、函数、参数） |
| `JSqlParser.Net.Expression.Operators.*` | 算术、条件、关系运算符 |
| `JSqlParser.Net.Expression.Cnf` | CNF 转换 |
| `JSqlParser.Net.Statement` | 所有语句类型、`StatementVisitor` |
| `JSqlParser.Net.Statement.Select` | SELECT 层级结构（`PlainSelect`、`Join`、`WithItem` 等） |
| `JSqlParser.Net.Statement.Piped` | BigQuery 风格管道查询（`FromQuery`、`PipeOperator` 及 17 种操作符） |
| `JSqlParser.Net.Schema` | `Table`、`Column`、`Database`、`Index`、`Sequence` 等 |
| `JSqlParser.Net.Util` | `TablesNamesFinder` |
| `JSqlParser.Net.Util.Validation` | SQL 校验框架 |