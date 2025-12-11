using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System.Reflection;

namespace PostgresSqlSample.EFCoreExtensions;

/// <summary>
/// 创建调用方法转换器
/// </summary>
public class PgDateTimeMethodTranslator : IMethodCallTranslator
{
    #region MethodInfo

    private static readonly MethodInfo ToChar = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(nameof(DbFunctionsExtensions.ToChar),
        [typeof(DbFunctions), typeof(DateTime), typeof(string)])!;

    #endregion

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public PgDateTimeMethodTranslator(NpgsqlTypeMappingSource typeMappingSource, ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments,
                                    IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(DbFunctionsExtensions))
        {
            return TranslateDbFunctionsMethod(instance, method, arguments);
        }

        return null;
    }

    private SqlExpression? TranslateDbFunctionsMethod(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
    {
        //判断方法是否一致
        if (method == ToChar)
        {
            return _sqlExpressionFactory.Function("to_char",
                new[]
                {
                    arguments[1],
                    arguments[2]
                },
                nullable: true,
                argumentsPropagateNullability: new[]
                                               {
                                                   true,
                                                   true
                                               },
                typeof(string),
                _typeMappingSource.FindMapping(typeof(string)));
        }

        return null;
    }
}