# 任务清单

> 本文件只维护当前活跃任务和最近完成的任务。已完成任务超过 5 条时自动删除最早的。

## 活跃任务

（无）

## 最近完成

| ID | 任务名称 | 状态 | 更新时间 |
|----|----------|------|----------|
| T066 | Common.Cache.MemoryCache 缓存审查问题修复（GetOrCreateAsync 工厂异常透出 + 单 Key 同步锁清理） | DONE | 2026-07-02 |
| T065 | 缓存三包联动破坏性升级（ICacheProvider 启用 nullable：Core 1.0.0 / MemoryCache 3.0.0 / Redis 3.0.0；Redis 重命名 RedisConfig→RedisCacheOptions；RedisManage 连接管理全链路异步化与 Dispose 并发收口） | DONE | 2026-07-03 |
| T064 | Common.Cache.MemoryCache 优化（MemoryConfig→MemoryCacheOptions + nullable + FailThrowException/GetAllAsync 修复 + 发布元数据） | DONE | 2026-07-02 |
| T063 | Common.HttpClients.Next README 文档同步（修正 Bearer 扩展方法失实描述、补充目录结构/JSON 约定/默认脱敏清单） | DONE | 2026-07-01 |
| T062 | Common.HttpClients.Next 目录结构整理（方案 A：纯物理分文件夹，命名空间不变） | DONE | 2026-07-01 |
