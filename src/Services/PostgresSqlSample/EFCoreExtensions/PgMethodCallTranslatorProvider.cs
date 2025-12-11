using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace PostgresSqlSample.EFCoreExtensions;

/// <summary>
/// 创建转换器提供程序
/// </summary>
public sealed class PgMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
{
    public PgMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies) : base(dependencies)
    {
        var sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
        var typeMappingSource = (NpgsqlTypeMappingSource)dependencies.RelationalTypeMappingSource;
        AddTranslators(new List<IMethodCallTranslator>
                       {
                           //将刚刚的方法转换器添加到扩展
                           new PgDateTimeMethodTranslator(typeMappingSource, sqlExpressionFactory)
                       });
    }
}