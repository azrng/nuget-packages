# 迁移对照：JSqlParser（Java 上游）→ Azrng.JSqlParser（.NET）

> 本文档记录从 JSqlParser 5.4 上游（Java）迁移到 Azrng.JSqlParser（C#）时的 API 对照关系，
> 以及 Azrng 在上游之上的 C# 风格封装。**后续从上游同步新功能/方言时，按本表对照定位**；
> 当前项目（及下游业务方）改造旧 visitor 写法时，按本表查找对应的 C# 推荐 API。

## 一、上游同步原则

Azrng.JSqlParser 是从 JSqlParser 5.4 移植而来。两类内容需要与上游保持对照能力：

| 内容 | 是否需要对照上游 | 说明 |
|------|----------------|------|
| ANTLR4 grammar（`.g4`） | ✅ 需要 | 语法规则源头，上游升级 grammar 时需同步 |
| AST 节点类（字段、继承关系） | ✅ 需要 | `PlainSelect`/`Expression` 等结构，上游加字段时需同步 |
| `ExpressionVisitor<T>` 等接口签名 | ✅ 需要 | `AstBuilderVisitor`（grammar→AST 接线）依赖，接口方法变化需同步 |
| 扩展方法（Descendants/Get*） | ❌ 无需对照 | Azrng 自有 C# 封装，上游无对应物 |
| 中性 DTO（TableReference/SelectColumn/WhereCondition） | ❌ 无需对照 | Azrng 自有，业务约定与前端契约不进库 |

**关键**：上游同步只动 grammar + AST 节点 + visitor 接口；扩展方法是 Azrng 在这三者之上的 C# 外壳，
新增节点类型时扩展方法自动覆盖（Descendants 编译期保证）或按需加结构化提取，不破坏对照能力。

---

## 二、解析入口

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `CCJSqlParserUtil.parse(sql)` | `CCJSqlParserUtil.Parse(sql)` | 单语句，返回 `Statement?` |
| `CCJSqlParserUtil.parseStatements(sql)` | `CCJSqlParserUtil.ParseStatements(sql)` | 多语句，返回 `Statements?` |
| `CCJSqlParserUtil.parseExpression(sql)` | `CCJSqlParserUtil.ParseExpression(sql)` | 独立表达式 |
| `CCJSqlParserUtil.parseCondExpression(sql)` | `CCJSqlParserUtil.ParseCondExpression(sql)` | 条件表达式 |
| —（无） | `CCJSqlParserUtil.ParseNullable(sql)` | Azrng 新增：失败返回 null 不抛异常 |
| `JSQLParserException` | `JSqlParserException` | 异常类名去掉 SQL 大写风格 |

> 命名差异：Java 的 `parse`（小驼峰）→ C# 的 `Parse`（大驼峰）；异常类名 `JSQLParserException`（上游）→ `JSqlParserException`（Azrng）。

---

## 三、遍历与节点收集（重点：消除 visitor 副作用写法）

### 上游/Azrng 通用写法（visitor，两种语言都有，不推荐日常使用）

```java
// Java 上游 / Azrng 都支持，但属于底层机制
class ColumnCollector extends ExpressionVisitorAdapter {
    List<Column> columns = new ArrayList<>();
    @Override visit(Column c, Object ctx) { columns.add(c); super.visit(c, ctx); }
}
var collector = new ColumnCollector();
expr.accept(collector, null);          // 无返回值，靠副作用
return collector.columns;              // 事后掏字段
```

### Azrng 推荐 C# 写法（扩展方法，上游无对应物）

| 上游 Java 写法 | Azrng C# 推荐 | 说明 |
|---------------|--------------|------|
| 自定义 `ExpressionVisitor` 收集某类节点 | `expr.Descendants<T>()` | **主力**：拉取式，返回 `IEnumerable<T>`，直接接 LINQ |
| 自定义 `StatementVisitor` 收集某类语句 | `stmt.Descendants<T>()` | 语句层收集（含嵌套子语句） |
| `expr.accept(visitor)` 推送遍历 | `expr.Descendants<T>().ToList()` / `foreach` | 不再有独立 Walk API，LINQ 即遍历 |

```csharp
// 收集 WHERE 中的所有列引用（替代自定义 ColumnCollector）
var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();

// 收集命名参数（替代自定义 ParameterCollector）
var params = where.Descendants<JdbcNamedParameter>().Select(p => p.Name).ToList();

// 收集所有后代节点（诊断/调试）
var allNodes = where.Descendants().ToList();
```

> **`Descendants<T>()` 覆盖保证**：内部 walker 直接实现 `ExpressionVisitor<T>` 接口，
> 编译期强制覆盖所有节点类型，不会对某类节点静默返回空。

---

## 四、结构化提取（Azrng 独有，上游无对应）

