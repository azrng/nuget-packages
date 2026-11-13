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
    options.VirtualHost = "/"; // 默认 vhost；guest 账号只允许本机连接默认 vhost
    options.UserName = "guest";
    options.Password = "guest";
    options.Port = 5672; // 本机 5672 端口被占用时可映射其他端口（如 docker -p 5872:5672 后改为 5872）
    options.ExchangeName = "direct_exchange";
    // 必填：本服务的消费队列名，每个服务必须唯一（共用同名队列会互相抢消息）
    options.SubscriptionClientName = "rabbitmq_sample_queue";
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