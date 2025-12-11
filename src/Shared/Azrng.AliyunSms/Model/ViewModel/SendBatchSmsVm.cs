using System;
using System.Collections.Generic;
using System.Text;

namespace Azrng.AliyunSms.Model.ViewModel
{
    /// <summary>
    /// 一次请求中分别向多个不同的手机号码发送不同签名的短信（最多100个）
    /// </summary>
    public class SendBatchSmsVm
    {
        public string PhoneNumberJson { get; set; }
        /// <summary>
        /// 接收短信的手机号码，JSON数组格式
        /// </summary>
        public List<string> SignNameJson { get; set; }
        /// <summary>
        /// 短信模板CODE 	["阿里云","阿里巴巴"]
        /// </summary>
        public List<string> TemplateCode { get; set; }
    }
}
