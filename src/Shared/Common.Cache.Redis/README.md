# Common.Cache.Redis

## 操作例子

首先需要安装nuget包，直接安装最新版本即可

```
services.AddRedisCacheService(x =>
{
    x.ConnectionString = "localhost:6379,password=123456,DefaultDatabase=0";
    x.KeyPrefix = "test";
});
```

然后在需要使用的地方进行注入`ICacheProvider`就可以使用了

## 版本更新记录

* 1.3.1
  * 更新异常信息输出
* 1.3.0
  * 更新正式包
* 1.2.0-beta9
  * 引用.Net10正式包
* 1.2.0-beta8
  * 适配.net10
* 1.2.0-beta7
    * 设置不缓存空值的时候问题修复
* 1.2.0-beta6
    * 增加可设置是否存储空字符串或者空集合选项，默认存储
* 1.2.0-beta5
    * 修复GetOrCreateAsync读取不到缓存还存储redis的问题
* 1.2.0-beta4
    * 支持.Net9
    * 增加扩展方法AddRedisCacheStore
* 1.2.0-beta-3
    * 修改方法KeyDeleteInBatchAsync为RemoveMatchKeyAsync
* 1.2.0-beta2
    * 依赖基类包：Azrng.Cache.Core
    * 优化代码
* 1.2.0-beta1
    * 支持netstandard2.1;net6.0;net7.0;net8.0
    * 将公共的缓存接口定义封装
* 1.1.1
    * 修改redis操作管理类
* 1.1.0
    * 更新版本为5.0
* 1.0.0
    * 3.1版本的redis公共库