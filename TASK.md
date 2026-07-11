# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| T082 | Job.Quartz 实现类归位 | 将 JobService/TriggerService/SchedulerService 从接口文件拆出,归入 Services/ 目录,统一文件组织 | 阶段1 | DONE | 中 | 2026-07-10 |

> 当前无活跃任务。库代码审查修复（T098）已完成，全量 1355 测试通过（T099 补充回归测试 +25）。审查发现的 H1-H4/M1-M10/L1-L7 共 22 项缺陷已全部修复（M5 误报、M6 边缘跳过）。**与上游 JSqlParser 已无已知语法层缺口**。

### 最近完成

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| T099 | T098 缺陷修复补充回归测试 | 为 T098 修复项补回归保护：ExpressionVisitorAdapter 子树遍历(M1/M2)、JsonFunction null 路径(M3)、ParenthesedSelect.GetPlainSelect 异常(L8)、Merge round-trip(H1)、Validation CREATE/ALTER/DROP/MERGE/TRUNCATE/MINUS 能力(M7) | 阶段1 | DONE | 中 | 2026-07-11 |

> **保留决策（非缺陷，不修）**：SetOperationList + FOR XML/EMIT CHANGES 组合（SQL Server FOR XML 与 UNION 混用极罕见，上游亦不支持）；ASTNodeAccess token 回放基础设施保留（SetASTNode 零调用，但 L3 已修复循环漏末 token 的潜在缺陷，未来可按需启用）。

> 当前无活跃任务。核心 SQL 功能已与上游 JSqlParser 全面对标（含 84 项上游覆盖度探针）。**全部 backlog 已清零，无已知未迁移缺口**。

## 待业务驱动 Backlog（未迁移缺口清单）

> 上游 HEAD `2b141568`（5.4-SNAPSHOT，2026-04-12），无新提交。
> 下方为经多轮核查（T088~T092）确认的**剩余未迁移/部分迁移项**，按优先级分组。已完成的 BL-01~18 不再列历史记录。

### P3 未迁移（模型就绪/语法待优化，低工作量）

> _P3 已清零。WithSearchClause grammar 接线由 T094 完成（BL-18c 关闭）。_

### P4 未迁移（小众方言/低频，按需启动）

> _P4 已全部清零。BL-19a（EXPORT/IMPORT）、BL-19d（TableStatement）、BL-19h（WITH FUNCTION/WITH ISOLATION/FOR CLAUSE）由 T096 完成。BL-19b/c/e/f/g 由 T095 完成。_

### 排除项（经核查非缺口，不计入未迁移）

| 项 | 排除原因 |
|----|----------|
| Oracle MODEL 子句 | **上游根本不存在**（grammar 零命中），非 Azrng 缺口 |

### 排除项（经核查非缺口，不计入未迁移）

| 项 | 排除原因 |
|----|----------|
| Oracle MODEL 子句 | **上游根本不存在**（grammar 零命中），非 Azrng 缺口 |
| 子 visitor 适配层（FromItemVisitor/OrderByVisitor 等 7 套） | **架构差异**：Azrng 扁平 visitor（StatementVisitor/ExpressionVisitor）已覆盖功能，强行移植是负优化 |
| Skip/First/OptimizeFor/SampleClause | **等价实现**：Azrng 内联为 PlainSelect 字段/Table.TableSample，功能等价 |
| MySQLGroupConcat/UserVariable/VariableAssignment/AllValue/JsonExpression/XMLSerializeExpr | **等价合并**：BL-15 已确认合入 Function.cs/SetStatement/AnyType.All 等 |
| CURRVAL / JSON_TRANSFORM | **上游不支持**：核查确认上游 main/ 无此特性 |
| Index/NamedConstraint 等枚举伴生类拆分 | **风格差异**：Azrng 用扁平 Constraint 类，功能等价 |

### BL-15 对齐基线说明

