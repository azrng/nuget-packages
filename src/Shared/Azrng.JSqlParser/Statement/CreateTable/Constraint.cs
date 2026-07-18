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
    /// MySQL 索引列参数（含 ASC/DESC 排序方向），用于 KEY/INDEX 类型约束。
    /// 对齐上游 commit 763e92d7。为空时回退到 Columns。
    /// </summary>
    public System.Collections.Generic.List<string>? IndexColumnParams { get; set; }

    /// <summary>
    /// Oracle/DB2 USING INDEX 子句的索引名（可空，仅 USING INDEX 无名时为 null 但 HasUsingIndex=true）。
    /// 对齐上游 commit c7b3bdbd。
    /// </summary>
    public string? UsingIndex { get; set; }

    /// <summary>是否存在 USING INDEX 子句（区分"无名 USING INDEX"与"无 USING INDEX"）。</summary>
    public bool HasUsingIndex { get; set; }

    /// <summary>
    /// MySQL 索引尾选项原始字符串列表（USING BTREE/HASH、COMMENT '...'、KEY_BLOCK_SIZE n、VISIBLE/INVISIBLE 等），
    /// 对齐上游 Index.idxSpec / IndexOption。输出时空格连接追加到约束末尾。
    /// </summary>
    public System.Collections.Generic.List<string>? IndexOptions { get; set; }

    /// <summary>
    /// SQL Server 索引聚集属性（CLUSTERED / NONCLUSTERED），PRIMARY KEY / UNIQUE 后可选。
    /// 未指定时为 null。对齐 #1589。
    /// </summary>
    public string? ClusterKind { get; set; }

    public override string ToString()
    {
        var name = Name != null ? $"{Name} " : "";
        var constraintPrefix = string.IsNullOrEmpty(name) ? "" : $"CONSTRAINT {Name} ";
        var usingIndex = HasUsingIndex
            ? (UsingIndex != null ? $" USING INDEX {UsingIndex}" : " USING INDEX")
            : "";
        // MySQL 索引尾选项（USING BTREE/HASH、COMMENT 等），空格连接追加
        var indexOpts = IndexOptions is { Count: > 0 } ? " " + string.Join(" ", IndexOptions) : "";
        // SQL Server 聚集属性（CLUSTERED/NONCLUSTERED），紧跟在 PRIMARY KEY / UNIQUE 之后
        var clusterSuffix = string.IsNullOrEmpty(ClusterKind) ? "" : $" {ClusterKind}";
        // 简单约束（PRIMARY KEY/UNIQUE/CHECK/FOREIGN KEY）输出 CONSTRAINT 前缀
        if (Type is "PRIMARY KEY" or "UNIQUE" or "CHECK" or "FOREIGN KEY")
        {
            return $"{constraintPrefix}{Type}{clusterSuffix} ({string.Join(", ", Columns)}){usingIndex}{indexOpts}";
        }
        // MySQL 索引（KEY/INDEX/UNIQUE KEY 等）输出索引名，优先用 IndexColumnParams（含 ASC/DESC）
        var indexCols = IndexColumnParams ?? Columns;
        return $"{Type} {name}({string.Join(", ", indexCols)}){indexOpts}";
    }
}
