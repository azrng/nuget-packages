using System;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class ExceptionExtensionTests
{
    #region GetExceptionAndStack

    [Fact]
    public void GetExceptionAndStack_WithSimpleException_ShouldContainMessage()
    {
        var ex = new InvalidOperationException("测试异常信息");

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("测试异常信息");
    }

    [Fact]
    public void GetExceptionAndStack_WithSimpleException_ShouldContainStackTraceLabel()
    {
        var ex = new InvalidOperationException("test");

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("stackTrace：");
    }

    [Fact]
    public void GetExceptionAndStack_WithSimpleException_ShouldContainInnerExceptionLabel()
    {
        var ex = new InvalidOperationException("test");

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("innerException：");
    }

    [Fact]
    public void GetExceptionAndStack_WithInnerException_ShouldContainInnerExceptionType()
    {
        var inner = new ArgumentException("内部异常");
        var ex = new InvalidOperationException("外部异常", inner);

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("内部异常");
        result.Should().Contain("ArgumentException");
    }

    [Fact]
    public void GetExceptionAndStack_WithNoInnerException_ShouldContainNull()
    {
        var ex = new InvalidOperationException("test");

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("innerException：");
    }

    [Fact]
    public void GetExceptionAndStack_ShouldContainMessageLabel()
    {
        var ex = new InvalidOperationException("test");

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("message：");
    }

    [Fact]
    public void GetExceptionAndStack_WithStackUnwinding_ShouldContainStackInfo()
    {
        Exception ex;
        try
        {
            throw new InvalidOperationException("通过throw生成堆栈");
        }
        catch (Exception caught)
        {
            ex = caught;
        }

        var result = ex.GetExceptionAndStack();

        result.Should().Contain("通过throw生成堆栈");
        result.Should().Contain("stackTrace：");
    }

    #endregion
}
