using Azrng.Core.Model;
using Azrng.EFCore.Entities;
using Azrng.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Azrng.EFCore.EntityTypeConfigurations
{
    /// <summary>
    /// 标识标识操作者配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityTypeConfigurationIdentityOperator<T> : EntityTypeConfigurationIdentityOperator<T, long>
        where T : IdentityOperatorEntity { }

    /// <summary>
    /// 标识标识操作者配置
    /// </summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public class EntityTypeConfigurationIdentityOperator<T, TKey> : EntityTypeConfigurationIdentity<T, TKey>
        where T : IdentityOperatorEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.Creator)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue(string.Empty)
                   .HasCustomerColumnName(EfCoreGlobalConfig.UseOldUpdateColumn ? EfCoreGlobalConfig.OldCreator : string.Empty)
                   .HasComment("创建人");
            builder.Property(x => x.CreateTime)
                   .IsUnTimeZoneDateTime(EfCoreGlobalConfig.DbType == DatabaseType.PostgresSql)
                   .IsRequired()
                   .HasComment("创建时间");
            builder.Property(x => x.Updater)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue(string.Empty)
                   .HasCustomerColumnName(EfCoreGlobalConfig.UseOldUpdateColumn ? EfCoreGlobalConfig.OldUpdater : string.Empty)
                   .HasComment("最后修改人");
            builder.Property(x => x.UpdateTime)
                   .IsUnTimeZoneDateTime(EfCoreGlobalConfig.DbType == DatabaseType.PostgresSql)
                   .HasCustomerColumnName(EfCoreGlobalConfig.UseOldUpdateColumn ? EfCoreGlobalConfig.OldUpdateTime : string.Empty)
                   .HasComment("最后修改时间");
        }
    }
}