# 核查 Azrng.AspNetCore.Core 代码审查

## 本次目标

核查 `doc/codeReview/Azrng.AspNetCore.Core-review-2026-05-26.md` 中未提交审查意见是否成立，并修复确认合理的问题。

## 核心改动

- 修复 `ShowAllServicesMiddleware` 输出服务类型名未 HTML 编码的问题。
- 修复 `AuditLogMiddleware` 共享 `Stopwatch`、异常时未恢复响应流的问题。
- 修复 `AuditLogMiddleware` 异常路径下 `EndTime` 未刷新导致耗时区间不准确的问题。
- 修正 `UseShowAllServicesMiddleware` 过时提示，改用框架内置 `Options.Create`。
- 将异常日志改为结构化日志模板。
- `CustomResultPackFilter` 改为按 `IResultModel` 判断是否已包装。
- `GetBaseUrl` 增加 Host 为空时的防护。
- 移除 `AppSettings.GetValue(params string[])` 的异常吞噬，并将旧静态 Helper 标记为过时。
- README 同步修正文档中过时或不存在的 API 名称。

## 修改文件

- `src/Shared/Azrng.AspNetCore.Core/`
- `test/Azrng.AspNetCore.Core.Test/CoreFeatureTests.cs`
- `TASK.md`

## 校验情况

- `dotnet test test\Azrng.AspNetCore.Core.Test\Azrng.AspNetCore.Core.Test.csproj` 通过，net8.0/net9.0 共 16 条测试通过，覆盖异常路径响应流恢复与 `EndTime` 刷新。
- `dotnet build src\Shared\Azrng.AspNetCore.Core\Azrng.AspNetCore.Core.csproj` 通过，net6.0/net7.0/net8.0/net9.0/net10.0 均构建成功。

## 风险或遗留项

- Review 中已注明“暂不处理/不能修改”的鉴权保护和 `UnsafeRelaxedJsonEscaping` 本次未改。
- `NoWarn` 抑制 AOT 警告属于兼容性策略问题，本次仅记录不调整。
