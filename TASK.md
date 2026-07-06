# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（逐项迁移 + 单元测试验证） | 逐步迁移上游 JSqlParser commit 7d2e6b65(5.4)..2b141568(HEAD) 的高价值功能与 Bug 修复，每项独立验证独立提交 | 阶段 1 后端实现 | DOING | high | 2026-07-05 |

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
- 子项 20 ✅：999cdca2 PostgreSQL Row Level Security — 新增 CreatePolicy 类；Lexer 新增 SECURITY/FORCE token；文法新增 createPolicy 规则及 alterOperation 的 RLS 分支；StatementVisitor/Adapter/TablesNamesFinder 新增 CreatePolicy 访问；CreatePolicyTest 12 个用例 + DdlStatementTest 4 个 ALTER RLS 用例。全量 559 测试通过。
- 子项 21 ✅：c5e2fdcd JSON_TABLE 表函数 — 新增 JsonTable 类(实现 FromItem)+JsonTableColumn；Lexer 新增 JSON_TABLE token；文法 tableOrSubquery 新增 jsonTable 分支；JsonTableTest 5 个用例。全量 564 测试通过。
- 子项 22 ✅（既有缺陷）：INSERT VALUES 序列化缺失 — Insert 新增 ValuesItems 字段；VisitInsertStatement 解析 valuesList 填充值数据；Insert.ToString 输出 VALUES 子句支持多行；DmlStatementTest 新增 3 个 VALUES 用例。全量 567 测试通过。
- 子项 23 ✅：RETURNING INTO 子句 — 文法 returningClause 新增可选 INTO 分支；ReturningClause 新增 DataItems 字段；ReturningClauseTest 新增 3 个用例。全量 570 测试通过。
- 子项 24 ✅：CREATE SEQUENCE 基础实现 — 扩展 Sequence 加参数；新增 CreateSequence 语句类；Lexer 新增 SEQUENCE 等 token；文法新增 createSequence/sequenceParameter 规则；CreateSequenceTest 19 个用例；修复 VisitTable 3 段限定名缺陷。全量 589 测试通过。
- 子项 25 ✅：FOR XML PATH — PlainSelect 加 ForXmlPath 字段；文法 selectStatement 末尾新增 FOR XML PATH [(name)] 可选分支；VisitSelectStatement 解析 XML PATH；Select.AppendTo 输出 FOR XML PATH；ForXmlPathTest 5 个用例。全量 594 测试通过。
- 子项 26 ✅：001ad1c2 BETWEEN SYMMETRIC/ASYMMETRIC — Between 类加 UsingSymmetric/UsingAsymmetric 字段；Lexer 新增 SYMMETRIC/ASYMMETRIC token；文法 predicateSuffix BETWEEN 加可选修饰符；ExpressionBasicTest 新增 3 个用例。
- 子项 27 ✅：8810c016 ORDER BY COLLATE — OrderByElement 加 CollateName 字段；文法 orderByItem 加可选 COLLATE；SelectStatementTest 新增 2 个用例。
- 子项 28 ✅：e17cdef4 DISTINCTROW — 文法 plainSelect 加 DISTINCTROW 别名。
- 子项 29 ✅：5fe938bc CORRESPONDING — Lexer 新增 CORRESPONDING token；SetOperation 加 Corresponding 字段；文法 setOperator 加可选 CORRESPONDING；SelectStatementTest 新增 2 个用例。全量 602 测试通过。
- 子项 30 ✅：157988d1 PG DELETE USING 完整语法 — Delete 新增 UsingItems 字段；文法 deleteStatement 的 USING 扩展为 fromItem (COMMA fromItem)* 支持多表；AstBuilder VisitDeleteStatement 处理 USING 子句填充 UsingItems；Delete.ToString 输出 USING 子句；TablesNamesFinder Visit(Delete) 遍历 UsingItems 提取表名；DmlStatementTest 新增 4 个用例 + TablesNamesFinderTest 新增 1 个用例。修复了 USING 子句解析后丢失的既有缺陷。全量 607 测试通过。
- 子项 31 ✅（评估）：f10b52ed Between 内括号表达式 — bug 在 ANTLR 版不存在（ALL(*) 解析天然规避 JavaCC LOOKAHEAD(3) 限制），补 2 个回归测试。
- 子项 32 ✅：4f982e74 Oracle INSERT ALL/FIRST with WHEN — 新增 MultiInsert/MultiInsertBranch 类；StatementVisitor/Adapter 加 Visit 方法；文法新增 multiInsertStatement/multiInsertBranch 规则，支持 WHEN/ELSE/无条件 INTO 分支 + VALUES/子查询；AstBuilder VisitMultiInsertStatement/VisitMultiInsertBranch；TablesNamesFinder 遍历分支表名；MultiInsertTest 6 个用例。全量 615 测试通过。
- 子项 33 ✅：ff28f826 MySQL GROUP_CONCAT SEPARATOR — Lexer 新增 SEPARATOR token；Function 类加 Distinct/OrderByElements/Separator 字段并在 ToString 输出；文法 functionExpr 新增 groupConcatFunction 分支支持 DISTINCT/ORDER BY/SEPARATOR 内部子句；AstBuilder 新增 VisitGroupConcatFunction；ExpressionCoverageTest 新增 5 个用例。全量 620 测试通过。
- 子项 34 ✅：95ebda5a PG dollar-quoted StringValue + 待评估清单回归 — literal 规则新增 S_DOLLAR_QUOTED_STRING 分支；StringValue 加 DollarPrefix 字段在 ToString 按原前缀输出；lexer 用 DollarTag fragment 支持 $$ 和 $tag$ 形式；AstBuilder VisitLiteral 处理 dollar-quoted；ExpressionBasicTest 新增 7 个用例（dollar-quoted 2 + WITH MATERIALIZED 2 + TRY_CAST 1 + 已有 Between 括号）。同步验证 2f6afbc3 WITH MATERIALIZED、9dfa0d68 TRY_CAST 已天然支持。全量 625 测试通过。
- 子项 35 ✅（评估）：b19d556e EXPLAIN for DML — Azrng explainStatement 用 (EXPLAIN|ANALYZE) statement 递归引用任意 statement，天然支持 DML；新增 ExplainStatementTest 5 个用例（SELECT/INSERT/UPDATE/DELETE/ANALYZE）。
- 子项 36 ✅（评估）：12489af6 overeager lambda — Azrng 文法 identifier LAMBDA_ARROW expression 较保守不会过度解析；新增 Lambda 单/多参数 2 个回归用例。
- 已完成步骤：36 个子项的迁移/评估与测试（全量 632 测试通过，净增 169）

