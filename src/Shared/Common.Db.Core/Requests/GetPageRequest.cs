using System;

namespace Azrng.Core.Requests
{
    /// <summary>
    /// 获取分页查询请求类
    /// </summary>
    public class GetPageRequest
    {
        public GetPageRequest() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页显示数</param>
        /// <param name="keyword">关键字</param>
        public GetPageRequest(int pageIndex, int pageSize, string? keyword = null)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Keyword = keyword ?? string.Empty;
        }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页多少条记录
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 查询关键字
        /// </summary>
        public string? Keyword { get; set; }
    }

    /// <summary>
    /// 获取分页查询请求类
    /// </summary>
    public class GetPageSortRequest : GetPageRequest
    {
        public GetPageSortRequest()
        {
            SortContents = Array.Empty<SortContent>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页显示数</param>
        /// <param name="sortContents">排序内容</param>
        public GetPageSortRequest(int pageIndex, int pageSize, SortContent[]? sortContents = null)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            SortContents = sortContents ?? Array.Empty<SortContent>();
        }

        /// <summary>
        /// 排序信息
        /// </summary>
        public SortContent[] SortContents { get; set; }
    }
}