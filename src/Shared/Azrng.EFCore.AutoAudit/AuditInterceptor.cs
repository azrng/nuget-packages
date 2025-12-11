using Azrng.EFCore.AutoAudit.Config;
using Azrng.EFCore.AutoAudit.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 审计拦截器，用于在实体保存前和保存后记录审计日志（如用户操作、修改内容等）。
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 保存审计日志的临时列表
    /// </summary>
    private List<AuditEntryDto>? AuditEntryDtos { get; set; }

    /// <summary>
    /// 构造函数，注入服务提供器
    /// </summary>
    /// <param name="serviceProvider">DI 服务提供器</param>
    public AuditInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 在保存之前（同步）执行预处理逻辑
    /// </summary>
    /// <param name="eventData">上下文事件数据</param>
    /// <param name="result">结果（默认为基类处理）</param>
    /// <returns>拦截结果</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
        {
            return base.SavingChanges(eventData, result);
        }

        PreSaveChanges(eventData.Context!); // 执行预保存逻辑
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// 在保存之前（异步）执行预处理逻辑
    /// </summary>
    /// <param name="eventData">上下文事件数据</param>
    /// <param name="result">结果（默认为基类处理）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>拦截结果</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
                                                                          InterceptionResult<int> result,
                                                                          CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        PreSaveChanges(eventData.Context!); // 执行预保存逻辑
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 在保存完成后（同步）执行后处理逻辑
    /// </summary>
    /// <param name="eventData">上下文事件数据</param>
    /// <param name="result">保存结果</param>
    /// <returns>保存结果</returns>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is null)
        {
            return base.SavedChanges(eventData, result);
        }

        PostSaveChanges().GetAwaiter().GetResult(); // 异步处理后置逻辑
        var savedChanges = base.SavedChanges(eventData, result);
        return savedChanges;
    }

    /// <summary>
    /// 在保存完成后（异步）执行后处理逻辑
    /// </summary>
    /// <param name="eventData">上下文事件数据</param>
    /// <param name="result">保存结果</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>保存结果</returns>
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
                                                           CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        await PostSaveChanges(); // 异步处理后置逻辑
        var savedChanges = await base.SavedChangesAsync(eventData, result, cancellationToken);
        return savedChanges;
    }

    /// <summary>
    /// 在保存失败时执行处理逻辑
    /// </summary>
    /// <param name="eventData">上下文错误事件数据</param>
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is null)
        {
            base.SaveChangesFailed(eventData);
        }

        base.SaveChangesFailed(eventData); // 调用基类处理
    }

    /// <summary>
    /// 在保存失败时异步执行处理逻辑
    /// </summary>
    /// <param name="eventData">上下文错误事件数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
                                                CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null)
        {
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken); // 调用基类处理
    }

    /// <summary>
    /// 预保存操作，用于收集需要审计的实体信息
    /// </summary>
    /// <param name="dbContext">当前数据库上下文</param>
    private void PreSaveChanges(DbContext dbContext)
    {
        if (!AuditConfig.Options.AuditEnabled) // 如果审计功能未启用，直接返回
            return;

        if (!_serviceProvider.GetServices<IAuditStore>().Any()) // 如果没有审计存储服务，直接返回
            return;

        if (AuditEntryDtos is null)
        {
            AuditEntryDtos = new List<AuditEntryDto>(); // 初始化审计条目列表
        }
        else
        {
            AuditEntryDtos.Clear(); // 清空已有条目，避免重复记录
        }

        foreach (var entityEntry in dbContext.ChangeTracker.Entries())
        {
            if (entityEntry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue; // 忽略未修改或分离的实体
            }

            // 应用实体过滤器，决定是否需要审计该实体
            if (AuditConfig.Options.EntityFilters.Any(entityFilter =>
                    entityFilter.Invoke(entityEntry) == false))
            {
                continue;
            }

            // 添加审计条目
            AuditEntryDtos.Add(new InternalAuditEntryDto(entityEntry));
        }
    }

    /// <summary>
    /// 保存后操作，用于更新审计条目并保存日志
    /// </summary>
    private async Task PostSaveChanges()
    {
        if (AuditEntryDtos is not { Count: > 0 }) // 如果没有审计条目，直接返回
        {
            return;
        }

        var auditUserIdProvider = AuditConfig.Options.UserIdProviderFactory?.Invoke(_serviceProvider); // 获取用户ID提供者
        var auditUser = auditUserIdProvider?.GetUserId(); // 获取当前用户的ID

        foreach (var entry in AuditEntryDtos)
        {
            // 更新临时属性，用于记录原始值和新值
            if (entry is InternalAuditEntryDto { TemporaryProperties.Count: > 0 } auditEntry)
            {
                foreach (var temporaryProperty in auditEntry.TemporaryProperties)
                {
                    var colName = temporaryProperty.GetColumnName();

                    if (temporaryProperty.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[colName] = temporaryProperty.CurrentValue; // 记录主键值
                    }

                    switch (auditEntry.OperationType)
                    {
                        case DataOperationType.Add:
                            auditEntry.NewValues![colName] = temporaryProperty.CurrentValue;
                            break;

                        case DataOperationType.Delete:
                            auditEntry.OriginalValues![colName] = temporaryProperty.OriginalValue;
                            break;

                        case DataOperationType.Update:
                            auditEntry.OriginalValues![colName] = temporaryProperty.OriginalValue;
                            auditEntry.NewValues![colName] = temporaryProperty.CurrentValue;
                            break;
                    }
                }
            }

            entry.UpdatedBy = auditUser; // 设置更新人
            entry.UpdatedAt = DateTimeOffset.UtcNow; // 设置更新时间
            entry.Succeeded = true; // 标记为成功
        }

        await Task.WhenAll(_serviceProvider.GetServices<IAuditStore>()
                                           .Select(store => store.SaveAsync(AuditEntryDtos))); // 保存所有审计日志
    }
}