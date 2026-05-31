using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Schema;

/// <summary>
/// Represents a database reference in SQL.
/// </summary>
public class Database : ASTNodeAccessImpl
{
    public string Name { get; set; } = "";

    public override string ToString() => Name;
}
