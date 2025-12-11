namespace Azrng.AspNetCore.Core
{
    /// <summary>
    /// mvc配置
    /// </summary>
    public class CommonMvcConfig
    {
        /// <summary>
        /// 是否启用自定义返回类包装
        /// </summary>
        public bool EnabledCustomerResultPack { get; set; } = true;

        /// <summary>
        /// 启用模型校验
        /// </summary>
        public bool EnabledModelVerify { get; set; } = true;

        /// <summary>
        /// 是否在返回结果使用http状态码
        /// </summary>
        public bool UseHttpStateCode { get; set; } = true;
    }
}