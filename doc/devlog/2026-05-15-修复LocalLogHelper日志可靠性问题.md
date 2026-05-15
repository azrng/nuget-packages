# 修复 LocalLogHelper 日志可靠性问题

## 背景

代码审查发现 `Azrng.Core` 的 `LocalLogHelper` 存在刷盘并发保护不可靠、缺少显式 flush、错误日志目录未兜底创建、日志保留天数配置与 README 不一致、测试吞异常等问题。

## 变更

- 使用 `SemaphoreSlim` 保护日志队列刷盘过程，避免多个线程同时写入同一日志文件。
- 新增 `LocalLogHelper.FlushAsync()`，用于需要立即落盘的测试、退出前和关键日志场景。
- 进程退出时尝试 flush 当前队列，降低正常退出时的日志丢失风险。
- 新增 `CoreGlobalConfig.LogRetentionDays`，清理逻辑改为读取该配置。
- 错误日志写入前创建 `logs` 目录，避免兜底日志再次失败。
- 更新 README 中 `LocalLogHelper` 的 API 说明。
- 调整 `LocalLogHelperTest`，移除吞异常逻辑，并补充 flush 与保留天数配置回归测试。

## 验证

- `dotnet build src\Shared\Azrng.Core\Azrng.Core.csproj --no-restore`
- `dotnet test test\Common.Core.Test\Common.Core.Test.csproj --no-restore`

## 风险

- `WriteMyLogsAsync` 仍保持入队式写入以避免每条日志同步刷盘带来的性能损耗；需要立即读取或关键路径落盘时应调用 `FlushAsync()`。
