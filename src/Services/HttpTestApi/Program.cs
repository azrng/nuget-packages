using Azrng.DevLogDashboard.Extensions;
using Common.HttpClients;
using HttpTestApi.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 DevLogDashboard 服务（默认使用内存存储）
builder.Services.AddDevLogDashboard(options =>
{
    options.EndpointPath = "/dev-logs";
    options.ApplicationName = "SampleWebApi";
    options.ApplicationVersion = "1.0.0";
});

// 如需使用 PostgreSQL 存储，请先确保数据库可访问，然后取消下面代码的注释：
// builder.Services.AddDevLogDashboard<PgSqlLogStore>(options =>
// {
//     options.EndpointPath = "/dev-logs";
//     options.ApplicationName = "SampleWebApi";
//     options.ApplicationVersion = "1.0.0";
// });

// 1) 非命名客户端（默认）：直接注入 IHttpHelper 即可使用
//    无参重载使用内置默认配置；如需自定义可换成 options => { } 重载。
builder.Services.AddHttpClientService();

// 2) 命名客户端：按业务名称注册，各客户端可独立配置 BaseAddress / 超时 / 重试等
//    使用时注入 IHttpHelperFactory，调用 CreateClient("名称") 取出对应 IHttpHelper。
builder.Services.AddHttpClientService("demo", options =>
{
    options.BaseAddress = "https://jsonplaceholder.typicode.com";
    options.Timeout = 30;
    options.MaxRetryAttempts = 2;
    options.AuditLog = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // 使用 DevLogDashboard（完全自包含，不需要额外的路由设置）
    app.UseDevLogDashboard();
}

app.UseAuthorization();
app.MapControllers();

// 输出访问地址
Console.WriteLine("===========================================");
Console.WriteLine("访问地址：http://localhost:5132/dev-logs");
Console.WriteLine("Swagger 地址：http://localhost:5132/swagger");
Console.WriteLine("===========================================");

app.Run();