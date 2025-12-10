using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

/// <summary>
/// 随机数组选择器测试
/// </summary>
public class RandomArraySelectorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RandomArraySelectorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void IntArray_ReturnOk()
    {
        var selector = new RandomArraySelector<int>(new[]
                                                       {
                                                           9,10,2,4,5,6,7,8,1
                                                       });
        for (var i = 0; i < 10; i++)
        {
            _testOutputHelper.WriteLine(selector.GetNext().ToString());
        }
    }

    [Fact]
    public void StringArray_ReturnOk()
    {
        // 使用示例
        var selector = new RandomArraySelector<string>(new[]
                                                       {
                                                           "Red",
                                                           "Green",
                                                           "Blue"
                                                       });
        for (var i = 0; i < 10; i++)
        {
            _testOutputHelper.WriteLine(selector.GetNext().ToString());
        }
    }
}