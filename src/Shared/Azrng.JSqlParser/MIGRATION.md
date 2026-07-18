# 迁移对照：JSqlParser（Java 上游）→ Azrng.JSqlParser（.NET）

> 本文档记录从 JSqlParser 5.4 上游（Java）迁移到 Azrng.JSqlParser（C#）时的 API 对照关系。
> **后续从上游同步新功能/方言时，按本表对照定位**；下游业务方查找 C# 推荐 API 时也按本表。

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
| `CCJSqlParserUtil.parse(sql)` | `SqlParser.Parse(sql)` | 单语句，返回 `Statement?`；旧名 `CCJSqlParserUtil.Parse` 保留 `[Obsolete]` 转发 |
| `CCJSqlParserUtil.parseStatements(sql)` | `SqlParser.ParseStatements(sql)` | 多语句，返回 `Statements?` |
| `CCJSqlParserUtil.parseExpression(sql)` | `SqlParser.ParseExpression(sql)` | 独立表达式 |
| `CCJSqlParserUtil.parseCondExpression(sql)` | `SqlParser.ParseCondExpression(sql)` | 条件表达式 |
| —（无） | `SqlParser.ParseNullable(sql)` | Azrng 新增：失败返回 null 不抛异常 |
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

> **`Descendants<T>()` 覆盖保证**：内部 walker 直接实现 `IExpressionVisitor<T>` 接口，
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
| `ExpressionVisitor<T>` | `IExpressionVisitor<T>` | 接口签名一致（双泛型 `T Visit<S>(X, S)`），C# 加 `I` 前缀 |
| `StatementVisitor<T>` | `IStatementVisitor<T>` | 同上 |
| `SelectVisitor<T>` | `ISelectVisitor<T>` | 同上 |
| `ExpressionVisitorAdapter<T>` | `ExpressionVisitorAdapter<T>` | 默认实现基类（public 保留），递归实现集中在此 |
| `StatementVisitorAdapter<T>` | `StatementVisitorAdapter<T>` | 同上 |
| `TablesNamesFinder` | `TablesNamesFinder` | `GetTables` 已标 `[Obsolete]`，改用 `GetTableNames` |

> **何时还用 visitor**：需要"自定义遍历顺序/上下文传递/部分节点特殊处理"等 Descendants 覆盖不了的复杂场景，
> 或与上游 visitor 实现对照时。日常收集/提取请优先用扩展方法。

---

## 六、关键类名差异速查

| 上游 Java | Azrng C# | 备注 |
|-----------|----------|------|
| `CCJSqlParserUtil` | `SqlParser`（推荐）/ `CCJSqlParserUtil`（`[Obsolete]` 转发） | C# 用新名，旧名保留转发 |
| `JSQLParserException` | `JSqlParserException` | 异常类名大小写 |
| `PlainSelect` | `PlainSelect` | 同名 |
| `TablesNamesFinder.findTables()` | `GetTableNames()`（推荐）/ `TablesNamesFinder.GetTables()`（`[Obsolete]`） | C# 用扩展方法 |
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

## 八、Parser 基础设施对照

### 8.1 `ASTNodeAccess` 接口（`Parser/IASTNodeAccess.cs`）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `ASTNodeAccess` | `IASTNodeAccess` | C# 接口加 `I` 前缀 |
| `jjtGetASTNode()` | `SimpleNode? GetASTNode()` | |
| `jjtSetASTNode(node)` | `void SetASTNode(SimpleNode node)` | |

### 8.2 `ASTNodeAccessImpl`（`Parser/ASTNodeAccessImpl.cs`）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `ASTNodeAccessImpl` | `ASTNodeAccessImpl` | 保留类名（含 `Impl` 后缀，被 30+ 类继承） |
| `SimpleNode node` 字段 | `private SimpleNode? _node` | 可空，无 `[NonSerialized]` |
| `void appendTo(StringBuilder)` | `virtual StringBuilder AppendTo(StringBuilder)` | 返回 StringBuilder 便于链式 |
| —（上游用 jjt token 区间） | 调用 `simpleNode.GetFirstToken()` / `GetLastToken()` | 见 8.3 |

### 8.3 `SimpleNode`（`Parser/SimpleNode.cs`）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `SimpleNode` | `SimpleNode` | 上游 JJTree 生成类名，保留有助对照 |
| `Token firstToken` / `lastToken` | `Token? FirstToken { get; set; }` / `LastToken` | PascalCase |
| `jjtGetFirstToken()` | `Token GetFirstToken()` | 去掉 JJTree 前缀；旧名 `JjtGetFirstToken()` 保留 `[Obsolete]` 转发 |
| `jjtGetLastToken()` | `Token GetLastToken()` | 同上；旧名 `JjtGetLastToken()` 保留 `[Obsolete]` 转发 |

