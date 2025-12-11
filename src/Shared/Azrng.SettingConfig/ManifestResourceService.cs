using System.Reflection;

namespace Azrng.SettingConfig
{
    /// <summary>
    /// 资源服务
    /// </summary>
    public class ManifestResourceService
    {
        /// <summary>
        /// 获取或者设置用于检索setting-ui页面的stream函数
        /// </summary>
        //internal Stream IndexStream = typeof(SettingUIOptions).GetTypeInfo().Assembly.GetManifestResourceStream("Azrng.SettingConfig.wwwroot.index.html");
        public async Task<byte[]> GetManifestResource()
        {
            var stream = typeof(DashboardOptions).GetTypeInfo().Assembly
                .GetManifestResourceStream("Azrng.SettingConfig.wwwroot.index.html");
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            var array = new byte[stream.Length];
            _ = await stream.ReadAsync(array).ConfigureAwait(false);
            stream.Seek(0L, SeekOrigin.Begin);

            return array;
        }
    }
}