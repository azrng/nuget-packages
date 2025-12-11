namespace Azrng.DbOperator.Dto
{
    /// <summary>
    /// 数据库下的存储过程
    /// </summary>
    public class DbProcModel
    {
        /// <summary>
        /// schema
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string ProcName { get; set; }

        /// <summary>
        /// 输入参数
        /// </summary>
        public string InputParam { get; set; }

        /// <summary>
        /// 输出参数
        /// </summary>
        public string OutputParam { get; set; }

        /// <summary>
        /// 定义
        /// </summary>
        public string ProcDefinition { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ProcDescription { get; set; }
    }

    public class ProcModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string ProcName { get; set; }

        /// <summary>
        /// 输入参数
        /// </summary>
        public string InputParam { get; set; }

        /// <summary>
        /// 输出参数
        /// </summary>
        public string OutputParam { get; set; }

        /// <summary>
        /// 定义
        /// </summary>
        public string ProcDefinition { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ProcDescription { get; set; }
    }
}