using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// DATETIME：zigzag varint 毫秒时间戳 + crc.UpdateLong。
/// <para>对应 PyODPS <c>_read_field</c> 中 <c>types.datetime</c> 分支，
/// 由 <c>utils.MillisecondsConverter.from_milliseconds</c> 转换。</para>
/// <para>
/// 转换语义：以 UTC epoch 1970-01-01 为基准的毫秒数。
/// 默认按本地时区解释（与 PyODPS <c>local_timezone=True</c> 默认行为一致）。
/// </para>
/// </summary>
public sealed class DateTimeDecoder : ITypeDecoder
{
    public static readonly DateTimeDecoder Instance = new();

    /// <summary>
    /// Unix epoch UTC。
    /// </summary>
    public static readonly DateTime EpochUtc = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var ms = reader.ReadSInt64();
        checksum.UpdateLong(ms);
        return FromMilliseconds(ms);
    }

    /// <summary>
    /// 毫秒时间戳转 DateTime（本地时区）。
    /// </summary>
    public static DateTime FromMilliseconds(long ms)
    {
        // PyODPS 默认 local_timezone=True：fromtimestamp 返回本地时区时间。
        // .NET DateTimeOffset.FromUnixTimeMilliseconds 返回 UTC，
        // 再 ToLocalTime 转本地。
        return EpochUtc.AddMilliseconds(ms).ToLocalTime();
    }
}

/// <summary>
/// DATE：zigzag varint 距 1970-01-01 的天数 + crc.UpdateLong。
/// <para>对应 PyODPS <c>_read_field</c> 中 <c>types.date</c> 分支。</para>
/// </summary>
public sealed class DateDecoder : ITypeDecoder
{
    public static readonly DateDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var days = reader.ReadSInt64();
        checksum.UpdateLong(days);
        return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(days * 86400_000L).UtcDateTime);
    }
}
