## Common.EFCore.PostgresSQL

####  操作例子

```csharp
services.AddEntityFramework<AuthDbContext>(options =>
{
    options.ConnectionString = Configuration["DbConfig:Npgsql:ConnectionString"];
    options.Schema = "auth";
});
```

#### SQL 日志输出

包内部不会再创建独立的 `ConsoleLoggerProvider`，而是复用宿主应用 DI 中的 `ILoggerFactory`。如需在控制台打印 EF Core 执行的 SQL，请在宿主应用 `Program.cs` 中配置日志：

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

builder.Logging.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
builder.Logging.AddConsole();
```

如果只想通过 `appsettings.json` 控制日志级别，可配置：

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

#### 版本更新记录

* 1.7.1
  * 修复 DbContextOptions 创建时重复创建 ConsoleLoggerProvider 的资源泄漏风险，SQL 日志输出改由宿主应用 Logging 配置控制
* 1.6.0
    * 适配Common.EFCore修改
* 1.5.0
  * 支持.Net10
* 1.4.3
  * 支持老的creater、modifyer、modify_time
* 1.4.2
  * 更新注入问题，释放IServiceProvider
* 1.4.1
  * 修复目录引用问题
* 1.4.0
    * 调整目录
* 1.4.0-beta5
  * IBaseRepository支持指定DbContext
* 1.4.0-beta4
  * 支持AddDbContextFactory
* 1.4.0-beta3
    * 移除针对netstandard2.1版本的支持
* 1.4.0-beta2
    * 修复包引用问题
* 1.4.0-beta1
    * 支持.Net9
* 1.3.0
    * 更新EFCore.NamingConventions包版本
    * 增加默认注入IUnitOfWork
* 1.3.0-beta2
    * 升级支持.Net8

* 1.3.0-beta1
    * 增加PostgreRepository继承自BaseRepository和IBaseRepository

* 1.2.0
    * 移除工作单元注入

* 1.2.0-beta2
    * 新增设置时区的扩展方法
    * 同步支持Common.EFCore设置时间方案

* 1.2.0-beta1
    * 升级支持.net7
* 1.1.0-beta4
    * 增加非追踪
* 1.1.0-beta3
    * 修改注入方法名称为AddEntityFramework
* 1.0.0-beta2
    * 解决不显示主键类型
* 1.0.0-beta1
    * 修改版本支持.net5、.net6、.netstandard2.1
* 0.0.3
    * 更新分页入参
* 0.0.2
    * 封装简单方法
