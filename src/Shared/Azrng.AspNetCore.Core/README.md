## 说明

> 该包从Common.Mvc迁移过来

常见的Asp.NetCore里面辅助方法

## 操作

### 扩展类

#### HttpContext扩展

```c#
var iPv4 = HttpContext.GetLocalIpAddressToIPv4();
var ipv6 = HttpContext.GetLocalIpAddressToIPv6();
var clientIp = HttpContext.GetClientIp(); // 反向代理后的真实客户端 IP
var requestInfo = HttpContext.Request.GetUrl();
```

### 帮助类

#### HttpContextManager帮助类

> 已过时，建议通过依赖注入使用 `IHttpContextAccessor`。

需要提前注册：HttpContextManager.Init(httpContextAccessor)

```c#
//获取HttpContext
HttpContextManager.Current
```

### 公共返回类

封装了公共的返回类

```c#
IResultModel
IResultModel<T>
ResultModel:IsSuccess、Code、Message、Errors
ResultModel<T>：IsSuccess、Code、Message、Data
```

> 属性描述
>
> IsSuccess：是否成功
> Code:状态码
> Data:返回的数据
> Errors：模型校验的错误信息

返回正确的方法

```c#
[HttpGet]
public IResultModel<IEnumerable<WeatherForecast>> Get()
{
    var result = Enumerable.Range(1, 3).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    })
    .ToArray();
    return ResultModel<IEnumerable<WeatherForecast>>.Success(result);
}
```

返回的示例效果

```json
{
    "data": [
        {
            "date": "2022-05-20T22:13:35.2501522+08:00",
            "temperatureC": 52,
            "temperatureF": 125,
            "summary": "Freezing"
        },
        {
            "date": "2022-05-21T22:13:35.2505438+08:00",
            "temperatureC": 4,
            "temperatureF": 39,
            "summary": "Balmy"
        },
        {
            "date": "2022-05-24T22:13:35.250546+08:00",
            "temperatureC": 7,
            "temperatureF": 44,
            "summary": "Hot"
        }
    ],
    "isSuccess": true,
    "code": "200",
    "message": "success",
    "errors": []
}
```

返回错误的效果

```c#
[HttpGet]
public IResultModel<IEnumerable<WeatherForecast>> Get()
{
    return ResultModel<IEnumerable<WeatherForecast>>.Error("参数为空", "400");
}
```

返回结果

```json
{
    "data": null,
    "isSuccess": false,
    "code": "400",
    "message": "参数为空",
    "errors": []
}
```

### 自定义返回结果包装

通过自定义结果过滤器来默认给所有接口最外层包装一层返回类

```c#
services.AddControllers(options =>
{
    options.Filters.Add(typeof(CustomResultPackFilter));
});

// 或者使用该简便的方案

services.AddMvcResultPackFilter();
```

若是有些Action不想包装一层，只需要标注特性即可在返回的时候不显示包装的一层

```c#
[NoWrapperAttribute]
```

或者通过传入忽略的前缀来忽略指定接口的返回值包装

```csharp
builder.Services.AddMvcResultPackFilter("/api/configDashboard");
```

### 自定义模型验证

因为默认是启用模型校验的，所以当你传的model参数有问题的时候，还未到达action的时候已经处理了校验。

举例，当我们有一个post的接口，入参为

```c#
public class Userinfo
{
    [Required]
    [MinLength(5)]
    public string Id { get; set; }

    [MinLength(6)]
    public string Name { get; set; }
}
```

当传输不符合条件的数据时候返回的状态码是400，效果如下

```
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-40ff21ce6815e3b18232fa00f2024f67-84a9ab2db0b01cc9-00",
    "errors": {
        "Id": [
            "The Id field is required."
        ],
        "Name": [
            "The field Name must be a string or array type with a minimum length of '6'."
        ]
    }
}
```

这个效果是不方便前端处理的，所以我们使用的，所以我们自己做模型校验来封装错误信息

> 注意：需要先关闭默认的模型校验。

在ConfigureServices中注册自定义模型验证过滤器并禁用默认的自动模型验证

```csharp
services.AddControllers(options =>
{
	options.Filters.Add<ModelVerifyFilter>(); //注册模型校验过滤器
}).ConfigureApiBehaviorOptions(options =>
{
	//[ApiController] 默认自带有400模型验证，且优先级比较高，如果需要自定义模型验证，则需要先关闭默认的模型验证
	options.SuppressModelStateInvalidFilter = true;
});

// 或者使用简便方案
// 注意：简便方案只覆盖 [ApiController] 的自动 400；
// 非 ApiController 控制器的模型校验失败不会有统一处理，请使用上面的手写方案
services.AddMvcModelVerifyFilter();
```

