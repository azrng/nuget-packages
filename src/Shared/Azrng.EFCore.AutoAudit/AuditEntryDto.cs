using Azrng.EFCore.AutoAudit.Config;
using Azrng.EFCore.AutoAudit.Domain;
using Azrng.EFCore.AutoAudit.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 审计记录
/// </summary>
public class AuditEntryDto
{
    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; } = null!;

    /// <summary>
    /// 原始值
    /// </summary>
    public Dictionary<string, object?>? OriginalValues { get; set; }

    /// <summary>
    /// 新值
    /// </summary>
    public Dictionary<string, object?>? NewValues { get; set; }

    public Dictionary<string, object?> KeyValues { get; } = new();

    /// <summary>
    /// 操作类型
    /// </summary>
    public DataOperationType OperationType { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    public Dictionary<string, object?> Properties { get; } = new();

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Succeeded { get; set; }

    internal AuditRecord ToAuditRecord()
    {
        return new AuditRecord
        {
            TableName = TableName,
            OperationType = OperationType,
            Extra = Properties.Count == 0 ? null : Properties.ToJson(),
            OriginValue = OriginalValues?.ToJson(),
            NewValue = NewValues?.ToJson(),
            ObjectId = KeyValues.ToJson(),
            UpdatedTime = UpdatedAt,
            Updater = UpdatedBy,
            IsSuccess = Succeeded
        };
    }
}

internal sealed class InternalAuditEntryDto : AuditEntryDto
{
    /// <summary>
    /// 临时属性
    /// </summary>
    public List<PropertyEntry>? TemporaryProperties { get; set; }

    public InternalAuditEntryDto(EntityEntry entityEntry)
    {
        TableName = entityEntry.Metadata.GetTableName() ?? entityEntry.Metadata.Name;
        OperationType = entityEntry.State switch
        {
            EntityState.Added => DataOperationType.Add,
            EntityState.Deleted => DataOperationType.Delete,
            EntityState.Modified => DataOperationType.Update,
            _ => OperationType
        };

        // 预检查是否有临时属性
        var hasTemporaryProperties = false;
        foreach (var propertyEntry in entityEntry.Properties)
        {
            // 应用属性过滤器
            if (AuditConfig.Options.PropertyFilters.Any(f => f.Invoke(entityEntry, propertyEntry) == false))
            {
                continue;
            }

            if (propertyEntry.IsTemporary)
            {
                hasTemporaryProperties = true;
                TemporaryProperties ??= new List<PropertyEntry>(4);
                TemporaryProperties.Add(propertyEntry);
                continue;
            }

            var columnName = propertyEntry.GetColumnName();

            // 处理主键
            if (propertyEntry.Metadata.IsPrimaryKey())
            {
                KeyValues[columnName] = propertyEntry.CurrentValue;
            }

            // 根据操作类型处理属性值
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    NewValues ??= new Dictionary<string, object?>();
                    NewValues[columnName] = propertyEntry.CurrentValue;
                    break;

                case EntityState.Deleted:
                    OriginalValues ??= new Dictionary<string, object?>();
                    OriginalValues[columnName] = propertyEntry.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (propertyEntry.IsModified || AuditConfig.Options.SaveUnModifiedProperties)
                    {
                        OriginalValues ??= new Dictionary<string, object?>();
                        NewValues ??= new Dictionary<string, object?>();
                        OriginalValues[columnName] = propertyEntry.OriginalValue;
                        NewValues[columnName] = propertyEntry.CurrentValue;
                    }
                    break;
            }
        }
    }
}