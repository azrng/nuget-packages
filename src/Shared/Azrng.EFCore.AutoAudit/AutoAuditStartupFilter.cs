using Azrng.Core.Model;
using Azrng.EFCore.AutoAudit.Config;
using Azrng.EFCore.AutoAudit.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azrng.EFCore.AutoAudit;

/// <summary>
/// 执行sql迁移过滤器
/// </summary>
public class AutoAuditStartupFilter : IStartupFilter
{
    private readonly ILogger<AutoAuditStartupFilter> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AutoAuditStartupFilter(ILogger<AutoAuditStartupFilter> logger,
                                  IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            ExecuteAsync().GetAwaiter().GetResult();
            next(builder);
        };
    }

    private async Task ExecuteAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContent = scope.ServiceProvider.GetRequiredService<AuditRecordsDbContext>();

        await using var conn = dbContent.Database.GetDbConnection();

        switch (AuditConfig.Options.DatabaseType)
        {
            case DatabaseType.MySql:
                await dbContent.Database.ExecuteSqlRawAsync(GetMySqlScript());
                break;
            case DatabaseType.SqlServer:
                await dbContent.Database.ExecuteSqlRawAsync(GetSqlServerScript());
                break;
            case DatabaseType.Sqlite:
                await dbContent.Database.ExecuteSqlRawAsync(GetSqliteScript());
                break;
            case DatabaseType.Oracle:
                await dbContent.Database.ExecuteSqlRawAsync(GetOracleScript());
                break;
            case DatabaseType.PostgresSql:
                await dbContent.Database.ExecuteSqlRawAsync(GetPgSqlScript());
                break;
            case DatabaseType.InMemory:
                // InMemory数据库不需要创建表，EF Core会自动处理
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // await dbContent.Database.EnsureCreatedAsync();
        _logger.LogInformation($"生成审计表成功");
    }

    private string GetPgSqlScript()
    {
        return @"create table if not exists public.audit_record
(
    id             varchar(50)              not null
        constraint audit_record_pk
            primary key,
    table_name     varchar(120),
    operation_type integer,
    object_id      text,
    origin_value   text,
    new_value      text,
    extra          text,
    updater        text,
    update_time    timestamp with time zone not null,
    is_success      boolean                  not null
);
comment on table public.audit_record is '审计记录表';
comment on column public.audit_record.table_name is '表名';
comment on column public.audit_record.operation_type is '操作类型 0查询，1添加，2修改，3删除';
comment on column public.audit_record.origin_value is '老值';
comment on column public.audit_record.new_value is '新值';
comment on column public.audit_record.extra is '扩展';
comment on column public.audit_record.updater is '更新人';
comment on column public.audit_record.update_time is '更新时间';
comment on column public.audit_record.is_success is '是否成功';";
    }

    private string GetMySqlScript()
    {
        return @"CREATE TABLE IF NOT EXISTS `audit_record` (
    `id` VARCHAR(50) NOT NULL COMMENT '主键',
    `table_name` VARCHAR(120) NULL COMMENT '表名',
    `operation_type` INT NULL COMMENT '操作类型 0查询，1添加，2修改，3删除',
    `object_id` TEXT NULL COMMENT '对象ID',
    `origin_value` TEXT NULL COMMENT '老值',
    `new_value` TEXT NULL COMMENT '新值',
    `extra` TEXT NULL COMMENT '扩展',
    `updater` TEXT NULL COMMENT '更新人',
    `update_time` DATETIME NOT NULL COMMENT '更新时间',
    `is_success` BOOLEAN NOT NULL COMMENT '是否成功',
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='审计记录表';";
    }

    private string GetSqlServerScript()
    {
        return @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='audit_record' AND xtype='U')
CREATE TABLE [dbo].[audit_record] (
    [id] NVARCHAR(50) NOT NULL,
    [table_name] NVARCHAR(120) NULL,
    [operation_type] INT NULL,
    [object_id] NVARCHAR(MAX) NULL,
    [origin_value] NVARCHAR(MAX) NULL,
    [new_value] NVARCHAR(MAX) NULL,
    [extra] NVARCHAR(MAX) NULL,
    [updater] NVARCHAR(MAX) NULL,
    [update_time] DATETIMEOFFSET NOT NULL,
    [is_success] BIT NOT NULL,
    CONSTRAINT [PK_audit_record] PRIMARY KEY CLUSTERED ([id] ASC)
);";
    }

    private string GetSqliteScript()
    {
        return @"CREATE TABLE IF NOT EXISTS audit_record (
    id TEXT NOT NULL PRIMARY KEY,
    table_name TEXT,
    operation_type INTEGER,
    object_id TEXT,
    origin_value TEXT,
    new_value TEXT,
    extra TEXT,
    updater TEXT,
    update_time TEXT NOT NULL,
    is_success INTEGER NOT NULL
);";
    }

    private string GetOracleScript()
    {
        return @"BEGIN
    -- 检查表是否存在
    IF NOT EXISTS (SELECT 1 FROM user_tables WHERE table_name = 'AUDIT_RECORD') THEN
        -- 创建表
        EXECUTE IMMEDIATE 'CREATE TABLE AUDIT_RECORD (
            ID VARCHAR2(50) NOT NULL,
            TABLE_NAME VARCHAR2(120),
            OPERATION_TYPE NUMBER(10),
            OBJECT_ID CLOB,
            ORIGIN_VALUE CLOB,
            NEW_VALUE CLOB,
            EXTRA CLOB,
            UPDATER CLOB,
            UPDATE_TIME TIMESTAMP WITH TIME ZONE NOT NULL,
            IS_SUCCESS NUMBER(1) NOT NULL,
            CONSTRAINT PK_AUDIT_RECORD PRIMARY KEY (ID)
        )';";
    }
}