### T075 与上游 HEAD 2b141568 直接对比补齐（基于 C:\Work\SourceCode\sqlparser\JSqlParser）
- 子项 37 ✅：StringValue 通用 prefix — lexer S_CHAR_LITERAL 新增 StringPrefix fragment 支持 N/E/U/R/B/RB/_utf8 前缀；新增 S_ORACLE_Q_STRING token 支持 Oracle q'[...]' 等 5 种自定义分隔引号；StringValue 重构对齐上游字段（Value/Prefix/QuoteStr/DollarPrefix），构造函数自动识别前缀、Oracle q-string、dollar-quoted；ExpressionBasicTest 新增 11 个用例。全量 643 测试通过。
- 子项 38 ✅：MySQL INSERT 修饰符 — 新增 InsertModifierPriority 枚举（None/LowPriority/Delayed/HighPriority）；Insert 加 ModifierPriority/ModifierIgnore 字段并在 ToString 输出；lexer 新增 LOW_PRIORITY/HIGH_PRIORITY token；文法 insertStatement 新增可选修饰符；AstBuilder VisitInsertStatement 填充；DmlStatementTest 新增 6 个用例。全量 649 测试通过。
- 子项 39 ✅：窗口框架 ROWS/RANGE/GROUPS BETWEEN — 新增 WindowFrame/FrameBound/FrameType/BoundType/ExcludeType 类型；AnalyticExpression 加 WindowFrame 字段并在 ToString 输出；文法 windowFrame 简化为统一通过 windowFrameBound 处理单边界和 BETWEEN；AstBuilder 新增 VisitWindowFrame + BuildFrameBound；AdvancedExpressionTest 新增 5 个用例。修复 overClause.windowSpecification 中 windowFrame 被丢弃的缺陷。全量 654 测试通过。
- 子项 40 ✅：PG ON CONFLICT 整套 — 新增 InsertConflictTarget（IndexColumnNames/IndexExpression/WhereExpression/ConstraintName）+ InsertConflictAction（ConflictActionType/UpdateSets/WhereExpression）+ ConflictActionType 枚举；Insert 加 ConflictTarget/ConflictAction 字段并在 ToString 输出 ON CONFLICT 子句；文法新增 onConflictClause/insertConflictTarget/insertConflictAction 规则，支持 (col1, col2) [WHERE] / ON CONSTRAINT name / DO NOTHING / DO UPDATE SET ... [WHERE]；AstBuilder 新增 VisitInsertConflictTarget/VisitInsertConflictAction；DmlStatementTest 新增 7 个用例。全量 661 测试通过。
- 子项 41 ✅：ParenthesedInsert 继承 Insert — 重构 ParenthesedInsert 为继承 Insert.Insert；新增 Alias/Insert 字段；新增 Accept 隐藏方法确保 visitor 走 Visit(ParenthesedInsert)；TablesNamesFinder 加空判断；新增 ParenthesedInsertTest 4 个用例。全量 665 测试通过。
- 子项 42 ✅：MultiInsert 重构支持单分支多 INTO 目标 — 新增 MultiInsertClause 类（Table/Columns/ValuesItems/Select）；MultiInsertBranch 移除单目标字段改为 Clauses: List<MultiInsertClause>；WhenCondition/IsElse 通过 backing field 实现互斥维护；文法 multiInsertBranch 改为 multiInsertClause+；AstBuilder 拆分 VisitMultiInsertBranch/VisitMultiInsertClause；MultiInsertTest 重写 7 个用例（含单分支多 INTO 用例）。Breaking change 但仅自身 API 使用。全量 666 测试通过。
- 子项 43 ✅：DateUnitExpression — 新增 DateUnitExpression 类 + DateUnit 枚举（Century/Decade/Year/Quarter/Month/Week/Day/Hour/Minute/Second/Millisecond/Microsecond/Nanosecond）；ExpressionVisitor 加 Visit 方法；nonReservedKeyword 新增 YEAR/MONTH/DAY/HOUR/MINUTE/SECOND 允许作列名/参数；ExpressionBasicTest 新增 5 个用例（修复 SELECT YEAR FROM t 此前解析失败的缺陷）。全量 671 测试通过。
- 子项 44 ✅：Function keyword arguments — Function 加 KeywordArguments: List<KeywordArgument> 字段并在 ToString 输出；新增 KeywordArgument 类（Keyword/Expression）；文法 functionExpr 通用分支末尾新增 functionKeywordArgument* 可选段；AstBuilder 新增 VisitFunctionKeywordArgument；ExpressionCoverageTest 新增 3 个用例（SEPARATOR/多参数/IGNORE）。全量 674 测试通过。
- 已完成步骤：44 个子项（全量 674 测试通过，本次会话基于上游 HEAD 直接对比补齐 8 项）

