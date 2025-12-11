using AuthenticationApiSample.Auths;
using AuthenticationApiSample.Current;
using Azrng.Core;
using Azrng.Core.Json;
using Azrng.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDefaultSwaggerGen(title: "授权测试API", showJwtToken: true);

builder.Services.ConfigureDefaultJson();

builder.Services.AddMyAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();

// builder.Services.AddMyAuthorization<PermissionVerifyService>("Path2");

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

// app.UseMiddleware<CustomExceptionMiddleware>();

// Configure the HTTP request pipeline.
app.UseDefaultSwagger();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();