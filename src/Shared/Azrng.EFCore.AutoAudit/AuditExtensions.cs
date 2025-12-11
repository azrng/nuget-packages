using System.Linq.Expressions;
using Azrng.EFCore.AutoAudit.Domain;
using Azrng.EFCore.AutoAudit.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 审计配置的扩展方法，提供链式调用风格的配置方式。
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// 配置审计记录使用数据库作为存储方式。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="optionsConfigure">用于配置数据库上下文的选项配置委托</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder WithAuditRecordsDbContextStore(this IAuditConfigBuilder configBuilder,
                                                                     Action<DbContextOptionsBuilder> optionsConfigure)
    {
        // 注册审计数据库上下文
        configBuilder.Services.AddDbContext<AuditRecordsDbContext>(optionsConfigure);

        // 注册审计存储服务为基于 DbContext 的实现
        configBuilder.WithStore<AuditRecordsDbContextStore>();
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定实体类型的审计记录。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="entityType">要忽略的实体类型</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreEntity(this IAuditConfigBuilder configBuilder, Type entityType)
    {
        // 设置实体过滤器，跳过指定类型的实体
        configBuilder.WithEntityFilter(entityEntry => entityEntry.Entity.GetType() != entityType);
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定实体类型的审计记录（泛型版本）。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <typeparam name="TEntity">要忽略的实体类型</typeparam>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreEntity<TEntity>(this IAuditConfigBuilder configBuilder)
        where TEntity : class
    {
        // 设置实体过滤器，跳过指定类型的实体
        configBuilder.WithEntityFilter(entityEntry => entityEntry.Entity.GetType() != typeof(TEntity));
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定表名的实体审计记录。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="tableName">要忽略的表名</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreTable(this IAuditConfigBuilder configBuilder, string tableName)
    {
        // 设置实体过滤器，跳过指定表名的实体
        configBuilder.WithEntityFilter(entityEntry => entityEntry.Metadata.GetTableName() != tableName);
        return configBuilder;
    }

    /// <summary>
    /// 设置实体过滤器，用于决定哪些实体需要被审计。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="filterFunc">用于判断实体是否需要审计的函数</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder WithEntityFilter(this IAuditConfigBuilder configBuilder,
                                                       Func<EntityEntry, bool> filterFunc)
    {
        // 设置实体过滤器
        configBuilder.WithEntityFilter(filterFunc);
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定属性的审计记录（使用表达式方式）。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="propertyExpression">用于指定属性的表达式</param>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreProperty<TEntity>(this IAuditConfigBuilder configBuilder,
                                                              Expression<Func<TEntity, object>> propertyExpression) where TEntity : class
    {
        // 从表达式中提取属性名
        var propertyName = propertyExpression.GetMemberName();

        // 设置属性过滤器，跳过指定的属性
        configBuilder.WithPropertyFilter(propertyEntry => propertyEntry.Metadata.Name != propertyName);
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定属性的审计记录。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="propertyName">要忽略的属性名</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreProperty(this IAuditConfigBuilder configBuilder, string propertyName)
    {
        // 设置属性过滤器，跳过指定的属性
        configBuilder.WithPropertyFilter(propertyEntry => propertyEntry.Metadata.Name != propertyName);
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定列的审计记录。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="columnName">要忽略的列名</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreColumn(this IAuditConfigBuilder configBuilder, string columnName)
    {
        // 设置属性过滤器，跳过指定的列
        configBuilder.WithPropertyFilter(propertyEntry => propertyEntry.GetColumnName() != columnName);
        return configBuilder;
    }

    /// <summary>
    /// 忽略指定表中的列的审计记录。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="tableName">表名</param>
    /// <param name="columnName">列名</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder IgnoreColumn(this IAuditConfigBuilder configBuilder, string tableName,
                                                   string columnName)
    {
        // 设置属性过滤器，跳过指定表中的列
        configBuilder.WithPropertyFilter((entityEntry, propertyEntry) =>
            entityEntry.Metadata.GetTableName() != tableName && propertyEntry.GetColumnName() != columnName);
        return configBuilder;
    }

    /// <summary>
    /// 设置属性过滤器，用于决定哪些属性需要被审计。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <param name="filterFunc">用于判断属性是否需要审计的函数</param>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder WithPropertyFilter(this IAuditConfigBuilder configBuilder,
                                                         Func<PropertyEntry, bool> filterFunc)
    {
        // 使用闭包包装，转换为接受 EntityEntry 和 PropertyEntry 的函数
        configBuilder.WithPropertyFilter((entity, prop) => filterFunc.Invoke(prop));
        return configBuilder;
    }

    /// <summary>
    /// 配置用户 ID 提供者，使用指定类型的默认构造函数。
    /// </summary>
    /// <param name="configBuilder">配置构建器</param>
    /// <typeparam name="TUserIdProvider">实现 IUserIdProvider 的类型</typeparam>
    /// <returns>返回当前配置构建器，支持链式调用。</returns>
    public static IAuditConfigBuilder WithUserIdProvider<TUserIdProvider>(this IAuditConfigBuilder configBuilder)
        where TUserIdProvider : IUserIdProvider, new()
    {
        // 创建用户 ID 提供者实例
        configBuilder.WithUserIdProvider(new TUserIdProvider());
        return configBuilder;
    }

    /// <summary>
    /// 添加审计拦截器
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder AddAuditInterceptor(this DbContextOptionsBuilder builder,IServiceProvider serviceProvider)
    {
        builder.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        return builder;
    }
}