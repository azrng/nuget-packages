using Azrng.DistributeLock.Core;
using Azrng.DistributeLock.Redis;
using Azrng.SqlMigration;
using Npgsql;
using WebDbSqlMigrationSample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedisLockProvider("localhost:6379,password=123456,defaultdatabase=0,abortConnect=false");

var conn = "Host=localhost;Username=postgres;Password=123456;Database=1413;port=5433";
var conn2 = "Host=localhost;Username=postgres;Password=123456;Database=0306";

builder.Services.AddSqlMigrationService("default", config =>
       {
           config.Schema = "aa";
           config.VersionPrefix = string.Empty;
           config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
           config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
           config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
       })
       .AddAutoMigration();

// 现在的问题是会执行到一个库中
// builder.Services.AddSqlMigrationService("default", config =>
// {
//     config.Schema = "aa";
//     config.VersionPrefix = string.Empty;
//     config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
//     config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
//     // config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
// }).AddSqlMigrationService("default2", config =>
// {
//     config.Schema = "bb";
//     config.VersionPrefix = string.Empty;
//     config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql2");
//     config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn2);
//     // config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
// }).AddAutoMigration();

// 短版本迁移
// builder.Services.AddSqlMigrationService<DefaultMigrationHandler>("default", config =>
//        {
//            config.Schema = "aa";
//            config.VersionPrefix = string.Empty;
//            config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "MigrationSql");
//            config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
//            config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
//        })
//        .AddAutoMigration();

// 长版本迁移
// builder.Services.AddSqlMigrationService<DefaultMigrationHandler>("default", config =>
//        {
//            config.Schema = "aa";
//            config.VersionPrefix = string.Empty;
//            config.SqlRootPath = Path.Combine(builder.Environment.WebRootPath, "LongMigrationSql");
//            config.ConnectionBuilder = (sp) => new NpgsqlConnection(conn);
//            config.LockProvider = x => x.GetRequiredService<ILockProvider>().LockAsync("project_init", TimeSpan.FromMinutes(1));
//        })
//        .AddAutoMigration();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
                {
                    "Freezing",
                    "Bracing",
                    "Chilly",
                    "Cool",
                    "Mild",
                    "Warm",
                    "Balmy",
                    "Hot",
                    "Sweltering",
                    "Scorching"
                };

app.MapGet("/weatherforecast", () =>
   {
       var forecast = Enumerable.Range(1, 5)
                                .Select(index =>
                                    new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                        Random.Shared.Next(-20, 55),
                                        summaries[Random.Shared.Next(summaries.Length)]))
                                .ToArray();
       return forecast;
   })
   .WithName("GetWeatherForecast")
   .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}