using System.Text;
using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement.Select;

namespace JSqlParser.Net.Expression;

/// <summary>
/// STRUCT type expression (BigQuery / DuckDB).
/// Supports: STRUCT&lt;INT64&gt;, STRUCT(1 AS a, 'abc' AS b), { a:1, b:'abc' }::STRUCT(...)
/// </summary>
public class StructType : ASTNodeAccessImpl, Expression
{
    public Dialect StructDialect { get; set; } = Dialect.BigQuery;
    public string? Keyword { get; set; }
    public List<KeyValuePair<string, string>>? Parameters { get; set; }
    public List<SelectItem>? Arguments { get; set; }

    public StructType() { }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        if (StructDialect != Dialect.DuckDB && Keyword != null)
            builder.Append(Keyword);

        if (StructDialect != Dialect.DuckDB && Parameters != null && Parameters.Count > 0)
        {
            builder.Append('<');
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i > 0) builder.Append(',');
                var e = Parameters[i];
                if (!string.IsNullOrEmpty(e.Key))
                    builder.Append(e.Key).Append(' ');
                builder.Append(e.Value);
            }
            builder.Append('>');
        }

        if (Arguments != null && Arguments.Count > 0)
        {
            if (StructDialect == Dialect.DuckDB)
            {
                builder.Append("{ ");
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (i > 0) builder.Append(',');
                    var e = Arguments[i];
                    builder.Append(e.Alias?.Name).Append(':').Append(e.Expression);
                }
                builder.Append(" }");
            }
            else
            {
                builder.Append('(');
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (i > 0) builder.Append(',');
                    builder.Append(Arguments[i]);
                }
                builder.Append(')');
            }
        }

        if (StructDialect == Dialect.DuckDB && Parameters != null && Parameters.Count > 0)
        {
            builder.Append("::STRUCT( ");
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(Parameters[i].Key).Append(' ').Append(Parameters[i].Value);
            }
            builder.Append(')');
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();

    public enum Dialect
    {
        BigQuery,
        DuckDB
    }
}
