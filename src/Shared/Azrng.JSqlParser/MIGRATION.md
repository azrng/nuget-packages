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

## 八、符号级对照总览（改造基线）

> **用途**：从本章起是逐符号对照表，作为"C# 风格化改造"的基准。每次改完一项，在该行 `备注` 列勾记 `已改造 @版本号`。
> **约定**：`当前 C#` 列是 1.0.0-beta7 实际现状；`建议 C#` 列是改造目标，并非已生效。
> **改 ACID 原则**：改 AST 内部符号（类名/字段名/枚举值名/方法名）前，先确认本表已记录映射；改完同步本表。改外层封装（扩展方法、DTO）无需查本表。

### 8.1 改造类型标记

| 标记 | 含义 | 是否破坏对照锚点 |
|------|------|----------------|
| `[外]` | Azrng 自有封装，上游无对应 | 否，可大胆改 |
| `[形]` | C# 形态改造（getter→property、删 builder、删无 context 重载），上游符号名不变 | 否，对照时按"上游字段 X = C# 属性 X"心算即可 |
| `[锚]` | 改造触及对照锚点本身（类名/字段名/枚举值名/方法名），上游对照需查本表 | **是，必须先记本表再改** |
| `[风险]` | 公开 API 破坏性，影响外部消费者 | 与对照无关，但需评估 README/示例 |

---

## 九、Parser 基础设施对照

### 9.1 `ASTNodeAccess` 接口（`Parser/ASTNodeAccess.cs`）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `ASTNodeAccess` | `ASTNodeAccess` | `IAstNodeAccess` | `[锚]` C# 接口加 `I` 前缀 |
| `jjtGetASTNode()` | `SimpleNode? GetASTNode()` | `SimpleNode? AstNode { get; }` | `[形]` getter→只读属性 |
| `jjtSetASTNode(node)` | `void SetASTNode(SimpleNode node)` | `SimpleNode? AstNode { set; }` 或合并为 `{ get; set; }` | `[形]` setter→属性 |

### 9.2 `ASTNodeAccessImpl`（`Parser/ASTNodeAccessImpl.cs`）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `ASTNodeAccessImpl` | `ASTNodeAccessImpl` | `AstNodeAccess` | `[锚]` 去掉 Java `Impl` 后缀，名字暗示"基类实现"在 C# 习惯里冗余 |
| `SimpleNode node` 字段 | `[NonSerialized] private SimpleNode? _node` | `private SimpleNode? _node` | `[形]` 删除 `[NonSerialized]`（本库无 `[Serializable]` 二进制序列化路径，attribute 无意义） |
| `void appendTo(StringBuilder)` | `virtual StringBuilder AppendTo(StringBuilder)` | 保留 | C# 习惯合理 |
| —（上游用 jjt token 区间） | 调用 `simpleNode.JjtGetFirstToken()` / `JjtGetLastToken()` | 改调 `GetFirstToken()` / `GetLastToken()` | `[锚]` 见 9.3 |

### 9.3 `SimpleNode`（`Parser/SimpleNode.cs`）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `SimpleNode` | `SimpleNode` | 保留类名 | 上游 JJTree 生成类名，保留有助对照 |
| `Token firstToken` / `lastToken` | `Token? FirstToken { get; set; }` / `LastToken` | 保留 | PascalCase 已正确 |
| `jjtGetFirstToken()` | `virtual Token JjtGetFirstToken() => FirstToken!` | `Token GetFirstToken() => FirstToken!` | `[锚]` 去掉 JJTree 前缀（JavaCC 历史包袱，C# 社区陌生） |
| `jjtGetLastToken()` | `virtual Token JjtGetLastToken() => LastToken!` | `Token GetLastToken() => LastToken!` | `[锚]` 同上 |

> **改造影响面**：`JjtGetFirstToken/JjtGetLastToken` 在 `ASTNodeAccessImpl.AppendTo` 和 `AstBuilderVisitor` 中被调用，改名需全仓替换 + 跑 round-trip 测试。

---

## 十、Visitor 体系对照

