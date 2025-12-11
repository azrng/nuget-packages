using Azrng.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostgresSqlSample.Model;

namespace PostgresSqlSample.EntityTypeConfigurations
{
    public class UserEntityConfiguration : EntityTypeConfigurationIdentity<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Account).IsRequired().HasMaxLength(50).HasComment("账号");
            builder.Property(x => x.Password).IsRequired().HasMaxLength(50).HasComment("密码");
            builder.Property(x => x.CreateTime).IsUnTimeZoneDateTime().IsRequired().HasMaxLength(50).HasComment("创建时间");
            builder.Property(x => x.UserName).IsRequired().HasMaxLength(50).HasComment("用户名");
            builder.Property(x => x.IsValid).IsRequired().HasMaxLength(50).HasComment("是否有效");
        }
    }
}