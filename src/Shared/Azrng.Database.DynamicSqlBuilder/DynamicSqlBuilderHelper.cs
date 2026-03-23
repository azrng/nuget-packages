using Azrng.Database.DynamicSqlBuilder.Model;
using Azrng.Database.DynamicSqlBuilder.Services;
using Azrng.Database.DynamicSqlBuilder.SqlOperation;
using Azrng.Database.DynamicSqlBuilder.Validation;
using Dapper;
using System.Diagnostics;
using System.Text;

namespace Azrng.Database.DynamicSqlBuilder;

public static class DynamicSqlBuilderHelper
{
    /// <summary>
    /// 创建SQL查询语句 (参数化查询,支持long等多种类型)
    /// </summary>
    /// <returns>SQL语句和DynamicParameters对象</returns>
    public static (string Sql, DynamicParameters Parameters) BuilderSqlQueryStatementGeneric(
        string tableName,
        string necessaryCondition,
        List<string> queryResultFields,
        List<SqlWhereClauseInfoDto> sqlWhereClauses,
        List<InOperatorFieldDto> inOperatorFields = null,
        List<NotInOperatorFieldDto> notInOperatorFields = null,
        int? pageIndex = null,
        int? pageSize = null,
        List<SortFieldDto> sortFields = null,
        bool isQueryTotalCount = false)
    {
        ValidateBuilderInputs(tableName, queryResultFields, sqlWhereClauses, inOperatorFields, notInOperatorFields, sortFields);

        var parameters = new DynamicParameters();
        var normalizedNecessaryCondition = SqlBuilderInputValidator.NormalizeNecessaryCondition(necessaryCondition);
        var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
        var orderByClause = GetOrderByClause(sortFields);

        var querySqlBuilder = new StringBuilder();
        var basicTemplate = isQueryTotalCount
            ? $" SELECT COUNT(1) FROM {tableName} {normalizedNecessaryCondition} "
            : $" SELECT {string.Join(",", queryResultFields)} FROM {tableName} {normalizedNecessaryCondition}";
        querySqlBuilder.Append(basicTemplate);

        foreach (var sqlWhereClauseItem in sqlWhereClauses ?? new List<SqlWhereClauseInfoDto>())
        {
            querySqlBuilder.Append(SqlWhereClauseHelper.SplicingWhereConditionSql(sqlWhereClauseItem, parameters));
        }

        if (inOperatorFields?.Any() == true)
        {
            querySqlBuilder.Append(SpecialHandlerInOperator(inOperatorFields, MatchOperator.In, parameters));
        }

        if (notInOperatorFields?.Any() == true)
        {
            querySqlBuilder.Append(SpecialHandlerNotInOperator(notInOperatorFields, MatchOperator.NotIn, parameters));
        }

        if (!isQueryTotalCount)
        {
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                var pagingSql = SqlDialectService.GetPagingSql(querySqlBuilder.ToString(), pageIndex.Value, pageSize.Value, orderByClause, dialect);
                querySqlBuilder.Clear();
                querySqlBuilder.Append(pagingSql);
            }
            else if (!string.IsNullOrWhiteSpace(orderByClause))
            {
                querySqlBuilder.Append(SqlConstant.SortOperatorOrderBy)
                    .Append(orderByClause);
            }
        }

