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
| `CCJSqlParserUtil.parse(sql)` | `SqlParser.Parse(sql)` | 单语句，返回 `Statement?`；旧名 `CCJSqlParserUtil` 已删除（beta9 二次迭代），统一用 `SqlParser` |
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
| `TablesNamesFinder` | `TablesNamesFinder` | `GetTables` 公开方法已删除（beta9 二次迭代），改用 `GetTableNames` 扩展方法 |

> **何时还用 visitor**：需要"自定义遍历顺序/上下文传递/部分节点特殊处理"等 Descendants 覆盖不了的复杂场景，
> 或与上游 visitor 实现对照时。日常收集/提取请优先用扩展方法。

---

## 六、关键类名差异速查

| 上游 Java | Azrng C# | 备注 |
|-----------|----------|------|
| `CCJSqlParserUtil` | `SqlParser`（实际实现） | beta9 二次迭代删除 `CCJSqlParserUtil`（旧 `[Obsolete]` 转发别名），统一用 `SqlParser` |
| `JSQLParserException` | `JSqlParserException` | 异常类名大小写 |
| `PlainSelect` | `PlainSelect` | 同名 |
| `TablesNamesFinder.findTables()` | `GetTableNames()` 扩展方法 | beta9 二次迭代删除 `TablesNamesFinder.GetTables()` 公开方法（仅保留 internal `Traverse`） |
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
| `jjtGetFirstToken()` | `Token GetFirstToken()` | 去掉 JJTree 前缀；旧名 `JjtGetFirstToken()` beta8 起 `[Obsolete]`，beta9 二次迭代删除 |
| `jjtGetLastToken()` | `Token GetLastToken()` | 同上；旧名 `JjtGetLastToken()` beta8 起 `[Obsolete]`，beta9 二次迭代删除 |

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
| `TablesNamesFinder.findTables()` | （已删除公开 `GetTables()`） | beta9 二次迭代删除公开方法；内部 `Traverse` 仅供 `stmt.GetTableNames()` 扩展方法复用 |

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
| `BinaryExpression.getStringExpression()` | `public abstract string OperatorSymbol { get; }` | 方法→只读属性；旧名 `GetStringExpression()` beta8 起 `[Obsolete]`，beta9 二次迭代删除 |
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
| `Table.getAlias()` / `setAlias()` | `Alias? Alias { get; set; }` | `GetAlias()`/`SetAlias()` beta8 起 `[Obsolete]`，beta9 二次迭代删除 |
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
| `Select.getSelectBody()` | （已删除 `GetSelectBody()`） | 上游为兼容旧 API 保留；Azrng beta8 起 `[Obsolete]`，beta9 二次迭代删除（用具体子类型 PlainSelect/SetOperationList/Values 等） |
| `Select.getForUpdateTable()` | `Table? ForUpdateTable` 属性 | getter→表达式属性；旧名 `GetForUpdateTable()` beta8 起 `[Obsolete]`，beta9 二次迭代删除 |
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
| `getFirstTable()` | `Table? FirstTable` 属性 | getter→表达式属性；旧名 `GetFirstTable()` beta8 起 `[Obsolete]`，beta9 二次迭代删除 |

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
| `CCJSqlParserUtil` | `SqlParser`（实际实现） | beta9 二次迭代删除 `CCJSqlParserUtil`（旧 `[Obsolete]` 转发别名），统一用 `SqlParser` |
| `CCJSqlParserUtil.parse()` | `SqlParser.Parse()` | 单语句 |
| `CCJSqlParserUtil.parseStatements()` | `SqlParser.ParseStatements()` | 多语句 |
| `CCJSqlParserUtil.parseExpression()` | `SqlParser.ParseExpression()` | 独立表达式 |
| `CCJSqlParserUtil.parseCondExpression()` | `SqlParser.ParseCondExpression()` | 条件表达式 |
| —（无） | `SqlParser.ParseNullable()` | Azrng 自有：失败返回 null 不抛异常 |
| `JSQLParserException` | `JSqlParserException` | 命名已合理 |

---

## 十三、T111 对齐审计修复对照（beta10）

> 本节记录 T111 系统对比上游 JSqlParser 5.4 发现的迁移走样，及修复后的字段/行为对照。
> 9 批 commit 已落库，覆盖 17 处高危 + 9 处中危。下表用于下游业务方迁移参考。

### 13.1 已修复对照表

