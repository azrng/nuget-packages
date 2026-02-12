using Dapper;

namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// sql 等于 运算符
    /// </summary>
    public class SqlEqualOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters,Type valueType)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $"  {fieldName} = {paramName}   ";
        }

        public virtual string GetSqlSentenceResult(string fieldName, IEnumerable<object> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues.First());
            return $"  {fieldName} = {paramName}   ";
        }

        public override string GetSqlSentenceResult(string fieldName, string fieldValue, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $"  {fieldName} = {paramName}   ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<string> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues.First());
            return $"  {fieldName} = {paramName}   ";
        }
    }
}