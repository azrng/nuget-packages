namespace Azrng.JSqlParser.Parser;

/// <summary>
/// Interface for classes that can provide access to their underlying AST node.
/// </summary>
public interface IASTNodeAccess
{
    SimpleNode? GetASTNode();
    void SetASTNode(SimpleNode node);
}
