namespace Azrng.NMaxCompute.Models;

/// <summary>
/// 查询结果模型
/// </summary>
public class QueryResult
{
    /// <summary>
    /// 列名集合
    /// </summary>
    public string[] Columns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 行数据集合
    /// </summary>
    public object[][] Rows { get; set; } = Array.Empty<object[]>();

    /// <summary>
    /// 行数
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public string? ExecutionTime { get; set; }
}

/// <summary>
/// 查询响应模型
/// </summary>
public class QueryResponse<T>
{
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
}
