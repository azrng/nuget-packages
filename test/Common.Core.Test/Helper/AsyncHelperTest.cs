using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

public class AsyncHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AsyncHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestExceptionThrown()
    {
        try
        {
            var flag = AsyncHelper.RunSync(() => ThrowInfo());
            _testOutputHelper.WriteLine($"flag状态:{flag}");
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine($"异常信息：{e.Message}");
        }
    }

    private async Task<bool> ThrowInfo()
    {
        await Task.Delay(100);
        throw new AggregateException("信息不能为空");
        return false;
    }
}