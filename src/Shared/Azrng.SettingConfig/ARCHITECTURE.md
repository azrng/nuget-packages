# Azrng.SettingConfig 架构与原理说明

## 一、项目概述

`Azrng.SettingConfig` 是一个基于 ASP.NET Core 的业务配置维护系统，提供配置的数据库存储、Web 管理界面、历史版本控制和缓存机制。

### 核心特性
- 配置数据库持久化存储（PostgreSQL）
- Web 界面管理与编辑
- 配置历史版本追踪与回滚
- 分布式缓存支持（默认内存缓存，可扩展 Redis）
- 可扩展的授权认证机制
- RESTful API 接口

---

## 二、整体架构设计

项目采用**分层架构**设计，自下而上分为以下几层：

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  ┌──────────────────┐         ┌────────────────────────┐   │
│  │ Dashboard UI     │         │ SystemSettingController│   │
│  │ (Embedded Files) │         │ (API Endpoints)        │   │
│  └──────────────────┘         └────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────┐
│                      Service Layer                           │
│  ┌──────────────────────┐  ┌──────────────────────────────┐ │
│  │ConfigExternalProvide │  │    ConfigSettingService      │ │
│  │Service               │  │    (Internal Business)       │ │
│  └──────────────────────┘  └──────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                         │
│  ┌──────────────────────┐  ┌──────────────────────────────┐ │
│  │IDataSourceProvider   │  │    IDapperRepository         │ │
│  │(PgsqlDataSourceProvider)│    (Dapper ORM)              │ │
│  └──────────────────────┘  └──────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                      │
│  ┌──────────┐  ┌─────────────┐  ┌────────────────────────┐  │
│  │PostgreSQL│  │ Distributed │  │ Authorization Filters  │  │
│  │Database  │  │ Cache       │  │ (Basic Auth/Local)     │  │
│  └──────────┘  └─────────────┘  └────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## 三、核心组件说明

### 3.1 配置与启动 (ServiceCollectionExtensions.cs)

**职责**: 服务注册与依赖注入配置

```csharp
// 核心注册逻辑
services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(setting.DbConnectionString));
services.AddScoped<IDapperRepository, DapperRepository>();
services.AddScoped<IDataSourceProvider, PgsqlDataSourceProvider>();
services.AddScoped<IConfigSettingService, ConfigSettingService>();
services.AddScoped<IConfigExternalProvideService, ConfigExternalProvideService>();
services.AddSingleton<ManifestResourceService>();
services.AddDistributedMemoryCache();
```

**关键点**:
- 使用 `ApplicationPart` 集成控制器到宿主应用
- 配置参数验证（`ParamVerify()`）
- 支持自定义授权过滤器注入

---

### 3.2 Dashboard 中间件 (AspNetCoreDashboardMiddleware.cs)

**职责**: 处理前端 UI 请求与静态资源

**工作流程**:
1. **路由匹配**: 正则匹配 `RoutePrefix` 路径
2. **授权验证**: 遍历 `Authorization` 过滤器链
3. **资源注入**: 动态替换 HTML 中的占位符（如 `%(PageTitle)%`）
4. **静态文件**: 使用 `EmbeddedFileProvider` 提供嵌入式资源

```
GET /systemSetting/
    ↓
重定向到 /systemSetting/index.html
    ↓
授权过滤器验证
    ↓
响应注入配置后的 index.html
```

---

### 3.3 数据访问层

#### 3.3.1 IDataSourceProvider 接口

**抽象数据操作接口**，解耦业务逻辑与具体数据库实现：

```csharp
public interface IDataSourceProvider
{
    Task<bool> InitAsync();                          // 初始化数据库表结构
    Task<List<GetSettingInfoDto>> GetPageListAsync(...);  // 分页查询
    Task<string> GetConfigValueAsync(string key);    // 获取配置值
    Task<bool> UpdateConfigVersionAsync(...);        // 更新配置
    Task<bool> RestoreConfigAsync(int historyId);    // 版本回滚
    // ...
}
```

#### 3.3.2 PgsqlDataSourceProvider

**PostgreSQL 具体实现**，核心功能：

