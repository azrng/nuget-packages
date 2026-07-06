using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// RawFunction 测试（移植自上游 RawFunction）。
/// 注意：上游解析器不产出此节点，仅用于 API 对齐与手动构造。
/// </summary>
public class RawFunctionTest
{
    [Fact]
    public void RawFunction_WithArguments_ShouldRenderRawText()
    {
        var func = new RawFunction("MY_SPECIAL_FUNC", "1, 2, 3 @ ARRAY");
        Assert.Equal("MY_SPECIAL_FUNC(1, 2, 3 @ ARRAY)", func.ToString());
    }

    [Fact]
    public void RawFunction_WithNullArguments_ShouldRenderEmptyParens()
    {
        var func = new RawFunction { Name = "F", RawArguments = null };
        Assert.Equal("F()", func.ToString());
    }

    [Fact]
    public void RawFunction_DefaultConstructor_ShouldRenderEmptyName()
    {
        var func = new RawFunction();
        Assert.Equal("()", func.ToString());
    }

    [Fact]
    public void RawFunction_IsAFunction_Subclassable()
    {
        Function func = new RawFunction("X", "y");
        Assert.Equal("X(y)", func.ToString());
        Assert.IsType<RawFunction>(func);
    }
}
