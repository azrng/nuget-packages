namespace Azrng.Core.Requests
{
    /// <summary>
    /// 排序类
    /// </summary>
    public class FiledOrderParam
    {
        /// <summary>
        /// 是否正序
        /// </summary>
        public bool IsAsc { get; set; }

        /// <summary>
        /// 排序名称
        /// </summary>
        public string? PropertyName { get; set; }
    }
}