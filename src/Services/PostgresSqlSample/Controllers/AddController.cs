using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PostgresSqlSample.Model;
using System.Linq.Expressions;

namespace PostgresSqlSample.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AddController : ControllerBase
{
    private readonly IBaseRepository<User> _userRep;
    private readonly OpenDbContext _dbContext;
    private readonly ILogger<AddController> _logger;
    private readonly DbContext _db;
    private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
    private readonly IUnitOfWork _unitOfWork2;

    public AddController(IBaseRepository<User> userRep,
                         OpenDbContext dbContext, ILogger<AddController> logger, DbContext db, IUnitOfWork<OpenDbContext> unitOfWork,
                         IUnitOfWork unitOfWork2)
    {
        _userRep = userRep;
        _dbContext = dbContext;
        _logger = logger;
        _db = db;
        _unitOfWork = unitOfWork;
        _unitOfWork2 = unitOfWork2;
    }

    [HttpGet]
    public async Task<int> AddAsync()
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

        // var flag = await _userRep.AddAsync(list, true);

        // 这组合使用的不是同一个上下文，所以保存失败
        // await _userRep.AddAsync(list);
        // var flag = await _unitOfWork.SaveChangesAsync();

        await _userRep.AddAsync(list);
        var flag = await _unitOfWork2.SaveChangesAsync();

        // var flag = await _db.SaveChangesAsync();
        return flag;
    }

    [HttpGet]
    public async Task AddAndUpdateAsync()
    {
        var entity = new User("zhangsan", "aaaa", true);
        await _userRep.AddAsync(entity, true);

        var bb = _userRep.Entities.First(t => t.Id == entity.Id);
        bb.UserName = "李四";
        await _userRep.UpdateAsync(entity, true);

        await _userRep.DeleteAsync(t => t.Account == "zhangsan");
    }
}