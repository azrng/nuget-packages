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
        var name = Name != null ? $"{Name} " : "";
        var constraintPrefix = string.IsNullOrEmpty(name) ? "" : $"CONSTRAINT {Name} ";
        // 简单约束（PRIMARY KEY/UNIQUE/CHECK/FOREIGN KEY）输出 CONSTRAINT 前缀
        if (Type is "PRIMARY KEY" or "UNIQUE" or "CHECK" or "FOREIGN KEY")
        {
            return $"{constraintPrefix}{Type} ({string.Join(", ", Columns)})";
        }
        // MySQL 索引（KEY/UNIQUE KEY/FULLTEXT KEY/SPATIAL KEY）输出索引名
        return $"{Type} {name}({string.Join(", ", Columns)})";
    }
}
