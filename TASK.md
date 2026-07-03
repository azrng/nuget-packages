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
- 已完成步骤：7 个子项的迁移/评估与测试
- 下一步：等待用户确认是否继续下一项（候选：JSON_TABLE c5e2fdcd、JOIN FETCH 091ef964 等）
- 阻塞项：无

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |
| T070 | Common.Cache.Redis 审查建议清理（删除死代码 + 清理残留注释 + 简化 SCAN 异常分支 + 补充行为说明） | DONE | 2026-07-03 |
