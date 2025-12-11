using System.Reflection;
using Azrng.EventBus.RabbitMQ;
using NewRabbitMQEventBusSample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 手动订阅
// builder.Services.AddRabbitMqEventBus((options) =>
//         {
//             options.SubscriptionClientName = "aaa";
//         },
//         Assembly.GetExecutingAssembly())
//     .AddSubscription<OrderIntegrationEvent, OrderIntegrationEventHandler>()
//     .AddSubscription<OrderIntegrationEvent, OrderIntegrationEventHandler2>();

// 自动订阅
builder.Services.AddRabbitMqEventBus(options =>
{
    options.HostName = "localhost";
    options.VirtualHost = "myQueue";
    options.UserName = "admin";
    options.Password = "123456";
    options.Port = 5672;
    options.ExchangeName = "direct_exchange";
    options.SubscriptionClientName = "bbbb";
})
.AddAutoSubscription(Assembly.GetExecutingAssembly());

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