using Common.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClientService(options =>
{
    options.AuditLog = true;
    options.FailThrowException = false;
    // options.Timeout = 30; // 现在可以设置自定义超时时间，超时后也会进行重试
    options.IgnoreUntrustedCertificate = true;
    options.RetryOnUnauthorized = true;
});

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