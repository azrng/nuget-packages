using Azrng.AspNetCore.Inject;
using Azrng.AspNetCore.Inject.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azrng.AspNetCore.Inject.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddModule_WithInvalidStartupModule_ThrowsTypeLoadException()
    {
        var services = CreateServices();

        var act = () => services.AddModule(typeof(NotAModule));

        act.Should().Throw<TypeLoadException>()
            .WithMessage("*不是有效的模块类*");
    }

    [Fact]
    public void AddModule_WithCircularDependency_ThrowsOverflowException()
    {
        var services = CreateServices();

        var act = () => services.AddModule<CircularRootModule>();

        act.Should().Throw<OverflowException>()
            .WithMessage("*循环依赖*");
    }

    [Fact]
    public void AddModule_RegistersModulesAndInvokesConfigureServicesInDependencyOrder()
    {
        ModuleExecutionRecorder.Reset();
        var services = CreateServices(("Mode", "Test"));

        services.AddModule<RootModule>();

        services.Should().ContainSingle(x =>
            x.ServiceType == typeof(RootModule) &&
            x.ImplementationType == typeof(RootModule) &&
            x.Lifetime == ServiceLifetime.Transient);
        services.Should().ContainSingle(x =>
            x.ServiceType == typeof(ChildModule) &&
            x.ImplementationType == typeof(ChildModule) &&
            x.Lifetime == ServiceLifetime.Transient);

        ModuleExecutionRecorder.ExecutionOrder.Should().Equal("child", "root");
        ModuleExecutionRecorder.ObservedModes.Should().Equal("Test", "Test");

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IRootMarker>().Should().BeOfType<RootMarker>();
        provider.GetRequiredService<IChildMarker>().Should().BeOfType<ChildMarker>();
    }

    [Fact]
    public void AddModule_AutoRegistersAttributedServicesWithoutDuplicateRegistrations()
    {
        var services = CreateServices();

        services.AddModule<RootModule>();

        services.Count(x => x.ServiceType == typeof(SelfRegisteredService)).Should().Be(1);
        services.Count(x => x.ServiceType == typeof(IInterfaceContract)).Should().Be(1);
        services.Count(x => x.ServiceType == typeof(BaseRegisteredServiceBase)).Should().Be(1);
        services.Count(x => x.ServiceType == typeof(IExplicitContract)).Should().Be(1);
        services.Count(x => x.ServiceType == typeof(ExplicitRegisteredServiceBase)).Should().Be(1);
        services.Count(x => x.ServiceType == typeof(ExplicitRegisteredService)).Should().Be(1);

        using var provider = services.BuildServiceProvider();
        var self1 = provider.GetRequiredService<SelfRegisteredService>();
        var self2 = provider.GetRequiredService<SelfRegisteredService>();
        self1.Should().BeSameAs(self2);

        provider.GetRequiredService<IInterfaceContract>()
            .Should().BeOfType<InterfaceRegisteredService>();

        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<BaseRegisteredServiceBase>()
            .Should().BeOfType<BaseRegisteredService>();

        provider.GetRequiredService<IExplicitContract>()
            .Should().BeOfType<ExplicitRegisteredService>();
        provider.GetRequiredService<ExplicitRegisteredServiceBase>()
            .Should().BeOfType<ExplicitRegisteredService>();
        provider.GetRequiredService<ExplicitRegisteredService>()
            .Should().BeOfType<ExplicitRegisteredService>();
    }

    private static ServiceCollection CreateServices(params (string Key, string Value)[] settings)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        return services;
    }
}

public sealed class NotAModule;

[InjectModule(typeof(ChildModule))]
public class RootModule : IModule
{
    public void ConfigureServices(ServiceContext services)
    {
        ModuleExecutionRecorder.Record("root", services.Configuration["Mode"]);
        services.Services.AddSingleton<IRootMarker, RootMarker>();
    }
}

public class ChildModule : IModule
{
    public void ConfigureServices(ServiceContext services)
    {
        ModuleExecutionRecorder.Record("child", services.Configuration["Mode"]);
        services.Services.AddSingleton<IChildMarker, ChildMarker>();
    }
}

[InjectModule(typeof(CircularChildModule))]
public class CircularRootModule : IModule
{
    public void ConfigureServices(ServiceContext services)
    {
    }
}

[InjectModule(typeof(CircularRootModule))]
public class CircularChildModule : IModule
{
    public void ConfigureServices(ServiceContext services)
    {
    }
}

public interface IRootMarker;

public sealed class RootMarker : IRootMarker;

public interface IChildMarker;

public sealed class ChildMarker : IChildMarker;

[InjectOn(ServiceLifetime.Singleton, InjectScheme.None, Own = true)]
public class SelfRegisteredService;

public interface IInterfaceContract;

[InjectOn(ServiceLifetime.Singleton, InjectScheme.OnlyInterfaces)]
public class InterfaceRegisteredService : IInterfaceContract;

public abstract class BaseRegisteredServiceBase;

[InjectOn(ServiceLifetime.Scoped, InjectScheme.OnlyBaseClass)]
public class BaseRegisteredService : BaseRegisteredServiceBase;

public interface IExplicitContract;

public abstract class ExplicitRegisteredServiceBase;

[InjectOn(ServiceLifetime.Singleton, InjectScheme.Some, Own = true, ServicesType = new[] { typeof(IExplicitContract), typeof(ExplicitRegisteredServiceBase) })]
public class ExplicitRegisteredService : ExplicitRegisteredServiceBase, IExplicitContract;

internal static class ModuleExecutionRecorder
{
    private static readonly List<string> _executionOrder = [];
    private static readonly List<string?> _observedModes = [];

    public static IReadOnlyList<string> ExecutionOrder => _executionOrder;

    public static IReadOnlyList<string?> ObservedModes => _observedModes;

    public static void Reset()
    {
        _executionOrder.Clear();
        _observedModes.Clear();
    }

    public static void Record(string moduleName, string? mode)
    {
        _executionOrder.Add(moduleName);
        _observedModes.Add(mode);
    }
}
