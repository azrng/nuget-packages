using System.Text;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// 表示 INSERT/UPDATE/DELETE 的 RETURNING/RETURN 子句。
/// <para>
/// 支持基础语法 <c>RETURNING col1, col2</c> 与 PostgreSQL 18 的
/// <c>RETURNING WITH (OLD AS o, NEW AS n) old.col, new.col</c>。
/// </para>
/// 移植自上游 JSqlParser 5.4 ReturningClause 及 commit f47a8b30 的 OLD/NEW 扩展。
/// </summary>
public class ReturningClause
{
    public enum Keyword
    {
        RETURN,
        RETURNING
    }

    public Keyword ReturningKeyword { get; set; } = Keyword.RETURNING;

    /// <summary>RETURNING WITH (..) 中的输出别名列表，未指定时为 null。</summary>
    public List<ReturningOutputAlias>? OutputAliases { get; set; }

    /// <summary>RETURNING 后的列/表达式列表。</summary>
    public List<SelectItem> SelectItems { get; set; } = new();

    public ReturningClause() { }

    public ReturningClause(Keyword keyword, List<SelectItem> selectItems,
        List<ReturningOutputAlias>? outputAliases = null)
    {
        ReturningKeyword = keyword;
        SelectItems = selectItems;
        OutputAliases = outputAliases;
    }

    public StringBuilder AppendTo(StringBuilder builder)
    {
        builder.Append(' ').Append(ReturningKeyword.ToString()).Append(' ');

        if (OutputAliases != null && OutputAliases.Count > 0)
        {
            builder.Append("WITH (");
            for (int i = 0; i < OutputAliases.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(OutputAliases[i]);
            }
            builder.Append(") ");
        }

        for (int i = 0; i < SelectItems.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(SelectItems[i]);
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
