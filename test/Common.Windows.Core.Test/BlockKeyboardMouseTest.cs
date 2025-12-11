namespace Common.Windows.Core.Test;

public class BlockKeyboardMouseTest
{
    [Fact]
    public void Test1()
    {
        BlockKeyboardMouseHelper.Off();
        Thread.Sleep(5000);
        BlockKeyboardMouseHelper.On();
    }
}