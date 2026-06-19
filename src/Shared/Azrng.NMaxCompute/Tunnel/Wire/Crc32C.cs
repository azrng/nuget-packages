namespace Azrng.NMaxCompute.Tunnel.Wire;

/// <summary>
/// CRC-32C（Castagnoli，多项式 0x82F63B78 反射）。
/// <para>对应 PyODPS <c>odps/crc.py::Crc32c</c>（内嵌 256 项查表，与 PyODPS 表逐字节一致）。</para>
/// </summary>
public sealed class Crc32C
{
    private static readonly uint[] Table = BuildTable(0x82F63B78);

    private uint _crc = 0xFFFFFFFF;

    public void Update(ReadOnlySpan<byte> buffer)
    {
        var crc = _crc;
        foreach (var b in buffer)
        {
            var idx = (crc ^ b) & 0xFF;
            crc = (Table[idx] ^ (crc >> 8)) & 0xFFFFFFFF;
        }
        _crc = crc;
    }

    public void Update(byte b)
    {
        var idx = (_crc ^ b) & 0xFF;
        _crc = (Table[idx] ^ (_crc >> 8)) & 0xFFFFFFFF;
    }

    public void Reset() => _crc = 0xFFFFFFFF;

    public uint GetValue() => _crc ^ 0xFFFFFFFF;

    private static uint[] BuildTable(uint polynomial)
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var crc = i;
            for (var j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0 ? (polynomial ^ (crc >> 1)) : (crc >> 1);
            }
            table[i] = crc;
        }
        return table;
    }
}
