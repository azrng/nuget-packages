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

/// <summary>
/// INTERVAL_DAY_TIME：sint64 秒 + sint32 纳秒，分别计入 CRC。
/// <para>对应 PyODPS <c>_read_field</c> 中 <c>types.interval_day_time</c> 分支
/// （<c>pd.Timedelta(seconds=l_val, nanoseconds=nano_secs)</c>）。</para>
/// <para>返回 <see cref="TimeSpan"/>，精度为 100ns tick（纳秒部分按 100ns 取整）。</para>
/// </summary>
public sealed class IntervalDayTimeDecoder : ITypeDecoder
{
    public static readonly IntervalDayTimeDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var seconds = reader.ReadSInt64();
        checksum.UpdateLong(seconds);
        var nanos = reader.ReadSInt32();
        checksum.UpdateInt(nanos);

        // 1 tick = 100ns；total = seconds * 10^7 ticks + nanos/100，符号随 seconds
        var ticks = seconds * TimeSpan.TicksPerSecond + nanos / 100;
        return TimeSpan.FromTicks(ticks);
    }
}

/// <summary>
/// INTERVAL_YEAR_MONTH：sint64 月份数 + crc.UpdateLong。
/// <para>对应 PyODPS <c>_read_field</c> 中 <c>types.interval_year_month</c> 分支
/// （<c>compat.Monthdelta(l_val)</c>）。</para>
/// <para>返回 <c>long</c>（signed 月份数）。调用方如需年/月拆分可自行换算。</para>
/// </summary>
public sealed class IntervalYearMonthDecoder : ITypeDecoder
{
    public static readonly IntervalYearMonthDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var months = reader.ReadSInt64();
        checksum.UpdateLong(months);
        return months;
    }
}
