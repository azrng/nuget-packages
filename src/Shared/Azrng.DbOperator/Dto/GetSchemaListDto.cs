namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 获取数据库Schema列表
    /// </summary>
    public class GetSchemaListDto
    {
        /// <summary>
        /// schema名称
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// schema说明
        /// </summary>
        public string SchemaComment { get; set; }
    }
}