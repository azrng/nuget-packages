using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Base class for schema objects with multi-part names (database.schema.name).
/// </summary>
public abstract class MultiPartName : ASTNodeAccessImpl
{
    public string? Database { get; set; }
    public string? SchemaName { get; set; }
    public string Name { get; set; } = "";

    public string GetFullyQualifiedName()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (Database != null) parts.Add(Database);
        if (SchemaName != null) parts.Add(SchemaName);
        parts.Add(Name);
        return string.Join(".", parts);
    }

    public override string ToString() => GetFullyQualifiedName();
}