---

## 九、Visitor 体系对照

### 9.1 `IExpressionVisitor<T>`（`Expression/ExpressionVisitor.cs`）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `ExpressionVisitor<T>` | `IExpressionVisitor<T>` | C# 接口加 `I` 前缀 |
| `T visit<S>(X, S)` 双泛型签名 | `T Visit<S>(X, S)` | 上游 JSqlParser 5.0+ 的设计，保留 |
| `T visit(X)` 无 context 重载（Java 没有） | `void Visit(X) => Visit<object?>(X, default)` ×100+ 行 | Azrng 自加的便利重载，`void` 返回，对"只遍历不取返回值"的调用方有意义 |
| 接口内 `default` 实现 15+ 节点递归（Java 8 风格） | 接口为纯契约，递归下沉到 `ExpressionVisitorAdapter<T>` | 消除接口+Adapter 双份重复递归 |

### 9.2 `IStatementVisitor<T>`（`Statement/StatementVisitor.cs`）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `StatementVisitor<T>` | `IStatementVisitor<T>` | C# 接口加 `I` 前缀 |
| `T visit<S>(X, S)` | `T Visit<S>(X, S)` | 保留 |
| 无 context 重载（Azrng 自加） | `void Visit(X) => Visit<object?>(X, default)` | 同 9.1 |

### 9.3 `ISelectVisitor<T>`、`ExpressionVisitorAdapter<T>`、`StatementVisitorAdapter<T>`

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `SelectVisitor<T>` | `ISelectVisitor<T>` | C# 接口加 `I` 前缀；含 6 个无 context 便利重载（同 9.1） |
| `ExpressionVisitorAdapter<T>` | `ExpressionVisitorAdapter<T>` | 保留类名；作为接口递归下沉后的唯一递归实现基类 |
| `StatementVisitorAdapter<T>` | `StatementVisitorAdapter<T>` | 同上 |
| `TablesNamesFinder.findTables()` | `TablesNamesFinder.GetTables()` `[Obsolete]` | 改用 `stmt.GetTableNames()` |

---

## 十、AST 节点类对照（核心）

### 10.1 节点基类与抽象

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `Expression`（interface） | `IExpression` | C# 接口加 `I` 前缀；命名冲突时全限定 |
| `Statement`（interface） | `IStatement` | 同上 |
| `Select`（abstract class） | `Select`（abstract class） | 同时实现 `IStatement`+`IExpression` 是上游设计（子查询可作表达式） |
| `BinaryExpression`（abstract） | `BinaryExpression`（abstract） | 同名 |
| `ComparisonOperator : BinaryExpression` | `ComparisonOperator : BinaryExpression` | 同名 |
| `Model`（marker interface） | `IModel` | 空标记接口，C# 加 `I` 前缀 |

### 10.2 运算符族 `OperatorSymbol` 属性

> 上游用 `getStringExpression()` 方法返回固定字符串，Azrng 改为抽象只读属性 `OperatorSymbol`。

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `BinaryExpression.getStringExpression()` | `public abstract string OperatorSymbol { get; }` | 方法→只读属性；旧名 `GetStringExpression()` 保留 `[Obsolete]` 转发 |
| —（上游用字段拼接） | `protected string OperatorExpressionText => $"{LeftExpression} {OperatorSymbol} {RightExpression}"` | 计算属性，`ToString()` 调用 |
| `Addition.getStringExpression()="+"` | `public override string OperatorSymbol => "+"` | 13 个运算符子类同步（Addition/Subtraction/Multiplication/Division/IntegerDivision/Modulo/Concat/BitwiseAnd/BitwiseOr/BitwiseXor/BitwiseLeftShift/BitwiseRightShift/AndExpression/OrExpression/XorExpression） |
| `EqualsTo("=")` 构造注入 | `ComparisonOperator.Operator { get; set; }` + `OperatorSymbol => Operator` | 6 个关系运算符子类（EqualsTo/NotEqualsTo/GreaterThan/GreaterThanEquals/MinorThan/MinorThanEquals）通过 `base("="/"<>" 等)` 构造注入符号 |

