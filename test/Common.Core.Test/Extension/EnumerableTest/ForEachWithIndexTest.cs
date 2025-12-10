
namespace Common.Core.Test.Extension.EnumerableTest;

public class ForEachWithIndexTest
{
    [Fact]
    public void ListForEachWithIndexReturnOk()
    {
        var list = new List<string> { "a", "b", "c" };
        foreach (var (item, index) in list.WithIndex())
        {
            Assert.NotEmpty(item);
            Assert.True(index >= 0);
        }
    }
}