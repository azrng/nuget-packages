# Azrng.AspNetCore.Core 更新记录

## 版本更新记录

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
* 1.4.0
  * 新增 `UnauthorizedException`（401）异常映射，补齐未认证语义，与 `ForbiddenException`（403）区分；`Azrng.Core` 依赖改为本地项目引用（最新版）
  * **破坏性变更**：`ForbiddenException` 由 HTTP 401 改为 403，与 HTTP 语义对齐；前端/网关若按 401 判断鉴权需同步调整
  * 修复 `CommonMvcConfig`（`UseHttpStateCode` 等）通过 `Configure<CommonMvcConfig>` 配置后在异常中间件、模型校验过滤器中不生效的问题，改为 `IOptions` 注入
  * 移除异常中间件 `HasStarted` 判断，避免上游已启动响应时整条管道被跳过
  * 审计中间件 `EndTime`/`ElapsedMilliseconds` 统一在响应完成回调内计算，消除与 `finally` 的赋值竞态；回调内日志写入补异常保护
  * 异常中间件 `JsonSerializerOptions` 改为静态实例复用，避免每次请求重建
  * 移除 `IsAotCompatible=true` 与 `IL2026`/`IL3050` 警告抑制，库内含反射扫描、泛型序列化等 trim 不安全路径，暂不支持 Native AOT
* 1.3.1
  * 测试项目覆盖 `net6.0`、`net8.0`、`net9.0`、`net10.0`，增强多目标框架回归验证
  * CORS 注册方法增加空策略名、空来源参数校验
  * 审计日志默认日志服务增加 `System.Text.Json` 兜底，不再因缺少 `IJsonSerializer` 直接抛 `NotImplementedException`
  * 补充统一返回、异常中间件、审计日志和 CORS 关键行为测试
* 1.3.0
  * 简化 CORS 配置，移除复杂的配置类
  * 提供 `AddAnyCors()` - 开发环境快速配置
  * 提供 `AddCorsByOrigins()` - 生产环境安全配置
  * 提供 `AddCorsPolicy()` - 高级自定义配置
  * 新增 `UseCorsPolicy()` 中间件方法
  * 改进 CORS 配置的易用性和安全性
* 1.2.1
  * 更新异常中间件
* 1.2.0
  * 支持.Net10并优化
  * 新增 AuditLogOptions 配置类，提供灵活的审计日志配置选项
  * 支持配置需要记录的 HTTP 方法（默认记录 POST、PUT、DELETE，不记录 GET）
  * 支持配置最大响应体大小，超过自动截断
  * 支持配置是否只记录 /api/ 开头的路由
  * 支持配置是否格式化 JSON 输出
  * 新增 UseAutoAuditLog 配置选项重载方法
  * CustomResultPackFilter 优化，不包装 ProblemDetails 类型
  * AddMvcResultPackFilterFilter 方法标记为过时（请使用 AddMvcResultPackFilter）
* 1.1.0
  * 移出Microsoft.AspNetCore.Mvc.NewtonsoftJson依赖
  * 将对于序列化类的依赖使用System.Text.Json替换
* 1.0.0
  * 更新正常版本
* 1.0.0-beta5
  * 增加将long序列化为字符串转换器
* 1.0.0-beta4
    * 引用.Net10正式包
* 1.0.0-beta3
    * 支持.Net10
* 1.0.0-beta2
    * 注册服务RegisterBusinessServices增加一个要忽略的命名空间
* 1.0.0-beta1
    * 适配Azrng.Core 1.6.0版本变更
    * 修改审计方法名UseCustomAuditLog为UseAutoAuditLog
* 0.1.2
    * 优化ResultModel相关依赖
* 0.1.1
    * 修复CustomResultPackFilter使用报错问题
    * 增加审计日志中间件
* 0.1.0
    * 返回值包装支持传入忽略包装的前缀
* 0.1.0-beta8
    * 修复MinValue特性bug
* 0.1.0-beta7
    * 增加RequestBodyAsync扩展，以及增加请求体重复读取中间件
    * 优化模型校验方法
* 0.1.0-beta6
    * 升级支持.net8
* 0.1.0-beta5

    * 修复批量注入的问题

* 0.1.0-beta4

    * 增加HttpContext的扩展，例如获取远程IP、本地IP
    * 增加CollectionNotEmpty、MinValue特性
    * 迁移ServiceCollectionExtension
    * 增加services.AddMvcModelVerifyFilter 模型校验过滤器
    * 增加services.AddMvcResultPackFilterFilter返回值包装过滤器

* 0.1.0-beta3

    * 支持.net8

    * 支持显示所有服务以及服务注入的生命周期

* 0.1.0-beta2

    * 优化代码

* 0.1.0-beta1

    * 升级支持.net7

* 0.0.1-beta6
    *
    考虑到该包只能在API层使用，所以移除增加appsettings、cron帮助类、HttpContextManager、HttpContextExtensions、ServiceProviderHelper、SessionHelper、ICurrentUser、BaseService到AzrngCommon包

    * 异常处理中间件增加请求日志输出

    * 优化AppSettings写法

    * 增加了如果是FileContentResult，那么就不包装返回

    * 如果没有注入配置，那么就使用默认的CommonMvcConfig配置

* 0.0.1-beta5
    * 优化AddDefaultControllers方法，返回值修改为IMvcBuilder

    * 公共返回包装的方法优化对415错误的处理，遇到415错误的时候，直接返回不再包装

* 0.0.1-beta4
    * 优化支持的框架版本，支持3.1、5.0、6.0

    * 增加默认的控制器处理，必须添加AddDefaultControllers操作

* 0.0.1-beta3
    * 优化支持的框架版本，支持3.1、5.0、6.0
    * 将cors默认全部允许继承，直接使用services.AddAnyCors(); app.UseAnyCors();
    * 处理自定义模型校验返回状态码为200的错误情况
    * 处理自定义模型校验和自定义返回类一起使用导致重复包装的问题

* 0.0.1-beat2

    * 将关于swagger的东西去掉
    * 优化扩展方法命名空间，正规化

* 0.0.1-beta1

    * 从common里面移出来一些方法

