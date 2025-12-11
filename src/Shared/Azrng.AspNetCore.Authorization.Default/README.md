# Azrng.AspNetCore.Authorization.Default

一个封装了jwt使用以及swagger启用等的包

# 快速上手

## 认证

注入jwt服务

``` csharp
services.AddJwtBearerAuthentication(options =>
{
    options.JwtAudience = "aaaa";
    options.JwtIssuer = "bbbb";
});
```
开启认证

``` csharp
app.UseAuthentication();
app.UseAuthorization();
```

使用服务IBearerAuthService

``` csharp
private readonly IBearerAuthService _jwtAuthService;
private readonly ILogger<TokenController> _logger;

public TokenController(IBearerAuthService jwtAuthService, ILogger<TokenController> logger)
{
    _jwtAuthService = jwtAuthService;
    _logger = logger;
}

/// <summary>
/// 获取token
/// </summary>
/// <returns></returns>
[HttpGet]
public string GetToken()
{
    var claim = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier,"111111"),
        new Claim(ClaimTypes.Role,"123456")
    };
    return _jwtAuthService.CreateToken(claim);
}
```

## 授权

注入服务

``` csharp
services.AddCustomerAuthorization("default");
```

实现授权获取接口,例如

``` csharp
public class PermissionService : IPermissionService
{
    public Task<List<RolePermissionRelationDto>> GetRoleAuthAsync()
    {
        return Task.FromResult(new List<RolePermissionRelationDto>()
        {
            new RolePermissionRelationDto("123456","/api/token/get")
        });
    }
}
```

注入

``` csharp
services.AddScoped<IPermissionService, PermissionService>();
```

在需要开启授权的Action上标注

``` csharp
[Authorize("default")]
```

## 版本更新记录

* 1.0.0
  * 升级.Net10
* 1.0.0-beta1
    * 更新依赖包