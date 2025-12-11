namespace Common.YuQueSdk.Dto
{
    /// <summary>
    /// 语雀配置
    /// </summary>
    public class YuQueConfig
    {
        /// <summary>
        /// 语雀个人设置的Token
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// 语雀收集的用户标识
        /// </summary>
        public string UserAgent { get; set; } = "netcoresdk";

        // /// <summary>
        // /// 语雀url
        // /// </summary>
        // public string YuQueUrl { get; set; } = "https://www.yuque.com/api/v2/";
    }
}