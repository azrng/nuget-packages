using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APIStudy.Controllers
{
    /// <summary>
    /// 自定义特性操作
    /// </summary>
    public class CustomerAttributeController : BaseController
    {
        /// <summary>
        /// 验证操作
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public string VerifyOperator(CustomerAttributeInfo request)
        {
            return "success";
        }
    }

    public class CustomerAttributeInfo
    {
        [MinValue(2)]
        public int Id { get; set; }

        [CollectionNotEmpty]
        public List<string> Names { get; set; }
    }
}