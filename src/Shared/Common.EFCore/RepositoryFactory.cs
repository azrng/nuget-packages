using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore
{
    public static class RepositoryFactory
    {
        public static IBaseRepository<TEntity> GenerateRepository<TEntity>(DbContext dbContext)
            where TEntity : IEntity =>
            new BaseRepository<TEntity>(dbContext);
    }
}