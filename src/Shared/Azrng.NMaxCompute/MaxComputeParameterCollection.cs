using System.Collections;
using System.Data;
using System.Data.Common;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 参数集合实现
/// </summary>
public class MaxComputeParameterCollection : DbParameterCollection
{
    private readonly List<MaxComputeParameter> _parameters = new();

    public override IEnumerator GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    public override void CopyTo(Array array, int index)
    {
        Array.Copy(_parameters.ToArray(), 0, array, index, Math.Min(array.Length, Count));
    }

    public override int Count => _parameters.Count;

    public override object SyncRoot { get; } = new object();

    public override int Add(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (value is not MaxComputeParameter parameter)
            throw new ArgumentException($"Value must be of type {nameof(MaxComputeParameter)}", nameof(value));

        _parameters.Add(parameter);
        return _parameters.Count - 1;
    }

    public MaxComputeParameter Add(string name, object? value)
    {
        var p = new MaxComputeParameter
        {
            ParameterName = name,
            Value = value
        };
        Add(p);
        return p;
    }

    public MaxComputeParameter Add(string name, DbType type, object? value)
    {
        var p = new MaxComputeParameter
        {
            ParameterName = name,
            DbType = type,
            Value = value
        };
        Add(p);
        return p;
    }

    public override void AddRange(Array values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        foreach (var item in values)
        {
            if (item is MaxComputeParameter parameter)
                _parameters.Add(parameter);
        }
    }

    public override bool Contains(object value)
    {
        return value is MaxComputeParameter parameter && _parameters.Contains(parameter);
    }

    public override bool Contains(string value)
    {
        return _parameters.Any(x => string.Equals(x.ParameterName, value, StringComparison.OrdinalIgnoreCase));
    }

    public override int IndexOf(object value)
    {
        return value is MaxComputeParameter parameter ? _parameters.IndexOf(parameter) : -1;
    }

    public override int IndexOf(string value)
    {
        return _parameters.FindIndex(x => string.Equals(x.ParameterName, value, StringComparison.OrdinalIgnoreCase));
    }

    public override void Insert(int index, object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (value is MaxComputeParameter parameter)
            _parameters.Insert(index, parameter);
        else
            throw new ArgumentException($"Value must be of type {nameof(MaxComputeParameter)}", nameof(value));
    }

    public override void Remove(object value)
    {
        if (value is MaxComputeParameter parameter)
            _parameters.Remove(parameter);
    }

    public void Add(MaxComputeParameter parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        _parameters.Add(parameter);
    }

    public bool Contains(MaxComputeParameter parameter)
    {
        return _parameters.Contains(parameter);
    }

    public override void Clear()
    {
        _parameters.Clear();
    }

    public int IndexOf(MaxComputeParameter parameter)
    {
        return _parameters.IndexOf(parameter);
    }

    public void Insert(int index, MaxComputeParameter parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        _parameters.Insert(index, parameter);
    }

    public void Remove(MaxComputeParameter parameter)
    {
        _parameters.Remove(parameter);
    }

    public bool IsReadOnly => false;
    public bool IsFixedSize => false;

    protected override DbParameter GetParameter(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _parameters[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        var parameter = _parameters.FirstOrDefault(x =>
            string.Equals(x.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase));

        if (parameter == null)
            throw new ArgumentException($"Parameter '{parameterName}' not found in the collection.", nameof(parameterName));

        return parameter;
    }

    public override void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _parameters.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        _parameters.RemoveAll(x => string.Equals(x.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase));
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (value is MaxComputeParameter parameter)
            _parameters[index] = parameter;
        else
            throw new ArgumentException($"Value must be of type {nameof(MaxComputeParameter)}", nameof(value));
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var index = IndexOf(parameterName);
        if (index < 0)
            throw new ArgumentException($"Parameter '{parameterName}' not found in the collection.", nameof(parameterName));

        if (value is MaxComputeParameter parameter)
            _parameters[index] = parameter;
        else
            throw new ArgumentException($"Value must be of type {nameof(MaxComputeParameter)}", nameof(value));
    }

    /// <summary>
    /// 获取参数值（按名称）
    /// </summary>
    public object? this[string name]
    {
        get
        {
            var parameter = _parameters.FirstOrDefault(x =>
                string.Equals(x.ParameterName, name, StringComparison.OrdinalIgnoreCase));
            return parameter?.Value;
        }
        set
        {
            var parameter = _parameters.FirstOrDefault(x =>
                string.Equals(x.ParameterName, name, StringComparison.OrdinalIgnoreCase));

            if (parameter != null)
                parameter.Value = value;
            else
                Add(name, value);
        }
    }

    /// <summary>
    /// 获取参数（按索引）
    /// </summary>
    public MaxComputeParameter this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _parameters[index];
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _parameters[index] = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
