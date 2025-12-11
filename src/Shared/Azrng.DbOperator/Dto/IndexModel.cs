namespace Azrng.DbOperator.Dto
{
    public class IndexModel
    {
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 索引名称
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// 索引定义
        /// </summary>
        public string Indexdef { get; set; }

        /// <summary>
        /// 是否唯一索引
        /// </summary>
        public bool Indisunique { get; set; }

        /// <summary>
        /// 是否主键索引
        /// </summary>
        public bool Indisprimary { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 索引位置
        /// </summary>
        public int IndexPostion { get; set; }

        /// <summary>
        /// 索引排序方式
        /// </summary>
        public string IndexSort { get; set; }
    }
}