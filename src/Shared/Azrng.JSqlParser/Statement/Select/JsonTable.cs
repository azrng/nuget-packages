using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// SQL/JSON 的 JSON_TABLE 表函数。
/// <para>
/// 语法：JSON_TABLE(json_expr [, path] COLUMNS (col_def, ...)) [AS] alias
/// </para>
/// 移植自上游 JSqlParser commit c5e2fdcd 的 JsonTableFunction（简化版）。
/// </summary>
public class JsonTable : ASTNodeAccessImpl, FromItem
{
    /// <summary>JSON 文档表达式（第一个参数）。</summary>
    public Azrng.JSqlParser.Expression.Expression? JsonExpression { get; set; }

    /// <summary>输入是否带 FORMAT JSON（Oracle），未指定时为 false。</summary>
    public bool InputFormatJson { get; set; }

    /// <summary>JSON 路径（如 '$.path'），未指定时为 null。</summary>
    public string? PathExpression { get; set; }

    /// <summary>PASSING 子句的参数列表（value AS name 形式）。</summary>
    public List<JsonTablePassingClause> PassingClauses { get; } = new();

    /// <summary>ON ERROR 行为（NULL/ERROR/EMPTY/TRUE/FALSE 等），未指定时为 null。</summary>
    public JsonFunction.JsonOnResponseBehavior? OnErrorBehavior { get; set; }

    /// <summary>ON EMPTY 行为，未指定时为 null。</summary>
    public JsonFunction.JsonOnResponseBehavior? OnEmptyBehavior { get; set; }

    /// <summary>TYPE (STRICT|LAX) 解析类型子句，未指定时为 null。</summary>
    public string? ParsingType { get; set; }

    /// <summary>PLAN [DEFAULT] (plan_expr) Oracle 计划子句，未指定时为 null。</summary>
    public string? Plan { get; set; }

    /// <summary>列定义列表。</summary>
    public List<JsonTableColumn> Columns { get; set; } = new();

    /// <summary>FROM 子句中的别名。</summary>
    public Alias? Alias { get; set; }

    public Alias? GetAlias() => Alias;
    public void SetAlias(Alias alias) => Alias = alias;

    public override string ToString()
    {
        var sb = new StringBuilder("JSON_TABLE(");
        sb.Append(JsonExpression);
        if (InputFormatJson) sb.Append(" FORMAT JSON");
        if (PathExpression != null) sb.Append(", ").Append(PathExpression);

        // PASSING
        if (PassingClauses.Count > 0)
        {
            sb.Append(" PASSING ").Append(string.Join(", ", PassingClauses));
        }

        // TYPE (STRICT|LAX)
        if (ParsingType != null) sb.Append(" TYPE (").Append(ParsingType).Append(')');

        // ON EMPTY / ON ERROR（按上游，COLUMNS 前）
        if (OnEmptyBehavior != null)
        {
            sb.Append(' ').Append(FormatBehavior(OnEmptyBehavior)).Append(" ON EMPTY");
        }
        if (OnErrorBehavior != null)
        {
            sb.Append(' ').Append(FormatBehavior(OnErrorBehavior)).Append(" ON ERROR");
        }

        sb.Append(" COLUMNS (");
        for (int i = 0; i < Columns.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Columns[i]);
        }
        sb.Append(')');

        // PLAN [DEFAULT] (plan_expr) Oracle，COLUMNS 后
        if (Plan != null) sb.Append(' ').Append(Plan);

        sb.Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }

    /// <summary>
    /// 格式化 ON EMPTY / ON ERROR 行为文本（无尾空格，避免与后接 " ON EMPTY/ON ERROR" 产生双空格）。
    /// 不直接用 JsonOnResponseBehavior.AppendTo（其 EMPTY 分支带尾空格，为 EMPTY ARRAY 设计）。
    /// </summary>
    internal static string FormatBehavior(JsonFunction.JsonOnResponseBehavior? behavior) => behavior switch
    {
        null => "",
        { Type: JsonFunction.OnResponseBehaviorType.DEFAULT } => $"DEFAULT {behavior.Expression}",
        { Type: JsonFunction.OnResponseBehaviorType.EMPTY_ARRAY } => "EMPTY ARRAY",
        { Type: JsonFunction.OnResponseBehaviorType.EMPTY_OBJECT } => "EMPTY OBJECT",
        _ => behavior.Type.ToString()
    };
}

/// <summary>
/// JSON_TABLE 的 PASSING 子句项：value AS name。
/// </summary>
public class JsonTablePassingClause
{
    public Azrng.JSqlParser.Expression.Expression? ValueExpression { get; set; }

    public string? ParameterName { get; set; }

    public override string ToString()
        => ParameterName != null ? $"{ValueExpression} AS {ParameterName}" : ValueExpression?.ToString() ?? "";
}

