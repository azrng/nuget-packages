# 修复 DevLogDashboard 核心缺陷

## 本次目标
- 修复内存存储默认分页统计错误
- 修复关键字查询 `or` 语义与大小写匹配问题
- 补齐 `ILogStore.InitializeAsync` 生命周期接入
- 让日志 Scope 与请求取消令牌生效

## 核心改动
- 重写 `InMemoryLogStore` 的分页与关键字过滤逻辑，修正快速路径 `total` 错误，并优先利用 `Id`、`RequestId` 索引缩小查询范围
- 重写 `BackgroundLogWriter`，在消费前确保执行 `InitializeAsync`
- 修复 `DevLogDashboardLogger.BeginScope()`，让 `_scopes` 可以真正写入日志
- API Handler 调用存储层时透传 `RequestAborted`
- 为选项增加基础兜底校验
- 新增针对分页总数、`or` 查询、Scope 记录、初始化时机的测试

## 修改文件
- `src/Shared/DevLogDashboard/Storage/InMemoryLogStore.cs`
- `src/Shared/DevLogDashboard/Background/BackgroundLogWriter.cs`
- `src/Shared/DevLogDashboard/Extensions/ServiceCollectionExtensions.cs`
- `src/Shared/DevLogDashboard/Middleware/DevLogDashboardLogger.cs`
- `src/Shared/DevLogDashboard/Middleware/DevLogDashboardApiHandler.cs`
- `test/DevLogDashboard.Test/Storage/InMemoryLogStoreTest.cs`
- `test/DevLogDashboard.Test/Background/BackgroundLogWriterTest.cs`
- `test/DevLogDashboard.Test/Middleware/DevLogDashboardLoggerTest.cs`
- `TASK.md`

## 校验情况
- 已通过：新增分页总数测试
- 已通过：新增 `or` 查询语义测试
- 已通过：新增 Scope 写入测试
- 已通过：新增初始化时机测试
- 已通过：`DevLogDashboard.PerformanceTest` 中的 `ConcurrentWriteStressTest` 与 `MemoryLeakTest` 相关 12 个测试
- 未全量执行：`DevLogDashboard.Test` 全量运行存在历史测试生命周期问题，部分 `BackgroundLogWriterTest` 使用 `StartAsync()` 但未统一 `StopAsync()`，会导致整套测试超时

## 风险或遗留项
- Dashboard 默认暴露面的安全边界本轮未改，仍建议后续补默认鉴权/环境限制
- 全量单测未全部跑通，当前验证以新增和受影响测试为主
