using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents a table reference in SQL.
/// </summary>
public class Table : ASTNodeAccessImpl, FromItem
{
    public string? Database { get; set; }
    public string? SchemaName { get; set; }
    public string Name { get; set; } = "";
    public Alias? Alias { get; set; }

    public string GetFullyQualifiedName()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (Database != null) parts.Add(Database);
        if (SchemaName != null) parts.Add(SchemaName);
        parts.Add(Name);
        return string.Join(".", parts);
    }

    public Alias? GetAlias() => Alias;
    public void SetAlias(Alias alias) { Alias = alias; }

    public override string ToString()
    {
        var name = GetFullyQualifiedName();
        return Alias != null ? $"{name} {Alias}" : name;
    }
}
