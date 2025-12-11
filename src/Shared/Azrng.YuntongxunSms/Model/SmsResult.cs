namespace Azrng.YuntongxunSms.Model
{
    /// <summary>
    /// 短信配置文件
    /// </summary>
    public class SmsConfig
    {
        /// <summary>
        /// 短信服务器地址
        /// </summary>
        public string SmsAddress { get; set; }
        /// <summary>
        /// 短信服务器端口
        /// </summary>
        public int SmsPort { get; set; }
        /// <summary>
        /// 短信有效时间 秒
        /// </summary>
        public int SMSValidCodeSecond { get; set; }

        /// <summary>
        /// 主账户
        /// </summary>
        public string SmsAccountSid { get; set; }
        /// <summary>
        /// 主账户令牌
        /// </summary>
        public string SmsAccountToken { get; set; }
        /// <summary>
        /// 应用id
        /// </summary>
        public string SmsAppId { get; set; }



    }
}
