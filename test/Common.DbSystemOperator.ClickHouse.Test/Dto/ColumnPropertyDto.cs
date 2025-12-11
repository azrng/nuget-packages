namespace Common.DbSystemOperator.ClickHouse.Test.Dto
{
    /// <summary>
    /// 列属性
    /// </summary>
    public class ColumnPropertyDto
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 列类型
        /// </summary>
        public string Type { get; set; } = null!;
    }
}