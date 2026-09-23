# 变更记录

## 2.0.0-beta2

- 未认证请求在处理器内直接拒绝，不再调用 `IPermissionEvaluator`，避免匿名流量触发评估器中的数据库或远程 ACL 调用。
- `AuthorizationDecision` 只保留允许与拒绝两态：移除 `NotConfigured`、`DependencyError` 工厂与 `DiagnosticCode` 诊断码，依赖故障统一抛异常由处理器 fail-closed 处理。
- `PermissionContext` 收敛为 `HttpContext`、`Path`、`Method`、`User`、`RequiredPermissions`：移除 `Endpoint` 与 `RouteValues`，资源级路由参数等其余信息经 `HttpContext` 获取。
- 增加匿名短路与评估器调用回归测试。

## 2.0.0-beta1

- 将权限判断统一为 `IPermissionEvaluator` + `PermissionContext` + `AuthorizationDecision`。
- 将 Endpoint 权限元数据作为 MVC 和 Minimal API 的统一声明入口。
- 保留 ASP.NET Core 标准认证、授权策略和策略提供器行为，不覆盖宿主策略提供器。
- 移除 1.x 路径字符串授权兼容层、匿名路径配置和自定义策略提供器残留实现。
- 处理器使用构造函数注入，并传递请求取消令牌。
- 增加 TestServer 401 / 403 / 200 管道回归和 Endpoint Metadata 回归测试。

## 1.3.0

- 增加权限上下文、结构化授权结果和 Endpoint 权限声明能力。
- 默认策略显式要求认证，处理器不再重复调用认证方案。
- 默认策略改由 `AuthorizationOptions` 配置。

## 1.2.0

- 修复路径段前缀匹配的边界问题。
- 增加授权需求集合的防御性复制。
- 补充认证和路径匹配回归测试。

## 1.1.0

- 修复授权处理器需求实现。
- 增加结构化日志和 XML 文档注释。

## 1.0.0

- 支持 .NET 6.0、7.0、8.0、9.0 和 10.0。

## 1.0.0-beta1

- 更新依赖包。
