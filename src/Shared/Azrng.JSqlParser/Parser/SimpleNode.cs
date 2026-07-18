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
}
