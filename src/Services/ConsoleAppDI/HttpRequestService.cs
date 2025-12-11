using Azrng.ConsoleApp.DependencyInjection;
using Common.HttpClients;

namespace ConsoleAppDI
{
    public class HttpRequestService : IServiceStart
    {
        private readonly IHttpHelper _httpHelper;

        public HttpRequestService(IHttpHelper httpHelper)
        {
            _httpHelper = httpHelper;
        }

        public string Title => "测试请求";

        public async Task RunAsync()
        {
            var response = await _httpHelper.GetAsync("https://jsonplaceholder.typicode.com/posts");
            Console.WriteLine(response);
        }
    }
}