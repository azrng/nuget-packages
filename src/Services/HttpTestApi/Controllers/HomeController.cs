using Common.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace HttpTestApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpHelper _httpHelper;          // 非命名客户端（default）
        private readonly IHttpHelper _demoHelper;          // 命名客户端（demo）

        public HomeController(ILogger<HomeController> logger,
                              IHttpHelper httpHelper,
                              IHttpHelperFactory httpHelperFactory)
        {
            _logger = logger;
            _httpHelper = httpHelper;
            // 按名称取出对应配置（BaseAddress / 超时 / 重试等）的客户端
            _demoHelper = httpHelperFactory.CreateClient("demo");
        }

        /// <summary>
        /// 使用非命名客户端请求（完整 URL，配置走 default）
        /// </summary>
        [HttpGet]
        public async Task<string> DefaultClient()
        {
            var result = await _httpHelper.GetAsync<string>("https://jsonplaceholder.typicode.com/todos/1");
            _logger.LogInformation("非命名客户端请求结果：{Result}", result.IsSuccess ? "成功" : "失败");
            return result.IsSuccess ? result.RawBody! : $"失败：{result.ErrorMessage}";
        }

        /// <summary>
        /// 使用命名客户端请求（相对路径，自动拼接 demo 的 BaseAddress）
        /// </summary>
        [HttpGet]
        public async Task<string> NamedClient()
        {
            // BaseAddress 已在 Program.cs 中配置为 https://jsonplaceholder.typicode.com
            var result = await _demoHelper.GetAsync<string>("todos/2");
            _logger.LogInformation("命名客户端请求结果：{Result}", result.IsSuccess ? "成功" : "失败");
            return result.IsSuccess ? result.RawBody! : $"失败：{result.ErrorMessage}";
        }
    }
}
