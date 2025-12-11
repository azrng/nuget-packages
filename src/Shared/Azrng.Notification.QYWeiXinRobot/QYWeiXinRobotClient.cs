using Azrng.Notification.QYWeiXinRobot.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Azrng.Notification.QYWeiXinRobot
{
    public class QyWeiXinRobotClient : IQyWeiXinRobotClient
    {
        private readonly HttpClient _client;
        private readonly IOptions<QyWeiXinRobotConfig> _options;

        public QyWeiXinRobotClient(IHttpClientFactory httpClientFactory, IOptions<QyWeiXinRobotConfig> options)
        {
            _client = httpClientFactory.CreateClient();
            _options = options;
        }

        public async Task<ApiResult> SendMsgAsync<T>(T messageDto) where T : IBaseSendMessageDto
        {
            var host = new Uri(_options.Value.BaseUrl);
            var url = new Uri(host, $"cgi-bin/webhook/send?key={_options.Value.Key}");
            var jsonData = JsonConvert.SerializeObject(messageDto);
            using var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content).ConfigureAwait(false);
            return await ConvertResponseResult<ApiResult>(response).ConfigureAwait(false);
        }

        public async Task<ApiResult> SendImageMsgAsync(byte[] imageBytes)
        {
            if (imageBytes.Length == 0)
                return new ApiResult { Code = 1, Message = "图片内容不能为空" };
            var base64Str = Helper.BytesToBase64(imageBytes);
            var md5Str = Helper.GetFileMd5Hash(imageBytes);
            var msg = new ImageMessageDto()
            {
                Text = new ImageContentDto
                {
                    Base64 = base64Str,
                    Md5 = md5Str
                }
            };
            return await SendMsgAsync(msg);
        }

        public async Task<ApiResult<FileUploadResult>> UpdateMediaAsync(UploadMediaRequest request)
        {
            var host = new Uri(_options.Value.BaseUrl);
            var url = new Uri(host, $"/cgi-bin/webhook/upload_media?key={_options.Value.Key}&type=file");

            var boundary = DateTime.Now.Ticks.ToString("X");
            _client.DefaultRequestHeaders.Remove("Expect");
            _client.DefaultRequestHeaders.Remove("Connection");
            using var content = new MultipartFormDataContent(boundary);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
            using var contentByte = new ByteArrayContent(request.Media);
            content.Add(contentByte);
            contentByte.Headers.Remove("Content-Disposition");
            contentByte.Headers.TryAddWithoutValidation("Content-Disposition",
                $"form-data; name=\"media\";filename=\"{request.FileName}\"");
            contentByte.Headers.Remove("Content-Type");
            contentByte.Headers.TryAddWithoutValidation(name: "Content-Type", request.ContentType);
            var response = await _client.PostAsync(url, content);
            var fileUploadResponse = await ConvertResponseResult<FileUploadResponse>(response).ConfigureAwait(false);
            if (fileUploadResponse.errcode != 0)
                return new ApiResult<FileUploadResult>
                { Code = fileUploadResponse.errcode, Message = fileUploadResponse.errmsg };

            return new ApiResult<FileUploadResult>
            {
                Code = fileUploadResponse.errcode,
                Message = fileUploadResponse.errmsg,
                Data = new FileUploadResult(fileUploadResponse.type, fileUploadResponse.media_id)
            };
        }

        /// <summary>
        /// 转换返回的结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        private static async Task<T> ConvertResponseResult<T>(HttpResponseMessage httpResponse)
        {
            var resStr = await httpResponse.Content.ReadAsStringAsync();
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(resStr, typeof(string));

            return JsonConvert.DeserializeObject<T>(resStr);
        }
    }
}