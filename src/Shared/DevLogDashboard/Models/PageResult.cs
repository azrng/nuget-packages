namespace DevLogDashboard;

/// <summary>
/// 分页结果模型
/// </summary>
/// <typeparam name="T"></typeparam>
public class PageResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext => PageIndex < TotalPages;

    public static PageResult<T> Create(List<T> items, int total, int pageIndex, int pageSize)
    {
        return new PageResult<T>
        {
            Items = items,
            Total = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}
