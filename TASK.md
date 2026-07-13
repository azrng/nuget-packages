# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

> 当前无活跃任务。

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T100 | Azrng.AspNetCore.Authentication.JwtBearer 审查修复与测试补充（默认密钥清空+IValidateOptions 收口、CreateToken/ValidateToken/GetJwtInfo 异常处理修正、共享 TVP、Singleton+IOptionsMonitor、OnChallenge 开关，测试 7→27 项，commit 662c35e） | DONE | 2026-07-13 |
| T099 | T098 缺陷修复补充回归测试（M1/M2/M3/L8/H1/M7 共 25 项，commit b85d8f6） | DONE | 2026-07-11 |
| T098 | Azrng.JSqlParser 库代码审查修复（审查发现 22 项缺陷全部修复：H1 Merge 三连失 SourceTable/WHEN AND/InsertValues、H2 区域性数值解析静默数据损坏、H3 多语句静默丢弃、H4 TablesNamesFinder 表名提取遗漏、M1/M2 ExpressionVisitorAdapter context 丢失与 Function 子树不全、M3 JsonFunction null 非法 SQL、M7/M8 Validation 校验补全、L1 CTE 括号、L2 Offset ROWS、L3 ASTNode 漏末 token、L4 SyntaxErrorListener 死代码、L6 KsqlJoinWindow 越界、L7 死变量；M5 误报、M6 边缘跳过） | DONE | 2026-07-11 |
| T097 | Azrng.JSqlParser VALUES 表构造器（补齐唯一语法层缺口：新增 Values 模型类继承 Select+FromItem、grammar selectBody 增加 valuesClause 分支、VisitValuesClause/VisitSelectBody 接入、SelectVisitor/TablesNamesFinder 补 Values、修复 INSERT/UPSERT VALUES 语义冲突） | DONE | 2026-07-11 |
| T096 | Azrng.JSqlParser P4 剩余方言清零（BL-19d TableStatement MySQL 8.2、BL-19a EXPORT/IMPORT Exasol 透传、BL-19h-1 WITH FUNCTION、BL-19h-2 WITH ISOLATION DB2、BL-19h-3 FOR CLAUSE 透传扩展；全部 backlog 清零） | DONE | 2026-07-10 |

文件结束。
