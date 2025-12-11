using Azrng.AliyunSms;
using Azrng.AliyunSms.Model.ViewModel;

namespace Common.AliyunSms.Tests
{
    public class SmsTest
    {
        private readonly ISmsService _smsService;

        public SmsTest(ISmsService smsService)
        {
            _smsService = smsService;
        }

        [Fact]
        public void Send_ReturnOk()
        {
            _smsService.SendSmsAsync(new SendSmsVm { PhoneNumbers = "18838940825", TemplateCode = "1111", SignName = "2222" });
        }
    }
}