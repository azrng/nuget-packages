using System.ComponentModel;

namespace Common.DbSystemOperator.Pgsql.Test.Model
{
    /// <summary>
    /// 表格导出模型
    /// </summary>
    public class ModelExportProInfo
    {
        /// <summary>
        /// 数据表种类sheet
        /// </summary>
        public List<DataTableCategorySchemaExport> DataTableCategorySheet { get; set; } = new List<DataTableCategorySchemaExport>();

        /// <summary>
        /// sheet sheet
        /// </summary>
        public List<SchemaSheetExport> SchemaSheets { get; set; } = new List<SchemaSheetExport>();
    }

    public class DataTableCategorySchemaExport
    {
        /// <summary>
        /// schema名称
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// schema中文名称
        /// </summary>
        public string SchemaCnName { get; set; }

        public List<SchemaStructModelExport> SchemaStructModelExports { get; set; } = new List<SchemaStructModelExport>();
    }

    public class SchemaStructModelExport
    {
        /// <summary>
        /// 模型类型名称(表,视图,存储过程)
        /// </summary>
        [Description("类型")]
        public string StructTypeName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Description("对象名称")]
        public string StructModelName { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        [Description("对象中文名")]
        public string StructModelCnName { get; set; }
    }

    public class SchemaSheetExport
    {
        public string SchemaName { get; set; }

        public string SchemaRemark { get; set; }

        public List<SchemaSheetTableExport> SchemaSheetTables { get; set; } = new List<SchemaSheetTableExport>();

        public List<SchemaSheetViewExport> SchemaSheetViews { get; set; } = new List<SchemaSheetViewExport>();

        public List<SchemaSheetProcExport> SchemaSheetProcList { get; set; } = new List<SchemaSheetProcExport>();
    }

    public class SchemaSheetProcExport
    {
        /// <summary>
        /// 存储过程名字
        /// </summary>
        public string ProcName { get; set; }

        /// <summary>
        /// 存储过程中文名
        /// </summary>
        public string ProcCnName { get; set; }

        /// <summary>
        /// 存储过程备注
        /// </summary>
        public string ProcComment { get; set; }

        /// <summary>
        /// 建存储过程SQL
        /// </summary>
        [Description("建存储过程SQL")]
        public string CreateSqlStr { get; set; }
    }

    public class SchemaSheetViewExport
    {
        /// <summary>
        /// 视图名字
        /// </summary>
        public string ViewName { get; set; }

        public string ViewCnName { get; set; }

        public string ViewComment { get; set; }

        [Description("建视图SQL")]
        public string CreateSqlStr { get; set; }
    }

    public class SchemaSheetTableExport
    {
        /// <summary>
        /// 表名字
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 表备注
        /// </summary>
        public string TableComment { get; set; }

        /// <summary>
        /// 表字段
        /// </summary>
        public List<SchemaSheetTableColumnExport> SchemaSheetTableColumns { get; set; } = new List<SchemaSheetTableColumnExport>();

        /// <summary>
        /// 表索引
        /// </summary>
        public List<SchemaSheetTableIndexExport> SchemaSheetTableIndexList { get; set; } = new List<SchemaSheetTableIndexExport>();

        /// <summary>
        /// 建表SQL
        /// </summary>
        [Description("建表SQL")]
        public string CreateSqlStr { get; set; }
    }

    /// <summary>
    /// 表字段导出模型
    /// </summary>
    public class SchemaSheetTableColumnExport
    {
        /// <summary>
        /// 字段名
        /// </summary>
        [Description("字段名")]
        public string ColumnName { get; set; } = "无";

        /// <summary>
        /// 字段中文名
        /// </summary>
        [Description("中文名")]
        public string ColumnCnName { get; set; } = "无";

        /// <summary>
        /// 字段类型
        /// </summary>
        [Description("字段类型")]
        public string ColumnType { get; set; } = "无";

        /// <summary>
        /// 字段说明
        /// </summary>
        [Description("字段说明")]
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// 默认值
        /// </summary>
        [Description("默认值")]
        public string DefaultValue { get; set; } = "无";

        /// <summary>
        /// 主键
        /// </summary>
        [Description("主键")]
        public string IsPrimary { get; set; } = "无";

        /// <summary>
        /// 外键
        /// </summary>
        [Description("外键")]
        public string IsForeignKey { get; set; } = "无";

        /// <summary>
        /// 非空
        /// </summary>
        [Description("非空")]
        public string IsNotNull { get; set; } = "无";
    }

    public class SchemaSheetTableIndexExport
    {
        [Description("索引")]
        public string Index { get; set; } = "";

        [Description("索引类型")]
        public string IndexType { get; set; } = "无";

        [Description("索引名")]
        public string IndexName { get; set; } = "无";

        [Description("索引字段列表")]
        public string ColName { get; set; } = "无";

        [Description("说明")]
        public string Description { get; set; } = "无";

        [Description("是否唯一")]
        public string Indisunique { get; set; } = "无";
    }
}