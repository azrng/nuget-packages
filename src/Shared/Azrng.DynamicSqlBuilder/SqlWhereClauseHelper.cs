using Azrng.Core.Exceptions;
using Azrng.Core.Extension;
using Azrng.DynamicSqlBuilder.Model;
using Azrng.DynamicSqlBuilder.SqlOperation;
using ConsoleApp.Models.DynamicSql;
using ConsoleApp.Models.DynamicSql.SqlOperation;
using Dapper;
using System.Text;

namespace Azrng.DynamicSqlBuilder;

public class SqlWhereClauseHelper
{
    /// <summary>
    /// 拼接where条件
    /// </summary>
    /// <param name="sqlWhereClause"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string SplicingWhereConditionSql(SqlWhereClauseInfoDto sqlWhereClause, DynamicParameters parameters)
    {
        if (sqlWhereClause.NestedChildrens == null)
        {
            switch (sqlWhereClause.MatchOperator)
            {
                case MatchOperator.Equal:
                case MatchOperator.In:
                case MatchOperator.Between:
                case MatchOperator.GreaterThanEqual:
                case MatchOperator.LessThanEqual:
                case MatchOperator.Like:
                case MatchOperator.NotLike:
                case MatchOperator.NotIn:
                case MatchOperator.GreaterThan:
                case MatchOperator.LessThan:
                case MatchOperator.NotEqual:
                    return GetSpecificConditionSqlWithSwitchOperator(sqlWhereClause, parameters);

                case MatchOperator.And:
                    return $" {sqlWhereClause.LogicalOperator} {sqlWhereClause.FieldName} ";

                default:
                    throw new LogicBusinessException($"不支持的操作符{sqlWhereClause.MatchOperator}");
            }
        }
        else
        {
            var sqlBuilder = new StringBuilder();
            foreach (var sqlWhereClauseInfo in sqlWhereClause.NestedChildrens)
            {
                sqlBuilder.Append(SplicingWhereConditionSql(sqlWhereClauseInfo, parameters));
            }

            return $" {SqlConstant.LogicalOperatorAnd} ( {sqlBuilder} ) ";
        }
    }

    /// <summary>
    /// 根据不同的操作符,得到具体匹配符条件sql
    /// </summary>
    /// <param name="sqlWhereClause"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static string GetSpecificConditionSqlWithSwitchOperator(SqlWhereClauseInfoDto sqlWhereClause, DynamicParameters parameters) =>
        sqlWhereClause.MatchOperator switch
        {
            //特殊处理Between ...And 目前只有日期类型支持
            MatchOperator.Between => GetBetweenOperatorConditionSql(sqlWhereClause, parameters),

            //like 操作
            MatchOperator.Like => GetLikeOrNotLikeOperatorConditionSql(sqlWhereClause, parameters),

            //not like操作
            MatchOperator.NotLike => GetLikeOrNotLikeOperatorConditionSql(sqlWhereClause, parameters),

            //其它操作符 统一方式
            _ => GeOtherOperatorConditionSql(sqlWhereClause, parameters)
        };

