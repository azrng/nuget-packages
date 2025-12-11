using Microsoft.AspNetCore.Mvc.Routing;

namespace Azrng.SettingConfig.Attributes
{
    /// <summary>
    /// 匹配路由设置
    /// </summary>
    public class SettingMatchRouteAttribute : Attribute, IRouteTemplateProvider
    {
        public string Template => $"{ServiceCollectionExtensions.ApiRoutePrefix}/[controller]";

        public int? Order { get; set; }

        public string Name { get; set; }
    }
}