/// <summary>
/// JSON_TABLE 的列定义。
/// </summary>
public class JsonTableColumn
{
    /// <summary>列名。</summary>
    public string Name { get; set; } = "";

    /// <summary>列数据类型（如 INT、VARCHAR(100)），FOR ORDINALITY 时为 null。</summary>
    public string? DataType { get; set; }

    /// <summary>JSON 路径（PATH '...'），未指定时为 null。</summary>
    public string? Path { get; set; }

    /// <summary>是否为 FORMAT JSON（Oracle），未指定时为 false。</summary>
    public bool FormatJson { get; set; }

    /// <summary>FORMAT JSON 的 ENCODING（如 UTF8），未指定时为 null。</summary>
    public string? Encoding { get; set; }

    /// <summary>(ALLOW|DISALLOW) SCALARS（Oracle），未指定时为 null。</summary>
    public JsonFunction.ScalarsType? Scalars { get; set; }

    /// <summary>EXISTS 谓词列（Oracle），未指定时为 false。</summary>
    public bool Exists { get; set; }

    /// <summary>列级 WRAPPER 子句（WITHOUT|WITH [CONDITIONAL|UNCONDITIONAL] [ARRAY] WRAPPER），未指定时为 null。</summary>
    public JsonFunction.WrapperType? Wrapper { get; set; }
    public JsonFunction.WrapperMode? WrapperMode { get; set; }
    public bool WrapperArray { get; set; }

    /// <summary>列级 QUOTES 子句（(KEEP|OMIT) QUOTES [ON SCALAR STRING]），未指定时为 null。</summary>
    public JsonFunction.QuotesType? Quotes { get; set; }
    public bool QuotesOnScalarString { get; set; }

    /// <summary>列级 ON EMPTY 行为，未指定时为 null。</summary>
    public JsonFunction.JsonOnResponseBehavior? OnEmptyBehavior { get; set; }

    /// <summary>列级 ON ERROR 行为，未指定时为 null。</summary>
    public JsonFunction.JsonOnResponseBehavior? OnErrorBehavior { get; set; }

    /// <summary>是否为 FOR ORDINALITY（序号列）。</summary>
    public bool ForOrdinality { get; set; }

    /// <summary>NESTED PATH 子列定义（递归），非 NESTED 时为 null。</summary>
    public List<JsonTableColumn>? NestedColumns { get; set; }

    /// <summary>是否为 NESTED PATH 列。</summary>
    public bool IsNested => NestedColumns != null;

    public override string ToString()
    {
        if (IsNested)
        {
            var sb = new StringBuilder("NESTED PATH ");
            if (Path != null) sb.Append(Path);
            sb.Append(" COLUMNS (");
            for (int i = 0; i < NestedColumns!.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(NestedColumns[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }
        if (ForOrdinality) return $"{Name} FOR ORDINALITY";
        var sb2 = new StringBuilder(Name);
        if (DataType != null) sb2.Append(' ').Append(DataType);
        if (Exists) sb2.Append(" EXISTS");
        if (Path != null) sb2.Append(" PATH ").Append(Path);
        if (FormatJson)
        {
            sb2.Append(" FORMAT JSON");
            if (Encoding != null) sb2.Append(" ENCODING ").Append(Encoding);
        }
        // WRAPPER：WITHOUT | WITH [CONDITIONAL|UNCONDITIONAL] [ARRAY] WRAPPER
        if (Wrapper != null)
        {
            sb2.Append(' ');
            if (Wrapper == JsonFunction.WrapperType.WITHOUT) sb2.Append("WITHOUT");
            else
            {
                sb2.Append("WITH");
                if (WrapperMode != null) sb2.Append(' ').Append(WrapperMode.ToString()!.ToUpper());
            }
            if (WrapperArray) sb2.Append(" ARRAY");
            sb2.Append(" WRAPPER");
        }
        // QUOTES：(KEEP|OMIT) QUOTES [ON SCALAR STRING]
        if (Quotes != null)
        {
            sb2.Append(' ').Append(Quotes == JsonFunction.QuotesType.KEEP ? "KEEP" : "OMIT").Append(" QUOTES");
            if (QuotesOnScalarString) sb2.Append(" ON SCALAR STRING");
        }
        // SCALARS：(ALLOW|DISALLOW) SCALARS
        if (Scalars != null)
        {
            sb2.Append(' ').Append(Scalars == JsonFunction.ScalarsType.ALLOW ? "ALLOW" : "DISALLOW").Append(" SCALARS");
        }
        if (OnEmptyBehavior != null)
        {
            sb2.Append(' ').Append(JsonTable.FormatBehavior(OnEmptyBehavior)).Append(" ON EMPTY");
        }
        if (OnErrorBehavior != null)
        {
            sb2.Append(' ').Append(JsonTable.FormatBehavior(OnErrorBehavior)).Append(" ON ERROR");
        }
        return sb2.ToString();
    }
}
