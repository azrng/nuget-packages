using Microsoft.AspNetCore.Mvc.Routing;

namespace Azrng.SettingConfig.Attributes
{
    /// <summary>
    /// 匹配路由设置
    /// 使用 ServiceCollectionExtensions.CurrentApiRoutePrefix 动态配置路由前缀
    /// </summary>
    public class SettingMatchRouteAttribute : Attribute, IRouteTemplateProvider
    {
        /// <summary>
        /// 路由模板 - 直接使用 API 前缀，不包含控制器名称
        /// 控制器方法的路由（如 [HttpGet("page")]）会直接附加此前缀
        /// 例如：/api/configDashboard/page
        /// </summary>
        public string Template => $"{ServiceCollectionExtensions.CurrentApiRoutePrefix.TrimEnd('/')}";

        /// <summary>
        /// 路由顺序
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// 路由名称
        /// </summary>
        public string? Name { get; set; }
    }
}