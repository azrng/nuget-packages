using CommonCollect.Extension;

namespace CommonCollect.Test.Extensions;

public class AutoMapperExtensionsTest
{
    /// <summary>
    /// 简单对象映射测试
    /// </summary>
    [Fact]
    public void ObjectMap_ReturnOk()
    {
        // 准备
        var sourceObj = new AutoMapperUserInfo() { Name = "张三", Sex = 1, Birthday = DateTime.Now.AddDays(1) };

        //  行为
        var resultObj = sourceObj.MapTo<AutoMapperUserInfoCopy>();

        // 断言
        Assert.Equal(sourceObj.Name, resultObj.Name);
    }
}

public class AutoMapperUserInfo
{
    public string Name { get; set; }

    public int Sex { get; set; }

    public DateTime Birthday { get; set; }
}

public class AutoMapperUserInfoCopy
{
    public string Name { get; set; }

    public int Sex { get; set; }

    public DateTime Birthday { get; set; }
}