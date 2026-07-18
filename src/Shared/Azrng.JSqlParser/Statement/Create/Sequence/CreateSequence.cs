using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Create.Sequence;

/// <summary>
/// CREATE SEQUENCE 语句。
/// <para>语法：CREATE SEQUENCE [schema.]name [参数...]</para>
/// 移植自上游 JSqlParser 5.4 的 CreateSequence + Sequence。
/// </summary>
public class CreateSequence : ASTNodeAccessImpl, IStatement
{
    public Azrng.JSqlParser.Schema.Sequence? Sequence { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var seq = Sequence;
        if (seq == null) return "CREATE SEQUENCE";
        var result = "CREATE SEQUENCE " + seq.GetFullyQualifiedName();
        if (seq.Parameters != null && seq.Parameters.Count > 0)
        {
            foreach (var param in seq.Parameters)
            {
                result += " " + param.FormatParameter();
            }
        }
        return result;
    }
}
