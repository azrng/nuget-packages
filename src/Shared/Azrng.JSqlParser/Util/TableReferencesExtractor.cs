using Azrng.JSqlParser.Models;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Piped;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Util;

/// <summary>
/// 表引用提取引擎：遍历 SELECT 语句的 FROM/JOIN/子查询，收集所有 <see cref="TableReference"/>。
/// </summary>
/// <remarks>
/// 不对外公开。遍历路径对齐 TablesNamesFinder.VisitPlainSelect（已验证覆盖 JOIN/子查询/VALUES/
/// TableFunction/JsonTable），扩展为返回带别名与全名的结构化结果。
/// 不做别名优先字典 key、去重——返回所有出现（含自连接的多次），业务方自行 DistinctBy 或按 Key 聚合。
/// </remarks>
internal static class TableReferencesExtractor
{
    /// <summary>提取 SELECT 语句（含 UNION 多分支、CTE、子查询）中引用的全部表。</summary>
    public static IReadOnlyList<TableReference> Extract(Statement.Statement statement)
    {
        var result = new List<TableReference>();
        CollectFromStatement(statement, result);
        return result;
    }

    private static void CollectFromStatement(Statement.Statement statement, List<TableReference> result)
    {
        // 仅 SELECT 语句有 FROM 子句；其余语句（INSERT/UPDATE/DELETE/MERGE）的表引用
        // 业务方通常需要 target/source 区分，不应混在一起，故这里只处理 SELECT。
        if (statement is not Select select)
        {
            return;
        }

        CollectFromSelect(select, result);
    }

    private static void CollectFromSelect(Select select, List<TableReference> result)
    {
        // CTE（WITH 子句）定义中的子查询也要遍历
        if (select.WithItemsList != null)
        {
            foreach (var withItem in select.WithItemsList)
            {
                if (withItem.Select != null) CollectFromSelect(withItem.Select, result);
            }
        }

        switch (select)
        {
            case PlainSelect plainSelect:
                CollectFromItem(plainSelect.IFromItem, result);
                if (plainSelect.Joins != null)
                {
                    foreach (var join in plainSelect.Joins)
                        CollectFromItem(join.RightItem, result);
                }
                break;

            case SetOperationList setOpList:
                foreach (var branch in setOpList.Selects)
                    CollectFromSelect(branch, result);
                break;

            case FromQuery fromQuery:
                CollectFromItem(fromQuery.IFromItem, result);
                if (fromQuery.Joins != null)
                {
                    foreach (var join in fromQuery.Joins)
                        CollectFromItem(join.RightItem, result);
                }
                break;

            case Values values:
                // VALUES 表构造器本身不是表引用，其内子查询由表达式层处理
                break;

            case TableStatement tableStatement:
                if (tableStatement.Table != null) AddTable(tableStatement.Table, result);
                break;
        }
    }

    /// <summary>遍历单个 FROM 项（表/子查询/VALUES/表函数），对齐 TablesNamesFinder.VisitFromItem。</summary>
    private static void CollectFromItem(IFromItem? fromItem, List<TableReference> result)
    {
        switch (fromItem)
        {
            case Table table:
                AddTable(table, result);
                break;

            case ParenthesedSelect parenthesedSelect:
                // LateralSubSelect 继承 ParenthesedSelect，一并覆盖
                CollectFromSelect(parenthesedSelect.Select, result);
                break;

            case Values:
                // FROM (VALUES ...) — VALUES 内部不产生表引用
                break;

            case TableFunction:
                // FROM generate_series(...) 等，函数名不作为表引用
                break;

            case JsonTable:
                // JSON_TABLE 的源是 JSON 表达式，不引用数据库表
                break;
        }
    }

    private static void AddTable(Table table, List<TableReference> result)
    {
        result.Add(new TableReference
        {
            Name = table.Name ?? string.Empty,
            Alias = table.Alias?.Name,
            FullName = table.GetFullyQualifiedName() ?? table.Name ?? string.Empty
        });
    }
}