    /// <summary>
    /// 获取Between...And 方式的具体条件SQL
    /// </summary>
    /// <param name="sqlWhereClause"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static string GetBetweenOperatorConditionSql(SqlWhereClauseInfoDto sqlWhereClause, DynamicParameters parameters)
    {
        //目前只有日期类型支持 between ... and
        if (sqlWhereClause.FieldValueInfos.Any())
        {
            var sqlOperation = SqlOperationFactory.CreateSqlOperation(sqlWhereClause.MatchOperator);
            var (beginTime, endTime) = GetTimeZoneWhereCondition(sqlWhereClause.FieldValueInfos?.ToList() ?? new List<FieldValueInfoDto>());
            if (beginTime.HasValue && endTime.HasValue)
            {
                return
                    $" {sqlWhereClause.LogicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, beginTime.Value, endTime.Value, parameters)}";
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取Like方式的具体条件SQL
    /// </summary>
    /// <param name="sqlWhereClause"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static string GetLikeOrNotLikeOperatorConditionSql(SqlWhereClauseInfoDto sqlWhereClause, DynamicParameters parameters)
    {
        if (sqlWhereClause.FieldValueInfos.Any())
        {
            var sqlOperation = SqlOperationFactory.CreateSqlOperation(sqlWhereClause.MatchOperator);
            var valuesList = sqlWhereClause.FieldValueInfos.Where(x => x.Value.ToString().IsNotNullOrWhiteSpace())
                                           .Select(t => t.Value)
                                           .ToList();
            if (valuesList.Any())
            {
                // Like操作需要字符串，如果Value不是字符串则转换
                var stringValue = valuesList.First().ToString() ?? string.Empty;
                return
                    $" {sqlWhereClause.LogicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, stringValue, parameters)} ";
            }
            else
            {
                // Like操作需要字符串，如果Value不是字符串则转换
                var codes = sqlWhereClause.FieldValueInfos.Select(x => x.Code).FirstOrDefault();
                var stringValue = codes?.ToString() ?? string.Empty;
                return
                    $" {sqlWhereClause.LogicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, stringValue, parameters)} ";
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取其他方式的具体条件SQL
    /// </summary>
    /// <param name="sqlWhereClause"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static string GeOtherOperatorConditionSql(SqlWhereClauseInfoDto sqlWhereClause, DynamicParameters parameters)
    {
        if (sqlWhereClause.FieldValueInfos.Count == 0)
        {
            return string.Empty;
        }

        var sqlOperation = SqlOperationFactory.CreateSqlOperation(sqlWhereClause.MatchOperator);

        // 检查是否有Code值
        var values = sqlWhereClause.FieldValueInfos.Where(x => x.Value.ToString().IsNotNullOrWhiteSpace())
                                   .Select(x => x.Value)
                                   .ToList();
        return values.Count switch
        {
            1 =>
                $" {sqlWhereClause.LogicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, values.First(), parameters, sqlWhereClause.ValueType)} ",
            > 1 =>
                $" {sqlWhereClause.LogicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, values, parameters, sqlWhereClause.ValueType)} ",
            _ =>
                $" {sqlWhereClause.LogicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, sqlWhereClause.FieldValueInfos.Select(x => x.Code), parameters, sqlWhereClause.ValueType)} "
        };
    }

    /// <summary>
    /// 获取时间区间边界查询
    /// </summary>
    /// <param name="fieldValueInfos"></param>
    /// <returns></returns>
    private static (DateTime? beginTime, DateTime? endTime) GetTimeZoneWhereCondition(
        IList<FieldValueInfoDto> fieldValueInfos)
    {
        DateTime? beginTime = null;
        DateTime? endTime = null;

        if (fieldValueInfos.Count >= 1 && fieldValueInfos[0].Value != null)
        {
            var valueStr = fieldValueInfos[0].Value.ToString();
            if (!string.IsNullOrEmpty(valueStr) && valueStr.IsIntFormat())
            {
                var month = int.Parse(valueStr);
                beginTime = DateTime.Now.ToNowDateTime().AddMonths(-month).Date;
                endTime = DateTime.Now.ToNowDateTime().Date.AddDays(1);
            }
        }

        if (!beginTime.HasValue && fieldValueInfos.Count >= 2)
        {
            var value0Str = (fieldValueInfos[0]?.Value?.ToString() ?? "");
            var value1Str = (fieldValueInfos[1]?.Value?.ToString() ?? "");

            if (value0Str.IsDateFormat() && value1Str.IsDateFormat())
            {
                beginTime = value0Str.ToDateTime()?.Date;
                endTime = value1Str.ToDateTime()?.Date.AddDays(1);
            }
        }

        return (beginTime, endTime);
    }
}