using Azrng.Database.DynamicSqlBuilder.Services;
using Azrng.Database.DynamicSqlBuilder.Utils;
using Dapper;

namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// SQL in 操作符
    /// </summary>
    public class SqlInOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            var paramName = GetParameterName(fieldName);
            var convertedValues = TypeConvertHelper.ConvertToTargetType(new List<object> { fieldValue }, valueType);
            parameters.Add(paramName, convertedValues);

            // 使用数据库方言服务生成正确的SQL
            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<string> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<object> fieldValues, DynamicParameters parameters,
                                                    Type valueType)
        {
            var paramName = GetParameterName(fieldName);

            // 根据valueType转换fieldValues为对应的类型
            var convertedValues = TypeConvertHelper.ConvertToTargetType(fieldValues, valueType);

            parameters.Add(paramName, convertedValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<int> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<long> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetInOperatorSql(fieldName, paramName, dialect);
        }
    }

    /// <summary>
    /// SQL not in 操作符
    /// </summary>
    public class SqlNotInOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            var paramName = GetParameterName(fieldName);
            var convertedValues = TypeConvertHelper.ConvertToTargetType(new List<object> { fieldValue }, valueType);
            parameters.Add(paramName, convertedValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetNotInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<string> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetNotInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<int> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetNotInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<long> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetNotInOperatorSql(fieldName, paramName, dialect);
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<object> fieldValues, DynamicParameters parameters,
                                                    Type valueType)
        {
            var paramName = GetParameterName(fieldName);

            // 根据valueType转换fieldValues为对应的类型
            var convertedValues = TypeConvertHelper.ConvertToTargetType(fieldValues, valueType);

            parameters.Add(paramName, convertedValues);

            var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
            return SqlDialectService.GetNotInOperatorSql(fieldName, paramName, dialect);
        }
    }
}