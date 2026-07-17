namespace Azrng.JSqlParser.Parser;

/// <summary>
/// Represents a node in the AST (Abstract Syntax Tree).
/// This is a placeholder for the JJTree-generated SimpleNode.
/// </summary>
public class SimpleNode
{
    public Token? FirstToken { get; set; }
    public Token? LastToken { get; set; }

    /// <summary>AST 节点覆盖区间的首个 token。</summary>
    public virtual Token GetFirstToken() => FirstToken!;

    /// <summary>AST 节点覆盖区间的末个 token。</summary>
    public virtual Token GetLastToken() => LastToken!;

    /// <summary>兼容上游 JJTree 命名（JavaCC 历史包袱），转发到 <see cref="GetFirstToken"/>。</summary>
    [Obsolete("改用 " + nameof(GetFirstToken) + "()")]
    public virtual Token JjtGetFirstToken() => GetFirstToken();

    /// <summary>兼容上游 JJTree 命名（JavaCC 历史包袱），转发到 <see cref="GetLastToken"/>。</summary>
    [Obsolete("改用 " + nameof(GetLastToken) + "()")]
    public virtual Token JjtGetLastToken() => GetLastToken();
}
