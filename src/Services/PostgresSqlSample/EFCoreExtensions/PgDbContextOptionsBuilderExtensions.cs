using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

namespace PostgresSqlSample.EFCoreExtensions
{
    public static class PgDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// 数据库扩展函数
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder UsePgToCharFunctions(
            this DbContextOptionsBuilder optionsBuilder)
        {
            //将自定义的配置类添加到配置选项中
            var extension = GetOrCreateExtension(optionsBuilder);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }

        /// <summary>
        /// 生成创建扩展类
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        private static PgDbContextOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.Options.FindExtension<PgDbContextOptionsExtension>() ?? new PgDbContextOptionsExtension();

        public static IServiceCollection AddEntityFrameworkPg(this IServiceCollection serviceCollection)
        {
            // new EntityFrameworkPglServicesBuilder(serviceCollection)
            //     .TryAdd<IMethodCallTranslatorProvider, PgMethodCallTranslatorProvider>();

            new EntityFrameworkPglServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, NpgsqlLoggingDefinitions>()
                .TryAdd<IValueGeneratorCache>(p => p.GetRequiredService<INpgsqlValueGeneratorCache>())
                .TryAdd<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, NpgsqlSqlGenerationHelper>()
                .TryAdd<IRelationalAnnotationProvider, NpgsqlAnnotationProvider>()
                .TryAdd<IModelValidator, NpgsqlModelValidator>()
                .TryAdd<IMigrator, NpgsqlMigrator>()
                .TryAdd<IProviderConventionSetBuilder, NpgsqlConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator, NpgsqlUpdateSqlGenerator>()
                .TryAdd<IModificationCommandFactory, NpgsqlModificationCommandFactory>()
                .TryAdd<IModificationCommandBatchFactory, NpgsqlModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, NpgsqlValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetRequiredService<INpgsqlRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, NpgsqlMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, NpgsqlDatabaseCreator>()
                .TryAdd<IHistoryRepository, NpgsqlHistoryRepository>()
                .TryAdd<ICompiledQueryCacheKeyGenerator, NpgsqlCompiledQueryCacheKeyGenerator>()
                .TryAdd<IExecutionStrategyFactory, NpgsqlExecutionStrategyFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, NpgsqlQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IMethodCallTranslatorProvider, PgMethodCallTranslatorProvider>()
                .TryAdd<IAggregateMethodCallTranslatorProvider, NpgsqlAggregateMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, NpgsqlMemberTranslatorProvider>()
                .TryAdd<IEvaluatableExpressionFilter, NpgsqlEvaluatableExpressionFilter>()
                .TryAdd<IQuerySqlGeneratorFactory, NpgsqlQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, NpgsqlSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, NpgsqlQueryTranslationPostprocessorFactory>()
                .TryAdd<IRelationalParameterBasedSqlProcessorFactory, NpgsqlParameterBasedSqlProcessorFactory>()
                .TryAdd<ISqlExpressionFactory, NpgsqlSqlExpressionFactory>()
                .TryAdd<ISingletonOptions, INpgsqlSingletonOptions>(p => p.GetRequiredService<INpgsqlSingletonOptions>())
                .TryAdd<IQueryCompilationContextFactory, NpgsqlQueryCompilationContextFactory>()
                .TryAddProviderSpecificServices(b => b
                                                     .TryAddSingleton<INpgsqlValueGeneratorCache, NpgsqlValueGeneratorCache>()
                                                     .TryAddSingleton<INpgsqlSingletonOptions, NpgsqlSingletonOptions>()
                                                     .TryAddSingleton<INpgsqlSequenceValueGeneratorFactory,
                                                         NpgsqlSequenceValueGeneratorFactory>()
                                                     .TryAddScoped<INpgsqlRelationalConnection, NpgsqlRelationalConnection>())
                .TryAddCoreServices();

            return serviceCollection;
        }
    }
}