using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// Tunnel wire 解码器：每种 MaxCompute 类型对应一个 decoder。
/// <para>对应 PyODPS <c>odps/tunnel/io/reader.py::_read_field</c> 中的分支。</para>
/// </summary>
public interface ITypeDecoder
{
    /// <summary>
    /// 从 wire 流读取一个字段值，并同步更新 CRC 校验。
    /// </summary>
    object? Read(ProtobufWireReader reader, Checksum checksum);
}
