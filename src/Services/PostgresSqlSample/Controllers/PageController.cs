// using Azrng.Core.CommonDto;
// using Azrng.Core.Requests;
// using Azrng.EFCore;
// using Azrng.EFCore.Extensions;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using PostgresSqlSample.Model;
//
// namespace PostgresSqlSample.Controllers;
//
// [ApiController]
// [Route("[controller]/[action]")]
// public class PageController : ControllerBase
// {
//     private readonly IBaseRepository<User> _userRep;
//     private readonly IUnitOfWork _ofWork;
//     private readonly OpenDbContext _dbContext;
//     private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
//     private readonly ILogger<PageController> _logger;
//
//     public PageController(IBaseRepository<User> userRep, IUnitOfWork ofWork, IUnitOfWork<OpenDbContext> unitOfWork,
//                           OpenDbContext dbContext, ILogger<PageController> logger)
//     {
//         _userRep = userRep;
//         _ofWork = ofWork;
//         _unitOfWork = unitOfWork;
//         _dbContext = dbContext;
//         _logger = logger;
//     }
//
//     [HttpGet]
//     public async Task<string> PagedBy()
//     {
//         {
//             // 根据类查询
//             var pageRequest = new GetPageRequest(1, 2);
//
//             var total = 0;
//             var list = await _userRep.EntitiesNoTacking.PagedBy(pageRequest)
//                                      .CountBy(out total)
//                                      .Select(t => new { t.Account, t.Password })
//                                      .ToListAsync();
//             _logger.LogInformation($"total:{total}");
//         }
//
//         {
//             var list = await _userRep.EntitiesNoTacking.PagedBy(1, 2, out int taotal)
//                                      .Select(t => new { t.Account, t.Password })
//                                      .ToListAsync();
//             _logger.LogInformation($"total:{taotal}");
//         }
//
//         return "success";
//     }
//
//     [HttpGet]
//     public async Task<string> ToPageList()
//     {
//         {
//             // 根据类查询
//             var pageRequest = new GetPageRequest(1, 2);
//             RefAsync<int> total = 0;
//
//             var list = await _userRep.EntitiesNoTacking
//                                      .Select(t => new { t.Account, t.Password })
//                                      .ToPageListAsync(pageRequest, total);
//
//             var list2 = await _userRep.EntitiesNoTacking
//                                       .Select(t => new { t.Account, t.Password })
//                                       .ToPageListAsync(pageRequest);
//         }
//
//         {
//             RefAsync<int> total = 0;
//             var list = await _userRep.EntitiesNoTacking
//                                      .Select(t => new { t.Account, t.Password })
//                                      .ToPageListAsync(1, 2, total);
//
//             var list2 = await _userRep.EntitiesNoTacking
//                                       .Select(t => new { t.Account, t.Password })
//                                       .ToPageListAsync(1, 2);
//         }
//
//         return "success";
//     }
// }