### 10.3 字面量族与基础表达式

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `NullValue` | `public sealed class NullValue` | 值语义叶子类加 `sealed` |
| `LongValue` | `public sealed class LongValue` | 同上；含 `LongValue()` / `LongValue(long)` / `LongValue(string)` 三构造 |
| `DoubleValue` / `StringValue` / `HexValue` | `public sealed class ...` | 同上 |
| `BooleanValue` | `public sealed class BooleanValue` | 同上 |
| `Parenthesis.Expression` | `public required IExpression Expression { get; set; }` | C# 11 `required` 强制非空；字段名保留 `Expression`（与类型同名） |
| `Between.LeftExpression` | `public required IExpression LeftExpression / BetweenExpressionStart / BetweenExpressionEnd` | 三个字段均 `required` |

### 10.4 Schema 类

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `Column` | `public class Column : ASTNodeAccessImpl, IExpression` | |
| `Column.getFullyQualifiedName()` | `public string GetFullyQualifiedName()` | |
| `Table` | `public class Table : ASTNodeAccessImpl, IFromItem` | |
| `Table.getFullyQualifiedName()` | `public string GetFullyQualifiedName()` | |
| `Table.getAlias()` / `setAlias()` | `Alias? Alias { get; set; }` | `GetAlias()`/`SetAlias()` 保留 `[Obsolete]` 转发 |
| `Alias` | `public sealed class Alias` | 已加 `sealed` + `Equals/GetHashCode`（按 Name+UseAs） |
| `OracleJoinSyntax`（上游 enum） | `enum OracleJoinSyntax { None, Right, Left }` | Java 5 之前的 `static final int` 模式改为 C# enum；`Column.OldOracleJoinSyntax` 字段类型同步改为 `OracleJoinSyntax` |

### 10.5 `PlainSelect` / `Select` 关键字段（节选高频项）

> **原则**：字段名与上游对齐（便于同步新字段），仅 C# 形态本地化。

| 上游 Java 字段 | Azrng C# | 说明 |
|---------------|----------|------|
| `PlainSelect.selectItems` | `List<SelectItem>? SelectItems` | 可空合理（SELECT * 无显式 items 时） |
| `PlainSelect.fromItem` | `FromItem? FromItem` | 可空 |
| `PlainSelect.where` | `Expression? Where` | 可空（`Expression` 为命名空间内 `IExpression` 的别名引用） |
| `PlainSelect.joinList` | `List<Join>? Joins` | 可空 |
| —（`PlainSelect` 静态 helper） | 已删除 | 上游 `StringUtils.join` 风格 helper 不再挂在 PlainSelect 上 |
| `Select.getSelectBody()` | `Select GetSelectBody() => this` `[Obsolete]` | 上游为兼容旧 API 保留，Azrng 标 Obsolete |
| `Select.getForUpdateTable()` | `Table? ForUpdateTable` 属性 + `GetForUpdateTable()` `[Obsolete]` 转发 | getter→表达式属性 |
| `Select.getForUpdate()` | `ForUpdateClause? GetForUpdate()` | |

### 10.6 `Function` 字段（节选）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `Function.name` | `string Name { get; set; } = ""` | |
| `Function.parameters` | `Expression? Parameters` | 可空 |
| `Function.allColumns` | `bool AllColumns` | |
| `Function.keywordArguments` | `List<KeywordArgument>? KeywordArguments` | 可空 |
| `KeywordArgument`（内部类） | `public class KeywordArgument : ASTNodeAccessImpl, IModel` | |

### 10.7 `ForUpdateClause`

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `ForUpdateClause` | `public class ForUpdateClause` | |
| —（无 builder） | 无 SetXxx 方法 | 用对象初始化器 `new ForUpdateClause { Mode=.., Tables=.. }`；字段已 public set |
| `getFirstTable()` | `Table? FirstTable` 属性 + `GetFirstTable()` `[Obsolete]` 转发 | getter→表达式属性 |

### 10.8 正则 / 模式匹配族（beta9 对齐上游）

> **重要**：本节是 beta9 破坏性变更的核心，迁移时务必逐项核对。
> 上游权威定义见 `JSqlParserCC.jjt:6833-6837`（符号形式）与 `LikeExpression.java`（关键字形式）。

**上游模型**（权威语义）：

| SQL 输入 | 上游 AST 类型 | 区分方式 |
|----------|--------------|----------|
| `c ~ 'x'` / `~*` / `!~` / `!~*` | `RegExpMatchOperator` | `RegExpMatchOperatorType` 枚举（4 态） |
| `c REGEXP 'x'` / `RLIKE` / `REGEXP_LIKE` | `LikeExpression` | `KeyWord` 枚举（`REGEXP`/`RLIKE`/`REGEXP_LIKE`） |
| `c LIKE 'x'` / `ILIKE` / `SIMILAR TO` / `MATCH_*` | `LikeExpression` | `KeyWord` 枚举 |
| `c @@ 'x'` | `Matches` | — |

