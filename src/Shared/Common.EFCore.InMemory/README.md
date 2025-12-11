## Common.EFCore.InMemory

###  操作例子

```c#
services.AddEntityFramework();
```

### 版本更新记录

* 1.5.0
  * 支持.Net10
* 1.4.1
  * 支持老的creater、modifyer、modify_time
* 1.4.0
  * 调整目录
* 1.4.0-beta3
  * 支持AddDbContextFactory
* 1.4.0-beta2
    * 移除针对netstandard2.1版本的支持
* 1.4.0-beta1
    * 支持.Net9
* 1.3.0
    - 增加默认注入IUnitOfWork
* 1.3.0-beta2
    * 支持.net8
* 1.3.0-beta1
    * 优化
* 1.2.0
    * 优化注入服务的方法
    * 增加InMemoryRepository继承自BaseRepository和IBaseRepository
* 1.2.0-beta1
    * 升级支持.net7
* 1.1.0-beta5
    * 增加非追踪
* 1.0.0-beta4
    * 修改注入方法名称为AddEntityFramework
* 1.1.0-beta3
    * 增加分页相关的类
    * 去除common包的依赖
* 1.1.0-beta2
    * 更新因为Common包升级导致的问题
* 1.1.0-beta1
    * 修改版本支持.net5、.net6、.netstandard2.1
* 1.0.3
    * 更新分页入参
* 1.0.2
    * 更新包版本
* 1.0.1
    * 基本操作内存数据库的封住哪个