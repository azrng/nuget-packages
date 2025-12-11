using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azrng.EFCore.AutoAudit.Domain;

[Table("audit_record")]
public class AuditRecord
{
    public AuditRecord()
    {
        // 这里在.net9中可以创建带排序的Guid
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [Column("id")]
    public string Id { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    [Required]
    [StringLength(128)]
    [Column("table_name")]
    public string TableName { get; set; } = null!;

    /// <summary>
    /// 操作类型
    /// </summary>
    [Column("operation_type")]
    [Required]
    public DataOperationType OperationType { get; set; }

    [Column("object_id")]
    [StringLength(256)]
    public string? ObjectId { get; set; }

    /// <summary>
    /// 老值
    /// </summary>
    [Column("origin_value")]
    public string? OriginValue { get; set; }

    /// <summary>
    /// 新值
    /// </summary>
    [Column("new_value")]
    public string? NewValue { get; set; }

    /// <summary>
    /// 扩展
    /// </summary>
    [Column("extra")]
    public string? Extra { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [StringLength(128)]
    [Column("updater")]
    public string? Updater { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("update_time")]
    [Required]
    public DateTimeOffset UpdatedTime { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [Column("is_success")]
    [Required]
    public bool IsSuccess { get; set; }
}