# Common.Dapper
## 使用场景说明

注入方案
```csharp
builder.Services.AddScoped<IDapperRepository, DapperRepository>();
```
然后安装指定数据库驱动包，比如pgsql数据库的Npgsql包等，然后注入数据库驱动
```csharp
var pgsqlConn = "数据库连接字符串";
builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(pgsqlConn));
```
然后再使用的时候直接注入IDapperRepository

## 更新说明

* 0.1.0
  * 支持查询的时候输出执行sql以及查询结果
* 0.0.1
  * 更新命名空间
* 0.0.1-beta1
  * 基础操作