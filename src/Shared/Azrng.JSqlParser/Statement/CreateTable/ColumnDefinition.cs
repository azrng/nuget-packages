using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Represents a column definition in CREATE TABLE.
/// </summary>
public class ColumnDefinition : ASTNodeAccessImpl
{
    public string ColumnName { get; set; } = "";
    public string DataType { get; set; } = "";
    public System.Collections.Generic.List<string> ColumnSpecs { get; set; } = new();

    public override string ToString()
    {
        var specs = ColumnSpecs.Count > 0 ? " " + string.Join(" ", ColumnSpecs) : "";
        return $"{ColumnName} {DataType}{specs}";
    }
}
