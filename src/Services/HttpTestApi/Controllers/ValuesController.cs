using Microsoft.AspNetCore.Mvc;

namespace HttpTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly ILogger<ValuesController> _logger;

    public ValuesController(ILogger<ValuesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取所有值
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("获取所有值");

        for (var i = 0; i < 30; i++)
        {
            _logger.LogInformation($"返回 第{i} 个值");
        }

        return Ok(0);
    }

    /// <summary>
    /// 根据 ID 获取值
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        _logger.LogDebug("查询 ID={Id}", id);

        if (id <= 0)
        {
            _logger.LogWarning("无效的 ID: {Id}", id);
            return BadRequest("ID 必须大于 0");
        }

        var value = new { Id = id, Name = $"Value {id}", Description = $"第 {id} 个值" };

        _logger.LogInformation("获取 ID={Id} 的值成功", id);

        return Ok(value);
    }

    /// <summary>
    /// 创建新值
    /// </summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateValueRequest request)
    {
        _logger.LogInformation("创建新值：{@Request}", request);

        if (string.IsNullOrEmpty(request.Name))
        {
            _logger.LogError("创建失败：名称不能为空");
            return BadRequest("名称不能为空");
        }

        var newValue = new { Id = new Random().Next(1000, 9999), request.Name, request.Description };

        _logger.LogInformation("创建成功，新 ID={Id}", newValue.Id);

        return Created($"/api/values/{newValue.Id}", newValue);
    }

    /// <summary>
    /// 模拟错误
    /// </summary>
    [HttpGet("error")]
    public IActionResult TriggerError()
    {
        _logger.LogWarning("准备触发错误...");

        try
        {
            throw new InvalidOperationException("这是一个测试错误！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "捕获到异常：{Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// 模拟慢请求
    /// </summary>
    [HttpGet("slow")]
    public IActionResult SlowRequest(int delayMs = 2000)
    {
        _logger.LogInformation("开始慢请求，延迟 {DelayMs}ms", delayMs);

        Thread.Sleep(delayMs);

        _logger.LogInformation("慢请求完成");

        return Ok(new { message = $"延迟了 {delayMs}ms" });
    }

    /// <summary>
    /// 测试结构化日志
    /// </summary>
    [HttpGet("structured")]
    public IActionResult StructuredLogTest()
    {
        var userData = new
                       {
                           UserId = 12345,
                           Username = "testuser",
                           Email = "test@example.com",
                           Roles = new[]
                                   {
                                       "Admin",
                                       "User"
                                   }
                       };

        _logger.LogInformation("用户数据：{@User}", userData);
        _logger.LogInformation("订单详情：{OrderId} - {Amount} - {Currency}",
            "ORD-2024-001", 99.99m, "USD");

        return Ok(new { message = "结构化日志已记录" });
    }

    /// <summary>
    /// 输出所有类型的日志
    /// </summary>
    [HttpGet("all-log-levels")]
    public IActionResult AllLogLevels()
    {
        _logger.LogTrace("这是 Trace 级别的日志");
        _logger.LogDebug("这是 Debug 级别的日志");
        _logger.LogInformation("这是 Information 级别的日志");
        _logger.LogWarning("这是 Warning 级别的日志");
        _logger.LogError("这是 Error 级别的日志");
        _logger.LogCritical("这是 Critical 级别的日志");

        return Ok(new { message = "已输出所有类型的日志", timestamp = DateTime.Now });
    }
}

public class CreateValueRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}