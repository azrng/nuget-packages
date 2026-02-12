namespace Azrng.Database.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// sql like 运算符
    /// </summary>
    public class SqlLikeOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, Dapper.DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            string likeValue;

            if (fieldValue.Contains("["))
            {
                //出现通配符 [ 特殊处理
                likeValue = $"%[{fieldValue}]%";
            }
            else if (fieldValue.Contains("%"))
            {
                //出现通配符 % 特殊处理 将 % 替换成 [%]
                likeValue = $"%{fieldValue.Replace("%", "[%]")}%";
            }
            else
            {
                likeValue = $"%{fieldValue}%";
            }

            parameters.Add(paramName, likeValue);
            return $"  {fieldName} like {paramName}   ";
        }
    }

    /// <summary>
    /// sql not like 运算符
    /// </summary>
    public class SqlNotLikeOperation : SqlOperation
    {
        public override string GetSqlSentenceResult(string fieldName, string fieldValue, Dapper.DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            string likeValue;

            if (fieldValue.Contains("["))
            {
                // 针对字段值中 出现通配符 [ 特殊处理
                likeValue = $"%[{fieldValue}]%";
            }
            else if (fieldValue.Contains("%"))
            {
                //将 % 替换成 [%]
                likeValue = $"%{fieldValue.Replace("%", "[%]")}%";
            }
            else
            {
                likeValue = $"%{fieldValue}%";
            }

            parameters.Add(paramName, likeValue);
            return $"  {fieldName} not like {paramName}   ";
        }
    }
}