### 10.1 `ExpressionVisitor<T>`（`Expression/ExpressionVisitor.cs`）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `ExpressionVisitor<T>` | `ExpressionVisitor<T>` | `IExpressionVisitor<T>` | `[锚]` C# 接口加 `I` 前缀 |
| `T visit<S>(X, S)` 双泛型签名 | `T Visit<S>(X, S)` | 保留 | 上游 JSqlParser 5.0+ 的设计，保留有助对照 |
| `T visit(X)` 无 context 重载（Java 没有，Azrng 自加） | `void Visit(X) => Visit<object?>(X, default)` ×100+ 行 | **删除全部无 context 重载** | `[形]` 100+ 行冗余转发；调用方改用 `node.Accept(visitor)` 或 `node.Accept(visitor, (object?)null)` |
| 接口内 `default` 实现 15+ 节点递归（Java 8 风格） | interface default method 内含 `foreach expr.Accept(this, context)` | **删除所有 interface default method，递归下沉到 `ExpressionVisitorAdapter<T>` 一处** | `[形]` 消除接口+Adapter 双份重复递归（README beta7 笔记里 Descendants 漏覆盖 12 类即此坑回响） |

### 10.2 `StatementVisitor<T>`（`Statement/StatementVisitor.cs`）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `StatementVisitor<T>` | `StatementVisitor<T>` | `IStatementVisitor<T>` | `[锚]` 同 10.1 |
| `T visit<S>(X, S)` | 保留 | 保留 | |
| 无 context 重载（Azrng 自加）×54 行 | `void Visit(X) => Visit<object?>(X, default)` | **删除全部无 context 重载** | `[形]` 同 10.1 |

### 10.3 `SelectVisitor<T>`、`ExpressionVisitorAdapter<T>`、`StatementVisitorAdapter<T>`

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `SelectVisitor<T>` | `SelectVisitor<T>` | `ISelectVisitor<T>` | `[锚]` |
| `ExpressionVisitorAdapter<T>` | `ExpressionVisitorAdapter<T>` | 保留类名 | 作为接口 default method 下沉后的唯一递归实现基类，地位提升 |
| `StatementVisitorAdapter<T>` | `StatementVisitorAdapter<T>` | 保留 | 同上 |
| `TablesNamesFinder.findTables()` | `TablesNamesFinder.GetTables()` `[Obsolete]` | 保留 `[Obsolete]` 一段时间后删 | `[外]` 改用 `stmt.GetTableNames()` |

---

## 十一、AST 节点类对照（核心）

### 11.1 节点基类与抽象

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `Expression`（interface） | `Expression`（interface） | `IExpression` | `[锚]` 接口加 `I` 前缀；命名冲突时（`Select : Statement, Expression`）需全限定，C# 14 后可 `using` 别名 |
| `Statement`（interface） | `Statement`（interface） | `IStatement` | `[锚]` |
| `Select`（abstract class） | `Select`（abstract class） | 保留 | 同时实现 `Statement`+`Expression` 是上游设计（子查询可作表达式），保留 |
| `BinaryExpression`（abstract） | `BinaryExpression`（abstract） | 保留 | |
| `ComparisonOperator : BinaryExpression` | `ComparisonOperator : BinaryExpression` | 保留 | |
| `Model`（marker interface） | `Model`（marker interface） | `IModel` 或删除 | `[锚]` 低优先级；空标记接口在 C# 价值不大 |

### 11.2 运算符族 `GetStringExpression()` 模式

> **统一问题**：13 个运算符子类各自 `public override string GetStringExpression() => "+"`，返回固定字符串却用方法。C# 习惯应为抽象只读属性。

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `BinaryExpression.getStringExpression()` | `public abstract string GetStringExpression()` | `protected abstract string OperatorSymbol { get; }` | `[形]` 方法→只读属性；`StringExpression` 计算属性改名 `OperatorSymbol` 更语义化 |
| —（上游用字段拼接） | `protected string StringExpression => $"{Left} {GetStringExpression()} {Right}"` | `protected string OperatorExpressionText => $"{LeftExpression} {OperatorSymbol} {RightExpression}"` | `[形]` |
| `Addition.getStringExpression()="+"` | `public override string GetStringExpression() => "+"` | `protected override string OperatorSymbol => "+"` | `[形]` 13 个运算符子类同步改 |
| `EqualsTo("=")` 构造注入 | `Operator { get; set; }` + `GetStringExpression()=>Operator` | `OperatorSymbol` 直接返回 `"="`，删 `Operator` 字段 | `[形]` `ComparisonOperator.Operator` 字段可移除，各子类构造时已知符号 |

