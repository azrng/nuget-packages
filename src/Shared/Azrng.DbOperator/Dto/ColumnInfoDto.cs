namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 查询表下列信息
    /// </summary>
    public class ColumnInfoDto
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
        /// 列类型
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        /// 列长度
        /// </summary>
        public string ColumnLength { get; set; }

        /// <summary>
        /// 列默认值
        /// </summary>
        public string ColumnDefault { get; set; }

        /// <summary>
        /// 是否为标识列
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 列备注
        /// </summary>
        public string? ColumnComment { get; set; }

        /// <summary>
        /// 是否为null
        /// </summary>
        public bool IsNull { get; set; }

        /// <summary>
        /// 是否为主键列
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 是否是外键
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int RowNumber { get; set; }
    }
}