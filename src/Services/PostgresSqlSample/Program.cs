using Azrng.Core.Model;
using Azrng.EFCore;
using Azrng.EFCore.AutoAudit;
using Azrng.EFCore.AutoAudit.Config;
using Microsoft.EntityFrameworkCore;
using PostgresSqlSample.Model;
using System.Reflection;
using DatabaseType = Azrng.Core.Model.DatabaseType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
var conn = builder.Configuration["Conn"];
builder.Services.AddEntityFramework<OpenDbContext>(config =>
       {
           config.ConnectionString = conn;
           config.UseOldUpdateColumn = true;
           config.Schema = "azrng";
       }, x =>
       {
           x.MigrationsAssembly(migrationsAssembly);
       })
       .AddUnitOfWork<OpenDbContext>();

// // 添加业务数据库
// builder.Services.AddDbContext<OpenDbContext>((provider, options) =>
// {
//     options.UseNpgsql(conn);
//     options.AddAuditInterceptor(provider);
// });

// 添加审计
builder.Services.AddEFCoreAutoAudit(config =>
{
    config // .WithStore<AuditFileStore>() // 自定义存储
        .WithAuditRecordsDbContextStore(options => { options.UseNpgsql(conn); });
}, DatabaseType.PostgresSql);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();