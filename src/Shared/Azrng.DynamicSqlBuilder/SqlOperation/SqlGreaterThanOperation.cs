using Dapper;

namespace Azrng.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// sql 大于 运算符
    /// </summary>
    public class SqlGreaterThanOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, Dapper.DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $"  {fieldName} > {paramName}   ";
        }

        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            return $"  {fieldName} > {fieldValue}   ";
        }
    }

    /// <summary>
    /// sql 大于等于 运算符
    /// </summary>
    public class SqlGreaterThanEqualOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, Dapper.DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $"  {fieldName} >= {paramName}   ";
        }

        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            return $"  {fieldName} >= {fieldValue}   ";
        }
    }
}