我们再次调用接口

```c#
{
    "isSuccess": false,
    "code": "400",
    "message": "参数格式不正确",
    "errors": [
        {
            "field": "Id",
            "message": "The Id field is required."
        },
        {
            "field": "Name",
            "message": "The field Name must be a string or array type with a minimum length of '6'."
        }
    ]
}
```

这时候我们的错误信息会显示到error属性里面并且http错误码为400。

### 自定义特性

#### MinValue

最小值校验

```csharp
[MinValue(2)] // 校验id的值必须大于2
public int Id { get; set; }
```

#### CollectionNotEmpty

集合不能为空校验

```csharp
[CollectionNotEmpty] // 校验names值不能为空
public List<string> Names { get; set; }
```

### 依赖注入批量注册

需要注册的实现类继承指定的接口，比如用户实现类继承自接口IScopedDependency

```c#
public class UserService : IScopedDependency, IUserService
```

也可以继承自：ITransientDependency、ISingletonDependency，根据自己需求不同继承合适声明周期的接口

```c#
//批量注入示例
services.RegisterBusinessServices("MySQL_NetCoreAPI_EFCore.dll");
// 或者
services.RegisterBusinessServices("MySQL_NetCoreAPI_EFCore.*.dll");

//或者使用基础的方法，让继承某一类的注入
services.RegisterUniteServices(assemblies, typeof(ISingletonDependency), ServiceLifetime.Singleton);
```

### 中间件

#### 显示所有服务信息

使用方法

```c#
services.AddShowAllServices("/allservices");
```

然后使用中间件

```c#
app.UseShowAllServices();
```

然后就可以访问页面：`http://localhost:5000/allservices`,可以看到当前项目注入的服务

> 注意：服务清单会暴露内部架构信息，非 Development 环境默认返回 404 拒绝访问；
> 确有需要时可通过 `Configure<ShowServiceConfig>` 设置 `AllowNonDevelopment = true` 放开。

#### Cors跨域

处理跨域问题，提供简单易用的 CORS 策略配置。

##### 基础使用（开发环境）

允许任何来源的请求，仅适用于开发环境：

```csharp
// 注册 CORS 服务
builder.Services.AddAnyCors();

// 启用 CORS 中间件
app.UseCorsPolicy();
```

##### 生产环境配置

在生产环境中，明确指定允许的来源：

```csharp
// 注册 CORS 服务
builder.Services.AddCorsByOrigins(
    new[] { "https://example.com", "https://api.example.com" },
    allowCredentials: true
);

// 启用 CORS 中间件
app.UseCorsPolicy();
```

##### SignalR 环境

SignalR 需要允许凭证以支持认证：

```csharp
// 注册 CORS 服务
builder.Services.AddCorsByOrigins(
    new[] { "https://example.com" },
    allowCredentials: true  // SignalR 必须启用此选项
);

// 启用 CORS 中间件
app.UseCorsPolicy();
```

##### 高级配置

需要更细粒度控制（如 `WithExposedHeaders`、`SetPreflightMaxAge`）时，直接使用框架原生写法，`UseCorsPolicy` 仍可按策略名启用：

```csharp
// 自定义配置（框架原生 API）
builder.Services.AddCors(options => options.AddPolicy("CustomPolicy", builder => builder
    .WithOrigins("https://example.com")
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Content-Type", "Authorization")
    .AllowCredentials()
    .WithExposedHeaders("X-Total-Count")
    .SetPreflightMaxAge(TimeSpan.FromHours(1))));

// 启用 CORS 中间件
app.UseCorsPolicy("CustomPolicy");
```

##### 方法说明

| 方法 | 适用场景 | 参数 |
|------|---------|------|
| `AddAnyCors()` | 开发环境 | `policyName`（可选） |
| `AddCorsByOrigins()` | 生产环境 | `allowedOrigins`, `policyName`, `allowCredentials` |

##### 中间件

| 方法 | 说明 |
|------|------|
| `UseCorsPolicy(policyName)` | 启用 CORS 中间件，默认使用 "DefaultCors" 策略 |

##### 重要提示

⚠️ **安全警告**：
- `AddAnyCors()` 仅适用于开发环境，生产环境必须使用 `AddCorsByOrigins()`
- 当 `allowCredentials` 为 `true` 时，不能使用 `AllowAnyOrigin()`
- SignalR 环境必须设置 `allowCredentials: true`
- `policyName` 不能为空；`AddCorsByOrigins()` 至少需要一个非空来源

