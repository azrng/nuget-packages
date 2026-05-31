namespace JSqlParser.Net.Parser;

/// <summary>
/// Represents a node in the AST (Abstract Syntax Tree).
/// This is a placeholder for the JJTree-generated SimpleNode.
/// </summary>
public class SimpleNode
{
    public Token? FirstToken { get; set; }
    public Token? LastToken { get; set; }

    public virtual Token JjtGetFirstToken() => FirstToken!;
    public virtual Token JjtGetLastToken() => LastToken!;
}
