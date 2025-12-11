namespace Common.YuQueSdk.Dto
{
    /// <summary>
    /// 语雀文档详情
    /// </summary>
    public class DocDetail
    {
        /// <summary>
        /// 文章详情的数据
        /// </summary>
        public DocDetailData Data { get; set; }

        /// <summary>
        /// 拥有的权限
        /// </summary>
        public AbilitiesDto Abilities { get; set; }
    }
}
