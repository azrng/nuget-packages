using Azrng.Core.Helpers;
using Azrng.EFCore;
using Azrng.EFCore.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Common.EFCore.PostgreSql.Test;

public class IdGenerationTests
{
    [Fact]
    public void IdentityBaseEntity_ShouldGenerateParsableUniqueIds()
    {
        var first = new TestEntity("first", "first");
        var second = new TestEntity("second", "second");

        Assert.True(first.Id > 0);
        Assert.True(second.Id > 0);
        Assert.NotEqual(first.Id, second.Id);

        Assert.True(Snowflake.TryParse(first.Id, out _, out var workerId, out _));
        Assert.Equal(Snowflake.WorkerId, workerId);
    }

    [Fact]
    public void AddEntityFramework_ShouldConfigureSnowflakeWorkerId()
    {
        var workerId = Snowflake.WorkerId;
        var services = new ServiceCollection();

        services.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = "Host=localhost;Username=test;Password=test;Database=test";
            options.WorkId = workerId;
        });

        Assert.Equal(workerId, Snowflake.WorkerId);
    }

    [Fact]
    public void AddEntityFrameworkFactory_ShouldConfigureSnowflakeWorkerId()
    {
        var workerId = Snowflake.WorkerId;
        var services = new ServiceCollection();

        services.AddEntityFrameworkFactory<TestDbContext>(options =>
        {
            options.ConnectionString = "Host=localhost;Username=test;Password=test;Database=test";
            options.WorkId = workerId;
        });

        Assert.Equal(workerId, Snowflake.WorkerId);
    }

    [Fact]
    public void AddIdHelper_ShouldKeepRegistrationApiAndConfigureSnowflake()
    {
        var workerId = Snowflake.WorkerId;
        var services = new ServiceCollection();

        var result = services.AddIdHelper(workerId);

        Assert.Same(services, result);
        Assert.Equal(workerId, Snowflake.WorkerId);
    }

    [Fact]
    public void EntityOperatorBaseTypes_ShouldGenerateParsableUniqueIds()
    {
        var operatorEntity = new TestOperatorEntity();
        var statusEntity = new TestStatusEntity();

        Assert.NotEqual(operatorEntity.Id, statusEntity.Id);
        Assert.True(Snowflake.TryParse(operatorEntity.Id, out _, out _, out _));
        Assert.True(Snowflake.TryParse(statusEntity.Id, out _, out _, out _));
    }

    [Fact]
    public void AddEntityFramework_ShouldRejectInvalidWorkerId()
    {
        var services = new ServiceCollection();

        var action = () => services.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = "Host=localhost;Username=test;Password=test;Database=test";
            options.WorkId = 1024;
        });

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void EfCoreConnectOption_ShouldReuseDefaultWorkerIdInSameProcess()
    {
        var first = new Azrng.EFCore.EfCoreConnectOption();
        var second = new Azrng.EFCore.EfCoreConnectOption();

        Assert.Equal(first.WorkId, second.WorkId);
    }
}

file sealed class TestOperatorEntity : IdentityOperatorEntity
{
}

file sealed class TestStatusEntity : IdentityOperatorStatusEntity
{
}
