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

    /// <summary>ENGINE 子句是否使用等号（ENGINE = InnoDB），对齐上游 engineOptionWithEqual。</summary>
    public bool UseEqualsForEngine { get; set; }
    /// <summary>COMMENT 子句是否使用等号（COMMENT = 'x'），对齐上游 commentWithEqualSign。</summary>
    public bool UseEqualsForComment { get; set; }

    // Column set default/visibility
    public List<ColumnSetDefault>? ColumnSetDefaultList { get; set; }
    public List<ColumnSetVisibility>? ColumnSetVisibilityList { get; set; }

    /// <summary>
    /// ALTER COLUMN x 的具体动作（SET DEFAULT/DROP DEFAULT/SET NOT NULL/DROP NOT NULL/TYPE/SET DATA TYPE）。
    /// 对齐上游 ALTER 分支的列级约束变更，此前 grammar 已解析但 visitor 静默丢弃。
    /// </summary>
    public AlterColumnAction? ColumnAlterAction { get; set; }

    /// <summary>ALTER COLUMN ... TYPE dataType 的类型文本（ColumnAlterAction=Type/SetDataType 时）。</summary>
    public string? AlterColumnType { get; set; }

    /// <summary>ALTER COLUMN ... SET DEFAULT expr 的表达式文本（ColumnAlterAction=SetDefault 时）。</summary>
    public string? AlterColumnDefaultExpression { get; set; }

    /// <summary>ALTER/MODIFY/CHANGE/DROP/ADD 操作是否显式使用 COLUMN 关键字（ALTER COLUMN x / ALTER x）。</summary>
    public bool UseColumnKeyword { get; set; }

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

    /// <summary>Oracle/DB2 USING INDEX [name] 子句（约束级），commit c7b3bdbd。</summary>
    public string? UsingIndex { get; set; }
    public bool HasUsingIndex { get; set; }

    /// <summary>
    /// Parameters for SET operations.
    /// </summary>
    public List<string>? Parameters { get; set; }

    public override string ToString()
    {
        // 新增/分区等需要特定关键字输出的操作：分支化输出，避免静默丢失语义。
        switch (Operation)
        {
            case AlterOperation.DROP when !string.IsNullOrEmpty(ConstraintSymbol):
                return $"DROP CONSTRAINT {ConstraintSymbol}";
            case AlterOperation.DROP_PRIMARY_KEY:
                return "DROP PRIMARY KEY";
            case AlterOperation.DROP_UNIQUE:
            {
                var sb = new System.Text.StringBuilder("DROP UNIQUE");
                if (!string.IsNullOrEmpty(ConstraintSymbol)) sb.Append(' ').Append(ConstraintSymbol);
                return sb.ToString();
            }
            case AlterOperation.DROP_FOREIGN_KEY:
            {
                var sb = new System.Text.StringBuilder("DROP FOREIGN KEY");
                if (!string.IsNullOrEmpty(ConstraintSymbol)) sb.Append(' ').Append(ConstraintSymbol);
                return sb.ToString();
            }
            case AlterOperation.RENAME_INDEX:
                return ColumnOldName != null && ColumnName != null
                    ? $"RENAME INDEX {ColumnOldName} TO {ColumnName}"
                    : "RENAME INDEX";
            case AlterOperation.RENAME_KEY:
                return ColumnOldName != null && ColumnName != null
                    ? $"RENAME KEY {ColumnOldName} TO {ColumnName}"
                    : "RENAME KEY";
            case AlterOperation.RENAME_CONSTRAINT:
                return ColumnOldName != null && ColumnName != null
                    ? $"RENAME CONSTRAINT {ColumnOldName} TO {ColumnName}"
                    : "RENAME CONSTRAINT";
            case AlterOperation.ENGINE:
            {
                var sb = new System.Text.StringBuilder("ENGINE");
                if (UseEqualsForEngine) sb.Append(" =");
                if (OptionalSpecifier != null) sb.Append(' ').Append(OptionalSpecifier);
                return sb.ToString();
            }
            case AlterOperation.COMMENT:
            case AlterOperation.COMMENT_WITH_EQUAL_SIGN:
            {
                var sb = new System.Text.StringBuilder("COMMENT");
                if (UseEqualsForComment || Operation == AlterOperation.COMMENT_WITH_EQUAL_SIGN) sb.Append(" =");
                if (OptionalSpecifier != null) sb.Append(' ').Append(OptionalSpecifier);
                return sb.ToString();
            }
            case AlterOperation.CONVERT:
            {
                var sbConv = new System.Text.StringBuilder("CONVERT TO CHARACTER SET ");
                if (CharacterSet != null) sbConv.Append(CharacterSet);
                if (Collation != null)
                {
                    sbConv.Append(" COLLATE");
                    if (UseEqualsForComment) sbConv.Append(" =");
                    sbConv.Append(' ').Append(Collation);
                }
                return sbConv.ToString();
            }
            case AlterOperation.COLLATE:
            {
                // DEFAULT CHARACTER SET x / CHARACTER SET x [COLLATE y]
                var sbCs = new System.Text.StringBuilder();
                if (DefaultCollateSpecified) sbCs.Append("DEFAULT ");
                sbCs.Append("CHARACTER SET ");
                if (CharacterSet != null) sbCs.Append(CharacterSet);
                if (Collation != null)
                {
                    sbCs.Append(" COLLATE");
                    if (UseEqualsForComment) sbCs.Append(" =");
                    sbCs.Append(' ').Append(Collation);
                }
                return sbCs.ToString();
            }
            case AlterOperation.ALTER when ColumnAlterAction != null:
            {
                // ALTER [COLUMN] x SET DEFAULT expr / DROP DEFAULT / SET NOT NULL / DROP NOT NULL / TYPE dataType / SET DATA TYPE dataType
                var sbAlter = new System.Text.StringBuilder("ALTER ");
                if (UseColumnKeyword) sbAlter.Append("COLUMN ");
                if (ColumnName != null) sbAlter.Append(ColumnName).Append(' ');
                sbAlter.Append(ColumnAlterAction switch
                {
                    AlterColumnAction.SetDefault => $"SET DEFAULT {AlterColumnDefaultExpression}",
                    AlterColumnAction.DropDefault => "DROP DEFAULT",
                    AlterColumnAction.SetNotNull => "SET NOT NULL",
                    AlterColumnAction.DropNotNull => "DROP NOT NULL",
                    AlterColumnAction.Type => $"TYPE {AlterColumnType}",
                    AlterColumnAction.SetDataType => $"SET DATA TYPE {AlterColumnType}",
                    AlterColumnAction.SetVisible => "SET VISIBLE",
                    AlterColumnAction.SetInvisible => "SET INVISIBLE",
                    _ => "",
                });
                return sbAlter.ToString();
            }
            case AlterOperation.REMOVE_PARTITIONING:
                return "REMOVE PARTITIONING";
            case AlterOperation.ADD_PARTITION:
            case AlterOperation.DROP_PARTITION:
            case AlterOperation.TRUNCATE_PARTITION:
            case AlterOperation.COALESCE_PARTITION:
            case AlterOperation.REORGANIZE_PARTITION:
            case AlterOperation.EXCHANGE_PARTITION:
                return FormatPartitionOperation();
        }

        // 通用分支：ADD/DROP COLUMN/MODIFY/CHANGE/ALTER COLUMN/RENAME COLUMN/RENAME TABLE 等
        var sb2 = new System.Text.StringBuilder(Operation.ToString().Replace('_', ' '));
        if (ColumnName != null) sb2.Append(' ').Append(ColumnName);
        if (DataType != null) sb2.Append(' ').Append(DataType);
        if (OptionalSpecifier != null) sb2.Append(' ').Append(OptionalSpecifier);
        if (ColDataTypeList != null && ColDataTypeList.Count > 0)
            sb2.Append(' ').Append(string.Join(", ", ColDataTypeList));

        // 约束分支：输出完整 CONSTRAINT name TYPE (cols) [USING INDEX ...]
        if (!string.IsNullOrEmpty(ConstraintType))
        {
            if (!string.IsNullOrEmpty(ConstraintSymbol))
                sb2.Append(" CONSTRAINT ").Append(ConstraintSymbol);
            sb2.Append(' ').Append(ConstraintType);
        }
        if (PkColumns != null && PkColumns.Count > 0)
            sb2.Append(" (").Append(string.Join(", ", PkColumns)).Append(')');
        if (UkColumns != null && UkColumns.Count > 0)
            sb2.Append(" (").Append(string.Join(", ", UkColumns)).Append(')');
        if (HasUsingIndex)
        {
            sb2.Append(" USING INDEX");
            if (UsingIndex != null) sb2.Append(' ').Append(UsingIndex);
        }
        if (PartitionDefinitions != null)
        {
            sb2.Append(" (");
            for (int i = 0; i < PartitionDefinitions.Count; i++)
            {
                if (i > 0) sb2.Append(", ");
                sb2.Append(PartitionDefinitions[i]);
            }
            sb2.Append(')');
        }
        if (PartitionNames != null)
            sb2.Append(' ').Append(string.Join(", ", PartitionNames));
        if (Parameters != null && Parameters.Count > 0)
            sb2.Append(' ').Append(string.Join(", ", Parameters));
        return sb2.ToString();
    }

    /// <summary>
    /// 格式化分区操作（ADD/DROP/TRUNCATE/COALESCE/REORGANIZE/EXCHANGE PARTITION）。
    /// </summary>
    private string FormatPartitionOperation()
    {
        var sb = new System.Text.StringBuilder();
        switch (Operation)
        {
            case AlterOperation.ADD_PARTITION:
                sb.Append("ADD PARTITION");
                if (PartitionDefinitions != null && PartitionDefinitions.Count > 0)
                {
                    sb.Append(" (");
                    for (int i = 0; i < PartitionDefinitions.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(PartitionDefinitions[i]);
                    }
                    sb.Append(')');
                }
                break;
            case AlterOperation.DROP_PARTITION:
                sb.Append("DROP PARTITION");
                if (PartitionNames != null) sb.Append(' ').Append(string.Join(", ", PartitionNames));
                break;
            case AlterOperation.TRUNCATE_PARTITION:
                sb.Append("TRUNCATE PARTITION");
                if (PartitionNames != null) sb.Append(' ').Append(string.Join(", ", PartitionNames));
                break;
            case AlterOperation.COALESCE_PARTITION:
                sb.Append("COALESCE PARTITION");
                if (CoalescePartitionNumber.HasValue) sb.Append(' ').Append(CoalescePartitionNumber.Value);
                break;
            case AlterOperation.REORGANIZE_PARTITION:
                sb.Append("REORGANIZE PARTITION");
                if (PartitionNames != null) sb.Append(' ').Append(string.Join(", ", PartitionNames));
                if (PartitionDefinitions != null && PartitionDefinitions.Count > 0)
                {
                    sb.Append(" INTO (");
                    for (int i = 0; i < PartitionDefinitions.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(PartitionDefinitions[i]);
                    }
                    sb.Append(')');
                }
                break;
            case AlterOperation.EXCHANGE_PARTITION:
                sb.Append("EXCHANGE PARTITION");
                if (PartitionNames != null) sb.Append(' ').Append(string.Join(", ", PartitionNames));
                if (!string.IsNullOrEmpty(ExchangePartitionTable)) sb.Append(" WITH TABLE ").Append(ExchangePartitionTable);
                break;
        }
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

        /// <summary>ALTER COLUMN x TYPE int 是否显式输出 TYPE 关键字（对齐上游 withType）。</summary>
        public bool WithType { get; set; }

        public override string ToString()
        {
            var typeKw = WithType ? "TYPE " : "";
            return $"{ColumnName} {typeKw}{DataType}";
        }
    }
}

/// <summary>
/// ALTER COLUMN x 的具体动作，对齐上游 ALTER 分支列级约束变更。
/// </summary>
public enum AlterColumnAction
{
    /// <summary>ALTER COLUMN x SET DEFAULT expr</summary>
    SetDefault,

    /// <summary>ALTER COLUMN x DROP DEFAULT</summary>
    DropDefault,

    /// <summary>ALTER COLUMN x SET NOT NULL</summary>
    SetNotNull,

    /// <summary>ALTER COLUMN x DROP NOT NULL</summary>
    DropNotNull,

    /// <summary>ALTER COLUMN x TYPE dataType</summary>
    Type,

    /// <summary>ALTER COLUMN x SET DATA TYPE dataType（SQL 标准）</summary>
    SetDataType,

    /// <summary>ALTER COLUMN x SET VISIBLE</summary>
    SetVisible,

    /// <summary>ALTER COLUMN x SET INVISIBLE</summary>
    SetInvisible,
}