**涉及子类清单**（`[形]` 同步改造）：`Addition(+), Subtraction(-), Multiplication(*), Division(/), IntegerDivision(DIV), Modulo(%), Concat(\|\|), BitwiseAnd(&), BitwiseOr(\|), BitwiseXor(^), BitwiseLeftShift(<<), BitwiseRightShift(>>), AndExpression(AND), OrExpression(OR), XorExpression(XOR)`，以及关系运算符族 `EqualsTo, NotEqualsTo, GreaterThan, GreaterThanEquals, MinorThan, MinorThanEquals, LikeExpression, SimilarToExpression, RegExpMatchOperator, IsDistinctExpression, JsonOperator, Matches, DoubleAnd, Contains, ContainedBy, CosineSimilarity, GeometryDistance, Plus, PriorTo`。

### 11.3 字面量族与基础表达式

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `NullValue` | `NullValue : ASTNodeAccessImpl, Expression` | 加 `sealed` | `[形]` 值语义叶子类不应被继承 |
| `LongValue` | `LongValue` 有 `LongValue()` / `LongValue(long)` / `LongValue(string)` 三构造 | 加 `sealed` | `[形]` |
| `DoubleValue` / `StringValue` / `HexValue` | 同上模式 | 加 `sealed` | `[形]` |
| `BooleanValue` | 同上 | 加 `sealed` | `[形]` |
| `Parenthesis.Expression` | `public Expression Expression { get; set; } = null!` | `required Expression Inner { get; set; }` 或构造注入 | `[锚]` `null!` 字段 → C# 11 `required` 或构造必填；字段名 `Expression` 与类型同名易混，建议改 `Inner` |
| `Between.LeftExpression` | `= null!` ×3（LeftExpression/BetweenExpressionStart/BetweenExpressionEnd） | 同上 `null!` 治理 | `[锚]` 见第十四章 |

> **值语义类相等性**：`NullValue`/`LongValue`/`DoubleValue`/`StringValue`/`HexValue`/`Column`/`Table`/`Alias` 均应补 `Equals/GetHashCode`，否则 LINQ `Distinct/Contains/GroupBy` 按引用比较，与 SQL 语义不符。当前仅 `FeaturesAllowed` 一处重写。

### 11.4 Schema 类

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `Column` | `Column` | 加 `sealed` + 补 `Equals/GetHashCode` | `[形]` 值语义（列引用应按 fully-qualified name 相等） |
| `Column.getFullyQualifiedName()` | `string GetFullyQualifiedName()` | `string FullyQualifiedName { get; }` | `[形]` getter→只读属性 |
| `Table` | `Table` | 加 `sealed` + 补 `Equals/GetHashCode` | `[形]` |
| `Table.getFullyQualifiedName()` | `string GetFullyQualifiedName()` | `string FullyQualifiedName { get; }` | `[形]` |
| `Table.getAlias()` / `setAlias()` | `Alias? GetAlias()` / `void SetAlias(Alias)` | 删方法，直接用 `Alias` 属性 | `[形]` 已有 `public Alias? Alias { get; set; }`，getter/setter 方法是冗余入口 |
| `Alias` | `Alias : ASTNodeAccessImpl` | 加 `sealed` + 补 `Equals/GetHashCode` 或改 `record` | `[形]` 值语义 |
| `OracleJoinSyntax`（上游 enum） | `public static class OracleJoinSyntax { public const int NoOracleJoin=0; ... }` + `Column.OldOracleJoinSyntax : int` | `enum OracleJoinSyntax { None, Right, Left }` + `Column.OldOracleJoinSyntax : OracleJoinSyntax` | `[锚]` Java 5 之前的 `static final int` 模式，C# 应直接 enum；字段类型同步改 |

### 11.5 `PlainSelect` / `Select` 关键字段（节选高频项）

> **原则**：字段名与上游对齐（便于同步新字段），仅改 C# 形态（`null!` 治理、`init`、`required`）。

