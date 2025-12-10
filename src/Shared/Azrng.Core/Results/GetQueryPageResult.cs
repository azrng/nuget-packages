using Azrng.Core.Requests;
using System;
using System.Collections.Generic;

namespace Azrng.Core.Results
{
    /// <summary>
    /// 分页返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GetQueryPageResult<T> where T : class
    {
        public GetQueryPageResult()
        {
        }

        public GetQueryPageResult(List<T> rows, GetQueryPageResult pageInfo)
        {
            Rows = rows;
            PageInfo = pageInfo;
        }

        public GetQueryPageResult(List<T> rows, int pageIndex, int pageSize, long totalCount)
        {
            Rows = rows;
            PageInfo = new GetQueryPageResult(pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<T> Rows { get; set; } = new List<T>(0);

        /// <summary>
        /// 分页信息
        /// </summary>
        public GetQueryPageResult PageInfo { get; set; } = new GetQueryPageResult();
    }

    /// <summary>
    /// 分页信息
    /// </summary>
    public class GetQueryPageResult : GetPageRequest
    {
        public GetQueryPageResult()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vm">页信息</param>
        /// <param name="totalCount">总条数</param>
        public GetQueryPageResult(GetPageRequest vm, long totalCount)
        {
            PageIndex = vm.PageIndex;
            PageSize = vm.PageSize;
            Total = totalCount;
            TotalPage = totalCount == 0 ? 0 : (long)Math.Ceiling(totalCount * 1.0 / vm.PageSize);
        }

        public GetQueryPageResult(int pageIndex, int pageSize, long totalCount)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Total = totalCount;
            TotalPage = totalCount == 0 ? 0 : (long)Math.Ceiling(totalCount * 1.0 / pageSize);
        }

        /// <summary>
        /// 总共多少页
        /// </summary>
        public long TotalPage { get; set; }

        /// <summary>
        /// 一共多少条记录
        /// </summary>
        public long Total { get; set; }
    }
}