namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 查询schema下的列
    /// </summary>
    public class GetSchemaColumnInfoDto : ColumnInfoDto
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
    }
}