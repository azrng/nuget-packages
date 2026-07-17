using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents an alias in SQL (e.g., AS alias_name).
/// </summary>
public sealed class Alias : ASTNodeAccessImpl
{
    public string Name { get; set; } = "";
    public bool UseAs { get; set; }

    public Alias() { }
    public Alias(string name) => Name = name;
    public Alias(string name, bool useAs) { Name = name; UseAs = useAs; }

    public override string ToString() => UseAs ? $"AS {Name}" : Name;

    /// <summary>按 Name + UseAs 做值相等（两个 AS t 别名视为相等）。</summary>
    public override bool Equals(object? obj) =>
        obj is Alias other && Name == other.Name && UseAs == other.UseAs;

    /// <summary>基于 Name + UseAs 的哈希。</summary>
    public override int GetHashCode() => HashCode.Combine(Name, UseAs);
}
