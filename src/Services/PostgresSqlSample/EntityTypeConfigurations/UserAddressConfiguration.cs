using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostgresSqlSample.Model;

namespace PostgresSqlSample.EntityTypeConfigurations
{
    public class UserAddressConfiguration : EntityTypeConfigurationIdentity<UserAddress>
    {
        public override void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.Name).IsRequired().HasComment("名称");
            builder.Property(x => x.Address).IsRequired().HasComment("地址");
            builder.Property(x => x.UserId).IsRequired().HasComment("用户Id");
        }
    }
}