| 类 / 子句 | 上游 Java | Azrng C#（修复后） | 修复 commit |
|----------|-----------|------------------|------------|
| ORDER BY NULLS FIRST/LAST | `OrderByElement.nullOrdering` | `NullOrdering? NullOrder` | 批次1 `a003064` |
| WITH RECURSIVE | `WithItem.recursive` | `bool Recursive`（任一为 true 则整体输出） | 批次1 `a003064` |
| JOIN 多 ON | `Join.onExpressions` | `List<IExpression> OnExpressions`（grammar 真支持多 ON） | 批次1 `a003064` |
| Contains 符号 | `&>` | `OperatorSymbol = "&>"`（继承 ComparisonOperator） | 批次2 `f4d21c9` |
| ContainedBy 符号 | `<&` | `OperatorSymbol = "<&"`（继承 ComparisonOperator） | 批次2 `f4d21c9` |
| JsonOperator 参数化 | `JsonOperator(String op)` | `string Operator { get; set; }`（默认 `->`） | 批次2 `f4d21c9` |
| ExpressionVisitorAdapter 空 Visit | `visitBinaryExpression` | 5 处空 Visit 改为 `VisitBinary`（IsBoolean/IsDistinct/JsonOperator/Contains/ContainedBy） | 批次3 `1fe7410` |
| ExpressionDescendantsWalker 空 Visit | `visitBinaryExpression` | 7 处空 Visit 改为下钻（含 Matches/RegExpMatchOperator 残留修复） | 批次3 `1fe7410` |
| VisitPredicate 默认兜底 | 抛异常 | 默认分支改抛 `JSqlParserException`（防未来静默误归类） | 批次4 `9f6e4a1` |
| IsNullExpression PG 简写 | `useIsNull` / `useNotNull` | `UseIsNull` / `UseNotNull`（输出 `x ISNULL` / `x NOTNULL`） | 批次5 `a0fb192` |
| InExpression GLOBAL | `global` | `bool Global`（grammar 支持 `GLOBAL [NOT] IN`） | 批次5 `a0fb192` |
| LikeExpression REGEXP_LIKE 下划线 | `likeKeyWord.toString()` | `OperatorSymbol` switch 显式映射（保留下划线/空格） | 批次6 `dc852e0` |
| LikeExpression MySQL BINARY | `useBinary` | `bool UseBinary`（grammar 在 LIKE 后加 `BINARY?`） | 批次6 `dc852e0` |
| FullTextSearch 列类型 | `ExpressionList<Column>` | `List<Column> MatchColumns` | 批次7 `267e0bb` |
| Pivot 多聚合 | `List<SelectItem<Function>>` | `List<Function> Functions`（保留 `Function` 单值兼容 API） | 批次7 `267e0bb` |
| SELECT INTO | `intoTables` / `intoTempTable` | `List<Table>? IntoTables` / `Table? IntoTempTable` | 批次8 `02ef2e9` |
| DISTINCT ON | `Distinct.onSelectItems` | `List<SelectItem>? OnSelectItems`（grammar 新增 `distinctOnClause`） | 批次8 `02ef2e9` |
| LIMIT BY（ClickHouse） | `Limit.byExpressions` | `List<IExpression>? ByExpressions`（grammar `LIMIT ... (BY expressionList)?`） | 批次8 `02ef2e9` |
| ORDER BY WITH ROLLUP | `OrderByElement.mysqlWithRollup` | `bool MysqlWithRollup`（grammar 末尾 `WITH ROLLUP?`） | 批次9 `41af1a4` |
| MySQL INDEX FOR | `MySQLIndexHint.forClause` | `string? ForClause`（FOR JOIN/ORDER BY/GROUP BY） | 批次9 `41af1a4` |

### 13.2 跳过项 TODO（后续批次评估）

下列问题本轮识别但未实施，需后续单独评估，原因和影响如下：

| 跳过项 | 原因 | 影响 |
|-------|------|------|
| `ComparisonOperator` / `InExpression` / `Matches` / `CosineSimilarity` / `GeometryDistance` 的 `oldOracleJoinSyntax` / `oraclePriorPosition` | 涉及 9+ 类，需引入 `SupportsOldOracleJoinSyntax` 接口设计，且 Oracle 老式 `a = b(+)` 外连接现代写法是 JOIN 语法 | Oracle 老式外连接与 `PRIOR` 表达式方位信息无法承载，round-trip 丢 `(+)`/`PRIOR` |
| `ParenthesedSelect : Select`（上游继承关系） | 改基类破坏现有 `Select.Select` 内嵌设计，影响 visitor 接线与所有子查询处理路径 | 外部消费者按 `Select` 接口拿 sub-select 属性需特殊处理 |
| `GROUP BY` 普通表达式 + `GROUPING SETS`/`ROLLUP`/`CUBE` 混用 | grammar 当前三选一互斥（`g4:432-436`），改造需重写 groupByClause 结构 | `GROUP BY a, GROUPING SETS ((b))` 混用形式无法解析 |
| `SqlServerHints` 完整 12+ hint 关键字 | `HOLDLOCK`/`SERIALIZABLE`/`READCOMMITTED` 等是 SQL 保留字，与 lexer 现有关键字冲突，需谨慎梳理 | `WITH (TABLOCK, HOLDLOCK)` 等多 hint 不支持（当前仅 INDEX/NOLOCK） |

