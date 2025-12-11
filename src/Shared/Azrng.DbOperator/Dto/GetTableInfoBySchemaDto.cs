namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 根据schema查询表信息
    /// </summary>
    public class GetTableInfoBySchemaDto
    {
        /// <summary>
        /// 表ID
        /// </summary>
        public int TableId { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 表备注
        /// </summary>
        public string TableComment { get; set; }
    }
}