using System.Collections;
using System.Data;
using System.Data.Common;
using Apache.Arrow;
using Apache.Arrow.Types;

namespace Azrng.DuckDB.Flight;

public sealed class DuckFlightDataReader : DbDataReader
{
    private readonly List<RecordBatch> _batches;
    private readonly List<object?[]> _rows = new();
    private readonly string[] _names;
    private readonly string[] _types;
    private readonly Type[] _fieldTypes;
    private int _cursor = -1;

    public DuckFlightDataReader(List<RecordBatch> batches)
    {
        _batches = batches;
        var schema = _batches.FirstOrDefault()?.Schema;
        _names = schema?.FieldsList.Select(f => f.Name).ToArray() ?? System.Array.Empty<string>();
        _types = schema?.FieldsList.Select(f => f.DataType.Name).ToArray() ?? System.Array.Empty<string>();
        _fieldTypes = schema?.FieldsList.Select(f => ArrowTypeToSystemType(f.DataType)).ToArray() ?? System.Array.Empty<Type>();

        foreach (var b in _batches)
        {
            for (var r = 0; r < b.Length; r++)
            {
                var vals = new object?[b.ColumnCount];
                for (var c = 0; c < b.ColumnCount; c++)
                {
                    vals[c] = ArrowValueToObject(b.Column(c), r);
                }

                _rows.Add(vals);
            }
        }
    }

    public override int FieldCount => _names.Length;

    public override bool HasRows => _rows.Count > 0;

    public override bool IsClosed => false;

    public override int RecordsAffected => -1;

    public override int Depth => 0;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override bool Read()
    {
        if (_cursor + 1 >= _rows.Count)
        {
            return false;
        }

        _cursor++;
        return true;
    }

    public override bool NextResult() => false;

    public override string GetName(int ordinal) => _names[ordinal];

    public override string GetDataTypeName(int ordinal) => _types[ordinal];

    public override Type GetFieldType(int ordinal) => _fieldTypes[ordinal];

    public override int GetOrdinal(string name) =>
        System.Array.FindIndex(_names, n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));

    public override object GetValue(int ordinal) => _rows[_cursor][ordinal] ?? DBNull.Value;

    public override int GetValues(object[] values)
    {
        var n = Math.Min(values.Length, FieldCount);
        for (var i = 0; i < n; i++)
        {
            values[i] = GetValue(i);
        }

        return n;
    }

    public override bool IsDBNull(int ordinal) => _rows[_cursor][ordinal] is null;

    public override bool GetBoolean(int ordinal) => Convert.ToBoolean(GetValue(ordinal));

    public override byte GetByte(int ordinal) => Convert.ToByte(GetValue(ordinal));

    public override char GetChar(int ordinal) => Convert.ToChar(GetValue(ordinal));

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException();

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException();

    public override Guid GetGuid(int ordinal) => Guid.Parse(GetString(ordinal));

    public override short GetInt16(int ordinal) => Convert.ToInt16(GetValue(ordinal));

    public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal));

    public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal));

    public override float GetFloat(int ordinal) => Convert.ToSingle(GetValue(ordinal));

    public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal));

    public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal)) ?? "";

    public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(GetValue(ordinal));

    public override DateTime GetDateTime(int ordinal) => Convert.ToDateTime(GetValue(ordinal));

    public override IEnumerator GetEnumerator() => _rows.GetEnumerator();

    public override DataTable GetSchemaTable() => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var b in _batches)
            {
                b.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private static object? ArrowValueToObject(IArrowArray column, int row)
    {
        if (column.IsNull(row))
        {
            return null;
        }

        return column switch
        {
            BooleanArray a => a.GetValue(row),
            Int8Array a => a.GetValue(row),
            Int16Array a => a.GetValue(row),
            Int32Array a => a.GetValue(row),
            Int64Array a => a.GetValue(row),
            UInt8Array a => a.GetValue(row),
            UInt16Array a => a.GetValue(row),
            UInt32Array a => a.GetValue(row),
            UInt64Array a => a.GetValue(row),
            FloatArray a => a.GetValue(row),
            DoubleArray a => a.GetValue(row),
            Decimal128Array a => a.GetValue(row),
            Decimal256Array a => a.GetValue(row),
            StringArray a => a.GetString(row),
            LargeStringArray a => a.GetString(row),
            BinaryArray a => a.GetBytes(row).ToArray(),
            LargeBinaryArray a => a.GetBytes(row).ToArray(),
            Date32Array a => a.GetDateTime(row),
            Date64Array a => a.GetDateTime(row),
            // Arrow TimestampArray.GetTimestamp 返回 DateTimeOffset?，但 DuckDB 的 TIMESTAMP 是
            // without time zone 的 naive 时刻；统一转为 DateTime(Kind=Utc)，与 GetFieldType/DATE 列保持一致，
            // 避免 Dapper 等 ADO.NET 消费者按 DateTime 取值时抛 InvalidCastException。
            TimestampArray a => a.GetTimestamp(row)?.UtcDateTime,
            NullArray a => null,
            _ => $"{{unsupported:{column.GetType().Name}}}"
        };
    }

    private static Type ArrowTypeToSystemType(IArrowType arrowType) =>
        arrowType switch
        {
            BooleanType => typeof(bool),
            Int8Type => typeof(sbyte),
            Int16Type => typeof(short),
            Int32Type => typeof(int),
            Int64Type => typeof(long),
            UInt8Type => typeof(byte),
            UInt16Type => typeof(ushort),
            UInt32Type => typeof(uint),
            UInt64Type => typeof(ulong),
            FloatType => typeof(float),
            DoubleType => typeof(double),
            Decimal128Type => typeof(decimal),
            Decimal256Type => typeof(decimal),
            StringType => typeof(string),
            LargeStringType => typeof(string),
            BinaryType => typeof(byte[]),
            LargeBinaryType => typeof(byte[]),
            Date32Type => typeof(DateTime),
            Date64Type => typeof(DateTime),
            TimestampType => typeof(DateTime),
            NullType => typeof(object),
            ListType => typeof(object),
            LargeListType => typeof(object),
            StructType => typeof(object),
            MapType => typeof(object),
            UnionType => typeof(object),
            _ => typeof(object)
        };
}
