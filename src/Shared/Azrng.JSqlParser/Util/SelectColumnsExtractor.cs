using Azrng.JSqlParser.Models;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Util;

/// <summary>
/// SELECT 列结构化提取引擎：把 <see cref="PlainSelect.SelectItems"/> 转为
/// 中性 <see cref="SelectColumn"/> 列表，区分 * / t.* / 列 / 表达式四种形态。
/// </summary>
/// <remarks>
/// 不对外公开。对应 LocalSqlParser.CollectSelectItems 的纯 AST 提取部分；
/// 虚拟列必填别名校验、来源列推断等产品规则由业务方拿到结果后自行处理。
/// </remarks>
internal static class SelectColumnsExtractor
{
    /// <summary>提取单个 PlainSelect 分支的 SELECT 列。</summary>
    public static IReadOnlyList<SelectColumn> Extract(PlainSelect? plainSelect)
    {
        var result = new List<SelectColumn>();
        if (plainSelect?.SelectItems == null) return result;

        foreach (var item in plainSelect.SelectItems)
        {
            result.Add(ToSelectColumn(item));
        }
        return result;
    }

    private static SelectColumn ToSelectColumn(SelectItem item)
    {
        var alias = item.Alias?.Name;

        switch (item.Expression)
        {
            case AllColumns:
                return new SelectColumn { Kind = SelectColumnKind.All, Alias = alias };

            case AllTableColumns allTableColumns:
                return new SelectColumn
                {
                    Kind = SelectColumnKind.AllTable,
                    TableAlias = allTableColumns.Table.Name,
                    Alias = alias
                };

            case Column column:
                return new SelectColumn
                {
                    Kind = SelectColumnKind.Column,
                    TableAlias = column.Table?.Name,
                    ColumnName = column.ColumnName,
                    Alias = alias,
                    Expression = item.Expression
                };

            default:
                // 表达式列（函数、算术、CASE 等）
                return new SelectColumn
                {
                    Kind = SelectColumnKind.Expression,
                    Alias = alias,
                    Expression = item.Expression
                };
        }
    }
}
