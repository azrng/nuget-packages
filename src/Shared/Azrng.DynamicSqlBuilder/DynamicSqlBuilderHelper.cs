using Azrng.DynamicSqlBuilder.Model;
using Azrng.DynamicSqlBuilder.SqlOperation;
using Dapper;
using System.Text;

namespace Azrng.DynamicSqlBuilder;

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
        var parameters = new DynamicParameters();

        if (string.IsNullOrEmpty(necessaryCondition))
        {
            necessaryCondition = " where 1=1 ";
        }

        var querySqlBuilder = new StringBuilder();
        var basicTemplate = isQueryTotalCount
            ? $" SELECT COUNT(1) FROM {tableName} {necessaryCondition} "
            : $" SELECT {string.Join(",", queryResultFields)} FROM {tableName} {necessaryCondition}";
        querySqlBuilder.Append(basicTemplate);

        foreach (var sqlWhereClauseItem in sqlWhereClauses ?? [])
        {
            var whereConditionSql = SqlWhereClauseHelper.SplicingWhereConditionSql(sqlWhereClauseItem, parameters);
            querySqlBuilder.Append(whereConditionSql);
        }

        if (inOperatorFields?.Any() == true)
        {
            querySqlBuilder.Append(SpecialHandlerInOperator(inOperatorFields, MatchOperator.In, parameters));
        }

        if (notInOperatorFields?.Any() == true)
        {
            querySqlBuilder.Append(SpecialHandlerNotInOperator(notInOperatorFields, MatchOperator.NotIn, parameters));
        }

        if (sortFields != null && !isQueryTotalCount && sortFields.Any())
        {
            querySqlBuilder.Append(SqlConstant.SortOperatorOrderBy)
                           .Append(string.Empty)
                           .Append(string.Join(",", sortFields.Select(p => p.Field + p.OrderStr)));
        }

        if (pageIndex.HasValue && pageSize.HasValue && !isQueryTotalCount)
        {
            querySqlBuilder.AppendFormat(SqlConstant.PaginationOperatorTemplate, pageSize.Value,
                (pageIndex.Value - 1) * pageSize.Value);
        }

        return (querySqlBuilder.ToString(), parameters);
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
        IEnumerable<string> queryResultFields,
        IReadOnlyCollection<SqlWhereClauseInfoDto> sqlWhereClauses,
        List<InOperatorFieldDto> inOperatorFields = null,
        List<NotInOperatorFieldDto> notInOperatorFields = null,
        int? pageIndex = null,
        int? pageSize = null,
        IReadOnlyCollection<SortFieldDto> sortFields = null)
    {
        var parameters = new DynamicParameters();

        if (string.IsNullOrEmpty(necessaryCondition))
        {
            necessaryCondition = " where 1=1 ";
        }

        var baseQuerySqlBuilder = new StringBuilder();

        foreach (var sqlWhereClauseItem in sqlWhereClauses ?? [])
        {
            var whereConditionSql = SqlWhereClauseHelper.SplicingWhereConditionSql(sqlWhereClauseItem, parameters);
            baseQuerySqlBuilder.Append(whereConditionSql);
        }

        if (inOperatorFields?.Any() == true)
        {
            baseQuerySqlBuilder.Append(SpecialHandlerInOperator(inOperatorFields, MatchOperator.In, parameters));
        }

        if (notInOperatorFields?.Any() == true)
        {
            baseQuerySqlBuilder.Append(SpecialHandlerNotInOperator(notInOperatorFields, MatchOperator.NotIn, parameters));
        }

        var countSql = $" SELECT COUNT(1) FROM {tableName} {necessaryCondition} {baseQuerySqlBuilder}";
        if (sortFields != null && sortFields.Any())
        {
            baseQuerySqlBuilder.Append(SqlConstant.SortOperatorOrderBy)
                               .Append(string.Empty)
                               .Append(string.Join(",", sortFields.Select(p => p.Field + p.OrderStr)));
        }

        if (pageIndex.HasValue && pageSize.HasValue)
        {
            baseQuerySqlBuilder.AppendFormat(SqlConstant.PaginationOperatorTemplate, pageSize.Value,
                (pageIndex.Value - 1) * pageSize.Value);
        }

        var querySql =
            $" SELECT {string.Join(",", queryResultFields)} FROM {tableName} {necessaryCondition} {baseQuerySqlBuilder}";

        return (querySql, countSql, parameters);
    }

    /// <summary>
    /// 单独 特殊处理 in查询的列筛选入参
    /// </summary>
    /// <returns></returns>
    private static string SpecialHandlerInOperator(IEnumerable<InOperatorFieldDto> inOperatorFields,
                                                   MatchOperator matchOperator,
                                                   DynamicParameters parameters)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var inOperatorField in inOperatorFields)
        {
            if (inOperatorField.Ids.Any())
            {
                var sqlOperation = SqlOperationFactory.CreateSqlOperation(matchOperator);

                // 根据ValueType使用新的重载方法
                var sqlResult = sqlOperation.GetSqlSentenceResult(inOperatorField.Field,
                    inOperatorField.Ids, parameters, inOperatorField.ValueType);

                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} {sqlResult}");
            }
            else
            {
                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} 1=2 ");
            }
        }

        return sqlBuilder.ToString();
    }

    /// <summary>
    ///单独 特殊处理 not in查询的列筛选入参
    /// </summary>
    /// <returns></returns>
    private static string SpecialHandlerNotInOperator(IEnumerable<NotInOperatorFieldDto> notInOperatorFields,
                                                      MatchOperator matchOperator,
                                                      DynamicParameters parameters)
    {
        var sqlBuilder = new StringBuilder();
        foreach (var notInOperatorField in notInOperatorFields)
        {
            if (notInOperatorField.Ids.Any())
            {
                var sqlOperation = SqlOperationFactory.CreateSqlOperation(matchOperator);

                // 根据ValueType使用新的重载方法
                var sqlResult = sqlOperation.GetSqlSentenceResult(notInOperatorField.Field,
                    notInOperatorField.Ids, parameters, notInOperatorField.ValueType);

                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} {sqlResult}");
            }
            else
            {
                sqlBuilder.Append($" {SqlConstant.LogicalOperatorAnd} 1=1 ");
            }
        }

        return sqlBuilder.ToString();
    }
}