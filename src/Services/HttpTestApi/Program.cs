using Azrng.DevLogDashboard.Extensions;
using Azrng.DevLogDashboard.Storage;
using Common.HttpClients;
using HttpTestApi.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 DevLogDashboard 服务（使用 PostgreSQL 存储）
var pgConnectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=dev_log";

builder.Services.AddDevLogDashboard<PgSqlLogStore>(options =>
{
    options.EndpointPath = "/dev-logs";
    options.ApplicationName = "SampleWebApi";
    options.ApplicationVersion = "1.0.0";
});

// builder.Services.AddDevLogDashboard(options =>
// {
//     options.EndpointPath = "/dev-logs";
//     options.ApplicationName = "SampleWebApi";
//     options.ApplicationVersion = "1.0.0";
// });


builder.Services.AddHttpClientService(options =>
{
    options.AuditLog = true;
    options.FailThrowException = false;
    // options.Timeout = 30; // 现在可以设置自定义超时时间，超时后也会进行重试
    options.IgnoreUntrustedCertificate = true;
    options.RetryOnUnauthorized = true;
});

var app = builder.Build();

// 初始化 LogStore（用于数据库存储的初始化）
using (var scope = app.Services.CreateScope())
{
    var logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();
    await logStore.InitializeAsync();
}

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