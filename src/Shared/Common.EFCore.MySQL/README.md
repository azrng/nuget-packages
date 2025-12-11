## Common.EFCore.MySQL

### 操作例子

```c#
services.AddEntityFramework<AuthDbContext>(options =>
{
    options.ConnectionString = connectionString;
    options.Schema = "auth";
});
```

### 版本更新记录

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
    * 优化注入服务逻辑

* 1.1.0
    * 升级支持.Net7
    * 修复迁移的时候自定义配置未生效问题

* 1.1.0-beta5
    * 增加非追踪
* 1.1.0-beta4
    * 修改注入方法名称为AddEntityFramework
* 1.1.0-beta3
    * 增加分页相关的类
    * 去除common包的依赖
* 1.1.0-beta2
    * 更新因为Common包升级导致的问题
* 1.1.0-beta1
    * 修改版本支持.net5、.net6、.netstandard2.1
    * 更新组件包版本
* 0.0.3
    * 更新分页入参
* 0.0.2
    * 封装简单方法