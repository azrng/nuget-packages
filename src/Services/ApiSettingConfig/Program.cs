using Azrng.SettingConfig;
using Azrng.SettingConfig.BasicAuthorization;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen()
       .AddMvcResultPackFilter("/api/configDashboard");

var conn = builder.Configuration.GetConnectionString("pgsql");
builder.Services.AddSettingConfig(options =>
{
    options.DbConnectionString = conn;
    options.DbSchema = "sample";
    options.RoutePrefix = "config";
    options.ApiRoutePrefix = "/api/configDashboard";
    options.PageTitle = "配置";
    options.Authorization = new[]
                            {
                                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                                                                 {
                                                                     RequireSsl = false,
                                                                     SslRedirect = false,
                                                                     LoginCaseSensitive = true,
                                                                     Users = new[]
                                                                             {
                                                                                 new BasicAuthAuthorizationUser
                                                                                 {
                                                                                     Login = "admin", PasswordClear = "123456"
                                                                                 }
                                                                             }
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

// 配置 Dashboard 中间件
app.UseSettingDashboard();

app.MapControllers();
app.Run();