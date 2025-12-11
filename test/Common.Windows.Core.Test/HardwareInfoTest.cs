using Xunit.Abstractions;

namespace Common.Windows.Core.Test;

public class HardwareInfoTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public HardwareInfoTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 获取机器名
    /// </summary>
    [Fact]
    public void HostName_Ok()
    {
        var result = HardwareInfo.GetHostName();
        _testOutputHelper.WriteLine(result);
        Assert.NotNull(result);
        Assert.Equal("Azrng", result);
    }

    /// <summary>
    /// 获取CPU ID
    /// </summary>
    [Fact]
    public void CpuId_Ok()
    {
        var result = HardwareInfo.GetCpuId();
        _testOutputHelper.WriteLine(result);
        Assert.NotNull(result);
        Assert.Equal("178BFBFF00860F01", result);
    }

    /// <summary>
    /// 获取HardDiskId
    /// </summary>
    [Fact]
    public void HardDiskId_Ok()
    {
        var result = HardwareInfo.GetMainDiskId();
        _testOutputHelper.WriteLine(result);
        Assert.NotNull(result);
        Assert.Equal("0025_388C_91E5_259B.", result);
    }

    /// <summary>
    /// 获取MacAddress
    /// </summary>
    [Fact]
    public void MacAddress_Ok()
    {
        var result = HardwareInfo.GetMacAddress();
        _testOutputHelper.WriteLine(result);
        Assert.NotNull(result);
        Assert.Equal("14:F6:D8:9F:44:30", result);
    }

    /// <summary>
    /// 获取BiosSerial
    /// </summary>
    [Fact]
    public void BiosSerial_Ok()
    {
        var result = HardwareInfo.GetBiosSerial();
        _testOutputHelper.WriteLine(result);
        Assert.NotNull(result);
        Assert.Equal("PF29EG2S", result);
    }

    [Fact]
    public void GenerateFingerprint_Ok()
    {
        var fingerprintOrigin = HardwareInfo.GenerateFingerprint(HardwareInfo.GetCpuId(), HardwareInfo.GetBiosSerial(),
           HardwareInfo.GetMainDiskId(), HardwareInfo.GetMacAddress());

        var result = HardwareInfo.GenerateFingerprint();
        _testOutputHelper.WriteLine(result);
        Assert.NotNull(result);
        Assert.Equal(fingerprintOrigin, result);
    }
}