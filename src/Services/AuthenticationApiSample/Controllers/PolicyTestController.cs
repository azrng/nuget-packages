using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApiSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PolicyTestController : ControllerBase
    {
        private readonly ILogger<PolicyTestController> _logger;

        public PolicyTestController(ILogger<PolicyTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public string Path1()
        {
            _logger.LogInformation($"Path1 我被请求了  {DateTime.Now}");

            return "success" + DateTime.Now;
        }
        
        [HttpGet]
        [Authorize]
        public string Path2()
        {
            _logger.LogInformation($"Path2 我被请求了  {DateTime.Now}");

            return "success" + DateTime.Now;
        }
    }
}