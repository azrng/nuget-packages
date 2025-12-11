using Azrng.Cache.MemoryCache;
using Microsoft.AspNetCore.Mvc;

namespace CacheAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CacheController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CacheController> _logger;
        private readonly IMemoryCacheProvider _cacheProvider;

        public CacheController(ILogger<CacheController> logger, IMemoryCacheProvider cacheProvider)
        {
            _logger = logger;
            _cacheProvider = cacheProvider;
        }

        [HttpGet]
        public async Task<string> GetOrCreate()
        {
            return await _cacheProvider.GetOrCreateAsync("timeNow", () =>
            {
                return DateTime.Now.ToString();
            }, TimeSpan.FromSeconds(10));
        }

        [HttpGet]
        public async Task<bool> ExistTime()
        {
            return await _cacheProvider.ExistAsync("timeNow");
        }

        [HttpGet]
        public List<string> GetKeys()
        {
            return _cacheProvider.GetAllKeys();
        }

        [HttpGet]
        public async Task<WeatherForecast[]> GetList()
        {
            var list = await _cacheProvider.GetAsync<WeatherForecast[]>("list");
            return list;
        }

        [HttpGet]
        public async Task<string> SetList()
        {
            var list = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            await _cacheProvider.SetAsync("list", list, TimeSpan.FromMinutes(2));
            return "success";
        }

        [HttpGet]
        public async Task<bool> RemoveList()
        {
            await _cacheProvider.RemoveAsync("list");
            return true;
        }

        /// <summary>
        /// 测试缓存穿透
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Test()
        {
            var user = await _cacheProvider.GetOrCreateAsync("user716", () =>
            {
                //就算查询不到，那么就把null存到缓存，防止缓存穿透
                var bb = Summaries.FirstOrDefault(t => t == "716");
                return bb;
            });
            return user;
        }
    }
}