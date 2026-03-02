using Azrng.Core.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace APIStudy.Controllers;

/// <summary>
/// http 示例控制器
/// </summary>
public class HttpSampleController : BaseController
{
    [HttpGet]
    public TestHttpRequest Get(string name)
    {
        return new TestHttpRequest { Id = "11", Name = name, LongId = Snowflake.NewId() };
    }

    [HttpPost]
    public string Post([FromBody] TestHttpRequest request)
    {
        return "success" + request.Name;
    }

    [HttpPut]
    public string Put([FromBody] TestHttpRequest request)
    {
        return "success" + request.Name;
    }

    [HttpDelete]
    public string Delete(string id)
    {
        return "success" + id;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public string Upload(IFormFile request)
    {
        return "success" + request.FileName;
    }
}

public class TestHttpRequest
{
    public string Id { get; set; }

    public string Name { get; set; }

    public long LongId { get; set; }
}