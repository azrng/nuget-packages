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
            var exception = Assert.Throws<Exception>(() => TaskHelper.ExecuteActionWithRetry(() =>
            {
                _testOutputHelper.WriteLine("Executing action...");
                throw new Exception("Something went wrong.");
            }, 3));

            Assert.Equal("Something went wrong.", exception.Message);
        }

        [Fact]
        public void ExecuteFuncWithRetry_Test()
        {
            var exception = Assert.Throws<Exception>(() => TaskHelper.ExecuteFuncWithRetry<string>(() =>
            {
                _testOutputHelper.WriteLine("Executing func...");
                throw new Exception("Something went wrong.");
            }, 3));

            Assert.Equal("Something went wrong.", exception.Message);
        }

        [Fact]
        public async Task ExecuteFuncWithRetryAsync_Test()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => TaskHelper.ExecuteFuncWithRetryAsync<string>(async () =>
            {
                await Task.Delay(1000);
                _testOutputHelper.WriteLine("Executing func...");
                throw new Exception("Something went wrong.");
            }, 3));

            Assert.Equal("Something went wrong.", exception.Message);
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