using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgresSqlSample.Model;

namespace PostgresSqlSample.Controllers;

/// <summary>
/// 执行原始sql
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class OriginalSqlController : ControllerBase
{
    private readonly IBaseRepository<User> _userRep;
    private readonly IUnitOfWork _ofWork;
    private readonly OpenDbContext _dbContext;
    private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
    private readonly ILogger<OriginalSqlController> _logger;

    public OriginalSqlController(IBaseRepository<User> userRep, IUnitOfWork ofWork, OpenDbContext dbContext,
                                 IUnitOfWork<OpenDbContext> unitOfWork, ILogger<OriginalSqlController> logger)
    {
        _userRep = userRep;
        _ofWork = ofWork;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<string> SqlQuery()
    {
        var schema = "public";
        var result = await _ofWork.SqlQuery<int>($"select count(1) from {schema}.\"user\"").FirstOrDefaultAsync();
        return "success";
    }

    [HttpGet]
    public async Task<string> ExecuteSqlAsync()
    {
        var userRep2 = _ofWork.GetRepository<User>();

        var entity = new User("zhangsan", "aaaa", true);
        await userRep2.AddAsync(entity, true);

        {
            var sql =
                $"INSERT INTO public.\"user\" (id, account, password, create_time, user_name, is_valid) VALUES ({DateTime.Now.ToString("yyyyMMddHHmmss")}, 'admin2', '123456', '2023-05-12 15:14:38.669958 +00:00', 'admin2', true);";
            var i = await _ofWork.ExecuteSqlCommandAsync(sql);

            await _ofWork.ExecuteSqlCommandAsync("delete from public.\"user\" where account='zhangsan';");
        }
        {
            var sql = "select account,password from public.\"user\"";
            var list = await _ofWork.SqlQueryListAsync<UserDto>(sql);
        }

        return "success";
    }
}