using Azrng.YuntongxunSms.Model;
using Common.Results;
using Common.Security;
using Common.Service;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Azrng.YuntongxunSms
{
    /// <summary>
    /// 云通讯短信服务
    /// </summary>
    public class SmsService : BaseService, ISmsService
    {
        /// <summary>
        /// 服务器api版本
        /// </summary>
        private const string _softVer = "2013-12-26";

        private readonly SmsConfig _smsConfig;

        public SmsService(IOptions<SmsConfig> options)
        {
            _smsConfig = options.Value;
        }

        ///<inheritdoc cref="ISmsService.SendTemplateSMS"/>
        public IApiResult<string> SendTemplateSMS(string phone)
        {
            string templateId = "442972";
            var data = new string[] { "3841", "1" };
            try
            {
                var config = _smsConfig;
                var verifyMsg = Init(config);
                if (!string.IsNullOrWhiteSpace(verifyMsg))
                    return Error<string>(verifyMsg);

                string date = DateTime.Now.ToString("yyyyMMddhhmmss");
                // 构建URL内容
                string sigstr = (config.SmsAccountSid + config.SmsAccountToken + date).Md5Hash();
                string uriStr = string.Format("https://{0}:{1}/{2}/Accounts/{3}/SMS/TemplateSMS?sig={4}", config.SmsAddress, config.SmsPort, _softVer, config.SmsAccountSid, sigstr);
                Uri address = new Uri(uriStr);
                //记录日志
                // WriteLog("SendTemplateSMS url = " + uriStr);
                // 创建网络请求
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                setCertificateValidationCallBack();
                // 构建Head
                request.Method = "POST";
                Encoding myEncoding = Encoding.GetEncoding("utf-8");
                byte[] myByte = myEncoding.GetBytes(config.SmsAccountSid + ":" + date);
                string authStr = Convert.ToBase64String(myByte);
                request.Headers.Add("Authorization", authStr);

                // 构建Body
                StringBuilder bodyData = new StringBuilder();
                request.Accept = "application/json";
                request.ContentType = "application/json;charset=utf-8";

                //{"to":"18838940825","appId":"8aaf07086b8862cb016b8eb9923e05f5","templateId":"442972","datas":["3841","1"]}
                bodyData.Append("{");
                bodyData.Append("\"to\":\"").Append(phone).Append("\"");
                bodyData.Append(",\"appId\":\"").Append(config.SmsAppId).Append("\"");
                bodyData.Append(",\"templateId\":\"").Append(templateId).Append("\"");
                if (data != null && data.Length > 0)
                {
                    bodyData.Append(",\"datas\":[");
                    int index = 0;
                    foreach (string item in data)
                    {
                        if (index == 0)
                        {
                            bodyData.Append("\"" + item + "\"");
                        }
                        else
                        {
                            bodyData.Append(",\"" + item + "\"");
                        }
                        index++;
                    }
                    bodyData.Append("]");
                }
                bodyData.Append("}");
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(bodyData.ToString());
                // 开始请求
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }
                // 获取请求
                using HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseStr = reader.ReadToEnd();
                //_logger.LogInformation("短信发送成功，手机号码是" + phone);
                return Success(responseStr);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "发送短信失败" + ex.Message);
                return Error<string>("系统出错，请联系超级管理员");
            }
        }

        #region 私有方法

        private string getDictionaryData(Dictionary<string, object> data)
        {
            string ret = null;
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    ret += item.Key.ToString() + "={";
                    ret += getDictionaryData((Dictionary<string, object>)item.Value);
                    ret += "};";
                }
                else
                {
                    ret += item.Key.ToString() + "=" + (item.Value == null ? "null" : item.Value.ToString()) + ";";
                }
            }
            return ret;
        }

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="restAddress">服务器地址</param>
        /// <param name="restPort">服务器端口</param>
        /// <returns></returns>
        private string Init(SmsConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.SmsAddress))
                return "服务器地址不能为空";
            if (config.SmsPort <= 0)
                return "服务器端口无效";
            if (config.SMSValidCodeSecond <= 0)
                return "短信有效时间不能为空";
            if (string.IsNullOrWhiteSpace(config.SmsAccountSid))
                return "主账户标识不能为空";
            if (string.IsNullOrWhiteSpace(config.SmsAccountToken))
                return "主账户令牌不能为空";
            if (string.IsNullOrWhiteSpace(config.SmsAppId))
                return "应用标识不能为空";

            return string.Empty;
        }

        /// <summary>
        /// 设置服务器证书验证回调
        /// </summary>
        private void setCertificateValidationCallBack()
        {
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationResult;
        }

        /// <summary>
        ///  证书验证回调函数
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cer"></param>
        /// <param name="chain"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool CertificateValidationResult(object obj, System.Security.Cryptography.X509Certificates.X509Certificate cer, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        #endregion
    }
}