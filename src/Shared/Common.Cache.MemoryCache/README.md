# Common.Cache.MemoryCache

## 操作例子

首先需要安装组件，建议直接安装最新版本，然后就需要注入服务

```
services.AddMemoryCacheExtension(x =>
{
    x.DefaultExpiry = TimeSpan.FromSeconds(5); // 默认缓存过期时间
});
```

然后就可以在需要使用的地方直接注入ICacheProvider进行使用

## 版本更新记录

* 1.3.1
  * 更新批量删除缓存的日志输出
* 1.3.0
  * 修复.Net8模糊匹配生效问题
  * 引用.Net10正式包
* 1.3.0-beta9
    * 更新.Net10
* 1.3.0-beta8
    * 设置不缓存空值的时候问题修复
* 1.3.0-beta7
    * 修复GetOrCreateAsync读取不到缓存还存储的问题
* 1.3.0-beta6
    * 更新命名空间
* 1.3.0-beta5
    * 支持.Net9
    * 移除对netstandard2.1的支持
    * 更新注入方法AddMemoryCacheStore
* 1.3.0-beta4
    * 修改方法KeyDeleteInBatchAsync为RemoveMatchKeyAsync
    * 修改方法GetAllCacheKeys为GetAllKeys
    * 修改方法RemoveCacheAllAsync改为RemoveAllKeyAsync