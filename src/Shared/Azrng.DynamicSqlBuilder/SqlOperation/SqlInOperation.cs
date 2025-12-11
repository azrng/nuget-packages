using Azrng.DynamicSqlBuilder.Utils;
using Dapper;

namespace Azrng.DynamicSqlBuilder.SqlOperation
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
            return $"  {fieldName} = ANY({paramName})   ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<string> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);
            return $"  {fieldName} = ANY({paramName})   ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<object> fieldValues, DynamicParameters parameters,
                                                    Type valueType)
        {
            var paramName = GetParameterName(fieldName);

            // 根据valueType转换fieldValues为对应的类型
            var convertedValues = TypeConvertHelper.ConvertToTargetType(fieldValues, valueType);

            parameters.Add(paramName, convertedValues);
            return $"  {fieldName} = ANY({paramName})   ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<int> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);
            return $"  {fieldName} = ANY({paramName})   ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<long> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);
            return $"  {fieldName} = ANY({paramName})   ";
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
            return $"  {fieldName} !=ANY({paramName})   ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<string> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);
            return $"  {fieldName} !=ANY({paramName})  ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<int> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);
            return $"  {fieldName} !=ANY({paramName})  ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<long> fieldValues, DynamicParameters parameters)
        {
            var paramName = GetParameterName(fieldName);
            parameters.Add(paramName, fieldValues);
            return $"  {fieldName} !=ANY({paramName})  ";
        }

        public override string GetSqlSentenceResult(string fieldName, IEnumerable<object> fieldValues, DynamicParameters parameters,
                                                    Type valueType)
        {
            var paramName = GetParameterName(fieldName);

            // 根据valueType转换fieldValues为对应的类型
            var convertedValues = TypeConvertHelper.ConvertToTargetType(fieldValues, valueType);

            parameters.Add(paramName, convertedValues);
            return $"  {fieldName} !=ANY({paramName})  ";
        }
    }
}