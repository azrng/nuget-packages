using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using PostgresSqlSample.Model;

namespace PostgresSqlSample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TranController : ControllerBase
    {
        private readonly IBaseRepository<User> _userRep;
        private readonly IBaseRepository<UserAddress> _userAddressRep;
        private readonly OpenDbContext _dbContext;
        private readonly ILogger<AddController> _logger;
        private readonly IUnitOfWork _ofWork;

        public TranController(IBaseRepository<User> userRep,
                              OpenDbContext dbContext, ILogger<AddController> logger,
                              IUnitOfWork ofWork, IBaseRepository<UserAddress> userAddressRep)
        {
            _userRep = userRep;
            _dbContext = dbContext;
            _logger = logger;
            _ofWork = ofWork;
            _userAddressRep = userAddressRep;
        }

        // [HttpGet]
        // public async Task<int> AddAsync()
        // {
        //     await using var tran = await _ofWork.GetDatabase().BeginTransactionAsync();
        //     try
        //     {
        //         var list = new List<User>
        //                    {
        //                        new User("admin1", "123456", true),
        //                        new User("admin2", "123456", true),
        //                        new User("admin3", "123456", true),
        //                        new User("admin4", "123456", true),
        //                        new User("admin5", "123456", true),
        //                        new User("admin6", "123456", true)
        //                    };
        //
        //         await _userRep.AddAsync(list, true);
        //
        //         var userAddress = new UserAddress { Name = "淘宝", UserId = list[0].Id, Address = "上海市" };
        //         await _userAddressRep.AddAsync(userAddress, true);
        //
        //         await tran.CommitAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"添加失败 {ex.GetExceptionAndStack()}");
        //         await tran.RollbackAsync();
        //     }
        //
        //     return 1;
        // }

        [HttpGet]
        public async Task<int> Add2Async()
        {
            await _ofWork.CommitTransactionAsync(async () =>
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

                await _userRep.AddAsync(list, true);

                var userAddress = new UserAddress { Name = "淘宝", UserId = list[0].Id, Address = "上海市" };
                await _userAddressRep.AddAsync(userAddress, true);
            });
            return 1;
        }
    }
}