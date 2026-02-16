using Azrng.SettingConfig.Service;

namespace Azrng.SettingConfig;

/// <summary>
/// 配置 Dashboard 选项设置
/// </summary>
public class DashboardOptions
{
    /// <summary>
    /// 获取或设置配置中心页面路由
    /// </summary>
    public string RoutePrefix { get; set; } = "systemSetting";

    /// <summary>
    /// 获取或设置配置中心业务接口前缀
    /// </summary>
    public string ApiRoutePrefix { get; set; } = "/api/setting";

    /// <summary>
    /// 获取或设置环境变量数据库连接字符串
    /// </summary>
    public string? DbConnectionString { get; set; }

    /// <summary>
    /// 获取或设置数据库模式（Schema）
    /// </summary>
    public string DbSchema { get; set; } = "setting";

    /// <summary>
    /// 获取或设置授权过滤器集合
    /// </summary>
    public IEnumerable<IDashboardAuthorizationFilter>? Authorization { get; set; }

    /// <summary>
    /// 获取或设置页面标题
    /// </summary>
    public string PageTitle { get; set; } = "系统配置页面";

    /// <summary>
    /// 获取或设置页面说明
    /// </summary>
    public string PageDescription { get; set; } = "系统设置界面";

    /// <summary>
    /// 获取或设置配置键缓存时间（分钟）
    /// </summary>
    public int ConfigCacheTime { get; set; } = 60;

    /// <summary>
    /// 初始化 <see cref="DashboardOptions"/> 的新实例
    /// </summary>
    public DashboardOptions()
    {
        // 设置默认授权过滤器（仅允许本地请求）
        Authorization = new IDashboardAuthorizationFilter[]
        {
            new LocalRequestsOnlyAuthorizationFilter()
        };
    }

    /// <summary>
    /// 验证配置参数
    /// </summary>
    /// <exception cref="ArgumentNullException">当必需参数为空时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">当参数值不在有效范围内时抛出</exception>
    public void ParamVerify()
    {
        if (string.IsNullOrWhiteSpace(DbConnectionString))
        {
            throw new ArgumentNullException(nameof(DbConnectionString), "数据库连接字符串不能为空");
        }

        if (string.IsNullOrWhiteSpace(DbSchema))
        {
            throw new ArgumentNullException(nameof(DbSchema), "数据库 Schema 不能为空");
        }

        if (string.IsNullOrWhiteSpace(PageTitle))
        {
            throw new ArgumentNullException(nameof(PageTitle), "页面标题不能为空");
        }

        if (string.IsNullOrWhiteSpace(PageDescription))
        {
            throw new ArgumentNullException(nameof(PageDescription), "页面说明不能为空");
        }

        if (ConfigCacheTime <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ConfigCacheTime), "配置缓存时间必须大于 0");
        }
    }
}