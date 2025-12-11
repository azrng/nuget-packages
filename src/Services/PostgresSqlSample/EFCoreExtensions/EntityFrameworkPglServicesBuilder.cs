using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PostgresSqlSample.EFCoreExtensions
{
    public class EntityFrameworkPglServicesBuilder : EntityFrameworkRelationalServicesBuilder
    {
        public EntityFrameworkPglServicesBuilder(IServiceCollection serviceCollection) : base(serviceCollection) { }
    }
}