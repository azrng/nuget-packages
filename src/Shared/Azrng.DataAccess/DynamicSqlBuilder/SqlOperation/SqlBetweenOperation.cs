namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// SQL between 操作符
    /// </summary>
    public class SqlBetweenOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, DateTime fieldValueBegin, DateTime filedValueEnd,
                                                    Dapper.DynamicParameters parameters)
        {
            var paramNameBegin = GetParameterName(fieldName + "_begin");
            var paramNameEnd = GetParameterName(fieldName + "_end");
            parameters.Add(paramNameBegin, fieldValueBegin);
            parameters.Add(paramNameEnd, filedValueEnd);
            return $"  {fieldName} BETWEEN {paramNameBegin} AND {paramNameEnd}   ";
        }
    }
}