### 13.3 测试规模

- 修复前：1465 项
- 修复后：1567 项（+102，3 TFM × 1567 全通过）
- 新增测试文件 8 个，覆盖每批改动的语法/语义/round-trip

---

## 十四、T114 上游 issue 修复对照（非 PG，beta10）

> 本节记录 T114 批次修复的上游 JSqlParser open issue（PG 专项已在 T113 处理，本批覆盖通用 + SQL Server + MySQL）。
> 探针定位 → grammar/lexer/visitor/model 改动 → round-trip 测试。
> 修复决策遵循「长期价值 + 引入风险」过滤：已死语法、冷门特性、鼓励已弃用语义的，**不修或仅解析兼容、不结构化字段**。

### 14.1 已修复对照表（8 条 issue，1 个修复覆盖 2 个 issue）

| Issue | 方言 | 上游缺陷 | Azrng C#（修复后） | 修复 commit |
|------|------|---------|------------------|------------|
| #1169 | 通用 | `GROUP BY c DESC` 解析失败 | `groupByColumn: expression (ASC\|DESC)?` + `GroupByElement.GroupByColumnReferences`。**不结构化 `IsAsc`/`IsDesc` 字段**（GROUP BY ASC/DESC 在 MySQL 8.0 已弃用且无实际效果），方向通过 `GroupByColumnReference.OriginalText` 整体透传保 round-trip，不鼓励下游依赖已弃用语义 | `0aacffc` |
| #911 | SQL Server | `FROM @table_variable` 解析失败 | `table` 规则加 `SINGLE_AT_IDENTIFIER`/`S_AT_IDENTIFIER` 分支，`Table.Name` 直接保留 `@name` | `0aacffc` |
| #1589 | SQL Server | `PRIMARY KEY NONCLUSTERED` 解析失败 | lexer 新增 `NONCLUSTERED`/`CLUSTERED` token（按顺序），`columnConstraint`/`tableConstraint` 加可选 `clusterKind`，`Constraint.ClusterKind` 字段 | `0aacffc` |
| #161 | SQL Server | `OPTION (MAXRECURSION 2)` 解析失败 | `plainSelect` 末尾加 `optionClause?`，新增 `optionClause`/`optionHint` 规则透传 hint 文本，`PlainSelect.OptionHints` 字段 | `0aacffc` |
| #854 | MySQL | `SELECT ... INTO @var` 解析失败 | `intoClause` 加 `INTO parameter (COMMA parameter)*` 分支（前置避免被 @table 抢匹配），`PlainSelect.IntoVariables` 字段 | `d65c275` |
| #1314 | MySQL | `INSERT INTO t SET a=1,b=2` 解析失败 | `insertStatement` 加 `SET assignmentItem` 形式，`Insert.UseSet` 字段。**仅支持 SET 主体**，`AS new(m,n,p)` 行别名极冷门不修 | `d65c275` |
| #2298 | MySQL | `CAST(x AS CHAR CHARACTER SET utf8)` 解析失败 | `castExpr` 加可选 `castCharacterSetClause`，`CastExpression.CharacterSet`/`Collation` 字段 | `d65c275` |
| #2427 + #2006 | MySQL | `_utf8mb4'...' [COLLATE ...]` / `_utf8mb4 '...'` 解析失败 | lexer `StringPrefix` 扩展为 `'_' [a-zA-Z0-9]+`，`S_CHAR_LITERAL` 加 introducer+空格分支，`StringValue` 构造支持任意 `_xxx` 前缀 | `d65c275` |

### 14.2 探针未修（本批次跳过）

| Issue | 跳过原因 |
|------|---------|
| #2421 BigQuery MERGE BY TARGET/SOURCE | 小众语言，本批次不做 |
| **#2428 MySQL PROCEDURE ANALYSE()** | **MySQL 5.7 弃用、8.0 移除的已死语法，扩 grammar/模型/测试是长期负债，不修**（探针 Skip 记录） |
| #2435/#2359/#1927 等 MySQL 词法/索引细节 | 留作后续批次（#1295/#1893/#823/#538/#1570 已在 T115 处理） |
| #397/#2033/#672/#2039 等 SQL Server/Oracle 专项 | 留作后续批次 |
| #2440/#1170/#2163/#2195/#2194 等 AST 正确性 | 已在 T115 核实并转绿（见第十五节） |
| #467/#2403/#2438 工程类 | 不在本批次 |

### 14.3 测试规模

- T113 修复后（基线）：1599 项
- T114 修复后：1635 项 active + 17 探针 Skip 记录现状
- 新增测试文件 2 个：`NonPgIssuesProbeTest`（26 探针，9 active + 17 Skip）+ `NonPgFixRoundTripTest`（25 round-trip）

