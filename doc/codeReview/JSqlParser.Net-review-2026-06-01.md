# JSqlParser.Net 代码审查

- 审查范围：`6ea1d8f` fix(jsqlparser): clear build warnings
- 审查时间：2026-06-01
- 审查结论：**通过**

---

## 1. `ASTNodeAccessImpl.AppendTo` 改为 `virtual` — 正确

将基类 `AppendTo` 从非虚方法改为 `virtual`，派生类型统一用 `override` 替代 `new`。修复了通过基类引用调用时无法多态派发的问题。

涉及类型：
- `ConnectByPriorOperator`、`HighExpression`、`Inverse`、`LambdaExpression`、`LowExpression`、`PreferringClause`、`StructType`
- `Select`、`FunctionAllColumns`
- `PipeOperator`（从 `new abstract` 改为 `abstract override`）

改动一致且正确。

## 2. ANTLR 语法修改 — 整体正确，有一处行为微调

| 改动 | 评价 |
|---|---|
| `EMPTY` → `EMPTY_KW` | 避免与可能的内置函数/关键字冲突，正确 |
| 删除 `BANG_EQUALS` (`!=`) | 安全，`NOT_EQUALS2` 已覆盖 `!=` |
| 新增 `APPLY`/`COMMENTS`/`INDEXES` | 对应 grammar 中已有的规则引用，补齐缺失 token |
| `joinType: INNER?` → `INNER` | 行为微调，见下文 |
| `columnOptions?` → `columnOptions` | 安全，`columnOptions` 内部已有 `?`，可匹配空 |
| `rollbackStatement` 去冗余括号 | 语义等价，正确 |
| pipe operator `joinType? JOIN` → `(joinType JOIN \| JOIN)` | 消除歧义，正确 |

### `joinType` 行为微调

之前 `INNER?` 会让 bare `JOIN` 匹配 `joinType`（匹配空），然后 `SetJoinType` 显式设 `join.Inner = true`。改后 bare `JOIN` 不再匹配 `joinType`，`Inner` 保持 `false`。

但 `Join.IsInnerJoin()` 的逻辑是 `Inner || !(Left || Right || Full || Outer || Cross || Natural)`，所以语义上 bare `JOIN` 仍被视为 inner join。唯一差异是 `ToString()` 输出 `JOIN` 而非 `INNER JOIN`——这实际上更忠于用户原始 SQL，是更优行为。

## 3. Nullable 修复 — 正确

- `SelectItem.ToString()`: `Expression.ToString() ?? string.Empty` — 合理防御
- `Validation.cs`: 增加 `_parsedStatements != null` 和 `text == null` 检查 — 合理
- 测试文件: `(PlainSelect)stmt` → `(PlainSelect)stmt!` — 可接受，测试中 `Parse` 不会返回 null，`!` 只消除编译器警告

## 4. `CA1716` 项目级抑制 — 合理

`Select`、`Alias`、`Function` 等名称来自 SQL 领域模型，重命名会破坏公共 API。项目级抑制是正确选择。

## 5. 测试断言优化 — 正确

`Assert.Equal(1, x.Count)` → `Assert.Single(x)` 是 xUnit 推荐写法，失败时提供更好的错误信息。

## 6. devlog 和 TASK.md 更新 — 符合规范

---

## 总结

改动质量好，风险低。`joinType` 的行为微调是唯一需要关注的点，但实际影响正面（`ToString()` 更忠实于原始 SQL）。463 个测试全部通过，0 警告。

---

## 处理结论

本次审查不需要继续修改代码，原因如下：

- 审查文件中未发现必须修复的缺陷项，结论已标记为“通过”。
- `ASTNodeAccessImpl.AppendTo`、ANTLR token 补齐、nullable 处理、测试断言调整均属于已完成且验证通过的修复。
- `CA1716` 未重命名公共类型是有意选择：`Select`、`Alias`、`Function` 等是 SQL 领域模型名称，重命名会破坏既有公共 API，项目级抑制更合适。
- `joinType` 的 bare `JOIN` 行为变化不会破坏语义；`Join.IsInnerJoin()` 仍会把无显式类型的 `JOIN` 视为 inner join，且 `ToString()` 输出 `JOIN` 比强制输出 `INNER JOIN` 更忠实于原始 SQL。
- 已执行的 `dotnet build` 和 `dotnet test` 均通过，风险已由现有测试覆盖。

因此该 review 文件仅补充“不需要修改”的判断依据，不新增代码修复项。
