using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 数据读取器实现
/// </summary>
public class MaxComputeDataReader : DbDataReader
{
    private readonly QueryResult _result;
    private int _currentRowIndex = -1;
    private bool _isClosed = false;
    private bool _disposed = false;
    private readonly ILogger? _logger;

    public MaxComputeDataReader(QueryResult result, ILogger? logger = null)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        _logger = logger;
    }

    public override int Depth => 0;

    public override bool HasRows => _result.Rows.Any();

    public override bool IsClosed => _isClosed;

    public override int RecordsAffected => 0; // MaxCompute 不提供此信息

    public override int FieldCount => _result.Columns?.Length ?? 0;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override void Close()
    {
        _isClosed = true;
    }

    public override bool GetBoolean(int ordinal)
    {
        return Convert.ToBoolean(GetValue(ordinal));
    }

    public override byte GetByte(int ordinal)
    {
        return Convert.ToByte(GetValue(ordinal));
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
    {
        throw new NotSupportedException("GetBytes is not supported.");
    }

    public override char GetChar(int ordinal)
    {
        return Convert.ToChar(GetValue(ordinal));
    }

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
    {
        throw new NotSupportedException("GetChars is not supported.");
    }

    public override string GetDataTypeName(int ordinal)
    {
        if (ordinal < 0 || ordinal >= FieldCount)
            throw new ArgumentOutOfRangeException(nameof(ordinal),
                $"Column index {ordinal} is out of range. Valid range is 0 to {FieldCount - 1}.");

        return "STRING"; // MaxCompute 默认返回字符串
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return Convert.ToDateTime(GetValue(ordinal));
    }

    public override decimal GetDecimal(int ordinal)
    {
        return Convert.ToDecimal(GetValue(ordinal));
    }

    public override double GetDouble(int ordinal)
    {
        return Convert.ToDouble(GetValue(ordinal));
    }

    public override Type GetFieldType(int ordinal)
    {
        if (ordinal < 0 || ordinal >= FieldCount)
            throw new ArgumentOutOfRangeException(nameof(ordinal),
                $"Column index {ordinal} is out of range. Valid range is 0 to {FieldCount - 1}.");

        return typeof(string);
    }

    public override float GetFloat(int ordinal)
    {
        return Convert.ToSingle(GetValue(ordinal));
    }

    public override Guid GetGuid(int ordinal)
    {
        var value = GetValue(ordinal);
        if (value == null || value == DBNull.Value)
            throw new InvalidCastException("Cannot convert NULL to Guid");

        return Guid.Parse(value.ToString()!);
    }

    public override short GetInt16(int ordinal)
    {
        return Convert.ToInt16(GetValue(ordinal));
    }

    public override int GetInt32(int ordinal)
    {
        return Convert.ToInt32(GetValue(ordinal));
    }

    public override long GetInt64(int ordinal)
    {
        return Convert.ToInt64(GetValue(ordinal));
    }

    public override string GetName(int ordinal)
    {
        if (ordinal < 0 || ordinal >= FieldCount)
            throw new ArgumentOutOfRangeException(nameof(ordinal),
                $"Column index {ordinal} is out of range. Valid range is 0 to {FieldCount - 1}.");

        return _result.Columns?[ordinal] ?? $"Column{ordinal}";
    }

    public override int GetOrdinal(string name)
    {
        if (_result.Columns == null)
            throw new InvalidOperationException("No columns available.");

        for (int i = 0; i < _result.Columns.Length; i++)
        {
            if (string.Equals(_result.Columns[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new ArgumentException($"Column '{name}' not found.", nameof(name));
    }

    public override string GetString(int ordinal)
    {
        return GetValue(ordinal)?.ToString() ?? string.Empty;
    }

    public override object GetValue(int ordinal)
    {
        if (_isClosed)
            throw new InvalidOperationException("DataReader is closed.");
        if (_currentRowIndex < 0 || _currentRowIndex >= _result.Rows.Length)
            throw new InvalidOperationException("No current row.");
        if (ordinal < 0 || ordinal >= FieldCount)
            throw new ArgumentOutOfRangeException(nameof(ordinal),
                $"Column index {ordinal} is out of range. Valid range is 0 to {FieldCount - 1}.");

        return _result.Rows[_currentRowIndex][ordinal];
    }

    public override int GetValues(object[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        int count = Math.Min(values.Length, FieldCount);
        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }

        return count;
    }

    public override bool IsDBNull(int ordinal)
    {
        var value = GetValue(ordinal);
        return value == null || value == DBNull.Value;
    }

    public override bool NextResult()
    {
        return false; // MaxCompute 不支持多个结果集
    }

    public override bool Read()
    {
        if (_isClosed)
            return false;

        _currentRowIndex++;
        return _currentRowIndex < _result.Rows.Length;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Close();
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }

    // 可选：实现 GetEnumerator 以支持 foreach
    public override System.Collections.IEnumerator GetEnumerator()
    {
        // 返回行数据的枚举器
        return _result.Rows.GetEnumerator();
    }
}
