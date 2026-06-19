namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// MaxCompute 类型枚举（基础类型）。
/// <para>对应 PyODPS <c>odps/types.py::types</c> 中已支持的 ODPS 类型。</para>
/// </summary>
public enum OdpsType
{
    Unknown,
    TinyInt,
    SmallInt,
    Int,
    BigInt,
    Float,
    Double,
    Boolean,
    String,
    Binary,
    DateTime,
    Date,
    Decimal,
    Char,
    VarChar
}