#### 启用body重复读

```csharp
app.UseRequestBodyRepetitionRead();
```

#### 请求ID传递

```csharp
app.UseRequestIdMiddleware();
```

客户端传入 `X-RequestId` 请求头时会原样返回到响应头（带长度上限与字符白名单校验），未传时沿用宿主 `TraceIdentifier`。

> 跨域场景下浏览器 JS 默认读不到自定义响应头，需在 CORS 策略中显式暴露：
> `policy.WithExposedHeaders("X-RequestId")`

#### 反向代理获取真实客户端 IP

部署在 Nginx/网关后时，`GetRemoteIpAddressToIPv4()` 拿到的是网关 IP。启用转发头处理后即可拿到真实客户端 IP：

```csharp
// 可信代理 IP 列表可选；不传时仅信任本机回环
app.UseForwardedHeaders("10.0.0.5", "10.0.0.6");

// 获取真实客户端 IP
var clientIp = HttpContext.GetClientIp();
```

⚠️ **安全提示**：
- `UseForwardedHeaders` 须在 `UseRouting()` 之前注册，且只消费可信代理来源的 `X-Forwarded-For`/`X-Forwarded-Proto`，直连请求伪造的转发头会被忽略
- 未启用该中间件时，`GetClientIp()` 直接解析请求头，结果可被客户端伪造，仅适用于展示、统计等非鉴权场景
- 需要按网段（CIDR）配置可信代理等高级场景，请直接使用框架原生 `ForwardedHeaders` 中间件

#### 启用自定义审计

##### 默认配置

```csharp
public void Configure(WebApplication app, IWebHostEnvironment env)
{
    app.UseRouting();
    app.UseStaticFiles();

    app.UseAutoAuditLog(); // 启用自动审计日志

    app.UseAuthorization();

    app.MapControllers();
}
```

默认情况下回将日志输出到默认到Logger中，且默认所有路由都会记录日志

如果没有注册 `ILoggerService`，中间件会使用默认日志服务写入 `ILogger`。默认日志服务优先使用容器中的
`IJsonSerializer`；未引用或未注册 `Azrng.Core.Json` 时，会自动使用 `System.Text.Json` 作为兜底序列化器。

审计日志内置以下安全与内存保护（可通过 `AuditLogOptions` 调整）：

- 敏感请求头脱敏：`Authorization`、`Cookie`、`Set-Cookie`、`Proxy-Authorization` 的值默认以 `******` 记录，名单由 `SensitiveHeaders` 配置，掩码文本由 `SensitiveHeaderMask` 配置
- 响应体限量捕获：只捕获前 `MaxResponseBodySize`（默认 1MB）字节用于日志，客户端仍收到完整响应，大响应不再整体驻留内存
- 请求体限量读取：超过 `MaxRequestBodySize`（默认 1MB）字符的请求体只记录前缀并标注截断；`multipart/` 与 `application/octet-stream` 请求直接跳过正文记录

##### HTTP请求日志示例

示例如下

```csharp
public class HttpSampleController : BaseController
{
    [HttpGet]
    public string Get(string name)
    {
        return "success" + name;
    }

    [HttpPost]
    public string Post([FromBody] TestHttpRequest request)
    {
        return "success" + request.Name;
    }

    [HttpPut]
    public string Put([FromBody] TestHttpRequest request)
    {
        return "success" + request.Name;
    }

    [HttpDelete]
    public string Delete(string id)
    {
        return "success" + id;
    }
}

public class TestHttpRequest
{
    public string Id { get; set; }

    public string Name { get; set; }
}
```

控制台输入日志如下，GET请求

