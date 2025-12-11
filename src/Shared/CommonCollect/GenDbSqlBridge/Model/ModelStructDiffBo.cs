namespace CommonCollect.GenDbSqlBridge.Model
{
    public class ModelStructDiffBo
    {
        public ModelStructDiffBo(StructDbTypeEnum structDbType)
        {
            OriginDbType = structDbType;
            TargetDbType = StructDbTypeEnum.PG;
        }

        /// <summary>
        /// 源库类型
        /// </summary>
        public StructDbTypeEnum OriginDbType { get; set; }

        /// <summary>
        /// 目标库类型
        /// </summary>
        public StructDbTypeEnum TargetDbType { get; set; }

        /// <summary>
        /// 默认域
        /// </summary>
        public string DefaultSchema { get; set; } = "default";

        /// <summary>
        /// schema区别
        /// </summary>
        public StructInfoBo SchemaDiff { get; set; } = new StructInfoBo();

        /// <summary>
        /// 表区别
        /// </summary>
        public TableStructInfoBo TableStructDiff { get; set; } = new TableStructInfoBo();

        /// <summary>
        /// 列信息区别
        /// </summary>
        public ColumnStructInfoBo ColumnStructInfoDiff { get; set; } = new ColumnStructInfoBo();
    }

    public class StructInfoBo
    {
        public List<string> Adds { get; set; } = new List<string>();

        public List<string> Removes { get; set; } = new List<string>();
    }

    public class ColumnStructInfoBo
    {
        public List<RemoveColumnInfo> Removes { get; set; } = new List<RemoveColumnInfo>();

        public List<ColumnBaseBo> Adds = new List<ColumnBaseBo>();
    }

    public class ColumnBaseBo : ColumnModel
    {
        public ColumnBaseBo() { }

        public ColumnBaseBo(string schemaName, string tableName, ModelTableColumnBo column)
        {
            SchemaName = schemaName;
            TableName = tableName;

            ColDefault = column.DefaultValue;
            ColName = column.ColumnName;
            ColComment = column.ColumnCnName;
            Is_Null = column.IsNull;
            IsPrimary = column.IsPrimaryKey;
            IsForeignKey = column.IsForeignKey;
            Is_Identity = column.IsIdentity;
            StructColType = column.ColumnType;
            StructColLength = column.ColumnLength;
            StructDefaultValue = column.DefaultValue;
            RowNumber = column.RowNumber ?? 0;
        }

        public string SchemaName { get; set; }
    }

    public class TableStructInfoBo
    {
        public List<RemoveTableInfo> Removes { get; set; } = new List<RemoveTableInfo>();

        public List<TableStructInfo> Adds { get; set; } = new List<TableStructInfo>();
    }

    public class RemoveTableInfo
    {
        public RemoveTableInfo(string schemaName, string tableName)
        {
            SchemaName = schemaName;
            TableName = tableName;
        }

        public string SchemaName { get; set; }

        public string TableName { get; set; }
    }

    public class TableStructInfo : TableModel
    {
        public string SchemaName { get; set; }
    }

    public class RemoveColumnInfo
    {
        public RemoveColumnInfo(string schemaName, string tableName, string columnName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            ColumnName = columnName;
        }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }
    }
}