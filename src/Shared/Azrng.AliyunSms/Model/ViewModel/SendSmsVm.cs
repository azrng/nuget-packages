namespace Azrng.AliyunSms.Model.ViewModel
{
    /// <summary>
    /// 发送短信请求类
    /// </summary>
    public class SendSmsVm
    {
        /// <summary>
        /// 手机号码  多个逗号隔开
        /// </summary>
        public string PhoneNumbers { get; set; }

        /// <summary>
        /// 短信签名名称
        /// </summary>
        public string SignName { get; set; }

        /// <summary>
        /// 短信模板ID
        /// </summary>
        public string TemplateCode { get; set; }
    }
}
