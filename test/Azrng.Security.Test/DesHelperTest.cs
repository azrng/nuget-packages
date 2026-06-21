using Xunit.Abstractions;

namespace Azrng.Security.Test;

public class DesHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DesHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private const string SecretKey = "csNH9xjQ";

    [Fact]
    public void Encrypt_ReturnOk()
    {
        var secretValue = "eLpviviwgNhKCzRVZvtLBA==";
        var str = "12345678";
        var result = DesHelper.Encrypt(str, SecretKey);
        _testOutputHelper.WriteLine($"输出结果 ：{result}");
        Assert.Equal(secretValue, result);
    }

    [Fact]
    public void Descrypt_ReturnOk()
    {
        var secretValue = "eLpviviwgNhKCzRVZvtLBA==";
        var str = "12345678";
        var result = DesHelper.Decrypt(secretValue, SecretKey);
        _testOutputHelper.WriteLine($"输出结果 ：{result}");
        Assert.Equal(str, result);
    }

    [Fact]
    public void Encrypt_NullPlaintext_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => DesHelper.Encrypt(null!, SecretKey));
    }
}