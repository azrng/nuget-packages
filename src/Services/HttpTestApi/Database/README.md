# DevLogDashboard PostgreSQL 存储实现

## 概述

本项目实现了基于 PostgreSQL 的 `ILogStore` 接口，用于持久化存储 DevLogDashboard 的日志数据。

## 功能特性

- ✅ 支持日志持久化存储到 PostgreSQL
- ✅ 支持完整的日志查询、过滤、分页
- ✅ 支持请求追踪（Trace）功能
- ✅ 自动创建数据库表和索引
- ✅ 使用 JSONB 存储结构化属性
- ✅ 支持按时间、级别、应用等多维度查询

## 配置说明

### 1. 安装依赖

项目已添加 Npgsql 包引用（PostgreSQL .NET 驱动）：

```xml
<PackageReference Include="Npgsql" Version="8.0.3" />
```

### 2. 配置连接字符串

在 `appsettings.json` 中添加 PostgreSQL 连接字符串：

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=devlogs"
  }
}
```

### 3. 注册服务

在 `Program.cs` 中使用泛型重载方法：

```csharp
builder.Services.AddDevLogDashboard<PgSqlLogStore>(options =>
{
    options.EndpointPath = "/dev-logs";
    options.ApplicationName = "SampleWebApi";
    options.ApplicationVersion = "1.0.0";
});
```

## 数据库初始化

### 方式一：自动初始化（推荐）

`PgSqlLogStore` 会在首次启动时自动创建所需的表和索引，无需手动操作。

### 方式二：手动初始化

如果需要手动创建数据库结构，请执行 `Database/init-postgres.sql` 脚本：

```bash
psql -U postgres -d devlogs -f Database/init-postgres.sql
```

## 数据表结构

### dev_logs 表

| 字段名 | 类型 | 说明 |
|--------|------|------|
| id | VARCHAR(50) | 日志唯一标识（主键） |
| request_id | VARCHAR(50) | 请求标识 |
| connection_id | VARCHAR(100) | 连接标识 |
| timestamp | TIMESTAMP | 日志时间戳 |
| level | VARCHAR(20) | 日志级别 |
| message | TEXT | 日志消息 |
| request_path | VARCHAR(500) | 请求路径 |
| request_method | VARCHAR(10) | 请求方法 |
| response_status_code | INTEGER | 响应状态码 |
| elapsed_milliseconds | BIGINT | 请求耗时（毫秒） |
| source | VARCHAR(200) | 日志来源 |
| exception | TEXT | 异常信息 |
| stack_trace | TEXT | 堆栈跟踪 |
| machine_name | VARCHAR(200) | 机器名称 |
| application | VARCHAR(200) | 应用名称 |
| app_version | VARCHAR(50) | 应用版本 |
| environment | VARCHAR(50) | 运行环境 |
| process_id | INTEGER | 进程 ID |
| thread_id | INTEGER | 线程 ID |
| logger | VARCHAR(200) | Logger 名称 |
| action_id | VARCHAR(100) | Action ID |
| action_name | VARCHAR(200) | Action 名称 |
| properties | JSONB | 结构化属性 |
| created_at | TIMESTAMP | 创建时间 |

### 索引

- `idx_dev_logs_timestamp` - 时间戳索引（降序）
- `idx_dev_logs_request_id` - 请求 ID 索引
- `idx_dev_logs_level` - 日志级别索引
- `idx_dev_logs_application` - 应用名称索引
- `idx_dev_logs_source` - 来源索引
- `idx_dev_logs_properties_gin` - JSONB GIN 索引

## 使用示例

### 查询所有日志

```sql
SELECT * FROM dev_logs ORDER BY timestamp DESC LIMIT 10;
```

### 按级别统计

```sql
SELECT level, COUNT(*) FROM dev_logs GROUP BY level ORDER BY level;
```

### 查询错误日志

```sql
SELECT * FROM dev_logs
WHERE level IN ('Error', 'Critical')
ORDER BY timestamp DESC;
```

### 按时间范围查询

```sql
SELECT * FROM dev_logs
WHERE timestamp BETWEEN '2024-01-01' AND '2024-12-31'
ORDER BY timestamp DESC;
```

### 查询请求追踪

```sql
SELECT * FROM dev_logs
WHERE request_id = 'your-request-id'
ORDER BY timestamp ASC;
```

### 清空所有日志

```sql
DELETE FROM dev_logs;
```

### 删除旧日志

```sql
-- 删除 30 天前的日志
DELETE FROM dev_logs WHERE timestamp < NOW() - INTERVAL '30 days';
```

## 性能优化建议

1. **定期清理旧日志**：避免数据量过大影响性能
   ```sql
   DELETE FROM dev_logs WHERE timestamp < NOW() - INTERVAL '30 days';
   ```

2. **创建分区表**（适用于大数据量场景）：
   ```sql
   -- 按月分区
   CREATE TABLE dev_logs_2024_01 PARTITION OF dev_logs
   FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
   ```

3. **定期 VACUUM**：
   ```sql
   VACUUM ANALYZE dev_logs;
   ```

## 与内存存储对比

| 特性 | InMemoryLogStore | PgSqlLogStore |
|------|------------------|---------------|
| 持久化 | ❌ 应用重启丢失 | ✅ 永久保存 |
| 存储容量 | 受内存限制 | 受磁盘空间限制 |
| 查询性能 | 极快 | 快（有索引） |
| 分布式支持 | ❌ | ✅ |
| 数据分析 | ❌ | ✅ 支持 SQL |
| 适用场景 | 开发、测试 | 生产环境 |

## 故障排查

### 连接失败

1. 检查 PostgreSQL 服务是否运行
2. 验证连接字符串配置
3. 确认网络和防火墙设置

### 表不存在

首次启动会自动创建，如果失败请检查：
1. 数据库用户是否有 CREATE TABLE 权限
2. 是否有足够的磁盘空间
3. 查看应用日志获取详细错误信息

### 查询慢

1. 确认索引已创建
2. 执行 `VACUUM ANALYZE dev_logs;`
3. 考虑清理旧数据或使用分区表

## 许可证

MIT License