| 上游 Java 字段 | 当前 C# | 建议 C# | 备注 |
|---------------|---------|---------|------|
| `PlainSelect.selectItems` | `List<SelectItem>? SelectItems` | 保留 | 可空合理（SELECT * 无显式 items 时） |
| `PlainSelect.fromItem` | `FromItem? FromItem` | 保留 | |
| `PlainSelect.where` | `Expression? Where` | 保留 | |
| `PlainSelect.joinList` | `List<Join>? Joins` | 保留 | |
| —（`PlainSelect` 静态 helper） | `public static string GetStringList<T>(...)` | **删除，内联 `string.Join` 或移到独立 `SqlString` 工具类** | `[外]` Java StringUtils.join 风格，挂在具体类上不合理 |
| `Select.getSelectBody()` | `Select GetSelectBody() => this` `[Obsolete]` | 保留 Obsolete 一段时间后删 | `[外]` 上游为兼容旧 API 保留，Azrng 已标 Obsolete |
| `Select.getForUpdateTable()` | `Table? GetForUpdateTable()` | `Table? ForUpdateTable => ForUpdateTables?.FirstOrDefault()` | `[形]` getter→表达式属性 |
| `Select.getForUpdate()` | `ForUpdateClause? GetForUpdate()` | `ForUpdateClause? ForUpdate => ForMode == null ? null : new(){...}` | `[形]` getter→表达式属性 |

### 11.6 `Function` 字段（节选）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `Function.name` | `string Name { get; set; } = ""` | `required string Name { get; set; }` | `[形]` `null!`/空串治理 |
| `Function.parameters` | `Expression? Parameters` | 保留 | |
| `Function.allColumns` | `bool AllColumns` | 保留 | |
| `Function.keywordArguments` | `List<KeywordArgument>? KeywordArguments` | 保留 | |
| `KeywordArgument`（内部类） | `public class KeywordArgument : ASTNodeAccessImpl, Model` | 改 `sealed record KeywordArgument(string Keyword, Expression? Expression)` | `[形]` 值语义 DTO，C# 用 record |

### 11.7 `ForUpdateClause`（builder 模式治理）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `ForUpdateClause` | `public class ForUpdateClause` | `public sealed record ForUpdateClause` | `[形]` DTO 改 record |
| —（无 builder） | `ForUpdateClause SetMode(...).SetTables(...).SetWait(...).SetNoWait(...).SetSkipLocked(...)` | **删除全部 SetXxx 方法**，改对象初始化器 `new ForUpdateClause { Mode=.., Tables=.. }` | `[形]` Java/Lombok 流式 builder 风格，C# 用初始化器；字段已 public set，SetXxx 是冗余入口 |
| `getFirstTable()` / `isForUpdate()` 等 | `GetFirstTable()` / `IsForUpdate()` | `FirstTable` 属性 / `IsForUpdate` 保留（bool 查询方法 C# 可保留动词前缀） | `[形]` |

---

## 十二、枚举对照（重点：命名风格统一）

> **现状诊断**：枚举命名风格**内部不一致**。`LockMode`/`ReferentialActionMode`/`RowMovementMode`/`FrameType`/`AnyType`/`AnalyticType`/`RefreshMode`/`SelectColumnKind` 已是 PascalCase（C# 正确）；`ForMode`/`AlterOperation`/`ReturningReferenceType`/`DateTimeType` 仍是 SCREAMING_SNAKE_CASE（Java 风格）。改造目标：全部统一 PascalCase。

### 12.1 需改名枚举（SCREAMING_CASE → PascalCase）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `ForMode.UPDATE` | `ForMode { UPDATE, SHARE, NO_KEY_UPDATE, KEY_SHARE, READ_ONLY, FETCH_ONLY }` | `ForMode { Update, Share, NoKeyUpdate, KeyShare, ReadOnly, FetchOnly }` | `[锚][风险]` 公开枚举，破坏性；同步删 `ForModeExtensions.GetValue()`，改 `[Description]` 或 `ToSqlString()` |
| `AlterOperation.ADD` 等 47 值 | `AlterOperation { ADD, ALTER, DROP, DROP_PRIMARY_KEY, ... }` | `AlterOperation { Add, Alter, Drop, DropPrimaryKey, ... }` | `[锚][风险]` 47 值全改，破坏性；外部 switch case 需全替换 |
| `ReturningReferenceType.OLD` | `ReturningReferenceType { OLD, NEW }` | `ReturningReferenceType { Old, New }` | `[锚]` 同步删 `ReturningReferenceTypeExtensions.From()`，改 `Enum.Parse<T>(ignoreCase:true)` |
| `DateTimeType.DATE` | `DateTimeType { DATE, DATETIME, TIME, TIMESTAMP, TIMESTAMPTZ }` | `DateTimeType { Date, DateTime, Time, Timestamp, TimestampTz }` | `[锚]` 注意 `DateTimeLiteralExpression.ToString()` 用 `$"{Type} {Value}"` 靠枚举名，改名后需改 ToSqlString() |

