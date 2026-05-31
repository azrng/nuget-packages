using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Schema;

/// <summary>
/// Represents an index reference in SQL.
/// </summary>
public class Index : ASTNodeAccessImpl
{
    public string Name { get; set; } = "";
    public System.Collections.Generic.List<Column> Columns { get; set; } = new();

    public override string ToString() => Name;
}
