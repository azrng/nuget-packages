using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Helpers.NativeSqlParse;

/// <summary>
/// 探针：从 SQL 装配 <see cref="NativeSqlParseResult"/>。
/// </summary>
/// <remarks>
/// 逻辑与 <c>C:\Work\temp-tools\SynyiTools\Service\NativeSqlParserService.cs</c> 的 ParseSelect 一致，
/// 仅做两点最小适配：JExpression -> IExpression、IsNotNullOrWhiteSpace -> !string.IsNullOrWhiteSpace。
/// 供测试项目作为"上层消费者"样板，验证 Azrng.JSqlParser 类库的结构化提取能力。
/// </remarks>
public static class NativeSqlParserService
{
    public static NativeSqlParseResult Parse(string sql)
    {
        var statement = SqlParser.Parse(sql) ?? throw new ArgumentException("解析失败");
        if (statement is not Select select)
            throw new ArgumentException("仅支持SELECT语句");

        // 可变累加器与不可变结果 DTO 职责分离：避免拿结果对象当临时容器
        var context = new ParseContext();
        CollectSelect(select, context, includeColumns: true);

        foreach (var operatorInfo in context.OperatorList)
        {
            operatorInfo.TableName = !string.IsNullOrWhiteSpace(operatorInfo.Alias) &&
                                     context.TableNameList.TryGetValue(operatorInfo.Alias, out var tableName)
                ? tableName
                : string.Empty;
        }

        return new NativeSqlParseResult
               {
                   TableNameList = context.TableNameList,
                   ColumnList = context.ColumnList,
                   WhereList = context.OperatorList.Where(t => !string.IsNullOrWhiteSpace(t.Alias)).ToList()
               };
    }

    private static void CollectSelect(Select select, ParseContext context, bool includeColumns)
    {
        switch (select)
        {
            case PlainSelect plainSelect:
                CollectPlainSelect(plainSelect, context, includeColumns);
                break;

            case SetOperationList setOperationList:
                for (var index = 0; index < setOperationList.Selects.Count; index++)
                {
                    CollectSelect(setOperationList.Selects[index], context, includeColumns && index == 0);
                }

                break;

            default:
                throw new ArgumentException($"不支持的SELECT类型:{select.GetType().Name}");
        }
    }

    private static void CollectPlainSelect(PlainSelect select, ParseContext context, bool includeColumns)
    {
        CollectFromItem(select.FromItem, context);
        if (select.Joins != null)
        {
            foreach (var join in select.Joins)
            {
                CollectFromItem(join.RightItem, context);
                // JOIN 的 ON 连接条件也参与操作符收集（之前遗漏，导致 ON 中的条件丢失）
                if (join.OnExpressions != null)
                {
                    foreach (var onExpr in join.OnExpressions)
                    {
                        CollectOperators(onExpr, context, string.Empty);
                    }
                }
            }
        }

        if (includeColumns)
            CollectSelectItems(select.SelectItems, context);

        CollectOperators(select.Where, context, string.Empty);
        CollectOperators(select.Having, context, string.Empty);
    }

    private static void CollectFromItem(IFromItem? fromItem, ParseContext context)
    {
        switch (fromItem)
        {
            case Table table:
                AddTable(table, context);
                break;

            case ParenthesedSelect parenthesedSelect:
                CollectSelect(parenthesedSelect.Select, context, false);
                break;
        }
    }

    private static void AddTable(Table table, ParseContext context)
    {
        var alias = table.Alias?.Name;
        if (string.IsNullOrWhiteSpace(alias))
            alias = table.Name;

        if (!context.TableNameList.ContainsKey(alias))
            context.TableNameList.Add(alias, table.GetFullyQualifiedName());
    }

    private static void CollectSelectItems(List<SelectItem>? selectItems, ParseContext context)
    {
        if (selectItems == null)
            return;

        foreach (var item in selectItems)
        {
            switch (item.Expression)
            {
                case AllColumns:
                    context.ColumnList.Add(new NativeSqlSelectColumnInfo { TableAlias = "*", ColumnName = "*" });
                    break;

                case AllTableColumns allTableColumns:
                    context.ColumnList.Add(new NativeSqlSelectColumnInfo { TableAlias = allTableColumns.Table.Name, ColumnName = "*" });
                    break;

                case Column column:
                    context.ColumnList.Add(new NativeSqlSelectColumnInfo
                                           {
                                               TableAlias = GetColumnTableAlias(column, context),
                                               ColumnName = column.ColumnName,
                                               ColumnAlias = item.Alias?.Name
                                           });
                    break;

                default:
                    context.ColumnList.Add(BuildVirtualColumn(item, context));
                    break;
            }
        }
    }

