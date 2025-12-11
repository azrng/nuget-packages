using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace APIStudy.Test;

public class BaseControllerTest<TStartup> : IClassFixture<CustomWebApplicationFactory<TStartup>> where TStartup : class
{
    private readonly HttpClient _httpClient;

    public BaseControllerTest(CustomWebApplicationFactory<TStartup> factory)
    {
        _httpClient = factory.CreateClient();
    }
}

/// <summary>
/// 自定义的WebApplicationFactory
/// </summary>
/// <typeparam name="TStartup"></typeparam>
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            //var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TestDbContext>));
            //if (descriptor != null)
            //{
            //    services.Remove(descriptor);
            //}

            //services.AddDbContext<TestDbContext>(options =>
            //{
            //    options.UseInMemoryDatabase("InMemoryTestDb");
            //});

            //var sp = services.BuildServiceProvider();

            //using (var scope = sp.CreateScope())
            //{
            //    var scopedServices = scope.ServiceProvider;
            //    var db = scopedServices.GetRequiredService<TestDbContext>();
            //    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            //    db.Database.EnsureCreated();

            //    try
            //    {
            //        Console.WriteLine("Seeding...");

            //        var players = Utilities.GetTestPlayers(); // Returns 3 test players
            //        db.Players.RemoveRange(db.Players); // Clear the table
            //        db.Players.AddRange(players); // Add 3 test players
            //        db.SaveChanges();

            //        Console.WriteLine($"Players seeded: {db.Players.Count()}");
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError(ex, "An error occurred seeding the " + "database with test messages. Error: {Message}", ex.Message);
            //    }
            //}
        });
        base.ConfigureWebHost(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // ...
        return base.CreateHost(builder);
    }

    protected override TestServer CreateServer(IWebHostBuilder builder)
    {
        // ...
        return base.CreateServer(builder);
    }

    protected override void ConfigureClient(HttpClient client)
    {
        // ...
    }
}