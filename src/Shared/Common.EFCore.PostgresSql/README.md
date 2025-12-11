## Common.EFCore.PostgresSQL

####  操作例子

```csharp
services.AddEntityFramework<AuthDbContext>(options =>
{
    options.ConnectionString = Configuration["DbConfig:Npgsql:ConnectionString"];
    options.Schema = "auth";
});
```

#### 版本更新记录

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