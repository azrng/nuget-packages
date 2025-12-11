using Azrng.Core.Requests;
using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgresSqlSample.Model;
using System.Linq.Expressions;

namespace PostgresSqlSample.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class BaseOperatorController : ControllerBase
{
    private readonly IBaseRepository<User> _userRep;
    private readonly IBaseRepository<UserAddress> _userAddressRep;
    private readonly IBaseRepository<Test2> _testRep;
    private readonly IUnitOfWork _ofWork;
    private readonly OpenDbContext _dbContext;
    private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
    private readonly ILogger<BaseOperatorController> _logger;

    public BaseOperatorController(IBaseRepository<User> userRep, IUnitOfWork ofWork, IUnitOfWork<OpenDbContext> unitOfWork,
                                  OpenDbContext dbContext, ILogger<BaseOperatorController> logger,
                                  IBaseRepository<UserAddress> userAddressRep, IBaseRepository<Test2> testRep)
    {
        _userRep = userRep;
        _ofWork = ofWork;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _logger = logger;
        _userAddressRep = userAddressRep;
        _testRep = testRep;
    }

    [HttpGet]
    public async Task<List<User>> GetListAsync()
    {
        var list = await _userRep.EntitiesNoTacking.ToListAsync();
        var test2List = await _testRep.EntitiesNoTacking.ToListAsync();

        return list;
    }

    [HttpGet]
    public async Task<string> OrderAsync()
    {
        {
            var query = _userRep.EntitiesNoTacking.OrderBy(new SortContent("CreateTime", SortEnum.Desc));
            var sql = query.ToQueryString();
            Console.WriteLine(sql);
            var list = await query.ToListAsync();
        }
        return "success";
    }

    [HttpGet]
    public async Task<string> JoinSelect()
    {
        var list = await _userRep.EntitiesNoTacking.Join(_userAddressRep.EntitiesNoTacking, u => u.Id, a => a.UserId,
                                     (u, a) => new { Account = u.Account, Address = a.Address })
                                 .ToListAsync();
        return list.Count.ToString();
    }

    [HttpGet]
    public async Task<string> LeftJoinSelect()
    {
        var list = await _userRep.EntitiesNoTacking.GroupJoin(_userAddressRep.EntitiesNoTacking, u => u.Id, a => a.UserId,
                                     (u, a) => new { u, a })
                                 .SelectMany(x => x.a.DefaultIfEmpty(),
                                     (u, a) => new { Account = u.u.Account, Address = a == null ? "暂无地址" : a.Address })
                                 .ToListAsync();
        Console.WriteLine(list.Count.ToString());

        // var list2 = await _userRep.EntitiesNoTacking.LeftJoin(_userAddressRep.EntitiesNoTacking, u => u.Id, a => a.UserId,
        //     (u, a) => new { Account = u.Account, Address = a == null ? "暂无地址" : a.Address });
        // Console.WriteLine(list2.Count);
        return "success";
    }

    /// <summary>
    /// 合并筛选
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Task<string> MergeFilter()
    {
        var list = new List<User>
                   {
                       new User("admin1", "123456", true),
                       new User("admin2", "123456", true),
                       new User("admin3", "123456", true),
                       new User("admin4", "123456", true),
                       new User("admin5", "123456", true),
                       new User("admin6", "123456", true)
                   };

        //表达式1
        Expression<Func<User, bool>> expression1 = t => t.Account == "001";

        //表达式2
        Expression<Func<User, bool>> expression2 = t => t.Password == "123456";

        //合并成 t => t.account=="001" && t.password=="123456"
        var allExpression = expression1.MergeAnd(expression2);
        Console.WriteLine(allExpression.Body.ToString());
        _logger.LogInformation(
            $"已处理集合(And)：{string.Join(',', list.Where(expression1.MergeAnd(expression2).Compile()).Select(o => o.Account))}");

        //合并成 t => t.account=="001" || t.password=="123456"
        allExpression = expression1.MergeOr(expression2);
        _logger.LogInformation(allExpression.Body.ToString());
        _logger.LogInformation($"已处理集合(Or)：{string.Join(',', list.Where(allExpression.Compile()).Select(o => o.Id))}");

        return Task.FromResult("success");
    }
}

public class UserDto
{
    public string Account { get; set; }

    public string Password { get; set; }
}