using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// JSON 标量函数统一 AST（JSON_OBJECT / JSON_ARRAY / JSON_VALUE / JSON_QUERY / JSON_EXISTS）。
/// 与上游 JsonFunction 对齐。
/// </summary>
public class JsonFunction : ASTNodeAccessImpl, Expression
{
    public enum FunctionType
    {
        OBJECT,
        ARRAY,
        VALUE,
        QUERY,
        EXISTS,
        /// <summary>MySQL 风格 OBJECTAGG 逗号分隔键值（上游标记 Deprecated 但仍活跃）。</summary>
        POSTGRES_OBJECT
    }

    public enum OnResponseBehaviorType
    {
        ERROR,
        NULL,
        DEFAULT,
        EMPTY,
        EMPTY_ARRAY,
        EMPTY_OBJECT,
        TRUE,
        FALSE,
        UNKNOWN
    }

    public enum WrapperType
    {
        WITHOUT,
        WITH
    }

    public enum WrapperMode
    {
        CONDITIONAL,
        UNCONDITIONAL
    }

    public enum QuotesType
    {
        KEEP,
        OMIT
    }

    public enum OnNullType
    {
        NULL,
        ABSENT
    }

    public enum UniqueKeysType
    {
        WITH,
        WITHOUT
    }

    /// <summary>ON EMPTY / ON ERROR 行为：type + 可选 DEFAULT 表达式。</summary>
    public class JsonOnResponseBehavior
    {
        public OnResponseBehaviorType Type { get; set; }
        public Expression? Expression { get; set; }

        public JsonOnResponseBehavior() { }

        public JsonOnResponseBehavior(OnResponseBehaviorType type)
        {
            Type = type;
        }

        public JsonOnResponseBehavior(OnResponseBehaviorType type, Expression? expression)
        {
            Type = type;
            Expression = expression;
        }

        public void AppendTo(StringBuilder sb)
        {
            switch (Type)
            {
                case OnResponseBehaviorType.DEFAULT:
                    sb.Append("DEFAULT ").Append(Expression);
                    break;
                case OnResponseBehaviorType.EMPTY:
                    // 上游 JsonOnResponseBehavior.EMPTY 输出 "EMPTY "（带尾空格）
                    sb.Append("EMPTY ");
                    break;
                case OnResponseBehaviorType.EMPTY_ARRAY:
                    sb.Append("EMPTY ARRAY");
                    break;
                case OnResponseBehaviorType.EMPTY_OBJECT:
                    sb.Append("EMPTY OBJECT");
                    break;
                default:
                    sb.Append(Type.ToString());
                    break;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            AppendTo(sb);
            return sb.ToString();
        }
    }

    public FunctionType Type { get; set; }

    /// <summary>OBJECT 的键值对列表。</summary>
    public List<JsonKeyValuePair> KeyValuePairs { get; } = new();

    /// <summary>ARRAY 的元素表达式列表。</summary>
    public List<JsonFunctionExpression> Expressions { get; } = new();

    /// <summary>VALUE/QUERY/EXISTS 的 PASSING 子句表达式列表。</summary>
    public List<Expression> PassingExpressions { get; } = new();

    /// <summary>QUERY 的 Legacy 额外 path 参数（已序列化为字符串原样拼接）。</summary>
    public List<string> AdditionalQueryPathArguments { get; } = new();

    public OnNullType? OnNull { get; set; }

    public UniqueKeysType? UniqueKeys { get; set; }

    public bool Strict { get; set; }

    public JsonFunctionExpression? InputExpression { get; set; }

    public Expression? JsonPathExpression { get; set; }

    /// <summary>RETURNING 数据类型（字符串形式，与 CastExpression 一致）。</summary>
    public string? ReturningType { get; set; }

    public bool ReturningFormatJson { get; set; }

    public string? ReturningEncoding { get; set; }

    public JsonOnResponseBehavior? OnEmptyBehavior { get; set; }

    public JsonOnResponseBehavior? OnErrorBehavior { get; set; }

    public WrapperType? Wrapper { get; set; }

    public WrapperMode? WrapperModeValue { get; set; }

    public bool WrapperArray { get; set; }

    public QuotesType? Quotes { get; set; }

    public bool QuotesOnScalarString { get; set; }

    public JsonFunction() { }

    public JsonFunction(FunctionType type)
    {
        Type = type;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder();
        switch (Type)
        {
            case FunctionType.OBJECT:
                AppendObject(sb);
                break;
            case FunctionType.ARRAY:
                AppendArray(sb);
                break;
            case FunctionType.VALUE:
                AppendValue(sb);
                break;
            case FunctionType.EXISTS:
                AppendExists(sb);
                break;
            case FunctionType.QUERY:
                AppendQuery(sb);
                break;
            default:
                sb.Append("JSON_").Append(Type);
                break;
        }
        return sb.ToString();
    }

