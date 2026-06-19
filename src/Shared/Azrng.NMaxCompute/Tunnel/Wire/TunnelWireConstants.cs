namespace Azrng.NMaxCompute.Tunnel.Wire;

/// <summary>
/// Tunnel 协议的 protobuf 字段魔数。
/// <para>对应 PyODPS <c>odps/tunnel/wireconstants.py::ProtoWireConstants</c>。</para>
/// </summary>
public static class TunnelWireConstants
{
    public const int TunnelMetaCount = 33554430;

    public const int TunnelMetaChecksum = 33554431;

    public const int TunnelEndRecord = 33553408;

    public const int TunnelEndMetrics = 33554176;
}

/// <summary>
/// Protobuf wire types.
/// <para>对应 PyODPS <c>odps/tunnel/pb/wire_format.py::WIRETYPE_*</c>。</para>
/// </summary>
public static class WireType
{
    public const int Varint = 0;

    public const int Fixed64 = 1;

    public const int LengthDelimited = 2;

    public const int Fixed32 = 5;
}
