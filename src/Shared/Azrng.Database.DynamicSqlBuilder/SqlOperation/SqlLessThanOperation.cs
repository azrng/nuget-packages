using Dapper;

namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// sql 小于 运算符
    /// </summary>
    public class SqlLessThanOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $"  {fieldName} < {paramName}   ";
        }

        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            return $"  {fieldName} < {fieldValue}   ";
        }
    }

    /// <summary>
    /// sql 小于等于 运算符
    /// </summary>
    public class SqlLessThanEqualOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $"  {fieldName} <= {paramName}   ";
        }

        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            return $"  {fieldName} <= {fieldValue}   ";
        }
    }
}