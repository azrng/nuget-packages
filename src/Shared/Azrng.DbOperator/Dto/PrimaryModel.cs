namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 主键信息
    /// </summary>
    public class PrimaryModel
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
        /// 约束名
        /// </summary>
        public string ColumnConstraintName { get; set; }
    }
}