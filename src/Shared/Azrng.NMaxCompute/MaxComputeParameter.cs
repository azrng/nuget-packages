using System.Data;
using System.Data.Common;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 参数实现
/// </summary>
public class MaxComputeParameter : DbParameter
{
    private object? _value;

    public override DbType DbType { get; set; } = DbType.String;

    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public override bool IsNullable { get; set; } = true;

    public override string ParameterName { get; set; } = string.Empty;

    public override int Size { get; set; }

    public override string SourceColumn { get; set; } = string.Empty;

    public override bool SourceColumnNullMapping { get; set; }

    public override object? Value
    {
        get => _value;
        set => _value = value;
    }

    public override void ResetDbType()
    {
        DbType = DbType.String;
    }

    public override string ToString()
    {
        return $"{ParameterName}({DbType}): {Value ?? "NULL"}";
    }
}
