namespace Azrng.Core.Requests
{
    /// <summary>
    /// 排序内容
    /// </summary>
    public class SortContent
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sortName">排序名（模型属性名）</param>
        /// <param name="sort">排序方向</param>
        public SortContent(string sortName, SortEnum sort)
        {
            SortName = sortName;
            Sort = sort;
        }

        /// <summary>
        /// 排序方向
        /// </summary>
        public SortEnum Sort { get; set; }

        /// <summary>
        /// 排序的字段名称（模型属性）
        /// </summary>
        public string SortName { get; set; }
    }

    /// <summary>
    /// 排序枚举
    /// </summary>
    public enum SortEnum
    {
        /// <summary>
        /// 正序
        /// </summary>
        Asc = 0,

        /// <summary>
        /// 倒叙
        /// </summary>
        Desc = 1
    }
}