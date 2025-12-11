using Azrng.AspNetCore.DbEnvConfig;
using DbEnvConfigSample;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration["ConnStr"];
builder.Configuration.AddDbConfiguration(options =>
{
    options.CreateDbConnection = () => new NpgsqlConnection(connStr);
    options.FilterWhere = "and is_delete=false";
}, new PgsqlScriptService());

builder.Configuration.AddDbConfiguration(options =>
{
    options.CreateDbConnection = () => new NpgsqlConnection(connStr);
    options.FilterWhere = "and is_delete=false";
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();