    private static NativeSqlSelectColumnInfo BuildVirtualColumn(SelectItem item, ParseContext context)
    {
        var alias = item.Alias?.Name;
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException($"表达式列必须指定别名:{item.Expression}");

        var columns = CollectColumns(item.Expression)
                      .Select(column => new ColumnRef { TableAlias = GetColumnTableAlias(column, context), ColumnName = column.ColumnName })
                      .DistinctBy(column => $"{column.TableAlias}.{column.ColumnName}")
                      .ToList();
        var sourceColumn = columns.Count == 1 ? columns[0] : null;

        return new NativeSqlSelectColumnInfo
               {
                   TableAlias = sourceColumn?.TableAlias,
                   ColumnName = alias,
                   ColumnAlias = alias,
                   IsVirtual = true,
                   ExpressionSql = item.Expression.ToString(),
                   ColumnType = item.Expression is CastExpression castExpression ? castExpression.DataType : null,
                   SourceTableAlias = sourceColumn?.TableAlias,
                   SourceColumnName = sourceColumn?.ColumnName
               };
    }

    private static void CollectOperators(IExpression? expression, ParseContext context, string linkType)
    {
        if (expression == null)
            return;

        switch (expression)
        {
            case AndExpression andExpression:
                CollectOperators(andExpression.LeftExpression, context, linkType);
                CollectOperators(andExpression.RightExpression, context, "AND");
                break;

            case OrExpression orExpression:
                CollectOperators(orExpression.LeftExpression, context, linkType);
                CollectOperators(orExpression.RightExpression, context, "OR");
                break;

            case BinaryExpression binaryExpression:
                AddBinaryOperator(binaryExpression, context, linkType);
                break;

            case InExpression inExpression:
                AddOperator(inExpression.LeftExpression, inExpression.RightExpression, inExpression.Not ? "NOT IN" : "IN",
                    context, linkType, inExpression.ToString() ?? string.Empty);
                break;

            case Between between:
                AddOperator(between.LeftExpression, between.BetweenExpressionStart, between.Not ? "NOT BETWEEN" : "BETWEEN",
                    context, linkType, between.ToString() ?? string.Empty);
                AddOperator(between.LeftExpression, between.BetweenExpressionEnd, between.Not ? "NOT BETWEEN" : "BETWEEN",
                    context, "AND", between.ToString() ?? string.Empty);
                break;
        }
    }

    private static void AddBinaryOperator(BinaryExpression binaryExpression, ParseContext context, string linkType)
    {
        AddOperator(binaryExpression.LeftExpression, binaryExpression.RightExpression, binaryExpression.OperatorSymbol,
            context, linkType, binaryExpression.ToString() ?? string.Empty);
    }

    private static void AddOperator(IExpression leftExpression, IExpression rightExpression, string operatorText,
                                    ParseContext context, string linkType, string sqlInfo)
    {
        var column = leftExpression as Column;
        var parameterExpression = rightExpression;
        if (column == null && rightExpression is Column rightColumn)
        {
            column = rightColumn;
            parameterExpression = leftExpression;
        }

        if (column == null)
            return;

        var parameters = CollectParameters(parameterExpression);
        foreach (var parameter in parameters)
        {
            context.OperatorList.Add(new NativeSqlOperatorInfo
                                  {
                                      Alias = GetColumnTableAlias(column, context),
                                      LinkType = linkType,
                                      LeftExpression =
                                          new NativeSqlExpressionInfo
                                          {
                                              ColumnName = column.ColumnName,
                                              FullyQualifiedName = column.GetFullyQualifiedName(),
                                              Value = column.ToString()
                                          },
                                      RightExpression = new NativeSqlExpressionInfo { Name = parameter, Value = parameter },
                                      StringExpression = operatorText,
                                      SqlInfo = sqlInfo,
                                      Order = context.OperatorList.Count + 1
                                  });
        }
    }

    private static string GetColumnTableAlias(Column column, ParseContext context)
    {
        var tableAlias = column.Table?.Name;
        if (!string.IsNullOrWhiteSpace(tableAlias))
            return tableAlias;

        return context.TableNameList.Count == 1 ? context.TableNameList.Keys.First() : string.Empty;
    }

    private static List<Column> CollectColumns(IExpression expression)
    {
        return expression.Descendants<Column>().ToList();
    }

    private static List<string> CollectParameters(IExpression expression)
    {
        return expression.Descendants<JdbcNamedParameter>()
                         .Select(parameter => parameter.Name)
                         .Where(parameterName => !string.IsNullOrWhiteSpace(parameterName))
                         .ToList();
    }

    /// <summary>
    /// 解析过程中的可变累加器：承载遍历期间的表/列/操作符收集。
    /// 与 <see cref="NativeSqlParseResult"/>（结果 DTO）职责分离，避免拿结果对象当临时容器。
    /// </summary>
    private sealed class ParseContext
    {
        public Dictionary<string, string> TableNameList { get; } = new();

        public List<NativeSqlSelectColumnInfo> ColumnList { get; } = new();

        public List<NativeSqlOperatorInfo> OperatorList { get; } = new();
    }
}
