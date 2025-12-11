using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Azrng.EFCore.EntityTypeConfigurations
{
    /// <summary>
    /// 标识操作者状态配置
    /// </summary>
    /// <typeparam name="T">实体类</typeparam>
    public class EntityTypeConfigurationIdentityOperatorStatus<T> : EntityTypeConfigurationIdentityOperatorStatus<T, long>
        where T : IdentityOperatorStatusEntity
    {
    }

    /// <summary>
    /// 标识操作者状态配置
    /// </summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public class EntityTypeConfigurationIdentityOperatorStatus<T, TKey> : EntityTypeConfigurationIdentityOperator<T, TKey>
        where T : IdentityOperatorStatusEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.Deleted).IsRequired().HasComment("是否删除");
            builder.Property(x => x.Disabled).IsRequired().HasComment("是否禁用");
            base.Configure(builder);
        }
    }
}