**`RegExpMatchOperator` 对照**：

| 上游 Java | Azrng C#（beta9） | 说明 |
|-----------|----------|------|
| `RegExpMatchOperatorType` enum（4 态） | `RegExpMatchOperatorType` enum（`MatchCaseSensitive`/`MatchCaseInsensitive`/`NotMatchCaseSensitive`/`NotMatchCaseInsensitive`） | 值对齐，仅大小写风格（PascalCase） |
| `RegExpMatchOperator(RegExpMatchOperatorType)` 构造必填 | 同（`new RegExpMatchOperator(RegExpMatchOperatorType)`） | 对齐上游 `requireNonNull` |
| `getOperatorType()` | `RegExpMatchOperatorType OperatorType { get; set; }` | getter→属性 |
| `getStringExpression()` switch 返回 `~`/`~*`/`!~`/`!~*` | `override string OperatorSymbol` switch 返回同 | 方法→只读属性（继承 `BinaryExpression`） |

> **beta8 及之前的偏差（已修复）**：曾用 `string Operator` + `bool Not` 两字段承载（丢失类型安全区分），且 PG 符号 `~`/`!~` 被 visitor 误建成 `EqualsTo`。beta9 改回枚举模型。

**`LikeExpression` 对照**：

| 上游 Java | Azrng C#（beta9） | 说明 |
|-----------|----------|------|
| `KeyWord` enum（11 态：LIKE/ILIKE/RLIKE/REGEXP_LIKE/REGEXP/SIMILAR_TO/MATCH_ANY/MATCH_ALL/MATCH_PHRASE/MATCH_PHRASE_PREFIX/MATCH_REGEXP） | `KeyWord` enum（`Like`/`Ilike`/`Rlike`/`RegexpLike`/`Regexp`/`SimilarTo`/`MatchAny`/`MatchAll`/`MatchPhrase`/`MatchPhrasePrefix`/`MatchRegexp`） | 值对齐，PascalCase；`SIMILAR_TO`→`SimilarTo`（去下划线） |
| `KeyWord likeKeyWord` 字段 | `KeyWord LikeKeyWord` 字段（默认 `Like`） | 对齐 |
| `Expression escapeExpression` | `IExpression? Escape` 字段 | 可空；grammar `ESCAPE` 子句接线 |
| `boolean not` | `bool Not` | 对齐 |
| `getStringExpression()` → `likeKeyWord.toString()` | `override string OperatorSymbol`（`SimilarTo` 特判为 `"SIMILAR TO"`，其余 `ToUpperInvariant()`） | 方法→只读属性 |
| `toString()` 含 NOT/KeyWord/ESCAPE 拼接 | `override string ToString()` 同 | 对齐上游拼接顺序 |

> **beta8 及之前的偏差（已修复）**：
> - `LikeExpression` 无 `KeyWord` 字段，`ILIKE`/`REGEXP` 等关键字信息丢失（统一记成 LIKE）
> - `REGEXP`/`RLIKE`/`REGEXP_LIKE` 被 visitor 错误建成 `RegExpMatchOperator`（应为 `LikeExpression`）
> - `SIMILAR TO` 是独立的 `SimilarToExpression` 类（上游归 `LikeExpression(KeyWord.SIMILAR_TO)`），beta9 已合并删除
> - `ESCAPE` 子句 grammar 已解析但 visitor 丢弃，beta9 已接线

**`SimilarToExpression`（beta9 已删除）**：

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| 无独立类（`SIMILAR TO` 归 `LikeExpression`） | beta9 起无独立类 | beta8 及之前有 `SimilarToExpression`，beta9 合并到 `LikeExpression(KeyWord.SimilarTo)` |

> **迁移**：`new SimilarToExpression { ... }` → `new LikeExpression { LikeKeyWord = LikeExpression.KeyWord.SimilarTo, ... }`；`is SimilarToExpression` → `is LikeExpression && ((LikeExpression)x).LikeKeyWord == LikeExpression.KeyWord.SimilarTo`。

**`Matches` 对照**：

| 上游 Java | Azrng C#（beta9） | 说明 |
|-----------|----------|------|
| `Matches` 类（`@@` 全文匹配） | `Matches` 类 | 同名 |
| `getStringExpression()="@"` → 实为 `"@@"` | `override string OperatorSymbol => "@@"` | beta8 及之前误写为 `"~"`，beta9 修正 |

