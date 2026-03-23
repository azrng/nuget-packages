namespace Azrng.DbOperator.Dto
{
    public class ViewModel
    {
        /// <summary>
        /// 视图名称
        /// </summary>
        public string ViewName { get; set; } = string.Empty;

        /// <summary>
        /// 视图所有者
        /// </summary>
        public string ViewOwner { get; set; } = string.Empty;

        /// <summary>
        /// 视图定义
        /// </summary>
        public string ViewDefinition { get; set; } = string.Empty;

        /// <summary>
        /// 视图描述
        /// </summary>
        public string ViewDescription { get; set; } = string.Empty;
    }
}
