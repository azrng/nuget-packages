using Azrng.SettingConfig;
using Azrng.SettingConfig.BasicAuthorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen()
    .AddMvcResultPackFilter("/api/configDashboard");

var conn = builder.Configuration.GetConnectionString("pgsql");
builder.Services.AddSettingConfig(options =>
{
    options.DbConnectionString = conn;
    options.DbSchema = "sample";
    options.RoutePrefix = "configDashboard";
    options.ApiRoutePrefix = "/api/configDashboard";
    options.Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false,
            SslRedirect = false,
            LoginCaseSensitive = true,
            Users = new[] { new BasicAuthAuthorizationUser { Login = "admin", PasswordClear = "123456" } }
        })
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSettingDashboard();

// 全局启用Basic认证中间件
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.Run();