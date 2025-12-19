## Azrng.SqlMigration

一个脚本迁移的nuget包，目前只支持pgsql数据库

### 快速上手

将需要迁移的脚本文件放到执行目录下，比如可以放在wwwroot下的MigrationSql目录下，文件命名格式为1.0.0.txt、1.1.0.txt等

举例迁移一个数据库示例

```csharp
builder.Services.AddSqlMigrationService("default", config =>
{
    config.Schema = "aa";
    config.VersionPrefix = string.Empty;
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);

    // config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
}).AddAutoMigration();
```

### 功能

#### 支持的版本号

支持以下形式版本号

* 1.0.0.txt、1.0.0.sql
* 1.0.0.0.txt、1.0.0.0.sql
* 111.111.111.txt、111.111.111.sql
* 111.111.111.111.txt、111.111.111.111.sql

#### 迁移多个数据库

default、default2为迁移名字

```c#
builder.Services.AddSqlMigrationService("default", config =>
{
    config.Schema = "aa";
    config.VersionPrefix = string.Empty;
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
    // config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
}).AddSqlMigrationService("default2", config =>
{
    config.Schema = "bb";
    config.VersionPrefix = string.Empty;
    config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql2");
    config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn2);
    // config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
}).AddAutoMigration();
```

#### 迁移操作回调

如果想实现在开始迁移之前以及版本更新前后以及迁移之后做一些处理，可以继承自IMigrationHandler编写迁移回调处理，示例如下

```csharp
public class DefaultMigrationHandler : IMigrationHandler
{
    private readonly ILogger<DefaultMigrationHandler> _logger;

    public DefaultMigrationHandler(ILogger<DefaultMigrationHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> BeforeMigrateAsync(string oloVersion)
    {
        _logger.LogInformation($"原始版本：{oloVersion}");
        return Task.FromResult(true);
    }

    public Task<bool> VersionUpdateBeforeMigrateAsync(string version)
    {
        _logger.LogInformation($"版本：{version}迁移前");
        return Task.FromResult(true);
    }

    public Task VersionUpdateMigratedAsync(string version)
    {
        _logger.LogInformation($"版本：{version}迁移成功后");
        return Task.FromResult(true);
    }

    public Task VersionUpdateMigrateFailedAsync(string version)
    {
        _logger.LogInformation($"版本：{version}迁移失败");
        return Task.FromResult(true);
    }

    public Task MigratedAsync(string oldVersion, string version)
    {
        _logger.LogInformation($"原始版本：{oldVersion} 当前版本：{version}迁移成功");
        return Task.FromResult(true);
    }

    public Task MigrateFailedAsync(string oldVersion,string version)
    {
        _logger.LogInformation($"原始版本：{oldVersion} 当前版本：{version}迁移失败");
        return Task.FromResult(true);
    }
}
```

服务配置

```csharp
builder.Services.AddSqlMigrationService<DefaultMigrationHandler>("default", config =>
       {
           config.Schema = "aa";
           config.VersionPrefix = string.Empty;
           config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
           config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
           config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
       })
       .AddAutoMigration();
```

#### 分布式锁处理

为了防止多实例的情况下迁移出现问题，这里可以使用nuget包Azrng.DistributeLock.Redis来实现多实例的情况下只有一个实例进行迁移

```csharp
builder.Services.AddRedisLockProvider("localhost:6379,password=123456,defaultdatabase=0,abortConnect=false");

builder.Services.AddSqlMigrationService("default", config =>
       {
           config.Schema = "aa";
           config.VersionPrefix = string.Empty;
           config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
           config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
           config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
       })
       .AddAutoMigration();
```

### 版本更新记录

* 0.4.0
  * 引用.Net10正式包
* 0.3.1
  * 忽略Blazor Server打包后出来的.br .gz文件
* 0.3.0
  * 忽略Blazor Server打包后出来的.br .gz文件
* 0.2.0
    * 优化执行脚本回调
* 0.1.0
    * 更新迁移输出内容
    * 支持四位版本号
    * 支持 xxx.xxx.xxx 三位细分版本号
* 0.0.2
    * 修复sql问题
* 0.0.1
    * 基本迁移操作
