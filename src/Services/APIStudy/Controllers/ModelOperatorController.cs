using Azrng.Core;
using Microsoft.AspNetCore.Mvc;

namespace APIStudy.Controllers
{
    /// <summary>
    /// 模型操作控制器
    /// </summary>
    public class ModelOperatorController : BaseController
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<ModelOperatorController> _logger;

        public ModelOperatorController(IJsonSerializer jsonSerializer, ILogger<ModelOperatorController> logger)
        {
            _jsonSerializer = jsonSerializer;
            _logger = logger;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public string AddData(IFormFile request)
        {
            return _jsonSerializer.ToJson(request);
        }

        [HttpGet]
        public List<TimeInfo> GetTime()
        {
            var list = new List<TimeInfo>();
            for (var i = 0; i < 5; i++)
            {
                list.Add(new TimeInfo { Id = i.ToString(), Time = DateTime.Now.AddHours(i) });
            }

            _logger.LogInformation($"信息：{_jsonSerializer.ToJson(list)}");
            return list;
        }
    }

    public class TimeInfo
    {
        public string Id { get; set; }

        public DateTime Time { get; set; }
    }
}