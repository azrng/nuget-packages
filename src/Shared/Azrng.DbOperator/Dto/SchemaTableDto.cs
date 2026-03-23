namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// schema表
    /// </summary>
    public class SchemaTableDto
    {
        /// <summary>
        /// schema
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; } = string.Empty;
    }
}