- **自动建表**: `InitAsync()` 检查并创建 `system_config` 和 `system_config_history` 表
- **触发器**: 创建 PostgreSQL 触发器自动记录配置变更历史
- **Dapper 操作**: 通过 `IDapperRepository` 执行 SQL

**数据库结构**:
```sql
-- 配置表
system_config (
    id, key, name, value, description, version,
    create_user_id, create_time, update_user_id, update_time, is_deleted
)

-- 历史表
system_config_history (
    id, key, value, version, update_user_id, update_time
)
```

**触发器机制**:
```sql
CREATE TRIGGER "trigger_AddSystemConfigFlow"
AFTER UPDATE OF "value" ON system_config
FOR EACH ROW
EXECUTE PROCEDURE func_addSystemConfigFlow()
```
每次配置值更新时，自动将旧值存入历史表。

---

### 3.4 服务层

#### 3.4.1 ConfigExternalProvideService

**对外提供配置查询服务**，核心流程：

```
GetConfigAsync<T>("key")
    ↓
检查缓存 (Cache Key: "setting_config:key")
    ↓
缓存命中 → 返回
    ↓
缓存未命中 → 查询数据库
    ↓
写入缓存 (过期时间: ConfigCacheTime 分钟)
    ↓
JSON 反序列化 → 返回强类型对象
```

**关键代码**:
```csharp
public async Task<string> GetConfigContentAsync(string configKey, bool throwError = true)
{
    var key = SettingConfigConst.ConfigPrefix + configKey;
    var crConfigContent = await _cache.GetStringAsync(key);
    if (crConfigContent != null)
        return crConfigContent;

    var crConfig = await _dataSourceProvider.GetConfigValueAsync(configKey);
    if (crConfig.IsNullOrWhiteSpace())
    {
        _logger.LogError("根据当前key{Key}没有查询到配置信息", key);
        return null;
    }

    await _cache.SetStringAsync(key, crConfig,
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_config.ConfigCacheTime) });
    return crConfig;
}
```

#### 3.4.2 ConfigSettingService

**内部业务逻辑服务**，处理 Dashboard 管理功能：

- 分页列表查询
- 配置详情获取
- 配置更新（清除缓存 + 更新数据库）
- 配置删除（逻辑删除 + 清除缓存）
- 版本历史查询与回滚

---

### 3.5 控制器层 (SystemSettingController.cs)

RESTful API 端点：

| HTTP 方法 | 路由 | 功能 |
|-----------|------|------|
| GET | `/page` | 分页查询配置列表 |
| GET | `/{configId}/details` | 获取配置详情 |
| PUT | `/details` | 更新配置 |
| GET | `/history/list/{key}` | 查询历史版本 |
| DELETE | `/{configId}` | 删除配置 |
| PUT | `/restore/{historyId}` | 还原历史版本 |

**路由匹配**: 使用 `[SettingMatchRoute]` 特性动态匹配 `ApiRoutePrefix`

---

### 3.6 授权机制 (IDashboardAuthorizationFilter)

**可扩展的授权过滤器接口**：

```csharp
public interface IDashboardAuthorizationFilter
{
    bool Authorize(DashboardContext context);
}
```

**内置实现**:
- `LocalRequestsOnlyAuthorizationFilter`: 仅允许本地访问（默认）
- `BasicAuthAuthorizationFilter`: HTTP Basic 认证（扩展包提供）

**使用方式**:
```csharp
options.Authorization = new IDashboardAuthorizationFilter[]
{
    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        Users = new[] { new BasicAuthAuthorizationUser { Login = "admin", PasswordClear = "123456" } }
    })
};
```

---

## 四、工作流程

### 4.1 配置查询流程

```
业务代码调用
IConfigExternalProvideService.GetConfigAsync<T>("key")
    ↓
检查缓存 (setting_config:key)
    ↓
[缓存命中]
    ↓
返回反序列化对象
    ↓
[缓存未命中]
    ↓
查询数据库 (system_config 表)
    ↓
写入缓存（60分钟过期）
    ↓
返回反序列化对象
```

### 4.2 配置更新流程

