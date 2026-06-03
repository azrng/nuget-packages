using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Piped;

public abstract class PipeOperator : ASTNodeAccessImpl
{
    public abstract T Accept<T, S>(PipeOperatorVisitor<T, S> visitor, S context);
    public abstract override StringBuilder AppendTo(StringBuilder builder);
}
