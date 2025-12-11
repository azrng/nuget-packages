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

        if (entityEntry.Properties.Any(x => x.IsTemporary))
        {
            TemporaryProperties = new List<PropertyEntry>(4);
        }

        switch (entityEntry.State)
        {
            case EntityState.Added:
                OperationType = DataOperationType.Add;
                NewValues = new Dictionary<string, object?>();
                break;
            case EntityState.Deleted:
                OperationType = DataOperationType.Delete;
                OriginalValues = new Dictionary<string, object?>();
                break;
            case EntityState.Modified:
                OperationType = DataOperationType.Update;
                OriginalValues = new Dictionary<string, object?>();
                NewValues = new Dictionary<string, object?>();
                break;
        }

        foreach (var propertyEntry in entityEntry.Properties)
        {
            if (AuditConfig.Options.PropertyFilters.Any(f => f.Invoke(entityEntry, propertyEntry) == false))
            {
                continue;
            }

            if (propertyEntry.IsTemporary)
            {
                TemporaryProperties!.Add(propertyEntry);
                continue;
            }

            var columnName = propertyEntry.GetColumnName();
            if (propertyEntry.Metadata.IsPrimaryKey())
            {
                KeyValues[columnName] = propertyEntry.CurrentValue;
            }

            switch (entityEntry.State)
            {
                case EntityState.Added:
                    NewValues![columnName] = propertyEntry.CurrentValue;
                    break;

                case EntityState.Deleted:
                    OriginalValues![columnName] = propertyEntry.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (propertyEntry.IsModified || AuditConfig.Options.SaveUnModifiedProperties)
                    {
                        OriginalValues![columnName] = propertyEntry.OriginalValue;
                        NewValues![columnName] = propertyEntry.CurrentValue;
                    }

                    break;
            }
        }
    }
}