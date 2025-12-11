namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 外键类
    /// </summary>
    public class ForeignModel
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 外键约束名
        /// </summary>
        public string ColumnConstraintName { get; set; }

        /// <summary>
        /// 外键schema名
        /// </summary>
        public string ForeignSchemaName { get; set; }

        /// <summary>
        /// 外键表明
        /// </summary>
        public string ForeignTableName { get; set; }

        /// <summary>
        /// 外键列名
        /// </summary>
        public string ForeignColumnName { get; set; }
    }
}