# Azrng.Database.DynamicSqlBuilder

该项目已合并到 `Azrng.DataAccess`，不再作为独立 NuGet 包维护。

## 迁移方式

安装或引用 `Azrng.DataAccess` 后继续使用原命名空间：

```csharp
using Azrng.Database.DynamicSqlBuilder;
using Azrng.Database.DynamicSqlBuilder.Model;
```

源码已移动到 `src/Shared/Azrng.DataAccess/DynamicSqlBuilder`，测试项目也改为引用 `Azrng.DataAccess`。

## 方言支持

当前动态 SQL 构建模块仅支持并验证 PostgreSQL 方言：

- `IN`：`field = ANY(@parameter)`
- `NOT IN`：`field != ALL(@parameter)`
- 分页：`LIMIT ... OFFSET ...`

`SqlBuilderOptions.Dialect` 和 `SqlDialectService` 保留为后续多数据库方言扩展点，但 MySQL、SQL Server、SQLite、ClickHouse、Oracle 等方言暂未声明支持。
