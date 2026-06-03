using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Represents a table constraint in CREATE TABLE.
/// </summary>
public class Constraint : ASTNodeAccessImpl
{
    public string? Name { get; set; }
    public string Type { get; set; } = "";
    public System.Collections.Generic.List<string> Columns { get; set; } = new();

    public override string ToString()
    {
        var name = Name != null ? $"CONSTRAINT {Name} " : "";
        return $"{name}{Type} ({string.Join(", ", Columns)})";
    }
}
