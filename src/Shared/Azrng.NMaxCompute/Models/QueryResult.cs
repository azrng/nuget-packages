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
    /// 列类型集合（与 <see cref="Columns"/> 等长，S1+ 路径填入，CSV 路径可能为空）
    /// <para>使用 MaxCompute 类型字符串，如 <c>bigint</c>、<c>double</c>。</para>
    /// </summary>
    public string[]? ColumnTypes { get; set; }

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
