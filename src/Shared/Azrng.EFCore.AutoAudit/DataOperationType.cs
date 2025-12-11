namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 表示数据操作的类型，用于审计日志记录。
/// </summary>
public enum DataOperationType
{
    /// <summary>
    /// 查询操作，如 SELECT。
    /// </summary>
    Query,

    /// <summary>
    /// 添加操作，如 INSERT。
    /// </summary>
    Add,

    /// <summary>
    /// 更新操作，如 UPDATE。
    /// </summary>
    Update,

    /// <summary>
    /// 删除操作，如 DELETE。
    /// </summary>
    Delete
}