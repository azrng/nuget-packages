using Microsoft.AspNetCore.Mvc;

namespace APIStudy.Controllers;

/// <summary>
/// http 示例控制器
/// </summary>
public class HttpSampleController : BaseController
{
    [HttpGet]
    public string Get(string name)
    {
        return "success" + name;
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
}

public class TestHttpRequest
{
    public string Id { get; set; }

    public string Name { get; set; }
}