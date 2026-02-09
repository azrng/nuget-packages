## Common.EFCore.SQLite

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
  * IBaseRepository支持指定DbContext
  * 支持AddDbContextFactory
* 1.4.0-beta2
    * 移除针对netstandard2.1版本的支持
* 1.4.0-beta1
    * 支持.Net9
* 1.3.0
    * 增加PostgreRepository继承自BaseRepository和IBaseRepository
    * 增加默认注入IUnitOfWork
* 1.2.0
    * 移除工作单元注入
* 1.1.2
    * 修复迁移的时候自定义配置未生效问题
* 1.1.1
    * 移除Zack.EFCore.Batch.Sqlite_NET6包
* 1.1.0
    * 升级包版本，支持.net6、.net7
* 1.0.0-beta3
    * 升级包版本，支持.netstandard2.1和.net5以及.net6
* 1.1.0-beta2
    * 增加非追踪
* 1.0.0-beta1
    * 修改注入方法名称为AddEntityFramework
* 0.0.3
    * 更新分页入参
* 0.0.2
    * 封装简单方法