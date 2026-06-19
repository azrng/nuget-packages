using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// TIMESTAMP：秒（sint64）+ 纳秒（sint32）两部分，分别计入 CRC。
/// <para>
/// 对应 PyODPS <c>_read_field</c> 中 <c>types.timestamp</c> 分支：
/// <c>l_val * 1000</c> 作为毫秒传给 <c>from_milliseconds</c>，等价于 l_val 是 Unix 秒；
/// nano_secs 为额外纳秒（0-999999）。
/// </para>
/// <para>
/// 默认按本地时区返回（与 PyODPS <c>local_timezone=True</c> 默认一致）。
/// 使用 <see cref="DateTimeOffset"/>，精度为 .NET tick（100ns），纳秒部分按 100ns 取整。
/// </para>
/// </summary>
public sealed class TimestampDecoder : ITypeDecoder
{
    public static readonly TimestampDecoder LocalInstance = new(useUtc: false);
    public static readonly TimestampDecoder UtcInstance = new(useUtc: true);

    private readonly bool _useUtc;

    private TimestampDecoder(bool useUtc) => _useUtc = useUtc;

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var seconds = reader.ReadSInt64();
        checksum.UpdateLong(seconds);
        var nanoSecs = reader.ReadSInt32();
        checksum.UpdateInt(nanoSecs);

        var dto = FromUnixSeconds(seconds);
        // nano → ticks：1 tick = 100ns，所以 nanoSecs / 100 = ticks（向下取整）
        // nanoSecs 可能为负（表示 seconds 之前的小数偏移），用整数除法保留符号语义
        dto = dto.AddTicks(nanoSecs / 100);
        return _useUtc ? dto : dto.ToLocalTime();
    }

    private static DateTimeOffset FromUnixSeconds(long seconds)
    {
        // DateTimeOffset.FromUnixTimeSeconds 范围有限，越界时夹到边界
        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }
        catch (ArgumentOutOfRangeException)
        {
            return seconds < 0
                ? DateTimeOffset.MinValue
                : DateTimeOffset.MaxValue;
        }
    }
}

/// <summary>
/// JSON：以 length-delimited UTF-8 字符串传输。
/// <para>
/// 对应 PyODPS <c>_read_field</c> 中 <c>types.json</c> 分支（PyODPS 调 <c>json.loads</c>）。
/// 这里返回原始 JSON 文本字符串，便于 ADO.NET <c>GetString</c> 直接读取；
/// 调用方需要结构化对象时可自行 <c>JsonDocument.Parse</c>。
/// </para>
/// </summary>
public sealed class JsonDecoder : ITypeDecoder
{
    public static readonly JsonDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var bytes = reader.ReadBytes();
        checksum.Update(bytes);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
