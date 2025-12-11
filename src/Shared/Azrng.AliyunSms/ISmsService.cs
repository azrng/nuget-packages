using Azrng.AliyunSms.Model;
using Azrng.AliyunSms.Model.ViewModel;

namespace Azrng.AliyunSms
{
    public interface ISmsService
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        ApiResult SendSmsAsync(SendSmsVm vm);
    }
}
