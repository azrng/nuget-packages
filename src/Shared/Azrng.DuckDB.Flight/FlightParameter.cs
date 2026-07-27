using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.DuckDB.Flight;

public sealed class FlightParameter : DbParameter
{
    [AllowNull]
    public override string ParameterName { get; set; }
    public override object? Value { get; set; }
    public override DbType DbType { get; set; } = DbType.Object;
    public override int Size { get; set; }
    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    public override bool IsNullable { get; set; }
    public override byte Precision { get; set; }
    public override byte Scale { get; set; }
    public override string SourceColumn { get; set; } = string.Empty;
    public override bool SourceColumnNullMapping { get; set; }
    public override DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;
    public override void ResetDbType() => DbType = DbType.Object;
}