### 12.2 已 PascalCase 枚举（保持现状，仅记录）

| 枚举 | 当前值 | 备注 |
|------|--------|------|
| `LockMode` | `Share, Exclusive, RowShare, RowExclusive, ShareUpdate, ShareRowExclusive` | `[形]` 已正确；`LockModeExtensions.GetValue()` 可保留（SQL 文本与枚举名空格差异） |
| `ReferentialActionType` | `Delete, Update` | `[形]` 已正确 |
| `ReferentialActionMode` | `Cascade, Restrict, NoAction, SetNull, SetDefault` | `[形]` 已正确 |
| `RowMovementMode` | `Enable, Disable` | `[形]` 已正确 |
| `FrameType` | `Rows, Range, Groups` | `[形]` 已正确 |
| `BoundType` | `UnboundedPreceding, UnboundedFollowing, CurrentRow, Preceding, Following` | `[形]` 已正确 |
| `ExcludeType` | `CurrentRow, Group, Ties, NoOthers` | `[形]` 已正确 |
| `AnyType` | `Any, Some, All` | `[形]` 已正确 |
| `AnalyticType` | `Over, WithinGroup, WithinGroupOver, FilterOnly` | `[形]` 已正确 |
| `RefreshMode` | `Default, WithData, WithNoData` | `[形]` 已正确 |
| `SelectColumnKind` | `All, AllTable, Column, Expression` | `[形]` 已正确（Azrng 自有 DTO，无对照） |

---

## 十三、解析入口对照（细化）

| 上游 Java | 当前 C# | 建议 C# | 备注 |
|-----------|---------|---------|------|
| `CCJSqlParserUtil` | `CCJSqlParserUtil` | `SqlParser`（新名）+ `CCJSqlParserUtil`（保留转发别名） | `[锚][风险]` 公开入口，README/示例/外部消费者全引用；建议 2.0 破坏性版本处理，期间保留旧名转发 |
| `CCJSqlParserUtil.parse()` | `Parse()` | 保留（或新类 `SqlParser.Parse()`） | |
| `CCJSqlParserUtil.parseStatements()` | `ParseStatements()` | 保留 | |
| `CCJSqlParserUtil.parseExpression()` | `ParseExpression()` | 保留 | |
| `CCJSqlParserUtil.parseCondExpression()` | `ParseCondExpression()` | 保留 | |
| —（无） | `ParseNullable()` | 保留 | `[外]` Azrng 自有 |
| `JSQLParserException` | `JSqlParserException` | 保留 | 命名已合理 |

---

## 十四、`null!` 字段治理专项

> **问题**：迁移时为关掉 NRT 警告大量用 `= null!`，运行时仍为 null，调用方拿到 NRE。143 个 AST 类广泛存在。

| 治理策略 | 适用场景 | 改造方式 |
|----------|---------|---------|
| `required`（C# 11） | 必填字段，对象初始化器必须赋值 | `required Expression Left { get; set; }` |
| 构造注入 | 必填字段，构造时强制非空 | 构造函数参数 + 属性 `init` |
| `init` only-setter | 构造后不可变 | `{ get; init; }` |
| 保留 `?` 可空 | 字段语义上确实可空 | `{ get; set; }` 加 `?` |
| 删除 `null!` | 字段真实默认值合理（如 `= ""`/`= new()`） | 显式默认值 |

**重点治理清单**（高频被访问字段）：

| 类 | 当前 | 建议 |
|----|------|------|
| `BinaryExpression.LeftExpression/RightExpression` | `= null!` | `required` |
| `Between.LeftExpression/BetweenExpressionStart/BetweenExpressionEnd` | `= null!` | `required` |
| `Parenthesis.Expression` | `= null!` | `required`（或改名 `Inner`） |
| `Column.ColumnName` | `= ""` | 保留（空串语义合理） |
| `Table.Name` | `= ""` | `required`（表名空无意义） |
| `Function.Name` | `= ""` | `required` |

