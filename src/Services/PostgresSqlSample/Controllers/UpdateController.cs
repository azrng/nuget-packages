// using Azrng.EFCore;
// using Azrng.EFCore.Extensions;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Query;
// using PostgresSqlSample.Model;
// using System.Linq.Expressions;
//
// namespace PostgresSqlSample.Controllers;
//
// [ApiController]
// [Route("[controller]/[action]")]
// public class UpdateController : ControllerBase
// {
//     private readonly IBaseRepository<User> _userRep;
//     private readonly IUnitOfWork _ofWork;
//     private readonly OpenDbContext _dbContext;
//     private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
//     private readonly ILogger<UpdateController> _logger;
//
//     public UpdateController(IBaseRepository<User> userRep, IUnitOfWork ofWork, IUnitOfWork<OpenDbContext> unitOfWork,
//                             OpenDbContext dbContext, ILogger<UpdateController> logger)
//     {
//         _userRep = userRep;
//         _ofWork = ofWork;
//         _unitOfWork = unitOfWork;
//         _dbContext = dbContext;
//         _logger = logger;
//     }
//
//     /// <summary>
//     /// 测试重复追踪问题
//     /// </summary>
//     [HttpGet]
//     public async Task TestTrackError()
//     {
//         // 场景：blazor中先不追踪查询列表，然后在根据id查询单个详情，然后再根据id更新，就会报错
//         var newUser = new User("1111", "2222", false);
//         await _userRep.AddAsync(newUser, true);
//
//         var entity = await _userRep.EntitiesNoTacking.FirstAsync(x => x.Id == newUser.Id);
//
//         // 模型更新信息
//         entity.UserName = entity.UserName + "1";
//         entity.CreateTime = DateTime.Now.ToUnspecifiedDateTime();
//
//         // 修改为_dbContext.Entry(entity).State = EntityState.Modified; 还不行，还是会提示已经被追踪
//         // 暂时方案，查询的时候还是使用Entities追踪，然后修改
//         await _userRep.UpdateAsync(entity, true);
//     }
//
//     /// <summary>
//     /// 不查询更新
//     /// </summary>
//     /// <returns></returns>
//     [HttpGet]
//     public async Task<string> NoSelectUpdate()
//     {
//         await _userRep.UpdateAsync(t => t.Account == "admin1", t =>
//             t.SetProperty(x => x.UserName, "李四")
//              .SetProperty(x => x.Password, "654321"));
//
//         // 选择性更新
//         Expression<Func<SetPropertyCalls<User>, SetPropertyCalls<User>>> setPropertyCalls =
//             b => b.SetProperty(x => x.UserName, "张三1");
//         setPropertyCalls = setPropertyCalls.AppendIf(false, x => x.SetProperty(o => o.Password, "6543210"));
//         await _userRep.UpdateAsync(t => t.Account == "admin2", setPropertyCalls);
//         return "success";
//     }
//
//     [HttpGet]
//     public async Task ForeachUpdate()
//     {
//         Expression<Func<User, bool>> exp = r => r.IsValid == true;
//         var list = await _dbContext.Users.AsNoTracking().Where(exp).ToListAsync();
//         if (list.Count > 0)
//         {
//             foreach (var item in list)
//             {
//                 await SetDeletedAsync(item.Id);
//                 await Task.Delay(1000);
//                 _logger.LogInformation($"更新 {item.Id} 的状态");
//             }
//
//             await _dbContext.SaveChangesAsync();
//         }
//     }
//
//     [NonAction]
//     public async Task SetDeletedAsync(long id)
//     {
//         var entity = await GetAsync(id);
//         if (entity != null)
//         {
//             entity.IsValid = false;
//             _dbContext.Set<User>().Update(entity);
//         }
//     }
//
//     [NonAction]
//     public Task<User?> GetAsync(long id)
//     {
//         return _dbContext.Set<User>()
//                          .AsNoTracking()
//                          .FirstOrDefaultAsync(r => Equals(r.Id, id) && r.IsValid);
//     }
// }