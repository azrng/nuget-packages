using Azrng.Core.Model;
using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Azrng.EFCore.EntityTypeConfigurations
{
    /// <summary>
    /// 基类标识配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityTypeConfigurationIdentity<T> : EntityTypeConfigurationIdentity<T, long>
        where T : IdentityBaseEntity
    {
    }

    /// <summary>
    /// 基类标识配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class EntityTypeConfigurationIdentity<T, TKey> : IEntityTypeConfiguration<T>
        where T : IdentityBaseEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            var genericType = typeof(T);
            if (EfCoreGlobalConfig.DbType == DatabaseType.PostgresSql && !string.IsNullOrWhiteSpace(EfCoreGlobalConfig.Schema))
            {
                builder.ToTable(genericType.Name.ToLowerInvariant(), EfCoreGlobalConfig.Schema);
            }
            else
            {
                builder.ToTable(genericType.Name.ToLowerInvariant());
            }

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired().HasMaxLength(36).HasComment("主键");
        }
    }
}