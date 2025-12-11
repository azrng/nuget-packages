using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgresSqlSample.EFCoreExtensions;
using PostgresSqlSample.Model;
using PostgresSqlSample.Model.Dto;

namespace PostgresSqlSample.Controllers;

/// <summary>
/// 自定义操作
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : ControllerBase
{
    private readonly IBaseRepository<User> _userRep;
    private readonly IUnitOfWork _ofWork;
    private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
    private readonly ILogger<CustomerController> _logger;
    private readonly OpenDbContext _dbContext;

    public CustomerController(IBaseRepository<User> userRep, IUnitOfWork ofWork, OpenDbContext dbContext,
                              IUnitOfWork<OpenDbContext> unitOfWork, ILogger<CustomerController> logger)
    {
        _userRep = userRep;
        _ofWork = ofWork;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 时间返回格式化
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> ToChar()
    {
        var list1 = await _dbContext
                          .Users.Select(t =>
                              new GetUserBaseDto { Id = t.Id, CreateTime = t.CreateTime.ToStandardString() })
                          .ToListAsync();

        // 现在还报错 这个网址是老版本，参考了没弄好  https://www.cnblogs.com/GuZhenYin/p/14657024.html
        var list = await _dbContext.Users.Select(t => new GetUserBaseDto
                                                      {
                                                          Id = t.Id, CreateTime = EF.Functions.ToChar(t.CreateTime, "yyyy-MM-dd HH:mm:ss")
                                                      })
                                   .ToListAsync();
        return list.Count.ToString();
    }

    [HttpGet]
    public async Task<string> Like()
    {
        var list = await _dbContext.Users.Where(t => EF.Functions.Like(t.UserName, "%5%"))
                                   .ToListAsync();
        return list.Count.ToString();
    }

    [HttpGet]
    public async Task<string> StringToArray()
    {
        var list = await _dbContext.Users
                                   .Select(t => new { t.Id, NameList = EF.Functions.StringToArray(t.UserName, ",") })
                                   .ToListAsync();
        return list.Count.ToString();
    }

    // /// <summary>
    // /// 将时间格式字符串转时间   ToTimestamp 在.Net9才支持
    // /// </summary>
    // /// <returns></returns>
    // [HttpGet]
    // public async Task<string> ToTimestamp()
    // {
    //     var str = "time_str_test";
    //     var exist = await _dbContext.Users.AnyAsync(t => t.Account == str);
    //     if (!exist)
    //     {
    //         var dateStr = DateTime.Now.ToStandardString();
    //         await _dbContext.Users.AddAsync(new User(str, dateStr, true));
    //         await _dbContext.SaveChangesAsync();
    //     }
    //
    //     var list = await _dbContext.Users
    //                                .Where(t => t.Account == str)
    //                                .Select(t => new { t.Id, Time = EF.Functions.ToTimestamp(t.Account, "yyyy-MM-dd HH:mm:ss") })
    //                                .ToListAsync();
    //     return list.Count.ToString();
    // }

    /// <summary>
    /// 只是测试，调用会报错，因为不是jsonb类型列
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> JsonContains()
    {
        var list = await _dbContext.Users.Where(t => EF.Functions.JsonContains(t.UserName, "%5%"))
                                   .ToListAsync();
        return list.Count.ToString();
    }
}