这些扩展方法把高频的"按业务概念切分 AST"封装成便捷入口，返回中性 DTO。
上游 JSqlParser 没有这些，Java 侧需手写 visitor 实现。

| Azrng C# 扩展方法 | 上游 Java 等价物 | 返回 | 用途 |
|------------------|----------------|------|------|
| `stmt.GetTableNames()` | `new TablesNamesFinder().GetTables(stmt)` | `IReadOnlyCollection<string>` | 全部表名（含 WHERE 子查询），已去重 |
| `stmt.GetTableReferences()` | 手写 FROM/JOIN 遍历 | `IReadOnlyList<TableReference>` | FROM/JOIN/CTE 表引用（带别名/全名） |
| `select.GetSelectColumns()` | 手写 SelectItems switch | `IReadOnlyList<SelectColumn>` | SELECT 列结构化（* / t.* / 列 / 表达式） |
| `where.GetWhereConditions()` | 手写 AND/OR 递归 | `IReadOnlyList<WhereCondition>` | WHERE 树拍平为条件列表 |

### 中性 DTO（Azrng 自有，上游无）

| DTO | 字段 | 说明 |
|-----|------|------|
| `TableReference` | `Name` / `Alias?` / `FullName` / `Key` | 表引用；`Key` = 别名优先 |
| `SelectColumn` | `Kind` / `TableAlias?` / `ColumnName?` / `Alias?` / `Expression?` | `Kind` 区分 All/AllTable/Column/Expression |
| `WhereCondition` | `LinkType` / `LeftExpression` / `RightExpression?` / `Operator` / `SqlInfo` | `RightExpression` 单目时为 null |

> **边界**：这些 DTO 只描述 AST 事实，不含产品业务约定（别名优先策略、虚拟列必填校验等），
> 不含前端契约字段。业务方拿到后自行套规则、映射到自己的业务 DTO。

---

## 五、visitor 体系（底层机制，保留用于复杂自定义遍历与上游对照）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `ExpressionVisitor<T>` | `ExpressionVisitor<T>` | 接口签名一致（双泛型 `T Visit<S>(X, S)`） |
| `StatementVisitor<T>` | `StatementVisitor<T>` | 同上 |
| `SelectVisitor<T>` | `SelectVisitor<T>` | 同上 |
| `ExpressionVisitorAdapter<T>` | `ExpressionVisitorAdapter<T>` | 默认实现基类（public 保留） |
| `StatementVisitorAdapter<T>` | `StatementVisitorAdapter<T>` | 同上 |
| `TablesNamesFinder` | `TablesNamesFinder` | `GetTables` 已标 `[Obsolete]`，改用 `GetTableNames` |

> **何时还用 visitor**：需要"自定义遍历顺序/上下文传递/部分节点特殊处理"等 Descendants 覆盖不了的复杂场景，
> 或与上游 visitor 实现对照时。日常收集/提取请优先用扩展方法。

---

## 六、关键类名差异速查

| 上游 Java | Azrng C# | 备注 |
|-----------|----------|------|
| `CCJSqlParserUtil` | `CCJSqlParserUtil` | 同名（保留对照） |
| `JSQLParserException` | `JSqlParserException` | 异常类名大小写 |
| `PlainSelect` | `PlainSelect` | 同名 |
| `TablesNamesFinder.findTables()` | `GetTableNames()`（推荐）/ `TablesNamesFinder.GetTables()`（Obsolete） | C# 用扩展方法 |
| `ExpressionVisitorAdapter` | `ExpressionVisitorAdapter` | 同名 |

---

## 七、改造示例：LocalSqlParser 风格的 visitor 写法 → Azrng C#

```csharp
// ❌ 旧写法（Java 风格 visitor 副作用返回）
private sealed class ColumnCollector : ExpressionVisitorAdapter<object?> {
    public List<Column> Columns { get; } = new();
    public override object? Visit<S>(Column c, S ctx) { Columns.Add(c); return base.Visit(c, ctx); }
}
var collector = new ColumnCollector();
expr.Accept(collector, default(object));
return collector.Columns;

// ✅ 新写法（Azrng C# 扩展方法）
return expr.Descendants<Column>().ToList();
```

```csharp
// ❌ 旧：提取表名靠 new Finder
var tables = new TablesNamesFinder().GetTables(stmt);

// ✅ 新：扩展方法
var tables = stmt.GetTableNames();              // 扁平表名
var refs = stmt.GetTableReferences();           // 结构化（带别名）
```

```csharp
// ❌ 旧：手写 WHERE 拆解
void CollectOperators(Expression e, ...) {
    switch (e) { case AndExpression and: ...; case InExpression in: ...; ... }
}

// ✅ 新：结构化提取
var conds = where.GetWhereConditions();         // 拍平好的条件列表
```

---

文件结束。
