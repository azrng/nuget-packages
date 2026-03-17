using Microsoft.AspNetCore.Mvc.Routing;

namespace Azrng.SettingConfig.Attributes
{
    /// <summary>
    /// 匹配路由设置
    /// </summary>
    public class SettingMatchRouteAttribute : Attribute, IRouteTemplateProvider
    {
        private readonly string _apiRoutePrefix;

        public SettingMatchRouteAttribute()
        {
            // 默认值，如果未配置则使用此值
            _apiRoutePrefix = "/api/platform";
        }

        /// <summary>
        /// 路由模板
        /// </summary>
        public string Template => $"{_apiRoutePrefix}/[controller]";

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