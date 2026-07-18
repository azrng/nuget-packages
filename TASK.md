# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 任务目标 | 当前阶段 | 负责人 AI | 状态 | 优先级 | 最近更新时间 |
|----|----------|----------|----------|-----------|------|--------|--------------|
| T114 | Azrng.JSqlParser 非 PG 上游 issue 修复（10 条） | 修复探针复现的 10 条上游 issue（PG 之外、剔除小众语言与无长期价值的）：#1169 GROUP BY DESC、#854 INTO @var、#1314 INSERT SET AS 别名、#1589 PRIMARY KEY NONCLUSTERED、#161 OPTION hint、#2298 CAST CHARACTER SET、#2427+#2006 _utf8mb4 introducer、#2428 PROCEDURE ANALYSE、#911 @table 表变量。每条改 lexer+grammar+visitor+模型+测试，按方言分 3 批 commit。剔除 #2421（BigQuery 小众）与剩余 MySQL 索引细节/过程化/工程类 | 阶段 1 | ZCode | DOING | P1 | 2026-07-19 |
| T107 | Azrng.JSqlParser 支持 @ 命名参数 | 修复 @name 被解析成普通 JDBC 参数导致变量名丢失的问题，补测试并产出新版包 | 阶段 2 | Codex | BLOCKED | P1 | 2026-07-16 |
| T111 | Azrng.JSqlParser 对齐审计修复（17 处走样） | 修复系统对比发现的 17 处迁移走样：A 类运算符符号错配(Contains/ContainedBy/JsonOperator) + C 类 ExpressionVisitorAdapter 空 Visit(5) + D 类 ExpressionDescendantsWalker 空 Visit(6) + E 类结构性沉默丢弃(ORDER BY NULLS/WITH RECURSIVE/JOIN 多 ON) + 中危项(IsNullExpression PG 简写/InExpression Global/LikeExpression useBinary+REGEXP_LIKE 下划线/FullTextSearch 类型/Pivot 多聚合/SELECT INTO/DISTINCT ON/LIMIT BY/ORDER BY WITH ROLLUP/MySQL INDEX FOR 等)。Oracle oldOracleJoinSyntax 体系、ParenthesedSelect 继承、GROUP BY 混用、SqlServerHints 完整关键字跳过记录 TODO 在 MIGRATION.md 第 13.2 节。9 批 commit + 1 批文档，测试 1465→1567（+102）。MIGRATION.md 第 13 节同步对照表 | 阶段 1 | ZCode | REVIEW | P1 | 2026-07-18 |
| T113 | Azrng.JSqlParser PostgreSQL 专项 12 条上游 issue 验证与修复 | 探针核查 issue 分类清单 ④ 全部 12 条：4 条移植版已支持(#2233 dollar-quoted / #2342 深层嵌套 / #2430 EXCLUDE TIES / #2431 GROUPS)，8 条复现已修复——#187 FTS @@/@@@ + gist 索引(新 AT_AT/AT_AT_AT token+comparisonOperator+Matches、createIndex USING)、#1416 EXPLAIN 选项(explainOptionList+Analyze/Verbose/Options+6 新 token)、#1511 WITH ORDINALITY(tableFunction 后缀+TableFunction 字段)、#1728 interval hour to minute(dataType INTERVAL 分支)、#2326 XMLTable(新 XMLTABLE token+xmlTable 规则+XmlTable 模型)、#2411 ROWS FROM(新 RowsFrom 模型)、#2412 (expr).*(selectItem 分支+RowGetExpression 保括号)、#2432 LIKE ANY/ALL(predicateSuffix+LikeExpression.LikeQuantifier)。补 23 项探针 + 10 项 round-trip 测试，全量 1566→1599 通过 | 阶段 1 | Claude | DONE | P1 | 2026-07-18 |
| T108 | Azrng.JSqlParser 加 Descendants/Walk 扩展方法消除 visitor 副作用返回 + ARCHITECTURE 同步对照（beta6） | 新增 ExpressionExtension/StatementExtension（Descendants/Walk/ExtractTableNames），内部 walker 引擎不改 AST 与 visitor 接口；ARCHITECTURE 加对照表；补 42 项测试，全量 1399 项通过；版本 beta5→beta6 | 阶段 1 | ZCode | DONE | P1 | 2026-07-16 |
| T109 | Azrng.JSqlParser 下沉结构化提取（GetTableReferences/GetSelectColumns/GetWhereConditions + 通用 DTO，beta7） | 下沉 LocalSqlParser 纯 AST 提取为库扩展方法 + 中性 DTO（TableReference/SelectColumn/WhereCondition），业务约定与 DTO 装配留业务方；WHERE 递归穿透 Parenthesis（比原逻辑更完整）；补 32 项测试，全量 1431 项通过；版本 beta6→beta7 | 阶段 1 | ZCode | DONE | P1 | 2026-07-17 |

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T112 | Azrng.JSqlParser 清理全部 Obsolete 成员 | 删除 21 处 [Obsolete] 转发壳（CCJSqlParserUtil 整类/GetAlias/SetAlias/GetStringExpression/Jjt*Token/ExtractTableNames/GetTables/GetForUpdateTable/GetFirstTable/GetSelectBody/Validation 两个 Getter），唯一内部调用点 TablesNamesFinder.GetTables 改 internal Traverse 内联；测试同步改用 stmt.GetTableNames()；README/MIGRATION 文档同步；版本保持 beta9 不变。编译 0 警告 0 错误，全量 1566 项 ×3 TFM 通过 | DONE | 2026-07-18 |
| T110 | Azrng.JSqlParser C# 风格化迁移（按 MIGRATION.md 批次表逐批改造，每批一提交） | 全部 10 批完成（批 7 枚举 PascalCase、批 9 接口 I 前缀、批 10 SqlParser 改名为破坏性变更；批 8 null! 治理分两增量完成：增量 1 Between/Parenthesis，增量 2 BinaryExpression 基类 + 28 个 AST 字段 + WhereCondition DTO 全改 required，visitor/CNFConverter 分步赋值改对象初始化器，4 个带参构造加 SetsRequiredMembers，删 CreateBinary<T> 辅助方法内联 14 调用点）。MIGRATION.md 收口为纯对照表（删改造基线/治理专项/执行清单/改动日志 4 章 + 表格列重组为「上游 Java | Azrng C# | 说明」三列 + 填代码真实现状，462→356 行）。全量 1436 项通过 | DONE | 2026-07-18 |
| T106 | Azrng.AspNetCore.Authorization.Default 审查问题修复（Requirement 保留 string[] 公开 API 并补防御性拷贝、路径规范化、setter/constructor null 校验，处理器使用内部规范化集合，README/ARCHITECTURE/IPermissionVerifyService 示例改用 StartsWithSegments，测试 20→26 项） | DONE | 2026-07-13 |
| T105 | JwtBearer 版本号修正（包版本与 README 最新版本说明从 1.5.1 改回 1.5.0，并验证 nupkg 产物版本为 1.5.0） | DONE | 2026-07-13 |
| T104 | JwtBearer 当前未提交改动审查与提交（修正 UseJwtBearerDefaultResponses 命名一致性、README/ARCHITECTURE/测试同步，示例保留 AddMyAuthentication 的 Basic+Bearer 分发并显式启用预置响应，纯 JWT helper 设置 Bearer 默认方案） | DONE | 2026-07-13 |

文件结束。