    private void AppendQuery(StringBuilder sb)
    {
        // JSON_QUERY(input, path [PASSING ...] [RETURNING ...] [WRAPPER ...] [QUOTES ...] [ON EMPTY ...] [ON ERROR ...] [, path2 ...])
        sb.Append("JSON_QUERY(");
        AppendInputAndPath(sb);
        AppendPassing(sb);
        AppendReturning(sb);
        AppendWrapper(sb);
        AppendQuotes(sb);
        AppendOnResponse(sb, OnEmptyBehavior, "ON EMPTY");
        AppendOnResponse(sb, OnErrorBehavior, "ON ERROR");
        // Legacy 额外 path 参数（仅无 PASSING 时存在），对齐上游 additionalQueryPathArguments
        foreach (var extra in AdditionalQueryPathArguments)
        {
            sb.Append(", ").Append(extra);
        }
        sb.Append(')');
    }

    private void AppendWrapper(StringBuilder sb)
    {
        if (Wrapper == null) return;
        sb.Append(' ');
        if (Wrapper == WrapperType.WITHOUT)
        {
            sb.Append("WITHOUT");
        }
        else
        {
            sb.Append("WITH");
            if (WrapperModeValue != null)
            {
                sb.Append(' ').Append(WrapperModeValue.ToString().ToUpper());
            }
        }
        if (WrapperArray)
        {
            sb.Append(" ARRAY");
        }
        sb.Append(" WRAPPER");
    }

    private void AppendQuotes(StringBuilder sb)
    {
        if (Quotes == null) return;
        sb.Append(' ').Append(Quotes == QuotesType.KEEP ? "KEEP" : "OMIT").Append(" QUOTES");
        if (QuotesOnScalarString)
        {
            sb.Append(" ON SCALAR STRING");
        }
    }

    private void AppendValue(StringBuilder sb)
    {
        // JSON_VALUE(input, path [PASSING ...] [RETURNING ...] [ON EMPTY ...] [ON ERROR ...])
        sb.Append("JSON_VALUE(");
        AppendInputAndPath(sb);
        AppendPassing(sb);
        if (ReturningType != null)
        {
            sb.Append(" RETURNING ").Append(ReturningType);
        }
        AppendOnResponse(sb, OnEmptyBehavior, "ON EMPTY");
        AppendOnResponse(sb, OnErrorBehavior, "ON ERROR");
        sb.Append(')');
    }

    private void AppendExists(StringBuilder sb)
    {
        // JSON_EXISTS(input, path [PASSING ...] [ON ERROR ...])
        sb.Append("JSON_EXISTS(");
        AppendInputAndPath(sb);
        AppendPassing(sb);
        AppendOnResponse(sb, OnErrorBehavior, "ON ERROR");
        sb.Append(')');
    }

    private void AppendInputAndPath(StringBuilder sb)
    {
        if (InputExpression != null)
        {
            InputExpression.AppendTo(sb);
        }
        sb.Append(", ").Append(JsonPathExpression);
    }

    private void AppendPassing(StringBuilder sb)
    {
        if (PassingExpressions.Count > 0)
        {
            sb.Append(" PASSING ").Append(string.Join(", ", PassingExpressions));
        }
    }

    private void AppendOnResponse(StringBuilder sb, JsonOnResponseBehavior? behavior, string clause)
    {
        if (behavior != null)
        {
            sb.Append(' ').Append(behavior).Append(' ').Append(clause);
        }
    }

    private void AppendObject(StringBuilder sb)
    {
        // 上游 OBJECT 输出格式："JSON_OBJECT( " + kvps + 子句 + " )"（末尾空格+括号）
        // 空键值对时输出 "JSON_OBJECT( )"
        sb.Append("JSON_OBJECT( ");
        sb.Append(string.Join(", ", KeyValuePairs));
        if (OnNull != null)
        {
            sb.Append(OnNull == OnNullType.ABSENT ? " ABSENT ON NULL" : " NULL ON NULL");
        }
        if (Strict)
        {
            sb.Append(" STRICT");
        }
        if (UniqueKeys != null)
        {
            sb.Append(UniqueKeys == UniqueKeysType.WITH ? " WITH UNIQUE KEYS" : " WITHOUT UNIQUE KEYS");
        }
        AppendReturning(sb);
        sb.Append(" )");
    }

    private void AppendArray(StringBuilder sb)
    {
        // 上游 ARRAY 输出格式：非空 "JSON_ARRAY( " + ... + ") "；空 "JSON_ARRAY()"
        if (Expressions.Count == 0 && OnNull == null && ReturningType == null)
        {
            sb.Append("JSON_ARRAY()");
            return;
        }
        sb.Append("JSON_ARRAY( ");
        sb.Append(string.Join(", ", Expressions));
        if (OnNull != null)
        {
            sb.Append(OnNull == OnNullType.ABSENT ? " ABSENT ON NULL" : " NULL ON NULL");
        }
        AppendReturning(sb);
        sb.Append(")");
    }

    private void AppendReturning(StringBuilder sb)
    {
        if (ReturningType != null)
        {
            sb.Append(" RETURNING ").Append(ReturningType);
            if (ReturningFormatJson)
            {
                sb.Append(" FORMAT JSON");
                if (ReturningEncoding != null)
                {
                    sb.Append(" ENCODING ").Append(ReturningEncoding);
                }
            }
        }
    }
}
