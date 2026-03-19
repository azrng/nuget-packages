using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.AspNetCore.Core.Helper
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    public static class AppSettings
    {
        private static IConfiguration? _configuration;

        private static IConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration has not been initialized.");

        public static void InitConfiguration(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

        public static T? GetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key)
        {
            return (T?)Configuration.GetValue(typeof(T), key);
        }

        public static IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

        public static T? GetSection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key)
        {
            return (T?)Configuration.GetSection(key).Get(typeof(T));
        }
    }
}