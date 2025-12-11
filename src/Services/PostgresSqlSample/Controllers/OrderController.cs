using Azrng.Core.Requests;
using Azrng.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PostgresSqlSample.Model;
using System.Linq.Expressions;

namespace PostgresSqlSample.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class OrderController : ControllerBase
{
    private readonly IBaseRepository<User> _userRep;
    private readonly IUnitOfWork _ofWork;
    private readonly OpenDbContext _dbContext;
    private readonly IUnitOfWork<OpenDbContext> _unitOfWork;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IBaseRepository<User> userRep, IUnitOfWork ofWork, IUnitOfWork<OpenDbContext> unitOfWork,
                           OpenDbContext dbContext, ILogger<OrderController> logger)
    {
        _userRep = userRep;
        _ofWork = ofWork;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _logger = logger;
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
}