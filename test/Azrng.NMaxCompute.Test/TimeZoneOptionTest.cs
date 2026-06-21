using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// datetime / timestamp 时区开关（对应 PyODPS options.local_timezone）：
/// 默认本地时区，UseLocalTimeZone=false 时返回 UTC。timestamp_ntz 无时区语义，始终 UTC。
/// </summary>
public class TimeZoneOptionTest
{
    private static ProtobufWireReader ReaderFor(params byte[] bytes)
        => new(new MemoryStream(bytes));

    [Fact]
    public void DateTimeDecoder_UtcInstance_ReturnsUtc()
    {
        // 0 ms = epoch
        var value = (DateTime)DateTimeDecoder.UtcInstance.Read(ReaderFor(0x00), new Checksum())!;

        Assert.Equal(DateTimeKind.Utc, value.Kind);
        Assert.Equal(DateTimeDecoder.EpochUtc, value);
    }

    [Fact]
    public void DateTimeDecoder_LocalInstance_ReturnsLocal()
    {
        var value = (DateTime)DateTimeDecoder.LocalInstance.Read(ReaderFor(0x00), new Checksum())!;

        Assert.Equal(DateTimeKind.Local, value.Kind);
        Assert.Equal(DateTimeDecoder.EpochUtc.ToLocalTime(), value);
    }

    [Fact]
    public void DateTimeDecoder_Instance_AliasForLocal()
        => Assert.Same(DateTimeDecoder.LocalInstance, DateTimeDecoder.Instance);

    [Fact]
    public void Factory_SelectsDateTimeTimestamp_ByUseUtc()
    {
        Assert.Same(DateTimeDecoder.UtcInstance, TypeDecoderFactory.GetDecoder("datetime", useUtc: true));
        Assert.Same(DateTimeDecoder.LocalInstance, TypeDecoderFactory.GetDecoder("datetime", useUtc: false));
        Assert.Same(TimestampDecoder.UtcInstance, TypeDecoderFactory.GetDecoder("timestamp", useUtc: true));
        Assert.Same(TimestampDecoder.LocalInstance, TypeDecoderFactory.GetDecoder("timestamp", useUtc: false));
    }

    [Fact]
    public void Factory_TimestampNtz_AlwaysUtc()
    {
        // timestamp_ntz 无时区语义，不受 useUtc 影响，始终 UTC
        Assert.Same(TimestampDecoder.UtcInstance, TypeDecoderFactory.GetDecoder("timestamp_ntz", useUtc: false));
        Assert.Same(TimestampDecoder.UtcInstance, TypeDecoderFactory.GetDecoder("timestamp_ntz", useUtc: true));
    }

    [Fact]
    public void Factory_GetDecoder_DefaultsToLocal()
    {
        // 不传 useUtc 时默认本地（向后兼容）
        Assert.Same(DateTimeDecoder.LocalInstance, TypeDecoderFactory.GetDecoder("datetime"));
        Assert.Same(TimestampDecoder.LocalInstance, TypeDecoderFactory.GetDecoder("timestamp"));
    }

    /// <summary>嵌套场景：array&lt;datetime&gt; 的元素应随 useUtc 切换（验证透传到 TypeStringParser）。</summary>
    [Fact]
    public void NestedArrayOfDateTime_RespectsUseUtc()
    {
        // array<datetime> 1 元素，ms=0：[size=1(varint)][not-null=false(0)][ms=0(varint)]
        var bytes = new byte[] { 0x01, 0x00, 0x00 };

        var localArr = (System.Collections.IList)TypeDecoderFactory
            .GetDecoder("array<datetime>", useUtc: false).Read(ReaderFor(bytes), new Checksum())!;
        var utcArr = (System.Collections.IList)TypeDecoderFactory
            .GetDecoder("array<datetime>", useUtc: true).Read(ReaderFor(bytes), new Checksum())!;

        Assert.Equal(DateTimeKind.Local, ((DateTime)localArr[0]!).Kind);
        Assert.Equal(DateTimeKind.Utc, ((DateTime)utcArr[0]!).Kind);
    }

    [Fact]
    public void MaxComputeConfig_DefaultsToLocalTimeZone()
        => Assert.True(new MaxComputeConfig().UseLocalTimeZone);
}
