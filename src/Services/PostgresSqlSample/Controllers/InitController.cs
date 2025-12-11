using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgresSqlSample.Model;

namespace PostgresSqlSample.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class InitController : ControllerBase
{
    private readonly IBaseRepository<User> _userRep;
    private readonly IBaseRepository<UserAddress> _userAddressRep;
    private readonly OpenDbContext _dbContext;
    private readonly ILogger<AddController> _logger;
    private readonly DbContext _db;
    private readonly IUnitOfWork _ofWork;

    public InitController(IBaseRepository<User> userRep,
                          OpenDbContext dbContext, ILogger<AddController> logger, DbContext db,
                          IUnitOfWork ofWork, IBaseRepository<UserAddress> userAddressRep)
    {
        _userRep = userRep;
        _dbContext = dbContext;
        _logger = logger;
        _db = db;
        _ofWork = ofWork;
        _userAddressRep = userAddressRep;
    }

    [HttpGet]
    public async Task InitDbAsync()
    {
        await _dbContext.Database.MigrateAsync();
    }

    [HttpGet]
    public async Task<int> InitDataAsync()
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

        await _userRep.AddAsync(list);

        var userAddress = new UserAddress { Name = "淘宝", UserId = list[0].Id, Address = "上海市" };
        await _userAddressRep.AddAsync(userAddress);

        var flag = await _ofWork.SaveChangesAsync();

        return flag;
    }
}