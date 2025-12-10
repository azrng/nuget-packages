using Azrng.Core.Exceptions;

namespace Common.Core.Test.Exceptions;

/// <summary>
/// 基础自定义异常测试
/// </summary>
public class BaseCustomerExceptionTest
{
    [Fact]
    public void BaseCustomerException_Equal_ReturnOk()
    {
        //arrange
        var originException = new BaseException("404", "实体未找到");

        // act
        Action actual = () => throw originException;

        // assert
        Assert.Throws<BaseException>(actual);
    }
}