```
TraceId:ebd108a89b06171c0501c2faa3752d3c,路由:/api/HttpSample/Get,请求方式:GET。{"id":0,"serviceName":"CommonService","aliasName":"AuditLog","traceId":"ebd108a89b06171c0501c2faa3752d3c","ipAddress":null,"userAgent":"Moz
0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0","logLevel":2,"route":"/api/HttpSample/Get","httpMethod":"GET","requestBody":"","responseBody":"{\"dat      t
a\":\"success\",\"isSuccess\":true,\"isFailure\":false,\"code\":\"200\",\"message\":\"success\",\"errors\":[]}","rawData":"{\"Accept\":\"text/plain\",\"Connection\":\"keep-alive\",\"Host\":\"localhost:5000\",\"User-Agent\":\"
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0\",\"Accept-Encoding\":\"gzip, deflate, br, zstd\",\"Accept-Language\":\"zh-CN,zh;q=0.9\",\"Cookie\"
:\"Hm_lvt_9bfcf5a38b268829d12324755e6544ba=1730271583,1731375604,1731555056; Hm_lvt_1f046e495f9c28ef302f30895bda829e=1736255999,1736347525,1736694053,1737034159\",\"Referer\":\"http://localhost:5000/index.html\",\"sec-ch-ua-p
latform\":\"\\\"Windows\\\"\",\"sec-ch-ua\":\"\\\"Microsoft Edge\\\";v=\\\"131\\\", \\\"Chromium\\\";v=\\\"131\\\", \\\"Not_A Brand\\\";v=\\\"24\\\"\",\"DNT\":\"1\",\"sec-ch-ua-mobile\":\"?0\",\"Sec-Fetch-Site\":\"same-origin
\",\"Sec-Fetch-Mode\":\"cors\",\"Sec-Fetch-Dest\":\"empty\"}","statusCode":200,"userId":null,"userName":null,"startTime":"2025-01-18 22:14:24","endTime":"2025-01-18 22:14:24","elapsedMilliseconds":45,"errorMessage":null,"createdOn":"2025-01-18 22:14:24"}

```

Post请求

```
TraceId:db253ec876e4c2bacb1c6fbfd9c08f64,路由:/api/HttpSample/Post,请求方式:POST。{"id":0,"serviceName":"CommonService","aliasName":"AuditLog","traceId":"db253ec876e4c2bacb1c6fbfd9c08f64","ipAddress":null,"userAgent":"M
ozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0","logLevel":2,"route":"/api/HttpSample/Post","httpMethod":"POST","requestBody":"{  \"id\": \"string\
",  \"name\": \"string\"}","responseBody":"{\"data\":\"successstring\",\"isSuccess\":true,\"isFailure\":false,\"code\":\"200\",\"message\":\"success\",\"errors\":[]}","rawData":"{\"Accept\":\"text/plain\",\"Connection\":\"kee
p-alive\",\"Host\":\"localhost:5000\",\"User-Agent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0\",\"Accept-Encoding\":\"gzip, deflate, br, z
std\",\"Accept-Language\":\"zh-CN,zh;q=0.9\",\"Content-Type\":\"application/json\",\"Cookie\":\"Hm_lvt_9bfcf5a38b268829d12324755e6544ba=1730271583,1731375604,1731555056; Hm_lvt_1f046e495f9c28ef302f30895bda829e=1736255999,1736
347525,1736694053,1737034159\",\"Origin\":\"http://localhost:5000\",\"Referer\":\"http://localhost:5000/index.html\",\"Content-Length\":\"40\",\"sec-ch-ua-platform\":\"\\\"Windows\\\"\",\"sec-ch-ua\":\"\\\"Microsoft Edge\\\";
v=\\\"131\\\", \\\"Chromium\\\";v=\\\"131\\\", \\\"Not_A Brand\\\";v=\\\"24\\\"\",\"DNT\":\"1\",\"sec-ch-ua-mobile\":\"?0\",\"Sec-Fetch-Site\":\"same-origin\",\"Sec-Fetch-Mode\":\"cors\",\"Sec-Fetch-Dest\":\"empty\"}","statusCode":200,"userId":null,"userName":null,"startTime":"2025-01-18 22:18:39","endTime":"2025-01-18 22:18:39","elapsedMilliseconds":4,"errorMessage":null,"createdOn":"2025-01-18 22:18:39"}
```

Put请求

