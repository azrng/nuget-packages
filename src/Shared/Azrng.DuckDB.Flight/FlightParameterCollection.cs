using System.Collections;
using System.Data.Common;

namespace Azrng.DuckDB.Flight;

internal sealed class FlightParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _items = new();

    public override int Count => _items.Count;

    public override object SyncRoot => this;

    public override int Add(object value)
    {
        _items.Add((DbParameter)value);
        return _items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (var v in values) Add(v!);
    }

    public override void Clear() => _items.Clear();

    public override bool Contains(object value) => _items.Contains((DbParameter)value);

    public override bool Contains(string value) => _items.Any(p => p.ParameterName == value);

    public override void CopyTo(Array array, int index) => _items.ToArray().CopyTo(array, index);

    public override IEnumerator GetEnumerator() => _items.GetEnumerator();

    public override int IndexOf(object value) => _items.IndexOf((DbParameter)value);

    public override int IndexOf(string parameterName) => _items.FindIndex(p => p.ParameterName == parameterName);

    public override void Insert(int index, object value) => _items.Insert(index, (DbParameter)value);

    public override void Remove(object value) => _items.Remove((DbParameter)value);

    public override void RemoveAt(int index) => _items.RemoveAt(index);

    public override void RemoveAt(string parameterName)
    {
        var i = IndexOf(parameterName);
        if (i >= 0) RemoveAt(i);
    }

    protected override DbParameter GetParameter(int index) => _items[index];

    protected override DbParameter GetParameter(string parameterName) => _items[IndexOf(parameterName)];

    protected override void SetParameter(int index, DbParameter value) => _items[index] = value;

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var i = IndexOf(parameterName);
        if (i >= 0) _items[i] = value;
        else _items.Add(value);
    }
}
