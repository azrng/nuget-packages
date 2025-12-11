using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;
using Azrng.AliyunSms.Model;
using Azrng.AliyunSms.Model.ViewModel;
using System;

namespace Azrng.AliyunSms
{
    public class SmsService : ISmsService
    {
        private readonly DefaultAcsClient _client;

        public SmsService(
            DefaultAcsClient client)
        {
            _client = client;
        }

        public ApiResult SendSmsAsync(SendSmsVm vm)
        {
            var request = new CommonRequest
                          {
                              Method = MethodType.POST,
                              Domain = "dysmsapi.aliyuncs.com",
                              Version = "2017-05-25",
                              Action = "QuerySendDetails"
                          };
            request.AddQueryParameters("PhoneNumbers", vm.PhoneNumbers);
            request.AddQueryParameters("SignName", vm.SignName);
            request.AddQueryParameters("TemplateCode", vm.TemplateCode);

            try
            {
                var response = _client.GetCommonResponse(request);
                Console.WriteLine(System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
            }
            catch (ServerException e)
            {
                Console.WriteLine(e);
            }
            catch (ClientException e)
            {
                Console.WriteLine(e);
            }

            return new ApiResult();
        }
    }
}