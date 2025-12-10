
namespace Common.Core.Test.Helper.TaskHelperTest
{
    /// <summary>
    /// 运行时间限制
    /// </summary>
    public class RunTimeLimitTest
    {
        [Fact]
        public async void TestNotTimeout()
        {
            var result = await GetName(2000);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async void TestTimeout()
        {
            var result = await GetName(4000);
            Assert.Empty(result);
        }

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetName(int runTime)
        {
            try
            {
                return await TaskHelper.RunTimeLimitAsync(async () =>
                {
                    await Task.Delay(runTime);
                    return "success";
                }, TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }
    }
}