namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// sql 不等于 运算符
    /// </summary>
    public class SqlNotEqualOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, object fieldValue, Dapper.DynamicParameters parameters, Type valueType)
        {
            var paramName = AddParameter(fieldName, fieldValue, parameters, valueType);
            return $" ( {fieldName} <> {paramName} or {fieldName} is null ) ";
        }

        public override string GetSqlSentenceResult(string fieldName, string fieldValue, Dapper.DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValue);
            return $" ( {fieldName} <> {paramName} or {fieldName} is null ) ";
        }
    }
}
