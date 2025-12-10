using Azrng.Core.Results;
using Xunit.Abstractions;

namespace Common.Core.Test.Helper.TaskHelperTest
{
    public class TryWaitAsyncTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TryWaitAsyncTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task TestNotTimeout()
        {
            var result = await GetName(2000, true);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task TestTimeout()
        {
            var result = await GetName(4000, true);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ExecuteActionWithRetry_Test()
        {
            TaskHelper.ExecuteActionWithRetry(() =>
            {
                // 这里是你想要执行的代码
                _testOutputHelper.WriteLine("Executing action...");

                // 模拟失败情况
                throw new Exception("Something went wrong.");
            }, 3);
        }

        [Fact]
        public void ExecuteFuncWithRetry_Test()
        {
            // 使用Func<T>示例
            var result = TaskHelper.ExecuteFuncWithRetry(() =>
            {
                // 这里是你想要执行的代码
                _testOutputHelper.WriteLine("Executing func...");

                // 模拟失败情况
                throw new Exception("Something went wrong.");
                return "Success"; // 正常情况下返回结果
            }, 3);
            _testOutputHelper.WriteLine(result); // 输出结果，可能是默认值
        }

        [Fact]
        public async Task ExecuteFuncWithRetryAsync_Test()
        {
            // 使用Func<T>示例
            var result = await TaskHelper.ExecuteFuncWithRetryAsync(async () =>
            {
                await Task.Delay(1000);

                // 这里是你想要执行的代码
                _testOutputHelper.WriteLine("Executing func...");

                // 模拟失败情况
                throw new Exception("Something went wrong.");
                return "Success"; // 正常情况下返回结果
            }, 3);
            _testOutputHelper.WriteLine(result); // 输出结果，可能是默认值
        }

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        private async Task<IResultModel<string>> GetName(int runTime, bool isSuccess)
        {
            return await TaskHelper.TryWaitAsync(async () =>
            {
                await Task.Delay(runTime);
                return isSuccess ? ResultModel<string>.Success("aa") : ResultModel<string>.Error("bb");
            }, TimeSpan.FromSeconds(3));
        }
    }
}