```
TraceId:2b23b7510b9bdf600d95a0d57679dcf4,路由:/api/HttpSample/Put,请求方式:PUT。{"id":0,"serviceName":"CommonService","aliasName":"AuditLog","traceId":"2b23b7510b9bdf600d95a0d57679dcf4","ipAddress":null,"userAgent":"Moz
illa/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0","logLevel":2,"route":"/api/HttpSample/Put","httpMethod":"PUT","requestBody":"{  \"id\": \"string\",
\"name\": \"string\"}","responseBody":"{\"data\":\"successstring\",\"isSuccess\":true,\"isFailure\":false,\"code\":\"200\",\"message\":\"success\",\"errors\":[]}","rawData":"{\"Accept\":\"text/plain\",\"Connection\":\"keep-al
ive\",\"Host\":\"localhost:5000\",\"User-Agent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0\",\"Accept-Encoding\":\"gzip, deflate, br, zstd\
",\"Accept-Language\":\"zh-CN,zh;q=0.9\",\"Content-Type\":\"application/json\",\"Cookie\":\"Hm_lvt_9bfcf5a38b268829d12324755e6544ba=1730271583,1731375604,1731555056; Hm_lvt_1f046e495f9c28ef302f30895bda829e=1736255999,17363475
25,1736694053,1737034159\",\"Origin\":\"http://localhost:5000\",\"Referer\":\"http://localhost:5000/index.html\",\"Content-Length\":\"40\",\"sec-ch-ua-platform\":\"\\\"Windows\\\"\",\"sec-ch-ua\":\"\\\"Microsoft Edge\\\";v=\\
\"131\\\", \\\"Chromium\\\";v=\\\"131\\\", \\\"Not_A Brand\\\";v=\\\"24\\\"\",\"DNT\":\"1\",\"sec-ch-ua-mobile\":\"?0\",\"Sec-Fetch-Site\":\"same-origin\",\"Sec-Fetch-Mode\":\"cors\",\"Sec-Fetch-Dest\":\"empty\"}","statusCode":200,"userId":null,"userName":null,"startTime":"2025-01-18 22:22:43","endTime":"2025-01-18 22:22:43","elapsedMilliseconds":23,"errorMessage":null,"createdOn":"2025-01-18 22:22:43"}
```

Delete请求

```
TraceId:370af85a7fbbefe0b7d9c2ad86a1f5aa,路由:/api/HttpSample/Delete,请求方式:DELETE。{"id":0,"serviceName":"CommonService","aliasName":"AuditLog","traceId":"370af85a7fbbefe0b7d9c2ad86a1f5aa","ipAddress":null,"userAgent
":"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0","logLevel":2,"route":"/api/HttpSample/Delete","httpMethod":"DELETE","requestBody":"?id=1111","r
esponseBody":"{\"data\":\"success1111\",\"isSuccess\":true,\"isFailure\":false,\"code\":\"200\",\"message\":\"success\",\"errors\":[]}","rawData":"{\"Accept\":\"text/plain\",\"Connection\":\"keep-alive\",\"Host\":\"localhost:
5000\",\"User-Agent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0\",\"Accept-Encoding\":\"gzip, deflate, br, zstd\",\"Accept-Language\":\"zh-
CN,zh;q=0.9\",\"Cookie\":\"Hm_lvt_9bfcf5a38b268829d12324755e6544ba=1730271583,1731375604,1731555056; Hm_lvt_1f046e495f9c28ef302f30895bda829e=1736255999,1736347525,1736694053,1737034159\",\"Origin\":\"http://localhost:5000\",\
"Referer\":\"http://localhost:5000/index.html\",\"sec-ch-ua-platform\":\"\\\"Windows\\\"\",\"sec-ch-ua\":\"\\\"Microsoft Edge\\\";v=\\\"131\\\", \\\"Chromium\\\";v=\\\"131\\\", \\\"Not_A Brand\\\";v=\\\"24\\\"\",\"DNT\":\"1\"
,\"sec-ch-ua-mobile\":\"?0\",\"Sec-Fetch-Site\":\"same-origin\",\"Sec-Fetch-Mode\":\"cors\",\"Sec-Fetch-Dest\":\"empty\"}","statusCode":200,"userId":null,"userName":null,"startTime":"2025-01-18 22:23:32","endTime":"2025-01-18 22:23:32","elapsedMilliseconds":11,"errorMessage":null,"createdOn":"2025-01-18 22:23:32"}
```

##### 自定义服务名

需要再配置文件中，增加配置项

```csharp
{
  "ServiceName" : "APIStudly"
}
```

输入日志

```csharp
TraceId:5a54f89b042f266b851e5cccc872ff2c,路由:/api/HttpSample/Get,请求方式:GET。{"id":0,"serviceName":"APIStudly","aliasName":"
```

##### 重写日志存储

如果想重写审计日志的存储，那么可以继承ILoggerService接口进行重写并注入

#### 全局异常处理

使用全局异常处理中间件来处理异常

