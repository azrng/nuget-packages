using Azrng.Core.Extension;
using Azrng.Core.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace APIStudy.Controllers;

/// <summary>
/// http 示例控制器
/// </summary>
public class HttpSampleController : BaseController
{
    private readonly ILogger<HttpSampleController> _logger;

    public HttpSampleController(ILogger<HttpSampleController> logger)
    {
        _logger = logger;
    }

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

    /// <summary>
    /// 流式分析上传的日志文件（SSE）
    /// POST /api/log-analysis/stream-upload
    /// </summary>
    [HttpPost("stream-upload")]
    [Consumes("multipart/form-data")]
    public async Task StreamAnalyzeUpload(IFormFile? file, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{DateTime.Now.ToStandardString()} fileName:{file?.FileName}");
        await Task.Delay(100, cancellationToken);
        return;
    }
}

public class TestHttpRequest
{
    public string Id { get; set; }

    public string Name { get; set; }

    public long LongId { get; set; }
}