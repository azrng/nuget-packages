using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// Represents a single ALTER operation (ADD/DROP/MODIFY COLUMN, ADD CONSTRAINT, etc.).
/// </summary>
public class AlterExpression : ASTNodeAccessImpl
{
    public AlterOperation Operation { get; set; }
    public string? ColumnName { get; set; }
    public string? DataType { get; set; }
    public string? ConstraintName { get; set; }
    public string? OptionalSpecifier { get; set; }
    public string? NewTableName { get; set; }
    public string? ColumnOldName { get; set; }

    // Partition operations
    public List<PartitionDefinition>? PartitionDefinitions { get; set; }
    public List<string>? PartitionNames { get; set; }
    public string? PartitionType { get; set; }
    public string? PartitionExpression { get; set; }
    public List<string>? PartitionColumns { get; set; }
    public int? CoalescePartitionNumber { get; set; }
    public string? ExchangePartitionTable { get; set; }
    public bool? ExchangePartitionValidation { get; set; }

    // Constraint properties
    public string? ConstraintSymbol { get; set; }
    public bool Enforced { get; set; }
    public string? ConstraintType { get; set; }

    // Table options
    public long? KeyBlockSize { get; set; }
    public string? LockOption { get; set; }
    public string? AlgorithmOption { get; set; }
    public string? CharacterSet { get; set; }
    public string? Collation { get; set; }
    public bool DefaultCollateSpecified { get; set; }
    public bool Invisible { get; set; }

    // Column set default/visibility
    public List<ColumnSetDefault>? ColumnSetDefaultList { get; set; }
    public List<ColumnSetVisibility>? ColumnSetVisibilityList { get; set; }

    /// <summary>
    /// Column definitions for ADD COLUMN operations.
    /// </summary>
    public List<ColumnDataType>? ColDataTypeList { get; set; }

    /// <summary>
    /// Primary key columns.
    /// </summary>
    public List<string>? PkColumns { get; set; }

    /// <summary>
    /// Unique key columns.
    /// </summary>
    public List<string>? UkColumns { get; set; }
    public string? UkName { get; set; }

    /// <summary>
    /// Parameters for SET operations.
    /// </summary>
    public List<string>? Parameters { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(Operation.ToString().Replace('_', ' '));
        if (ColumnName != null) sb.Append(' ').Append(ColumnName);
        if (DataType != null) sb.Append(' ').Append(DataType);
        if (OptionalSpecifier != null) sb.Append(' ').Append(OptionalSpecifier);
        if (ColDataTypeList != null && ColDataTypeList.Count > 0)
            sb.Append(' ').Append(string.Join(", ", ColDataTypeList));
        if (PkColumns != null && PkColumns.Count > 0)
            sb.Append(" (").Append(string.Join(", ", PkColumns)).Append(')');
        if (UkColumns != null && UkColumns.Count > 0)
            sb.Append(" (").Append(string.Join(", ", UkColumns)).Append(')');
        if (PartitionDefinitions != null)
        {
            sb.Append(" (");
            for (int i = 0; i < PartitionDefinitions.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(PartitionDefinitions[i]);
            }
            sb.Append(')');
        }
        if (PartitionNames != null)
            sb.Append(' ').Append(string.Join(", ", PartitionNames));
        if (Parameters != null && Parameters.Count > 0)
            sb.Append(' ').Append(string.Join(", ", Parameters));
        return sb.ToString();
    }

    /// <summary>
    /// Represents a column with a SET DEFAULT operation.
    /// </summary>
    public class ColumnSetDefault
    {
        public string ColumnName { get; set; } = "";
        public Expression.Expression? DefaultExpression { get; set; }

        public override string ToString() => $"{ColumnName} SET DEFAULT {DefaultExpression}";
    }

    /// <summary>
    /// Represents a column with a SET VISIBLE/INVISIBLE operation.
    /// </summary>
    public class ColumnSetVisibility
    {
        public string ColumnName { get; set; } = "";
        public bool Visible { get; set; }

        public override string ToString() => $"{ColumnName} SET {(Visible ? "VISIBLE" : "INVISIBLE")}";
    }

    /// <summary>
    /// Represents a column data type definition for ADD COLUMN operations.
    /// </summary>
    public class ColumnDataType
    {
        public string ColumnName { get; set; } = "";
        public string DataType { get; set; } = "";

        public override string ToString() => $"{ColumnName} {DataType}";
    }
}
