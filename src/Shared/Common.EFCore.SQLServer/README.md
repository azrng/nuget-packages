## Common.EFCore.SQLServer

###  操作例子

```c#
services.AddEntityFramework<AuthDbContext>(options =>
{
    options.ConnectionString = connectionString;
    options.Schema = "auth";
});
```

### 版本更新记录

* 1.5.0
    * 适配Common.EFCore修改
* 1.4.2
  * 支持老的creater、modifyer、modify_time
* 1.4.1
  * 更新注入问题，释放IServiceProvider
* 1.4.0
    * 调整目录
* 1.4.0-beta3
    * 支持AddDbContextFactory
    * IBaseRepository支持指定DbContext
* 1.4.0-beta2
    * 移除针对netstandard2.1版本的支持
* 1.4.0-beta1
    * 支持.Net9
* 1.3.0
    * 增加PostgreRepository继承自BaseRepository和IBaseRepository
    * 支持.net7、.net8
    * 增加默认注入IUnitOfWork
* 1.0.0-beta3
    * 升级包版本，支持.net5和.net6
* 1.1.0-beta2
    * 增加非追踪
* 1.0.0-beta1
    * 修改注入方法名称为AddEntityFramework
* 0.0.3
    * 更新分页入参
* 0.0.2
    * 封装简单方法