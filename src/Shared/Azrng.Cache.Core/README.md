# Azrng.Cache.Core

缓存公共接口的定义

## 版本更新记录

* 1.0.0
    * **破坏性更新**：`GetAsync(string)` 返回类型改为 `Task<string?>`，`GetAsync<T>` 返回类型改为 `Task<T?>`，如实表达未命中返回 `null`/`default` 的语义
    * 启用 `<Nullable>enable</Nullable>`
    * 补充 `PackageLicenseExpression`、`RepositoryType`、符号包等发布元数据
* 0.0.3
    * 修改方法RemoveAsync返回值
* 0.0.2
    * 修改方法KeyDeleteInBatchAsync为RemoveMatchKeyAsync
* 0.0.1
    * 第一版本接口定义
