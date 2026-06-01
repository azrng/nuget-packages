using System.Text;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement.Piped;

public abstract class PipeOperator : ASTNodeAccessImpl
{
    public abstract T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context);
    public abstract override StringBuilder AppendTo(StringBuilder builder);
}
