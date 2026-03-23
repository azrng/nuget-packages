using Azrng.Core.Exceptions;
using Azrng.Core.Extension;
using Azrng.Database.DynamicSqlBuilder.Model;
using Azrng.Database.DynamicSqlBuilder.SqlOperation;
using Azrng.Database.DynamicSqlBuilder.Validation;
using Dapper;
using System.Text;

namespace Azrng.Database.DynamicSqlBuilder;

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
        var logicalOperator = SqlBuilderInputValidator.NormalizeLogicalOperator(sqlWhereClause.LogicalOperator);
        var hasNestedChildren = sqlWhereClause.NestedChildrens?.Any() == true;

        if (!hasNestedChildren)
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
                    return GetSpecificConditionSqlWithSwitchOperator(sqlWhereClause, parameters, logicalOperator);

                case MatchOperator.And:
                    return $" {logicalOperator} {sqlWhereClause.FieldName} ";

                default:
                    throw new LogicBusinessException($"不支持的操作符{sqlWhereClause.MatchOperator}");
            }
        }

        var sqlBuilder = new StringBuilder();
        foreach (var sqlWhereClauseInfo in sqlWhereClause.NestedChildrens!)
        {
            sqlBuilder.Append(SplicingWhereConditionSql(sqlWhereClauseInfo, parameters));
        }

        return $" {logicalOperator} ( {sqlBuilder} ) ";
    }

    /// <summary>
    /// 根据不同的操作符,得到具体匹配符条件sql
    /// </summary>
    private static string GetSpecificConditionSqlWithSwitchOperator(
        SqlWhereClauseInfoDto sqlWhereClause,
        DynamicParameters parameters,
        string logicalOperator) =>
        sqlWhereClause.MatchOperator switch
        {
            MatchOperator.Between => GetBetweenOperatorConditionSql(sqlWhereClause, parameters, logicalOperator),
            MatchOperator.Like => GetLikeOrNotLikeOperatorConditionSql(sqlWhereClause, parameters, logicalOperator),
            MatchOperator.NotLike => GetLikeOrNotLikeOperatorConditionSql(sqlWhereClause, parameters, logicalOperator),
            _ => GetOtherOperatorConditionSql(sqlWhereClause, parameters, logicalOperator)
        };

    private static string GetBetweenOperatorConditionSql(
        SqlWhereClauseInfoDto sqlWhereClause,
        DynamicParameters parameters,
        string logicalOperator)
    {
        var fieldValueInfos = sqlWhereClause.FieldValueInfos ?? new List<FieldValueInfoDto>();
        if (!fieldValueInfos.Any())
        {
            return string.Empty;
        }

        var sqlOperation = SqlOperationFactory.CreateSqlOperation(sqlWhereClause.MatchOperator);
        var (beginTime, endTime) = GetTimeZoneWhereCondition(fieldValueInfos);
        if (beginTime.HasValue && endTime.HasValue)
        {
            return $" {logicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, beginTime.Value, endTime.Value, parameters)}";
        }

        return string.Empty;
    }

    private static string GetLikeOrNotLikeOperatorConditionSql(
        SqlWhereClauseInfoDto sqlWhereClause,
        DynamicParameters parameters,
        string logicalOperator)
    {
        var fieldValueInfos = sqlWhereClause.FieldValueInfos ?? new List<FieldValueInfoDto>();
        if (!fieldValueInfos.Any())
        {
            return string.Empty;
        }

        var sqlOperation = SqlOperationFactory.CreateSqlOperation(sqlWhereClause.MatchOperator);
        var valuesList = fieldValueInfos
            .Where(x => x.Value?.ToString().IsNotNullOrWhiteSpace() == true)
            .Select(t => t.Value)
            .ToList();

        if (valuesList.Any())
        {
            var stringValue = valuesList.First().ToString() ?? string.Empty;
            return $" {logicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, stringValue, parameters)} ";
        }

        var codeValue = fieldValueInfos.Select(x => x.Code).FirstOrDefault();
        return $" {logicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, codeValue?.ToString() ?? string.Empty, parameters)} ";
    }

    private static string GetOtherOperatorConditionSql(
        SqlWhereClauseInfoDto sqlWhereClause,
        DynamicParameters parameters,
        string logicalOperator)
    {
        var fieldValueInfos = sqlWhereClause.FieldValueInfos ?? new List<FieldValueInfoDto>();
        if (fieldValueInfos.Count == 0)
        {
            return string.Empty;
        }

        var sqlOperation = SqlOperationFactory.CreateSqlOperation(sqlWhereClause.MatchOperator);
        var values = fieldValueInfos
            .Where(x => x.Value?.ToString().IsNotNullOrWhiteSpace() == true)
            .Select(x => x.Value)
            .ToList();

        return values.Count switch
        {
            1 => $" {logicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, values.First(), parameters, sqlWhereClause.ValueType)} ",
            > 1 => $" {logicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, values, parameters, sqlWhereClause.ValueType)} ",
            _ => $" {logicalOperator} {sqlOperation.GetSqlSentenceResult(sqlWhereClause.FieldName, fieldValueInfos.Select(x => x.Code), parameters, sqlWhereClause.ValueType)} "
        };
    }

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
            var value0Str = fieldValueInfos[0]?.Value?.ToString() ?? string.Empty;
            var value1Str = fieldValueInfos[1]?.Value?.ToString() ?? string.Empty;

            if (value0Str.IsDateFormat() && value1Str.IsDateFormat())
            {
                beginTime = value0Str.ToDateTime()?.Date;
                endTime = value1Str.ToDateTime()?.Date.AddDays(1);
            }
        }

        return (beginTime, endTime);
    }
}
