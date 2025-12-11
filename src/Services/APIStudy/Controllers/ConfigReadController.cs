using APIStudy.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace APIStudy.Controllers
{
    /// <summary>
    /// 配置读取
    /// </summary>
    public class ConfigReadController : BaseController
    {
        private readonly TestOptions _testOptions;

        public ConfigReadController(IOptions<TestOptions> options)
        {
            _testOptions = options.Value;
        }

        [HttpGet]
        public string GetConfigName()
        {
            return _testOptions.Name;
        }
    }
}