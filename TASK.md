# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| _无活跃任务_ | | | | | | |

> 当前无活跃任务。P4 剩余方言清零（TableStatement/EXPORT/IMPORT/WITH FUNCTION/WITH ISOLATION/FOR CLAUSE）已由 T096 完成（1275 测试通过）。**全部 backlog 清零**。

> 当前无活跃任务。核心 SQL 功能已与上游 JSqlParser 全面对标（1275 测试通过，含 84 项上游覆盖度探针）。**全部 backlog 已清零，无已知未迁移缺口**。

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
| T096 | Azrng.JSqlParser P4 剩余方言清零（BL-19d TableStatement MySQL 8.2、BL-19a EXPORT/IMPORT Exasol 透传、BL-19h-1 WITH FUNCTION、BL-19h-2 WITH ISOLATION DB2、BL-19h-3 FOR CLAUSE 透传扩展；全量 1275 测试通过，净增 21 项，**全部 backlog 清零**） | DONE | 2026-07-10 |
| T095 | Azrng.JSqlParser P4 小众方言批量补齐（BL-19b KSQL 窗口 HOPPING/TUMBLING/SESSION+WITHIN+EMIT CHANGES、BL-19c CREATE VIEW FORCE/SECURE/WITH READ ONLY、BL-19e PivotXml、BL-19f ParenthesedFromItem alias 保真、BL-19g ON DUPLICATE KEY UPDATE WHERE；全量 1254 测试通过，净增 20 项） | DONE | 2026-07-10 |
| T094 | Azrng.JSqlParser WithSearchClause grammar 接线（withItem 接 withSearchClause? + 结构化 WithSearchClause 模型类 + VisitWithSearchClause；修正"破坏 LL 预测"误判；全量 1234 测试通过，P3 backlog 清零） | DONE | 2026-07-10 |
| T093 | Azrng.JSqlParser ALTER 字段结构化（ALTER COLUMN 子句接线修复静默丢弃 + SET DATA TYPE/VISIBLE/INVISIBLE + CONVERT/CHARACTER SET；全量 1230 测试通过） | DONE | 2026-07-10 |
| T092 | Azrng.JSqlParser 长期对标剩余缺口（P2 UPDATE/DELETE修饰符、P3a CREATE VIEW补齐+修复CHECK OPTION位置bug、P3b LateralView、P3c JoinHint LOOP/HASH/MERGE、P3d WithSearchClause模型就绪；全量 1217 测试通过） | DONE | 2026-07-10 |

文件结束。
