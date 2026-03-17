namespace Azrng.SettingConfig
{
    /// <summary>
    /// 资源服务
    /// </summary>
    public class ManifestResourceService
    {
        /// <summary>
        /// 获取嵌入的 HTML 资源内容
        /// </summary>
        public async Task<byte[]> GetManifestResource()
        {
            var assembly = typeof(DashboardOptions).Assembly;
            await using var stream = assembly.GetManifestResourceStream("Azrng.SettingConfig.wwwroot.index.html");

            if (stream is null)
                throw new FileNotFoundException("无法找到嵌入的 index.html 资源", "Azrng.SettingConfig.wwwroot.index.html");

            // 使用内存流读取所有数据
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}