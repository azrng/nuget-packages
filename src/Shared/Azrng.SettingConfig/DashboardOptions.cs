using Azrng.Core.Extension;
using Azrng.SettingConfig.Service;

namespace Azrng.SettingConfig;

/// <summary>
/// 配置Dashboard选项设置
/// </summary>
public class DashboardOptions
{
    private static readonly IDashboardAuthorizationFilter[] DefaultAuthorization =
        new[] { new LocalRequestsOnlyAuthorizationFilter() };

    public DashboardOptions()
    {
        RoutePrefix = "systemSetting";
        ApiRoutePrefix = "/api/setting";
        Authorization = DefaultAuthorization;
        ConfigCacheTime = 60;
    }

    /// <summary>
    /// 配置中心页面路由
    /// </summary>
    public string RoutePrefix { get; set; }

    /// <summary>
    /// 配置中心业务接口前缀
    /// </summary>
    public string ApiRoutePrefix { get; set; }

    /// <summary>
    /// 环境变量库连接字符串
    /// </summary>
    public string DbConnectionString { get; set; }

    /// <summary>
    /// 数据库模式
    /// </summary>
    public string DbSchema { get; set; } = "setting";

    // /// <summary>应用的登录页面前端路由</summary>
    // public string LogInUrl { get; set; } = "login";

    // /// <summary>localStorge获取登录授权token的路径</summary>
    // public IList<string> TokenKeys { get; set; } = new List<string>()
    // {
    //     "currentUser",
    //     "access_token"
    // };

    public IEnumerable<IDashboardAuthorizationFilter> Authorization { get; set; }

    /// <summary>页面Title</summary>
    public string PageTitle { get; set; } = "系统配置页面";

    /// <summary>
    /// 页面说明
    /// </summary>
    public string PageDescription { get; set; } = "系统设置界面";

    /// <summary>
    /// 配置key缓存时间(分钟)
    /// </summary>
    public int ConfigCacheTime { get; set; }

    /// <summary>
    /// 参数校验
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public void ParamVerify()
    {
        if (DbConnectionString.IsNullOrWhiteSpace())
            throw new ArgumentNullException("数据库地址参数不能为空");
        if (DbSchema.IsNullOrWhiteSpace())
            throw new ArgumentNullException("数据库schema地址不能为空");
        if (PageTitle.IsNullOrWhiteSpace())
            throw new ArgumentNullException("页面Title不能为空");
        if (PageDescription.IsNullOrWhiteSpace())
            throw new ArgumentNullException("页面说明不能为空");
        if (ConfigCacheTime <= 0)
            throw new ArgumentNullException("配置缓存时间不能小于0");
    }
}