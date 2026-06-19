namespace Azrng.NMaxCompute.Tunnel.Wire;

/// <summary>
/// 标准 CRC-32（ISO-HDLC，多项式 0xEDB88320 反射），与 <c>zlib.crc32</c> 等价。
/// <para>对应 PyODPS <c>odps/crc.py::Crc32</c>（Python 走 zlib.crc32，C# 用查表法）。</para>
/// <para>累积更新：内部保留运行值（init 0xFFFFFFFF），<see cref="GetValue"/> 时反转变为最终结果。</para>
/// </summary>
public sealed class Crc32
{
    private static readonly uint[] Table = BuildTable(0xEDB88320);

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
