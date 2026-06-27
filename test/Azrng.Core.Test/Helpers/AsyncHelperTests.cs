using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class AsyncHelperTests
{
    [Fact]
    public void RunSync_WithGenericResult_ShouldReturnValue()
    {
        var result = AsyncHelper.RunSync(async () =>
        {
            await Task.Yield();
            return 42;
        });

        result.Should().Be(42);
    }

    [Fact]
    public void RunSync_WithGenericResult_ShouldReturnString()
    {
        var result = AsyncHelper.RunSync(async () =>
        {
            await Task.Yield();
            return "hello";
        });

        result.Should().Be("hello");
    }

    [Fact]
    public void RunSync_WithGenericResult_ShouldPropagateException()
    {
        Action act = () => AsyncHelper.RunSync<int>(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("test error");
        });

        act.Should().Throw<InvalidOperationException>().WithMessage("test error");
    }

    [Fact]
    public void RunSync_Void_ShouldComplete()
    {
        var executed = false;

        AsyncHelper.RunSync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeTrue();
    }

    [Fact]
    public void RunSync_Void_ShouldPropagateException()
    {
        Action act = () => AsyncHelper.RunSync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("test error");
        });

        act.Should().Throw<InvalidOperationException>().WithMessage("test error");
    }

    [Fact]
    public void RunSync_WithGenericResult_ShouldHandleMultipleAwaits()
    {
        var result = AsyncHelper.RunSync(async () =>
        {
            var a = await Task.FromResult(10);
            var b = await Task.FromResult(20);
            return a + b;
        });

        result.Should().Be(30);
    }
}
