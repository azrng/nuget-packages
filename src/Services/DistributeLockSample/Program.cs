using Azrng.DistributeLock.Core;
using Azrng.DistributeLock.Redis;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var redisConn = "127.0.0.1:6379,defaultDatabase=1,connectTimeout=100000,syncTimeout=100000,connectRetry=50";
redisConn = "localhost:8011,defaultDatabase=0,connectTimeout=100000,syncTimeout=100000,connectRetry=50";
builder.Services.AddRedisLockProvider(redisConn);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 等待超时取消
app.MapGet("/timeout", async (ILockProvider lockProvider,ILogger<Program> logger) =>
   {
       var count = 5;
       await Task.Delay(2 * 1000);
       var lockKey = "lockKey";

       await Parallel.ForAsync(0, 3, async (i, _) =>
       {
           await using var result = await lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(3));
           if (result is null)
           {
               logger.LogInformation($"{i} 获取锁失败");
               logger.LogInformation("一直获取不到锁结束");
               return;
           }

           logger.LogInformation($"{i} 获取锁成功");

           if (count <= 0)
           {
               logger.LogInformation($"库存不足，{i}失败");
               return;
           }

           await Task.Delay(TimeSpan.FromSeconds(new Random().Next(3, 5)));
           count--;
           logger.LogInformation($"{i} 购买成功，剩余{count}");
       });

       return "succ";
   })
   .WithName("timeout")
   .WithOpenApi();

app.MapGet("/multiTask", async (ILockProvider lockProvider) =>
   {
       var count = 5;
       await Task.Delay(2 * 1000);
       var lockKey = "lockKey";

       await Parallel.ForAsync(0, 10, async (i, _) =>
       {
           await using var result = await lockProvider.LockAsync(lockKey);
           if (result is null)
           {
               Console.WriteLine($"{i} 获取锁失败");
               return;
           }

           Console.WriteLine($"{i} 获取锁成功");

           if (count <= 0)
           {
               Console.WriteLine($"库存不足，{i}失败");
               return;
           }

           await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)));
           count--;
           Console.WriteLine($"{i} 购买成功，剩余{count}");
       });

       return "succ";
   })
   .WithName("multiTask")
   .WithOpenApi();

app.Run();