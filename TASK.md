# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T107 | Azrng.JSqlParser 支持 @ 命名参数 | 修复 @name 被解析成普通 JDBC 参数导致变量名丢失的问题，补测试并产出新版包 | 阶段 2 | Codex | BLOCKED | P1 | 2026-07-16 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样：A 类运算符符号错配(Contains/ContainedBy/JsonOperator) + C 类 ExpressionVisitorAdapter 空 Visit(5) + D 类 ExpressionDescendantsWalker 空 Visit(6) + E 类结构性沉默丢弃(ORDER BY NULLS/WITH RECURSIVE/JOIN 多 ON) + 中危项(IsNullExpression PG 简写/InExpression Global/LikeExpression useBinary+REGEXP_LIKE 下划线/FullTextSearch 类型/Pivot 多聚合/SELECT INTO/DISTINCT ON/LIMIT BY/ORDER BY WITH ROLLUP/MySQL INDEX FOR 等)。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。9 批 commit + 1 批文档，测试 1465→1567（+102）。MIGRATION.md 第 13 节同步对照表 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-18 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T114 | Azrng.JSqlParser 非 PG 上游 issue 修复（8 条） | 修复探针复现的 8 条上游 issue：#1169 GROUP BY DESC（仅解析兼容，方向不结构化为字段）、#854 INTO @var、#1314 INSERT SET 主体（AS 行别名不修）、#1589 PRIMARY KEY NONCLUSTERED、#161 OPTION hint、#2298 CAST CHARACTER SET、#2427+#2006 _utf8mb4 introducer、#911 @table 表变量。按方言分 3 批 commit + 1 批文档 + 1 批回退精简。每条改 lexer+grammar+visitor+模型，新增 26 探针 + 25 round-trip 测试，全量 1599→1635 通过。剔除 #2421（BigQuery 小众）、#2428（MySQL 已死语法）。MIGRATION.md 第 14 节 + issue 分类清单状态列同步 | DONE | 2026-07-19 |
| T113 | Azrng.JSqlParser PostgreSQL 专项 12 条上游 issue 验证与修复 | 探针核查 issue 分类清单 ④ 全部 12 条：4 条移植版已支持，8 条复现已修复（#187 FTS @@/@@@ + gist 索引、#1416 EXPLAIN 选项、#1511 WITH ORDINALITY、#1728 interval hour to minute、#2326 XMLTable、#2411 ROWS FROM、#2412 (expr).*、#2432 LIKE ANY/ALL）。补 23 项探针 + 10 项 round-trip 测试，全量 1566→1599 通过 | DONE | 2026-07-18 |
| T112 | Azrng.JSqlParser 清理全部 Obsolete 成员 | 删除 21 处 [Obsolete] 转发壳，唯一内部调用点 TablesNamesFinder.GetTables 改 internal Traverse 内联；测试同步改用 stmt.GetTableNames()；README/MIGRATION 文档同步；版本保持 beta9 不变。编译 0 警告 0 错误，全量 1566 项 ×3 TFM 通过 | DONE | 2026-07-18 |
| T110 | Azrng.JSqlParser C# 风格化迁移 | 全部 10 批完成（批 7 枚举 PascalCase、批 9 接口 I 前缀、批 10 SqlParser 改名为破坏性变更；批 8 null! 治理分两增量完成）。MIGRATION.md 收口为纯对照表（462→356 行）。全量 1436 项通过 | DONE | 2026-07-18 |
| T109 | Azrng.JSqlParser 下沉结构化提取（GetTableReferences/GetSelectColumns/GetWhereConditions + 通用 DTO，beta7） | 下沉 LocalSqlParser 纯 AST 提取为库扩展方法 + 中性 DTO；补 32 项测试，全量 1431 项通过；版本 beta6→beta7 | DONE | 2026-07-17 |

文件结束。