> **风险**：上游 Java 用无参构造 + setter，`AstBuilderVisitor` 接线大量依赖 `new X()` 后逐字段赋值。改 `required`/构造注入需同步改 visitor 接线，工作量大，建议放在专门迭代。

---

## 十五、改造执行顺序建议

按"低风险高性价比"排序，每完成一批在对应表格行勾记：

| 批次 | 内容 | 风险 | 对照锚点 |
|------|------|------|----------|
| **批 1** | 删 visitor 接口 default method（下沉到 Adapter）；删无 context 重载 | 低 | `[形]` |
| **批 2** | `GetXxx()/SetXxx()` → property；删 `ForUpdateClause` builder；内联 `GetStringList` | 低 | `[形]` |
| **批 3** | 值语义类加 `sealed` + `Equals/GetHashCode`；`KeywordArgument` 改 record | 低 | `[形]` |
| **批 4** | `GetStringExpression()` → `OperatorSymbol` 属性；13 运算符子类同步 | 中 | `[形]` |
| **批 5** | `[NonSerialized]` 删除；`JjtGet*Token` → `Get*Token` | 中 | `[锚]` 需跑 round-trip 测试 |
| **批 6** | `OracleJoinSyntax` const → enum；字段类型同步 | 中 | `[锚]` |
| **批 7** | 枚举 SCREAMING_CASE → PascalCase（`ForMode`/`AlterOperation`/`ReturningReferenceType`/`DateTimeType`） | **高** | `[锚][风险]` 公开 API 破坏性，建议 2.0 |
| **批 8** | `null!` 治理（`required`/构造注入） | **高** | `[形]` 工作量大，触及 visitor 接线 |
| **批 9** | 接口加 `I` 前缀（`Expression`→`IExpression` 等） | **高** | `[锚][风险]` 公开 API 破坏性，建议 2.0 |
| **批 10** | `CCJSqlParserUtil` → `SqlParser`（保留旧名转发） | **高** | `[锚][风险]` 公开 API 破坏性，建议 2.0 |

---

## 十六、改造记录

> 每完成一批改造，在此追加一行。格式：`YYYY-MM-DD 批次N：改了什么，对应本表第X章，影响面，验证方式`

| 日期 | 批次 | 改动摘要 | 影响面 | 验证 |
|------|------|----------|--------|------|
| 2026-07-17 | 批 1（动作 B） | `ExpressionVisitor<T>` 接口的 17 个 default method（含递归逻辑）下沉为纯签名声明；递归实现统一搬到 `ExpressionVisitorAdapter<T>`（补全原先缺失的 12 个边缘节点 override：TrimFunction/CollateExpression/TimezoneExpression/ArrayConstructor/ArrayExpression/RowConstructor/NextValExpression/AnyComparisonExpression/DateUnitExpression/OracleHint/OracleNamedFunctionParameter/PostgresNamedFunctionParameter）；`TablesNamesFinder`（直接实现接口）补全对应 17 个 Visit 方法 + 加 `using Expression.Cnf`。**动作 A（删无 context 重载）未做**：`ValuesSelectVisitorTest` 有测试明确依赖无 context 重载作为公开 API 契约，升级为破坏性变更挪至 2.0 | 库内 3 文件；公开接口行为不变 | 全量 1431 项通过，0 失败 |
| 2026-07-17 | 批 2 | 删除 `ForUpdateClause` 的 5 个 builder 方法（SetMode/SetTables/SetWait/SetNoWait/SetSkipLocked），`Select.GetForUpdate()` 改对象初始化器；`ForUpdateClause.GetFirstTable()` → `FirstTable` 属性（旧名标 Obsolete 转发）；`Select.GetForUpdateTable()` → `ForUpdateTable` 属性（旧名标 Obsolete 转发）；`Validation.GetParsedStatements()/GetErrors()` → `ParsedStatements`/`Errors` 属性（旧名标 Obsolete 转发，`Errors` 返回类型升为 `IReadOnlyList`）；删除零调用的死代码 `PlainSelect.GetStringList<T>()`；测试同步改用新属性。**`GetAlias()/SetAlias()` 暂未改**：属 `FromItem` 接口契约，多实现类，单独评估 | 库内 4 文件 + 测试 1 文件；旧 API 全部保留 Obsolete 转发，无破坏性 | 全量 1431 项通过，0 失败 |

---

文件结束。
