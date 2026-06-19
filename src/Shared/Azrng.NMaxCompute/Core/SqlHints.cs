using System.Collections.Generic;

namespace Azrng.NMaxCompute.Core;

/// <summary>
/// MaxCompute SQL hints（注入到 settings JSON）
/// </summary>
public sealed class SqlHints : Dictionary<string, string>
{
    public SqlHints() : base() { }

    public SqlHints(IDictionary<string, string> source) : base(source) { }

    public SqlHints AddHint(string key, string value)
    {
        this[key] = value;
        return this;
    }
}
