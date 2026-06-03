using System.Text;

namespace Azrng.JSqlParser.Parser;

/// <summary>
/// Default implementation of ASTNodeAccess.
/// </summary>
public class ASTNodeAccessImpl : ASTNodeAccess
{
    [NonSerialized]
    private SimpleNode? _node;

    public SimpleNode? GetASTNode() => _node;

    public void SetASTNode(SimpleNode node)
    {
        _node = node;
    }

    public virtual StringBuilder AppendTo(StringBuilder builder)
    {
        var simpleNode = GetASTNode();
        if (simpleNode == null) return builder;

        var token = simpleNode.JjtGetFirstToken();
        var lastToken = simpleNode.JjtGetLastToken();

        while (token?.Next != null && token.AbsoluteEnd <= lastToken.AbsoluteEnd)
        {
            builder.Append(' ').Append(token.Image);
            token = token.Next;
        }

        return builder;
    }
}