> **已知遗留**：当前 grammar 未定义 `@@` token，AstBuilderVisitor 也不构建 `Matches` 实例，故 `@@` 运算符目前不可解析。类骨架已对齐上游（符号正确），待后续补 grammar 时激活。

---

## 十一、枚举对照

> 枚举值全部已统一为 PascalCase。SQL 文本输出通过扩展方法（`ForModeExtensions.GetValue()` 等）或 `ToString()` + `ToUpperInvariant()` 转换。

### 11.1 已 PascalCase 的枚举（原 SCREAMING_SNAKE_CASE）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `ForMode.UPDATE` 等 | `ForMode { Update, Share, NoKeyUpdate, KeyShare, ReadOnly, FetchOnly }` | SQL 文本由 `ForModeExtensions.GetValue()` 提供（"UPDATE"/"NO KEY UPDATE" 等） |
| `AlterOperation.ADD` 等 47 值 | `AlterOperation { Add, Alter, Drop, DropPrimaryKey, DropUnique, DropForeignKey, Modify, Change, Rename, RenameTable, RenameIndex, RenameKey, RenameConstraint, ... }` | `AlterExpression.ToString()` 通用分支加 `ToUpperInvariant()` 保证 SQL 输出 |
| `ReturningReferenceType.OLD` | `ReturningReferenceType { Old, New }` | 解析由 `ReturningReferenceTypeExtensions.From()` 完成 |
| `DateTimeType.DATE` | `DateTimeType { Date, Datetime, Time, Timestamp, Timestamptz }` | `DateTimeLiteralExpression.ToString()` 加 `ToUpperInvariant()`；`Datetime` 避免 C# `DateTime` 类型同名冲突 |

### 11.2 上游已 PascalCase 的枚举（保持现状）

| 枚举 | 当前值 | 备注 |
|------|--------|------|
| `LockMode` | `Share, Exclusive, RowShare, RowExclusive, ShareUpdate, ShareRowExclusive` | `LockModeExtensions.GetValue()` 保留（SQL 文本与枚举名空格差异） |
| `ReferentialActionType` | `Delete, Update` | |
| `ReferentialActionMode` | `Cascade, Restrict, NoAction, SetNull, SetDefault` | |
| `RowMovementMode` | `Enable, Disable` | |
| `FrameType` | `Rows, Range, Groups` | |
| `BoundType` | `UnboundedPreceding, UnboundedFollowing, CurrentRow, Preceding, Following` | |
| `ExcludeType` | `CurrentRow, Group, Ties, NoOthers` | |
| `AnyType` | `Any, Some, All` | |
| `AnalyticType` | `Over, WithinGroup, WithinGroupOver, FilterOnly` | |
| `RefreshMode` | `Default, WithData, WithNoData` | |
| `SelectColumnKind` | `All, AllTable, Column, Expression` | Azrng 自有 DTO，无对照 |
| `RegExpMatchOperatorType` | `MatchCaseSensitive, MatchCaseInsensitive, NotMatchCaseSensitive, NotMatchCaseInsensitive` | beta9 新增，对齐上游同名枚举（值 PascalCase） |
| `LikeExpression.KeyWord` | `Like, Ilike, Rlike, RegexpLike, Regexp, SimilarTo, MatchAny, MatchAll, MatchPhrase, MatchPhrasePrefix, MatchRegexp` | beta9 新增，对齐上游 `LikeExpression.KeyWord`（`SIMILAR_TO`→`SimilarTo` 去下划线） |

---

## 十二、解析入口对照（细化）

| 上游 Java | Azrng C# | 说明 |
|-----------|----------|------|
| `CCJSqlParserUtil` | `SqlParser`（实际实现）+ `CCJSqlParserUtil`（`[Obsolete]` 转发别名） | 新代码用 `SqlParser`，旧代码无需改（仅 obsolete 警告） |
| `CCJSqlParserUtil.parse()` | `SqlParser.Parse()` | 单语句 |
| `CCJSqlParserUtil.parseStatements()` | `SqlParser.ParseStatements()` | 多语句 |
| `CCJSqlParserUtil.parseExpression()` | `SqlParser.ParseExpression()` | 独立表达式 |
| `CCJSqlParserUtil.parseCondExpression()` | `SqlParser.ParseCondExpression()` | 条件表达式 |
| —（无） | `SqlParser.ParseNullable()` | Azrng 自有：失败返回 null 不抛异常 |
| `JSQLParserException` | `JSqlParserException` | 命名已合理 |

---

文件结束。
