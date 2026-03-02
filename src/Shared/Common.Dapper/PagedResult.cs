using System.Collections.Generic;

namespace Azrng.Dapper.Repository
{
    /// <summary>
    /// 分页查询结果
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();

        public long TotalCount { get; init; }
    }
}
