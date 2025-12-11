using Common.Results;

namespace Azrng.YuntongxunSms
{
    public interface ISmsService
    {
        /// <summary>
        /// 发送模版短信
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        IApiResult<string> SendTemplateSMS(string phone);
    }
}
