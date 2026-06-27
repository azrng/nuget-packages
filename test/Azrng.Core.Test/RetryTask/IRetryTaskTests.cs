using System.Reflection;
using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class IRetryTaskTests
{
    [Fact]
    public void IRetryTask_Should_Be_Interface()
    {
        typeof(IRetryTask<>).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IRetryTask_Should_Be_Generic_Type_Definition()
    {
        typeof(IRetryTask<>).IsGenericTypeDefinition.Should().BeTrue();
    }

    [Fact]
    public void IRetryTask_Should_Have_One_Generic_Parameter()
    {
        typeof(IRetryTask<>).GetGenericArguments().Should().HaveCount(1);
    }

    [Fact]
    public void IRetryTask_Should_Inherit_From_ITask()
    {
        var interfaces = typeof(IRetryTask<>).GetInterfaces();
        interfaces.Should().Contain(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITask<>));
    }

    [Fact]
    public void IRetryTask_Should_Be_In_RetryTask_Namespace()
    {
        typeof(IRetryTask<>).Namespace.Should().Be("Azrng.Core.RetryTask");
    }

    [Fact]
    public void WhenCatch_Generic_Should_Exist()
    {
        var method = typeof(IRetryTask<>).GetMethod("WhenCatch",
            genericParameterCount: 1,
            Type.EmptyTypes);

        method.Should().NotBeNull();
    }

    [Fact]
    public void WhenCatch_Generic_Should_Return_IRetryTask()
    {
        var method = typeof(IRetryTask<>).GetMethod("WhenCatch",
            genericParameterCount: 1,
            Type.EmptyTypes);

        method!.ReturnType.IsGenericType.Should().BeTrue();
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(IRetryTask<>));
    }

    [Fact]
    public void WhenCatch_Generic_Should_Have_Exception_Constraint()
    {
        var method = typeof(IRetryTask<>).GetMethod("WhenCatch",
            genericParameterCount: 1,
            Type.EmptyTypes);

        var constraints = method!.GetGenericArguments()[0].GetGenericParameterConstraints();
        constraints.Should().Contain(typeof(Exception));
    }

    [Fact]
    public void WhenCatch_With_Action_Handler_Should_Exist()
    {
        var methods = typeof(IRetryTask<>).GetMethods()
            .Where(m => m.Name == "WhenCatch" && m.GetGenericArguments().Length == 1);

        methods.Should().Contain(m =>
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType.IsGenericType &&
            m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));
    }

    [Fact]
    public void WhenCatch_With_Func_Predicate_Should_Exist()
    {
        var methods = typeof(IRetryTask<>).GetMethods()
            .Where(m => m.Name == "WhenCatch" && m.GetGenericArguments().Length == 1);

        methods.Should().Contain(m =>
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType.IsGenericType &&
            m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));
    }

    [Fact]
    public void WhenCatchAsync_With_Func_Task_Handler_Should_Exist()
    {
        var methods = typeof(IRetryTask<>).GetMethods()
            .Where(m => m.Name == "WhenCatchAsync" && m.GetGenericArguments().Length == 1);

        methods.Should().Contain(m =>
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType.IsGenericType &&
            m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));
    }

    [Fact]
    public void WhenCatchAsync_With_Func_Task_Bool_Predicate_Should_Exist()
    {
        var methods = typeof(IRetryTask<>).GetMethods()
            .Where(m => m.Name == "WhenCatchAsync" && m.GetGenericArguments().Length == 1);

        methods.Should().Contain(m =>
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType.IsGenericType &&
            m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));
    }

    [Fact]
    public void WhenResult_Should_Exist()
    {
        var methods = typeof(IRetryTask<>).GetMethods()
            .Where(m => m.Name == "WhenResult");

        methods.Should().Contain(m =>
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType.IsGenericType &&
            m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));
    }

    [Fact]
    public void WhenResult_Should_Return_IRetryTask()
    {
        var methods = typeof(IRetryTask<>).GetMethods()
            .Where(m => m.Name == "WhenResult");

        var method = methods.First(m =>
            m.GetParameters().Length == 1 &&
            !m.GetParameters()[0].ParameterType.GetGenericArguments().Last().IsGenericType);

        method.ReturnType.IsGenericType.Should().BeTrue();
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(IRetryTask<>));
    }

    [Fact]
    public void WhenResultAsync_Should_Exist()
    {
        var method = typeof(IRetryTask<>).GetMethod("WhenResultAsync");

        method.Should().NotBeNull();
    }

    [Fact]
    public void WhenResultAsync_Should_Return_IRetryTask()
    {
        var method = typeof(IRetryTask<>).GetMethod("WhenResultAsync");

        method!.ReturnType.IsGenericType.Should().BeTrue();
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(IRetryTask<>));
    }

    [Fact]
    public void IRetryTask_Should_Have_All_Expected_Methods()
    {
        var methods = typeof(IRetryTask<>).GetMethods();
        var methodNames = methods.Select(m => m.Name).Distinct().ToList();

        methodNames.Should().Contain("WhenCatch");
        methodNames.Should().Contain("WhenCatchAsync");
        methodNames.Should().Contain("WhenResult");
        methodNames.Should().Contain("WhenResultAsync");
    }

    [Fact]
    public void All_Methods_Should_Return_IRetryTask()
    {
        var methods = typeof(IRetryTask<>).GetMethods();

        methods.Should().AllSatisfy(m =>
        {
            m.ReturnType.IsGenericType.Should().BeTrue();
            m.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(IRetryTask<>));
        });
    }
}
