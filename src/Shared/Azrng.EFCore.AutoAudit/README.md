# Azrng.EFCore.AutoAudit

一个EFCore自动审计的包

## 预操作

需要手动提前创建审计表，pgsql示例如下

```sql
create table public.audit_record
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

comment on column public.audit_record.is_success is '是否成功';
```

## 操作

### 默认自动审计

```csharp
// 添加业务数据库
builder.Services.AddDbContext<OpenDbContext>((provider, options) =>
{
    options.UseNpgsql(conn);
    options.AddAuditInterceptor(provider);
});

// 添加审计
builder.Services.AddEFCoreAutoAudit(config =>
{
    config // .WithStore<AuditFileStore>() // 自定义存储
        .WithAuditRecordsDbContextStore(options => { options.UseNpgsql(conn); });
});
```

### 忽略指定表

```c#
services.AddEFCoreAutoAudit(builder =>
{
    builder.IgnoreEntity<Test2Entity>() // 忽略指定表 还可以IgnoreTable("test2")
        .WithAuditRecordsDbContextStore(options => { options.UseSqlite("Data Source=d:\\db\\AutoAudit.db");    });
});
```


### 版本更新记录

* 1.0.0-beta1
  * EFCore自动审计包