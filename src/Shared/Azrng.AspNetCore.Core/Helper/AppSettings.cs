using Microsoft.Extensions.Configuration;

namespace Azrng.AspNetCore.Core.Helper
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    public static class AppSettings
    {
        private static IConfiguration Configuration { get; set; }

        public static void InitConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 查询配置信息
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static string? GetValue(params string[] sections)
        {
            try
            {
                if (sections.Length > 0)
                    return Configuration[string.Join(":", sections)];
            }
            catch (Exception)
            {
                // ignored
            }

            return string.Empty;
        }

        public static string? GetValue(string key)
        {
            return Configuration.GetValue<string>(key);
        }

        public static T? GetValue<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }

        public static IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

        public static T? GetSection<T>(string key)
        {
            return Configuration.GetSection(key).Get<T>();
        }
    }
}