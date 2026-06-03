using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents an alias in SQL (e.g., AS alias_name).
/// </summary>
public class Alias : ASTNodeAccessImpl
{
    public string Name { get; set; } = "";
    public bool UseAs { get; set; }

    public Alias() { }
    public Alias(string name) => Name = name;
    public Alias(string name, bool useAs) { Name = name; UseAs = useAs; }

    public override string ToString() => UseAs ? $"AS {Name}" : Name;
}
