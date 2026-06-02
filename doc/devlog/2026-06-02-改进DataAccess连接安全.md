# 改进 DataAccess 连接安全

## 本次目标
- 核查 `Azrng.DataAccess` 连接安全建议的合理性。
- 在保持现有连接字符串构造入口兼容的前提下，收敛 `DataSourceConfig` 连接字符串构造和脱敏逻辑。

## 核心改动
- 新增 `DataSourceConnectionStringBuilder`，统一按数据库类型构建连接字符串，并提供 `MaskConnectionString` 脱敏入口。
- `MySqlDbHelper`、`PostgresSqlDbHelper`、`SqlServerDbHelper`、`OracleDbHelper`、`SqliteDbHelper`、`ClickHouseDbHelper` 的 `DataSourceConfig` 构造路径统一调用包内构建工具。
- `DbHelperBase` 新增 `MaskedConnectionString`，并提供 `IDbHelper.GetMaskedConnectionString()` 扩展方法，调用方可直接获取脱敏连接字符串且不破坏外部自定义接口实现。
- 补充单元测试覆盖连接字符串构建、池化配置和脱敏行为。

## 修改文件
- `src/Shared/Azrng.DataAccess/DataSourceConnectionStringBuilder.cs`
- `src/Shared/Azrng.DataAccess/DbHelperExtensions.cs`
- `src/Shared/Azrng.DataAccess/Helper/*.cs`
- `test/Azrng.DataAccess.Test/DbOperatorUnitTests.cs`
- `TASK.md`

## 校验情况
- `dotnet build .\src\Shared\Azrng.DataAccess\Azrng.DataAccess.csproj` 通过。
- `dotnet test .\test\Azrng.DataAccess.Test\Azrng.DataAccess.Test.csproj --logger "console;verbosity=normal"` 通过，12 个测试全部通过。

## 风险或遗留项
- 旧的 `string connectionString` 构造入口保留用于兼容，调用方如果继续直接传明文连接字符串，运行时仍会持有完整连接字符串。
- 本次未做真实数据库连接 smoke test，仅验证连接字符串构建与单元行为。
