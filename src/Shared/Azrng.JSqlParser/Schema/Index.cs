using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents an index reference in SQL.
/// </summary>
public class Index : ASTNodeAccessImpl
{
    public string Name { get; set; } = "";

    public List<Column> Columns { get; set; } = new();

    public override string ToString() => Name;
}