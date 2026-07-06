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

    /// <summary>
    /// Oracle/DB2 USING INDEX 子句的索引名（可空，仅 USING INDEX 无名时为 null 但 HasUsingIndex=true）。
    /// 对齐上游 commit c7b3bdbd。
    /// </summary>
    public string? UsingIndex { get; set; }

    /// <summary>是否存在 USING INDEX 子句（区分"无名 USING INDEX"与"无 USING INDEX"）。</summary>
    public bool HasUsingIndex { get; set; }

    public override string ToString()
    {
        var name = Name != null ? $"{Name} " : "";
        var constraintPrefix = string.IsNullOrEmpty(name) ? "" : $"CONSTRAINT {Name} ";
        var usingIndex = HasUsingIndex
            ? (UsingIndex != null ? $" USING INDEX {UsingIndex}" : " USING INDEX")
            : "";
        // 简单约束（PRIMARY KEY/UNIQUE/CHECK/FOREIGN KEY）输出 CONSTRAINT 前缀
        if (Type is "PRIMARY KEY" or "UNIQUE" or "CHECK" or "FOREIGN KEY")
        {
            return $"{constraintPrefix}{Type} ({string.Join(", ", Columns)}){usingIndex}";
        }
        // MySQL 索引（KEY/UNIQUE KEY/FULLTEXT KEY/SPATIAL KEY）输出索引名
        return $"{Type} {name}({string.Join(", ", Columns)})";
    }
}