```c#
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用异常中间件
app.UseGlobalException();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

行为说明：

- 客户端主动取消的请求（`OperationCanceledException`）按 Information 级别记录，不再产生 Error 日志
- 响应已开始输出后再发生异常时无法改写状态码与响应体，此时重新抛出异常由服务器重置连接，避免客户端收到 200 + 半截响应

编写一个接口抛出异常

```C#
[HttpGet]
public IEnumerable<WeatherForecast> Get()
{
    throw new ParameterException("参数有误");


    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    })
        .ToArray();
}
```

返回结果

```json
{
    "isSuccess": false,
    "code": "",
    "message": "参数有误",
    "errors": []
}
```

支持抛出异常类型

```c#
//500 自定义错误
BaseException

//500 系统异常
InternalServerException

//404 对象未找到
NotFoundException

//400 参数异常
ParameterException

// 400 业务逻辑错误
LogicBusinessException

//401 未认证（未登录或凭证无效）
UnauthorizedException

//403 禁止访问（已认证但无权限）
ForbiddenException
```

## 📝 更新记录

完整版本历史见 [CHANGELOG.md](https://github.com/azrng/nuget-packages/blob/master/src/Shared/Azrng.AspNetCore.Core/CHANGELOG.md)。

### 当前版本更新

* 1.6.0
  * 全局异常中间件加固：客户端主动取消（`OperationCanceledException` 且 `RequestAborted`）按 Information 记录不再产生 Error 日志；响应已开始后无法写统一错误响应时重新抛出异常由服务器重置连接（原吞掉二次异常后客户端收到 200 + 半截响应）（行为变更）
  * `CustomResultPackFilter` 包装时保留 action 声明的状态码（`CreatedAtAction` 201、`Accepted` 202、`StatusCode(...)` 等不再被强制改为 200）（行为变更）
  * `MinValueAttribute` 对 null 值校验失败（行为变更：原 null 直接通过，与 remarks 声称的“已经包含了Required”不符）
  * 审计日志日志等级按状态码段映射：2xx/3xx 记 Information、4xx 记 Warning、5xx 记 Error（原仅 200 记 Information，201/204 等正常响应全部误报为 Error）；`IgnoreRoutePrefix` 改为前缀匹配（原为子串包含，配置 /health 会连带忽略 /api/monitor/healthcheck）（行为变更）
  * `ShowAllServicesMiddleware` 非 Development 环境默认返回 404，可通过 `ShowServiceConfig.AllowNonDevelopment` 放开；`UseShowAllServicesMiddleware` 改用非 Obsolete 的 `UseShowAllServices`
  * 新增 `LongToNullableStringConverter`：`long?` 与字符串互转，JSON null/空串反序列化为 null
  * `AuditLogInfo` 大文本字段 `[MaxLength(-2)]` 修正为约定俗成的 `[-1]`（nvarchar(max)）
  * 删除全文件注释的死文件 `SessionHelper.cs`/`CustomContractResolver.cs` 并同步 ARCHITECTURE.md；csproj Description 修正为实际定位并补 `PackageProjectUrl`
  * 审计日志安全与内存加固：请求头全量记录改为敏感头脱敏（`SensitiveHeaders` 名单 + `SensitiveHeaderMask` 掩码，默认覆盖 `Authorization`/`Cookie`/`Set-Cookie`/`Proxy-Authorization`）；响应体由整包内存缓冲改为限量捕获（`MaxResponseBodySize`，客户端不受影响）；新增请求体上限 `MaxRequestBodySize`（默认 1MB），`multipart/` 与 `application/octet-stream` 请求体不再进日志（行为变更：请求体日志默认新增截断与跳过）
  * `RequestIdMiddleware` 加固：入站 `X-RequestId` 增加长度上限与字符白名单校验（防日志注入与伪造关联）；无合法入站头时沿用宿主 `TraceIdentifier`，与诊断 traceId 体系对齐（行为变更：不再重新生成 N 格式 GUID）；多值头取第一个值；移除 .NET 3.0 起已失效的 `IHttpRequestIdentifierFeature` 死代码
  * 新增反向代理真实客户端 IP 支持：`UseForwardedHeaders()` 转发头中间件封装（可信代理列表 + 防伪造）与 `HttpContext.GetClientIp()` 扩展
  * **破坏性变更**：移除 `AddCorsPolicy()`，该方法与框架原生 `services.AddCors(o => o.AddPolicy(name, builder => ...))` 完全等价；高级场景请直接使用原生写法，`AddAnyCors()` 与 `AddCorsByOrigins()` 保持不变
  * **破坏性变更**：移除 `PreConfigure`/`GetPreConfigureActions`/`AddObjectAccessor`/`GetObjectOrNull` 预配置体系；该机制存入的委托从不被 Options 管道消费，属于无效 API，配置默认值请直接使用框架 `services.Configure<T>()`

