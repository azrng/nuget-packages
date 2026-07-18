using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// ALTER SEQUENCE 语句（Oracle/PostgreSQL），对齐上游 AlterSequence。
/// 形式：<c>ALTER SEQUENCE name [RESTART [WITH n]] [INCREMENT BY n] [MINVALUE n] [MAXVALUE n] [CACHE n] [CYCLE|NOCYCLE]</c>。
/// 复用 <see cref="Schema.Sequence"/> 承载名称与结构化参数。
/// </summary>
public class AlterSequence : ASTNodeAccessImpl, IStatement
{
    public Sequence? Sequence { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("ALTER SEQUENCE ");
        if (Sequence != null)
        {
            // Sequence.ToString 只输出限定名，参数需单独拼接
            sb.Append(Sequence.GetFullyQualifiedName());
            if (Sequence.Parameters is { Count: > 0 })
            {
                for (int i = 0; i < Sequence.Parameters.Count; i++)
                {
                    sb.Append(' ');
                    sb.Append(Sequence.Parameters[i].FormatParameter());
                }
            }
        }
        return sb.ToString();
    }
}
