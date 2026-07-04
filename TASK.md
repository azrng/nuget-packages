# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（逐项迁移 + 单元测试验证） | 逐步迁移上游 JSqlParser commit 7d2e6b65(5.4)..2b141568(HEAD) 的高价值功能与 Bug 修复，每项独立验证独立提交 | 阶段 1 后端实现 | DOING | high | 2026-07-03 |

### T075 进度

- 子项 1 ✅：ForUpdateClause（上游 commit 2b141568）— FOR UPDATE/FOR SHARE 多表 + ORDER BY 支持。已提交 b965f93。
- 子项 2 ✅（评估）：fff8a081 嵌套括号回溯修复 — 不适用（JavaCC 特定 LOOKAHEAD 优化，ANTLR 用 ALL(*) 无此机制），跳过。
- 子项 3 ✅（评估）：0f9e4779 InExpression 优先级修复 — bug 在 ANTLR 版不存在（文法通过显式括号分组天然规避），补 2 个回归测试，已提交 a78e7c2。
- 子项 4 ✅：e4004444 FOR READ ONLY/FETCH ONLY — 扩展 ForMode 枚举 + 文法 + 测试。顺带修复 fetchClause 既有的两处缺陷（VisitSelectStatement 漏赋值 select.Fetch、Fetch.ToString 漏输出 ONLY）。已提交 1cb2196。
- 子项 5 ✅（既有缺陷）：JOIN USING 序列化丢失 — VisitJoinClause 漏处理 USING 分支，已修复。已提交 0cb2945。
- 子项 6 ✅：6697c063 LOCK TABLE 语句 — 新增 LockMode/LockStatement 类、StatementVisitor 接口方法、文法 lockStatement/lockMode 规则、AstBuilder VisitLockStatement/VisitLockMode、TablesNamesFinder 表名提取；新增 LockTest 16 个用例。已提交 1d9a08f。
- 子项 7 ✅：f47a8b30 PG RETURNING OLD/NEW — 新增 ReturningClause/ReturningOutputAlias/ReturningReferenceType 类、Column/AllTableColumns 加 ReturningReference 字段、Insert/Update/Delete 加 Returning 属性、文法 returningClause 扩展 WITH 别名、AstBuilder VisitReturningClause 含 OLD/NEW 归一化；新增 ReturningClauseTest 5 个用例。全量 503 测试通过。
- 子项 8 ✅：7fc300f7 EXCEPT/MINUS DISTINCT 支持 — 文法 setOperator 加 DISTINCT 支持并分离 EXCEPT/MINUS；SetOperation 加 MINUS 类型、构造函数接收 distinct、ToString 输出 DISTINCT；CreateSetOperation 传入 distinct 并区分 MINUS；SelectStatementTest 新增 13 个用例。全量 516 测试通过。
- 子项 9 ✅：091ef964 JOIN FETCH — Join 加 Fetch 属性并在 ToString 输出 FETCH；文法 joinClause 加 FETCH?；VisitJoinClause 设置 Fetch；SelectStatementTest 新增 2 个用例。全量 518 测试通过。
- 子项 10 ✅：5b5fe6c2 PG cast 后复合字段访问 — 新增 RowGetExpression 类；ExpressionVisitor/Adapter 加 Visit 方法；VisitPostfixExpr 补充处理 DOT identifier 后缀(此前仅处理 DOUBLE_COLON cast，字段访问被丢弃)；ExpressionCoverageTest 新增 3 个用例。全量 521 测试通过。
- 子项 11 ✅：bfcb8b75 KEY 前缀表达式 — 新增 KeyExpression 类；ExpressionVisitor/Adapter 加 Visit 方法；文法 primaryExpr 加 keyExpression 分支(KEY columnRef)；VisitPrimaryExpr/VisitKeyExpression 处理；ExpressionCoverageTest 新增 3 个用例。全量 524 测试通过。
- 子项 12 ✅：5788ca06 MySQL FULLTEXT AGAINST — Lexer 新增 AGAINST/LANGUAGE/EXPANSION token；文法 primaryExpr 新增 fullTextSearch 规则(MATCH..AGAINST..searchModifier)支持 IN BOOLEAN/NATURAL LANGUAGE MODE 及 WITH QUERY EXPANSION；FullTextSearch 类重构(SearchModifier 属性替代 Filter)；AstBuilder 新增 VisitFullTextSearch；ExpressionCoverageTest 新增 4 个用例。全量 528 测试通过。
- 子项 13 ✅（评估）：bd3ce05f GENERATED ALWAYS AS IDENTITY — bug 在 ANTLR 版不存在（文法用显式 GENERATED (ALWAYS | BY DEFAULT) AS IDENTITY 规则天然规避 JavaCC 版的 ALWAYS token 兜底问题），补 2 个回归测试。全量 530 测试通过。
- 子项 14 ✅：468aefae openGauss ON DUPLICATE KEY UPDATE NOTHING — Lexer 新增 NOTHING token；文法 onDuplicateKey 扩展支持 UPDATE NOTHING 分支；Insert 新增 DuplicateUpdateNothing 标志；VisitInsertStatement 补充 onDuplicateKey 处理(此前完全遗漏，顺带修复 ON DUPLICATE KEY UPDATE 序列化)；Insert ToString 输出 ON DUPLICATE KEY UPDATE；DmlStatementTest 新增 2 个用例。全量 532 测试通过。
- 子项 15 ✅：a019aa01 MySQL SPATIAL KEY — Lexer 新增 SPATIAL token；文法 tableConstraint 新增 [UNIQUE|FULLTEXT|SPATIAL]? KEY 和 KEY 索引分支；VisitTableConstraint 重构按分支设置 Type/Columns/Name(此前只设 Name)；Constraint.ToString 区分简单约束与 MySQL 索引输出格式；DdlStatementTest 新增 4 个用例。全量 536 测试通过。
- 子项 16 ✅：c0e1d052 MySQL SELECT INTO OUTFILE/DUMPFILE — Lexer 新增 OUTFILE/DUMPFILE token；文法 intoClause 扩展支持 INTO OUTFILE/DUMPFILE(前置)及 plainSelect 末尾尾部位置；新增 MySqlIntoOutfile 类(Type/FileName/BeforeFrom)；PlainSelect 加 MySqlIntoOutfile 属性并在 AppendSelectBodyTo 按位置输出；VisitPlainSelect 处理前置和尾部 INTO；SelectStatementTest 新增 3 个用例。简化版不含 FIELDS/LINES 格式化子句。全量 539 测试通过。
- 子项 17 ✅（评估）：8d967803 DROP INDEX 限定表名 — bug 在 ANTLR 版不存在（文法 table 规则 identifier (DOT identifier)* 天然支持多段限定符），补 1 个回归测试。全量 540 测试通过。
- 子项 18 ✅（评估）：2d83cea9 DATA 作列名 — bug 不存在（DATA 已在 nonReservedKeyword），补 1 个回归测试。
- 子项 19 ✅（评估）：7b87d081 MySQL 函数式索引键 — bug 不存在（createIndex 用 orderByItem 含 expression 天然支持），补 2 个回归测试。
- 子项 20 ✅：999cdca2 PostgreSQL Row Level Security — 新增 CreatePolicy 类(PolicyName/Table/Command/Roles/UsingExpression/WithCheckExpression)；Lexer 新增 SECURITY/FORCE token；文法新增 createPolicy 规则(FOR/TO/USING/WITH CHECK)及 alterOperation 的 RLS 分支(ENABLE/DISABLE/FORCE/NO FORCE ROW LEVEL SECURITY)；StatementVisitor/Adapter/TablesNamesFinder 新增 CreatePolicy 访问；AstBuilder 新增 VisitCreatePolicy；CreatePolicyTest 12 个用例 + DdlStatementTest 4 个 ALTER RLS 用例。全量 559 测试通过。
- 跳过项：40ccf4b8/22da3265 CREATE SEQUENCE（Azrng 无 CREATE SEQUENCE 基础实现，需先建立完整功能）；9de70747 FOR XML PATH（Azrng 无 FOR XML PATH 基础实现）；c5b85abf dollar-quoted 切分（JavaCC 词法特定，不适用 ANTLR）。
- 已完成步骤：20 个子项的迁移/评估与测试
- 下一步：剩余大型新功能 JSON_TABLE c5e2fdcd 等
- 阻塞项：无

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |
| T070 | Common.Cache.Redis 审查建议清理（删除死代码 + 清理残留注释 + 简化 SCAN 异常分支 + 补充行为说明） | DONE | 2026-07-03 |
