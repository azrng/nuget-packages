using Azrng.DynamicSqlBuilder.Model;
using Dapper;

namespace Azrng.DynamicSqlBuilder.SqlOperation
{
    /// <summary>
    /// Sql操作符工厂
    /// </summary>
    public static class SqlOperationFactory
    {
        public static SqlOperation CreateSqlOperation(MatchOperator operation)
        {
            SqlOperation sqlOperation = operation switch
            {
                MatchOperator.Equal => new SqlEqualOperation(),
                MatchOperator.NotEqual => new SqlNotEqualOperation(),
                MatchOperator.GreaterThanEqual => new SqlGreaterThanEqualOperation(),
                MatchOperator.GreaterThan => new SqlGreaterThanOperation(),
                MatchOperator.LessThanEqual => new SqlLessThanEqualOperation(),
                MatchOperator.LessThan => new SqlLessThanOperation(),
                MatchOperator.Like => new SqlLikeOperation(),
                MatchOperator.NotLike => new SqlNotLikeOperation(),
                MatchOperator.Between => new SqlBetweenOperation(),
                MatchOperator.In => new SqlInOperation(),
                MatchOperator.NotIn => new SqlNotInOperation(),
                _ => throw new NotSupportedException("不支持的类型")
            };

            return sqlOperation;
        }
    }

    /// <summary>
    /// 数据库操作符抽象类
    /// </summary>
    public abstract class SqlOperation
    {
        private static int _parameterIndex = 0;

        /// <summary>
        /// 生成唯一参数名
        /// </summary>
        protected string GetParameterName(string fieldName)
        {
            return $"@p_{fieldName}_{Interlocked.Increment(ref _parameterIndex)}";
        }

        /// <summary>
        /// sql语句结果 (参数化查询 - object类型，支持任意类型)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValue">字段值</param>
        /// <param name="parameters">动态参数对象</param>
        /// <param name="valueType">值的目标类型</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, object fieldValue, DynamicParameters parameters, Type valueType)
        {
            return string.Empty;
        }

        /// <summary>
        /// sql语句结果 (参数化查询 - object集合类型，支持任意类型，带类型转换)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValues">字段值集合</param>
        /// <param name="parameters">动态参数对象</param>
        /// <param name="valueType">值的目标类型</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, IEnumerable<object> fieldValues, DynamicParameters parameters,
                                                   Type valueType)
        {
            return string.Empty;
        }

        /// <summary>
        /// sql语句结果 (参数化查询)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValue">字段值</param>
        /// <param name="parameters">动态参数对象</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, string fieldValue, DynamicParameters parameters)
        {
            return string.Empty;
        }

        /// <summary>
        /// sql语句结果 (参数化查询)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValues">字段值集合</param>
        /// <param name="parameters">动态参数对象</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, IEnumerable<string> fieldValues, DynamicParameters parameters)
        {
            return string.Empty;
        }

        /// <summary>
        /// sql语句结果 (参数化查询)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValues">字段值集合</param>
        /// <param name="parameters">动态参数对象</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, IEnumerable<int> fieldValues, DynamicParameters parameters)
        {
            return string.Empty;
        }

        /// <summary>
        /// sql语句结果 (参数化查询 - long类型)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValues">字段值集合</param>
        /// <param name="parameters">动态参数对象</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, IEnumerable<long> fieldValues, DynamicParameters parameters)
        {
            return string.Empty;
        }

        /// <summary>
        /// sql语句结果 (参数化查询)
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValueBegin">字段值开始区间</param>
        /// <param name="fieldValueEnd">字段值结束区间</param>
        /// <param name="parameters">动态参数对象</param>
        /// <returns></returns>
        public virtual string GetSqlSentenceResult(string fieldName, DateTime fieldValueBegin, DateTime fieldValueEnd,
                                                   DynamicParameters parameters)
        {
            return string.Empty;
        }

        /// <summary>
        /// 转成 sql 运算符
        /// </summary>
        /// <returns></returns>
        public virtual string ConvertToSqlOperator() { return string.Empty; }
    }

    /// <summary>
    /// sql 且 运算符
    /// </summary>
    public class SqlAndOperation : SqlOperation
    {
        public override string ConvertToSqlOperator()
        {
            return " AND ";
        }
    }

    /// <summary>
    /// sql 或 运算符
    /// </summary>
    public class SqlOrOperation : SqlOperation
    {
        public override string ConvertToSqlOperator()
        {
            return " OR ";
        }
    }
}