        var sql = querySqlBuilder.ToString();
        NotifySqlGenerated(sql, parameters);
        return (sql, parameters);
    }

    /// <summary>
    /// 创建SQL查询语句 (参数化查询 - 分页查询)
    /// </summary>
    /// <param name="tableName">表名称</param>
    /// <param name="necessaryCondition">必要的where条件内容 格式: Where xx列=xx具体值 </param>
    /// <param name="queryResultFields">查询的结果列</param>
    /// <param name="sqlWhereClauses">SQLWhere条件子句列信息集合</param>
    /// <param name="inOperatorFields">in 查询的列筛选</param>
    /// <param name="notInOperatorFields">not in 查询的列筛选</param>
    /// <param name="pageIndex">当前页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="sortFields">需要排序的字段列</param>
    /// <returns>查询行sql、查询总条数SQL、DynamicParameters对象</returns>
    public static (string QuerySql, string CountSql, DynamicParameters Parameters) BuilderSqlQueryCountStatementGeneric(
        string tableName,
        string necessaryCondition,
        List<string> queryResultFields,
        IReadOnlyCollection<SqlWhereClauseInfoDto> sqlWhereClauses,
        List<InOperatorFieldDto> inOperatorFields = null,
        List<NotInOperatorFieldDto> notInOperatorFields = null,
        int? pageIndex = null,
        int? pageSize = null,
        IReadOnlyCollection<SortFieldDto> sortFields = null)
    {
        ValidateBuilderInputs(tableName, queryResultFields, sqlWhereClauses, inOperatorFields, notInOperatorFields, sortFields);

        var parameters = new DynamicParameters();
        var normalizedNecessaryCondition = SqlBuilderInputValidator.NormalizeNecessaryCondition(necessaryCondition);
        var dialect = SqlBuilderConfigurer.GetCurrentOptions().Dialect;
        var orderByClause = GetOrderByClause(sortFields);

        var whereSqlBuilder = new StringBuilder();
        foreach (var sqlWhereClauseItem in sqlWhereClauses ?? new List<SqlWhereClauseInfoDto>())
        {
            whereSqlBuilder.Append(SqlWhereClauseHelper.SplicingWhereConditionSql(sqlWhereClauseItem, parameters));
        }

        if (inOperatorFields?.Any() == true)
        {
            whereSqlBuilder.Append(SpecialHandlerInOperator(inOperatorFields, MatchOperator.In, parameters));
        }

        if (notInOperatorFields?.Any() == true)
        {
            whereSqlBuilder.Append(SpecialHandlerNotInOperator(notInOperatorFields, MatchOperator.NotIn, parameters));
        }

        var whereSql = whereSqlBuilder.ToString();
        var countSql = $" SELECT COUNT(1) FROM {tableName} {normalizedNecessaryCondition} {whereSql}";
        var querySql = $" SELECT {string.Join(",", queryResultFields)} FROM {tableName} {normalizedNecessaryCondition} {whereSql}";

        if (pageIndex.HasValue && pageSize.HasValue)
        {
            querySql = SqlDialectService.GetPagingSql(querySql, pageIndex.Value, pageSize.Value, orderByClause, dialect);
        }
        else if (!string.IsNullOrWhiteSpace(orderByClause))
        {
            querySql = $"{querySql}{SqlConstant.SortOperatorOrderBy}{orderByClause}";
        }

        NotifySqlGenerated(querySql, parameters);
        NotifySqlGenerated(countSql, parameters);
        return (querySql, countSql, parameters);
    }

    private static string SpecialHandlerInOperator(
        IEnumerable<InOperatorFieldDto> inOperatorFields,
        MatchOperator matchOperator,
        DynamicParameters parameters)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var inOperatorField in inOperatorFields)
        {
            if (inOperatorField.Ids.Any())
            {
                var sqlOperation = SqlOperationFactory.CreateSqlOperation(matchOperator);
                var sqlResult = sqlOperation.GetSqlSentenceResult(inOperatorField.Field, inOperatorField.Ids, parameters, inOperatorField.ValueType);
                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} {sqlResult}");
            }
            else
            {
                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} 1=2 ");
            }
        }

        return sqlBuilder.ToString();
    }

    private static string SpecialHandlerNotInOperator(
        IEnumerable<NotInOperatorFieldDto> notInOperatorFields,
        MatchOperator matchOperator,
        DynamicParameters parameters)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var notInOperatorField in notInOperatorFields)
        {
            if (notInOperatorField.Ids.Any())
            {
                var sqlOperation = SqlOperationFactory.CreateSqlOperation(matchOperator);
                var sqlResult = sqlOperation.GetSqlSentenceResult(notInOperatorField.Field, notInOperatorField.Ids, parameters, notInOperatorField.ValueType);
                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} {sqlResult}");
            }
            else
            {
                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} 1=1 ");
            }
        }

        return sqlBuilder.ToString();
    }

    private static void ValidateBuilderInputs(
        string tableName,
        List<string> queryResultFields,
        IEnumerable<SqlWhereClauseInfoDto> sqlWhereClauses,
        IEnumerable<InOperatorFieldDto> inOperatorFields,
        IEnumerable<NotInOperatorFieldDto> notInOperatorFields,
        IEnumerable<SortFieldDto> sortFields)
    {
        if (queryResultFields == null || queryResultFields.Count == 0)
        {
            throw new ArgumentException("查询字段列表不能为空", nameof(queryResultFields));
        }

        var options = SqlBuilderConfigurer.GetCurrentOptions();
        if (!options.EnableFieldNameValidation)
        {
            return;
        }

        FieldNameValidator.ValidateFieldName(tableName, nameof(tableName));

        if (!FieldNameValidator.AreValidFieldNames(queryResultFields, out var invalidFields))
        {
            throw new ArgumentException($"查询字段包含无效字段名: {string.Join(", ", invalidFields)}", nameof(queryResultFields));
        }

        foreach (var whereClause in sqlWhereClauses ?? Enumerable.Empty<SqlWhereClauseInfoDto>())
        {
            ValidateWhereClause(whereClause);
        }

        foreach (var inField in inOperatorFields ?? Enumerable.Empty<InOperatorFieldDto>())
        {
            FieldNameValidator.ValidateFieldName(inField.Field, nameof(inOperatorFields));
        }

        foreach (var notInField in notInOperatorFields ?? Enumerable.Empty<NotInOperatorFieldDto>())
        {
            FieldNameValidator.ValidateFieldName(notInField.Field, nameof(notInOperatorFields));
        }

        foreach (var sortField in sortFields ?? Enumerable.Empty<SortFieldDto>())
        {
            FieldNameValidator.ValidateFieldName(sortField.Field, nameof(sortFields));
        }
    }

    private static void ValidateWhereClause(SqlWhereClauseInfoDto whereClause)
    {
        if (whereClause == null)
        {
            return;
        }

        SqlBuilderInputValidator.NormalizeLogicalOperator(whereClause.LogicalOperator);

        if (whereClause.NestedChildrens?.Any() == true)
        {
            foreach (var childClause in whereClause.NestedChildrens)
            {
                ValidateWhereClause(childClause);
            }

            return;
        }

        FieldNameValidator.ValidateFieldName(whereClause.FieldName, nameof(whereClause.FieldName));
    }

    private static string GetOrderByClause(IEnumerable<SortFieldDto> sortFields)
    {
        return string.Join(",", (sortFields ?? Enumerable.Empty<SortFieldDto>())
            .Select(p => p.Field + p.OrderStr));
    }

    private static void NotifySqlGenerated(string sql, DynamicParameters parameters)
    {
        var options = SqlBuilderConfigurer.GetCurrentOptions();
        if (options.EnableSqlLogging)
        {
            Debug.WriteLine(sql);
        }

        options.OnSqlGenerated?.Invoke(sql, parameters);
    }
}
