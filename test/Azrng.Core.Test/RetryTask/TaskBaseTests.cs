using Azrng.Core.RetryTask;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.RetryTask;

public class TaskBaseTests
{
    private class TestableTask<TResult> : TaskBase<TResult>
    {
        private readonly Task<TResult> _task;

        public TestableTask(Task<TResult> task) => _task = task;

        protected override Task<TResult> InvokeAsync() => _task;
    }

    [Fact]
    public async Task GetAwaiter_ShouldReturnAwaiter_ThatResolvesWithCorrectValue()
    {
        var expected = 42;
        var task = new TestableTask<int>(Task.FromResult(expected));

        var result = await task;

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetAwaiter_ShouldPropagateException()
    {
        var task = new TestableTask<int>(Task.FromException<int>(new InvalidOperationException("test")));

        Func<Task> act = async () => await task;

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test");
    }

    [Fact]
    public async Task ConfigureAwait_False_ShouldReturnConfiguredAwaitable()
    {
        var expected = "hello";
        var task = new TestableTask<string>(Task.FromResult(expected));

        var result = await task.ConfigureAwait(false);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task ConfigureAwait_True_ShouldReturnConfiguredAwaitable()
    {
        var expected = "world";
        var task = new TestableTask<string>(Task.FromResult(expected));

        var result = await task.ConfigureAwait(true);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task ConfigureAwait_ShouldPropagateException()
    {
        var task = new TestableTask<int>(Task.FromException<int>(new ArgumentException("bad")));

        Func<Task> act = async () => await task.ConfigureAwait(false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("bad");
    }

    [Fact]
    public async Task GetAwaiter_WithComplexType_ShouldWork()
    {
        var expected = new List<int> { 1, 2, 3 };
        var task = new TestableTask<List<int>>(Task.FromResult(expected));

        var result = await task;

        result.Should().BeEquivalentTo(expected);
    }
}