---

## 十五、T115 上游 issue 修复对照（AST 正确性 + MySQL DDL 索引族，beta10）

> 本节记录 T115 批次对「⑨ AST 正确性 5 条 + ① DDL 索引族 5 条」的探针核实结果。
> **核心结论**：10 条上游 issue 中，仅 2 条（#1570/#538）在 Azrng 移植版真实复现需修，其余 8 条移植版不复现、已支持或不适用。
> 清单原标注「⛔ 复现且未修复」与实际探针结果有显著出入，本节固化修正后的状态判断。

### 15.1 已修复对照表（2 条真实复现）

| Issue | 方言 | 上游缺陷 | Azrng C#（修复后） | 修复 commit |
|------|------|---------|------------------|------------|
| #1570 | MySQL | `CONSTRAINT c UNIQUE KEY idx (cols)` 双名场景吞掉约束名 c | `VisitTableConstraint` KEY/INDEX 分支按 token 位置分离：CONSTRAINT 名入 `Constraint.Name`、索引名入新增字段 `Constraint.IndexName`；`ToString` 双名输出 `CONSTRAINT c UNIQUE KEY idx (cols)` | `85d3dd7` |
| #538 | MySQL | `UNIQUE idx USING BTREE (cols) COMMENT '...'`（UNIQUE 后直接跟索引名，无 KEY/INDEX 关键字）grammar 不支持 | grammar `tableConstraint` 新增 `UNIQUE identifier? clusterKind? [USING identifier]? (indexColumnList) indexOption*` 分支；visitor 按 token 位置区分索引名与列前 USING method（BTREE/HASH） | `85d3dd7` |

### 15.2 探针转绿/不适用对照表（8 条）

| Issue | 方言 | 清单原标注 | 实际状态 | 处理 |
|------|------|----------|---------|------|
| #2440 | 通用 | ⛔ 复现 | **不复现** | 探针转绿 + AST 结构断言（AndExpression[InExpression, GreaterThanEquals]） |
| #1170 | 通用 | ⛔ 复现 | **不复现** | 探针转绿 + 输出断言（`NOT NOT 1 = 1`，上游 bug 是多输出一个 NOT） |
| #2163 | PG | ⛔ 复现 | **建模不同但 round-trip 正确** | 探针转绿；移植版用 `LambdaExpression` 承载 `col -> 'key'`（非 `JsonOperator`），输出正确。不引入 JsonOperator 改造（避免 `->` 的 lambda/JSON 二义性处理） |
| #2195 | 通用 | ⛔ 复现 | **不复现** | 探针转绿 + 参数断言（`(x,y,z) -> x+y` 三个 identifier 完整保留） |
| #2194 | 通用 | ⛔ 复现 | **不适用** | 探针转绿 + 注释说明；移植版 `SimpleNode`/`ASTNodeAccessImpl` 完全无 `Parent` 字段（架构差异），上游问题不存在 |
| #1893 | MySQL | ⛔ 复现 | **已支持** | 探针转绿 + round-trip 断言（`UNIQUE INDEX name (cols) USING BTREE COMMENT '...'` 完整保留） |
| #823 | MySQL | ⛔ 复现 | **主路径已支持** | 探针转绿 + 注释；原始 SQL 失败是 `bigint unsigned` 数据类型修饰符（独立问题，不属索引族，留待数据类型专项） |
| #1295 | MySQL | ⛔ 复现 | **已支持** | 探针转绿 + round-trip 断言（`ALTER TABLE t ADD INDEX (col)`） |

### 15.3 测试规模

- T114 修复后（基线）：1635 项 active + 17 探针 Skip
- T115 修复后：1654 项 active + 10 探针 Skip（探针转绿 8 条：⑨ AST 5 条 + ① DDL 3 条，#1927 函数索引仍 Skip）
- 新增测试：探针转 active 8 条 + AST 结构断言加强 5 条 + round-trip 新增 9 条

### 15.4 保留限制（评估后主动不修）

| 项目 | 不修理由 |
|------|---------|
| #2194 Parent 字段 | 移植版架构无 Parent 概念，引入需改 `SimpleNode`/`ASTNodeAccessImpl`/`IASTNodeAccess` 并在所有 visitor Visit 中传播，属架构性大改，超出 issue 修复范围 |
| #2163 JsonOperator 改造 | 移植版用 LambdaExpression 承载 `->` round-trip 正确；改建成 JsonOperator 需处理 `->` 在 SELECT 项（JSON 访问）vs lambda 上下文（`x -> x+1`）的二义性，改造面大且无 round-trip 缺陷驱动 |
| #823 原始 SQL 的 `bigint unsigned` | MySQL 数据类型修饰符（unsigned/signed/zerofill）独立问题，与索引族无关，留待数据类型专项批次 |

---

文件结束。
