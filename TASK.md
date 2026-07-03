# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

（无）

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T071 | Azrng.AspNetCore.Core DI 标记接口过滤修复（过滤生命周期标记接口 + 仅标记服务按自身类型注册 + 补回归测试） | DONE | 2026-07-03 |
| T070 | Common.Cache.Redis 审查建议清理（删除死代码 + 清理残留注释 + 简化 SCAN 异常分支 + 补充行为说明） | DONE | 2026-07-03 |
| T069 | Common.Cache.Redis 审查问题修复（连接任务登记竞态 + GetOrCreate 工厂异常重复执行 + RemoveMatchKey 扫描失败误报成功） | DONE | 2026-07-03 |
| T068 | Common.Cache.Redis RedisManage 连接异常透出与 Dispose 收尾（StartConnectTracked 返回原始 task + ConnectCoreAsync 过滤 OCE + 删除终结器消除终结器线程 sync-over-async） | DONE | 2026-07-03 |
| T067 | Common.Cache.Redis Dispose 并发等待测试加固（消除 flaky sleep 断言 + 补多任务并发等待测试 + 简化 WaitForActiveConnectAsync 分配） | DONE | 2026-07-03 |