```
Dashboard UI 调用
PUT /api/configDashboard/details
    ↓
SystemSettingController.UpdateConfigDetailsAsync()
    ↓
ConfigSettingService.UpdateConfigDetailsAsync()
    ↓
获取配置 Key → 清除缓存
    ↓
更新数据库 (UPDATE system_config)
    ↓
[触发器自动记录历史]
    ↓
返回操作结果
```

### 4.3 版本回滚流程

```
Dashboard UI 调用
PUT /api/configDashboard/restore/{historyId}
    ↓
从 system_config_history 查询历史值
    ↓
更新 system_config 的 value 字段
    ↓
再次触发触发器 → 产生新的历史记录
    ↓
清除缓存
    ↓
返回成功
```

---

## 五、缓存策略

### 5.1 缓存设计

- **接口**: `IDistributedCache`（支持内存缓存和 Redis）
- **Key 格式**: `setting_config:{configKey}`
- **过期时间**: `ConfigCacheTime` 分钟（默认 60 分钟）
- **失效时机**: 配置更新/删除时主动清除

### 5.2 扩展 Redis 缓存

默认使用 `AddDistributedMemoryCache()`，可替换为 Redis：

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

---

## 六、扩展点

### 6.1 自定义数据源

实现 `IDataSourceProvider` 接口支持其他数据库：

```csharp
public class MySqlDataSourceProvider : IDataSourceProvider
{
    // 实现 MySQL 版本的数据访问
}
```

### 6.2 自定义授权

实现 `IDashboardAuthorizationFilter` 接口：

```csharp
public class CustomAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // 自定义授权逻辑
    }
}
```

### 6.3 自定义缓存

替换 `IDistributedCache` 实现：

```csharp
services.AddSingleton<IDistributedCache, CustomCacheProvider>();
```

---

## 七、技术栈

| 层级 | 技术 |
|------|------|
| Web 框架 | ASP.NET Core |
| 数据访问 | Dapper |
| 数据库 | PostgreSQL |
| 缓存 | IDistributedCache (Memory/Redis) |
| 嵌入式资源 | EmbeddedFileProvider |
| JSON 序列化 | Newtonsoft.Json |

---

## 八、文件组织结构

```
Azrng.SettingConfig/
├── Attributes/                    # 特性
│   └── SettingMatchRouteAttribute.cs
├── Controller/                    # 控制器
│   └── SystemSettingController.cs
├── Dto/                          # 数据传输对象
├── Repository/                   # 数据访问层
│   ├── IDapperRepository.cs
│   └── DapperRepository.cs
├── Service/                      # 服务层
│   ├── IConfigExternalProvideService.cs
│   ├── ConfigExternalProvideService.cs
│   ├── IConfigSettingService.cs
│   ├── ConfigSettingService.cs
│   ├── IDataSourceProvider.cs
│   ├── PgsqlDataSourceProvider.cs
│   └── IDashboardAuthorizationFilter.cs
├── DashboardOptions.cs           # 配置选项
├── ServiceCollectionExtensions.cs # 服务注册
├── AspNetCoreDashboardMiddleware.cs # UI 中间件
├── ApplicationBuilderExtensions.cs   # 应用扩展
└── wwwroot/                      # 嵌入式前端资源
```

---

## 九、设计原则

1. **接口隔离**: 通过 `IDataSourceProvider` 抽象数据访问，支持多数据库
2. **依赖注入**: 所有组件通过 DI 容器管理
3. **单一职责**: 每个服务职责明确（外部查询 vs 内部管理）
4. **开闭原则**: 通过接口和过滤器支持扩展
5. **缓存优先**: 减少数据库访问，提升性能
6. **自动化历史**: 通过数据库触发器自动记录版本历史

---

## 十、注意事项

1. **连接字符串**: 必须配置有效的 PostgreSQL 连接字符串
2. **Schema 配置**: 默认使用 `setting` schema，可通过 `DbSchema` 配置
3. **缓存时间**: 根据业务需求调整 `ConfigCacheTime`
4. **授权策略**: 生产环境建议配置 Basic Auth 或自定义授权
5. **并发更新**: 触发器机制保证每次更新都有历史记录，但不解决并发冲突