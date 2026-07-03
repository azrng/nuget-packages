# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

| ID | 任务名称 | 目标 | 阶段 | 状态 | 优先级 | 更新时间 |
|----|----------|------|------|------|--------|----------|
| T075 | Azrng.JSqlParser 同步上游 5.4..HEAD 高价值变更（逐项迁移 + 单元测试验证） | 逐步迁移上游 JSqlParser commit 7d2e6b65(5.4)..2b141568(HEAD) 的高价值功能与 Bug 修复，每项独立验证独立提交 | 阶段 1 后端实现 | DOING | high | 2026-07-03 |

### T075 进度

- 子项 1 ✅：ForUpdateClause（上游 commit 2b141568）— FOR UPDATE/FOR SHARE 多表 + ORDER BY 支持。新增 ForMode/Wait/ForUpdateClause 类、Select 基类字段、文法规则、AstBuilder、ToString；新增 ForUpdateTest 10 个用例。全量 473 测试通过。
- 已完成步骤：变更分析、ForUpdateClause 完整实现与测试
- 下一步：等待用户确认是否继续下一项（候选：嵌套括号指数回溯修复 commit fff8a081、InExpression 优先级修复 commit 0f9e4779、JSON_TABLE commit c5e2fdcd 等）
- 阻塞项：无

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T074 | Azrng.JSqlParser README 补充上游溯源信息（标注基于 jsqlparser-5.4 tag / commit 7d2e6b65） | DONE | 2026-07-03 |
| T073 | Common.Cache.Redis 连接事件日志增强（订阅 StackExchange.Redis 连接事件并记录日志，不改变现有重连策略） | DONE | 2026-07-03 |
| T072 | Azrng.AspNetCore.Core 修复发包版本号递增（1.3.1 -> 1.3.2 + Release 包构建验证） | DONE | 2026-07-03 |
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |
| T070 | Common.Cache.Redis 审查建议清理（删除死代码 + 清理残留注释 + 简化 SCAN 异常分支 + 补充行为说明） | DONE | 2026-07-03 |
