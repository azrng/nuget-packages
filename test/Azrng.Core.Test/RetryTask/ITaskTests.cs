using System.Runtime.CompilerServices;
using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class ITaskTests
{
    [Fact]
    public void ITask_Should_Be_Interface()
    {
        typeof(ITask<>).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void ITask_Should_Be_Generic_Type_Definition()
    {
        typeof(ITask<>).IsGenericTypeDefinition.Should().BeTrue();
    }

    [Fact]
    public void ITask_Should_Have_One_Generic_Parameter()
    {
        typeof(ITask<>).GetGenericArguments().Should().HaveCount(1);
    }

    [Fact]
    public void ITask_Should_Be_In_RetryTask_Namespace()
    {
        typeof(ITask<>).Namespace.Should().Be("Azrng.Core.RetryTask");
    }

    [Fact]
    public void GetAwaiter_Should_Exist()
    {
        var method = typeof(ITask<>).GetMethod("GetAwaiter");

        method.Should().NotBeNull();
    }

    [Fact]
    public void GetAwaiter_Should_Return_TaskAwaiter()
    {
        var method = typeof(ITask<>).GetMethod("GetAwaiter");

        method!.ReturnType.IsGenericType.Should().BeTrue();
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(TaskAwaiter<>));
    }

    [Fact]
    public void GetAwaiter_Should_Have_No_Parameters()
    {
        var method = typeof(ITask<>).GetMethod("GetAwaiter");

        method!.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void ConfigureAwait_Should_Exist()
    {
        var method = typeof(ITask<>).GetMethod("ConfigureAwait");

        method.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureAwait_Should_Return_ConfiguredTaskAwaitable()
    {
        var method = typeof(ITask<>).GetMethod("ConfigureAwait");

        method!.ReturnType.IsGenericType.Should().BeTrue();
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(ConfiguredTaskAwaitable<>));
    }

    [Fact]
    public void ConfigureAwait_Should_Have_One_Bool_Parameter()
    {
        var method = typeof(ITask<>).GetMethod("ConfigureAwait");

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(bool));
    }

    [Fact]
    public void ITask_Should_Have_Only_Two_Methods()
    {
        var methods = typeof(ITask<>).GetMethods();

        methods.Should().HaveCount(2);
    }

    [Fact]
    public void ITask_Should_Have_All_Expected_Methods()
    {
        var methods = typeof(ITask<>).GetMethods();
        var methodNames = methods.Select(m => m.Name).Distinct().ToList();

        methodNames.Should().Contain("GetAwaiter");
        methodNames.Should().Contain("ConfigureAwait");
    }
}