### T075 剩余待办清单（5.4..HEAD 共 87 个 feat/fix，已处理 36 个，剩余 51 个）

#### 待评估适用性（可能已支持或不适用）
| commit | 内容 | 初步判断 |
|--------|------|----------|
| 528dd722 | array<double> 函数声明 | Azrng 无 CREATE FUNCTION 文法 |

#### 通用功能/修复（可迁移）
| commit | 内容 | 优先级 |
|--------|------|--------|
| 49958b6b | avoid visiting twice | medium |
| eeb04004 | avoid NPE and expose modifier | medium |
| 834afe18 | oracle outer join nvl/coalesce | medium |
| c7b3bdbd | ALTER TABLE USING INDEX clause | medium |
| 763e92d7 | alter table index descending | medium |
| ac46c434 | CREATE SCHEMA with catalog | medium |
| 624a768b | Oracle hierarchical queries | medium |
| 7c52e7fe | legacy Postgres named parameter | medium |
| 74607624 | Exasol IMPORT/EXPORT | low |
| cd71aada | Function keyword arguments | low |
| c60ff739 | normalised backtick quotes | low |
| 4fdfa785 | DateUnitExpression | low |
| 6c98f10f | SessionStatement with options | low |

#### 不适用（JavaCC 特定，ANTLR 无对应机制）
| commit | 内容 | 原因 |
|--------|------|------|
| cf5bbc9a | split CCJSqlParserTokenManager | JavaCC 词法层 |
| 59dfc3b0 | grammar LOOKAHEAD conflicts | JavaCC LOOKAHEAD |
| 08d0bcc9 | rework tokens and preserved keywords | JavaCC token 体系重构 |
| c5b85abf | dollar-quoted CREATE FUNCTION splitting | JavaCC 词法状态机 |
| 93515149 | remove obsolete Grammar option | JavaCC 配置 |
| 6049fd72/ac175138/7d42ff61/fe860ddd | 性能优化(JMH/lookahead) | JavaCC 性能优化 |

#### 方言专项（按需取用）
| commit | 内容 |
|--------|------|
| a34db0ce | ClickHouse SELECT SETTINGS |
| 64542c86 | ClickHouse parametric aggregate |
| 0e1715e9 | DuckDB CREATE TABLE STRUCT |
| 297ef846/aaebe591 | DuckDB STRUCT 数据类型 |
| 6ce95d54 | Trino UDF |
| 6f4c4fb2 | Snowflake time travel |
| df5e6690 | Databricks Temporal spec |
| 5fa071ef | BigQuery Historic Version |
- 下一步：按优先级逐项处理剩余通用功能/修复
- 阻塞项：无

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |
| T070 | Common.Cache.Redis 审查建议清理（删除死代码 + 清理残留注释 + 简化 SCAN 异常分支 + 补充行为说明） | DONE | 2026-07-03 |
