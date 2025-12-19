## Azrng.AspNetCore.DbEnvConfig

这是一个基于 ASP.NET Core 的数据库环境配置库，允许从数据库中读取配置信息并将其集成到标准的 IConfiguration 系统中。

### 功能特性

- 从数据库加载配置项到 IConfiguration
- 支持自动刷新配置（可配置刷新间隔）
- 支持 JSON 格式的复杂配置值
- 支持多种数据库（通过自定义连接）
- 自动创建配置表结构
- 支持 PostgreSQL、SQL Server、MySQL 等关系型数据库
- 线程安全的配置访问
- 支持多框架：.NET 6.0 / 8.0 / 9.0 / 10.0

### 安装

通过 NuGet 安装:

```
Install-Package Azrng.AspNetCore.DbEnvConfig
```

或通过 .NET CLI:

```
dotnet add package Azrng.AspNetCore.DbEnvConfig
```

### 使用方法

#### 基本配置

在 Program.cs 中添加数据库配置提供程序：

```c#
var builder = WebApplication.CreateBuilder(args);

// 添加数据库配置提供程序
builder.Configuration.AddDbConfiguration(options =>
{
    // 设置数据库连接工厂
    options.CreateDbConnection = () => new NpgsqlConnection(connectionString);

    // 设置表名（对于PostgreSQL可以指定schema）
    options.TableName = "config.system_config";

    // 设置键和值的列名
    options.ConfigKeyField = "code";
    options.ConfigValueField = "value";

    // 启用自动刷新，默认为true
    options.ReloadOnChange = true;

    // 设置刷新间隔，默认5秒
    options.ReloadInterval = TimeSpan.FromSeconds(10);

    // 添加筛选条件（可选）
    options.FilterWhere = " AND is_delete = false";
});
```

#### 数据库表结构

默认情况下，组件会尝试创建如下结构的表：

```sql
-- PostgreSQL 示例
CREATE TABLE config.system_config (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL,        -- 配置键
    value VARCHAR(2000) NOT NULL,     -- 配置值
    is_delete BOOLEAN NOT NULL        -- 删除标记
);
```

如果需要自定义表结构，可以实现 [IScriptService](file:///C:/Work/gitee/nuget-packages/src/Shared/Azrng.AspNetCore.DbEnvConfig/IScriptService.cs#L3-L23) 接口并传入：

```c#
builder.Configuration.AddDbConfiguration(
    options => {
        // 配置选项...
        options.CreateDbConnection = () => new NpgsqlConnection(connectionString);
    },
    new CustomScriptService() // 自定义脚本服务
);
```

#### 配置值格式

该库支持多种配置值格式：

1. 简单字符串值：
   ```
   Key: "AppName"
   Value: "MyApplication"
   ```

2. JSON 数组：
   ```
   Key: "AllowedHosts"
   Value: "[\"localhost\", \"example.com\"]"
   ```

3. JSON 对象：
   ```
   Key: "Database"
   Value: "{\"ConnectionString\": \"...\, \"Timeout\": 30}"
   ```

在代码中使用配置：

```c#
public class MyService
{
    private readonly IConfiguration _configuration;

    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void DoSomething()
    {
        // 读取简单配置
        var appName = _configuration["AppName"];

        // 读取数组配置
        var allowedHosts = _configuration.GetSection("AllowedHosts").Get<string[]>();

        // 读取对象配置
        var dbConfig = _configuration.GetSection("Database").Get<DatabaseConfig>();
    }
}
```

#### 自定义脚本服务

如果需要支持不同的数据库或表结构，可以实现 [IScriptService](file:///C:/Work/gitee/nuget-packages/src/Shared/Azrng.AspNetCore.DbEnvConfig/IScriptService.cs#L3-L23) 接口：

```c#
public class CustomScriptService : IScriptService
{
    public string GetInitTableScript(string tableName, string field, string value, string? schema = null)
    {
        // 返回创建表的SQL脚本
        return $@"
            CREATE TABLE IF NOT EXISTS {tableName} (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                {field} NVARCHAR(50) NOT NULL,
                {value} NVARCHAR(MAX) NOT NULL,
                IsDelete BIT NOT NULL DEFAULT 0
            )";
    }

    public string GetInitTableDataScript()
    {
        // 返回初始化数据的SQL脚本（可选）
        return string.Empty;
    }
}
```

### 配置选项说明

| 属性 | 说明 | 默认值 |
|------|------|--------|
| CreateDbConnection | 数据库连接工厂函数 | 必填 |
| TableName | 表名（支持schema，如 config.system_config） | "config.system_config" |
| ConfigKeyField | 配置键字段名 | "code" |
| ConfigValueField | 配置值字段名 | "value" |
| ReloadOnChange | 是否启用自动刷新 | true |
| ReloadInterval | 刷新间隔 | 5秒 |
| FilterWhere | SQL查询筛选条件 | 空 |
| IsConsoleQueryLog | 是否输出查询日志 | true |

### 注意事项

1. 确保数据库连接字符串正确配置
2. 表必须包含配置键和值两个字段
3. 自动刷新会在后台线程中定期执行查询
4. 复杂JSON配置会被展开为层次结构
5. 组件会尝试自动创建表结构，但可能需要数据库用户具有相应权限

### 依赖包

- Microsoft.Extensions.Configuration
- System.Data.Common
- System.Text.Json

### 版本更新记录

* 1.1.0
  * 支持.Net9、.Net10
* 1.0.0
  * 基本操作