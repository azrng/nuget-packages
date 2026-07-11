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

        // L3 修复：去掉 token?.Next != null 前置条件（会漏掉区间内最后一个 token），
        // 改由 AbsoluteEnd 边界控制循环。token 为 null 时直接退出。
        while (token != null && token.AbsoluteEnd <= lastToken.AbsoluteEnd)
        {
            builder.Append(' ').Append(token.Image);
            token = token.Next;
        }

        return builder;
    }
}
