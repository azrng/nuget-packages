using Azrng.Core.Extension;
using Common.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace HttpTestApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
                                                     {
                                                         "Freezing",
                                                         "Bracing",
                                                         "Chilly",
                                                         "Cool",
                                                         "Mild",
                                                         "Warm",
                                                         "Balmy",
                                                         "Hot",
                                                         "Sweltering",
                                                         "Scorching"
                                                     };

        private readonly ILogger<HomeController> _logger;
        private readonly IHttpHelper _httpHelper;

        public HomeController(ILogger<HomeController> logger, IHttpHelper httpHelper)
        {
            _logger = logger;
            _httpHelper = httpHelper;
        }

        /// <summary>
        /// 自定义超时重试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> CustomerTimeOutRetry()
        {
            var result = await _httpHelper.GetAsync<string>("http://localhost:5138/home?type=2");
            _logger.LogInformation($"请求响应：{result}");
            return result;
        }

        /// <summary>
        /// 默认超时重试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> DefaultTimeOutRetry()
        {
            var result = await _httpHelper.GetAsync<string>("http://localhost:5138/home?type=1");
            _logger.LogInformation($"请求响应：{result}");
            return result;
        }

        /// <summary>
        /// 401后重试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Status401Retry()
        {
            var result = await _httpHelper.GetAsync<string>("http://localhost:5138/home?type=3");
            _logger.LogInformation($"请求响应：{result}");
            return result;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get(int type)
        {
            var requestEventId = Request.Headers.FirstOrDefault(x => x.Key == "X-Trace-Id").Value;
            _logger.LogInformation($"{DateTime.Now.ToStandardString()}  {requestEventId}  发起请求" + type);
            if (type == 0) { }
            else if (type == 1)
            {
                await Task.Delay(105 * 1000);
            }
            else if (type == 2)
            {
                await Task.Delay(35 * 1000);
            }
            else if (type == 3)
            {
                return Unauthorized("授权失败");
            }

            return Enumerable.Range(1, 5)
                             .Select(index => new WeatherForecast
                                              {
                                                  Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                                  TemperatureC = Random.Shared.Next(-20, 55),
                                                  Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                                              })
                             .ToArray();
        }
    }
}