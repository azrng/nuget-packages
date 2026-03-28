namespace Azrng.DataAccess.Dto
{
    /// <summary>
    /// 数据库例程信息
    /// </summary>
    public class RoutineModel
    {
        /// <summary>
        /// Schema 名称
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// 例程名称
        /// </summary>
        public string RoutineName { get; set; } = string.Empty;

        /// <summary>
        /// 例程类型
        /// </summary>
        public string RoutineType { get; set; } = string.Empty;

        /// <summary>
        /// 输入参数
        /// </summary>
        public string InputParam { get; set; } = string.Empty;

        /// <summary>
        /// 输出参数
        /// </summary>
        public string OutputParam { get; set; } = string.Empty;

        /// <summary>
        /// 定义
        /// </summary>
        public string RoutineDefinition { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string RoutineDescription { get; set; } = string.Empty;
    }
}
