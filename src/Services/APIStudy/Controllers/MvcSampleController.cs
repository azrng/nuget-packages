using Azrng.AspNetCore.Core.Extension;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APIStudy.Controllers
{
    /// <summary>
    /// mvc包示例测试
    /// </summary>
    public class MvcSampleController : BaseController
    {
        [HttpGet]
        public string GetStr()
        {
            var iPv4 = HttpContext.GetLocalIpAddressToIPv4();
            var ipv6 = HttpContext.GetLocalIpAddressToIPv6();
            var requestInfo = HttpContext.Request.GetUrl();
            return "success";
        }

        [HttpPost]
        public async Task<string> GetBody(UserInfo userInfo)
        {
            return await HttpContext.Request.ReadBodyContentAsync();
        }
    }
}

public class UserInfo
{
    /// <summary>
    /// id
    /// </summary>
    [Required]
    public int Id { get; set; }
}