- **对比时间**：2026-07-07（多轮核查持续至 2026-07-10）
- **上游仓库**：`C:/Work/SourceCode/sqlparser/JSqlParser`
- **上游对照点**：commit `2b141568`（5.4-SNAPSHOT，2026-04-12），`feat: add ForUpdateClause class with multi-table and ORDER BY support (#2426)`
- **上游无新提交**：`2b141568` 即上游 HEAD，无后续 commit 需追赶
- **对比维度**：Expression 类、Statement 类、Select/Schema 子类、grammar 关键字、ALTER 操作、CREATE TABLE 全方言、Select 子特性、INSERT/UPDATE/DELETE 修饰符、约束结构化、窗口函数、JSON 函数族、方言专项

### 测试规模差异说明（非缺陷，仅供参考）

- **Azrng**：1275 测试（截至 T096 P4 剩余方言清零，全部 backlog 完成）
- **上游 JSqlParser**：2309 测试
- **差距来源**：主要来自方言专项测试（ClickHouse/Snowflake/BigQuery 等上游 CreateTableTest/SelectTest 方言用例）及 EXPORT/IMPORT/KSQL 等小众方言；Azrng 测试独立设计，覆盖核心 SQL 路径 + 84 项上游代表性 SQL 解析覆盖度探针
- **上游覆盖度探针**：`UpstreamCoverageProbeTest`（84 项），从上游 CreateTableTest/SelectTest 抽取代表性 SQL，84/84 全通过

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T098 | Azrng.JSqlParser 库代码审查修复（审查发现 22 项缺陷全部修复：H1 Merge 三连失 SourceTable/WHEN AND/InsertValues、H2 区域性数值解析静默数据损坏、H3 多语句静默丢弃、H4 TablesNamesFinder 表名提取遗漏、M1/M2 ExpressionVisitorAdapter context 丢失与 Function 子树不全、M3 JsonFunction null 非法 SQL、M7/M8 Validation 校验补全、L1 CTE 括号、L2 Offset ROWS、L3 ASTNode 漏末 token、L4 SyntaxErrorListener 死代码、L6 KsqlJoinWindow 越界、L7 死变量；M5 误报、M6 边缘跳过；全量 1318 测试通过，净增 15 项回归测试） | DONE | 2026-07-11 |
| T097 | Azrng.JSqlParser VALUES 表构造器（补齐唯一语法层缺口：新增 Values 模型类继承 Select+FromItem、grammar selectBody 增加 valuesClause 分支、VisitValuesClause/VisitSelectBody 接入、SelectVisitor/TablesNamesFinder 补 Values、修复 INSERT/UPSERT VALUES 语义冲突；补充测试覆盖 SelectVisitor 调度/TablesNamesFinder 表名提取/集合运算修饰符/程序化构造；全量 1303 测试通过，净增 28 项，**与上游无已知语法层缺口**） | DONE | 2026-07-11 |
| T096 | Azrng.JSqlParser P4 剩余方言清零（BL-19d TableStatement MySQL 8.2、BL-19a EXPORT/IMPORT Exasol 透传、BL-19h-1 WITH FUNCTION、BL-19h-2 WITH ISOLATION DB2、BL-19h-3 FOR CLAUSE 透传扩展；全量 1275 测试通过，净增 21 项，**全部 backlog 清零**） | DONE | 2026-07-10 |
| T095 | Azrng.JSqlParser P4 小众方言批量补齐（BL-19b KSQL 窗口 HOPPING/TUMBLING/SESSION+WITHIN+EMIT CHANGES、BL-19c CREATE VIEW FORCE/SECURE/WITH READ ONLY、BL-19e PivotXml、BL-19f ParenthesedFromItem alias 保真、BL-19g ON DUPLICATE KEY UPDATE WHERE；全量 1254 测试通过，净增 20 项） | DONE | 2026-07-10 |
| T094 | Azrng.JSqlParser WithSearchClause grammar 接线（withItem 接 withSearchClause? + 结构化 WithSearchClause 模型类 + VisitWithSearchClause；修正"破坏 LL 预测"误判；全量 1234 测试通过，P3 backlog 清零） | DONE | 2026-07-10 |

文件结束。
