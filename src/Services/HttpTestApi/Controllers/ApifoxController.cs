using Common.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace HttpTestApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ApifoxController : ControllerBase
    {
        private readonly string Host = "https://echo.apifox.com";

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

        private readonly ILogger<ApifoxController> _logger;
        private readonly IHttpHelper _httpHelper;

        public ApifoxController(ILogger<ApifoxController> logger, IHttpHelper httpHelper)
        {
            _logger = logger;
            _httpHelper = httpHelper;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var result = await _httpHelper.GetAsync<string>(Host + "/get?q1=11&q2=22");
            return Enumerable.Range(1, 5)
                             .Select(index => new WeatherForecast
                                              {
                                                  Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                                  TemperatureC = Random.Shared.Next(-20, 55),
                                                  Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                                              })
                             .ToArray();
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> Post()
        {
            var list = Enumerable.Range(1, 5)
                                 .Select(index => new WeatherForecast
                                                  {
                                                      Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                                      TemperatureC = Random.Shared.Next(-20, 55),
                                                      Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                                                  })
                                 .ToList();
            var result = await _httpHelper.PostAsync<string>(Host + "/anything", list);

            return true;
        }

        /// <summary>
        /// post请求 忽略日志
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> PostIgnoreLog()
        {
            var list = Enumerable.Range(1, 5)
                                 .Select(index => new WeatherForecast
                                                  {
                                                      Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                                      TemperatureC = Random.Shared.Next(-20, 55),
                                                      Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                                                  })
                                 .ToList();
            var result = await _httpHelper.PostAsync<string>(Host + "/anything", list,
                headers: new Dictionary<string, string>() { { "X-Logger", "skip" } });

            var result2 = await _httpHelper.PostAsync<string>(Host + "/anything", list,
                headers: new Dictionary<string, string>() { { "X-Skip-Logger", "" } });

            return true;
        }
    }
}