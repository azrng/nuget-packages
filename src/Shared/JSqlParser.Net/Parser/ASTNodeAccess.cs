namespace JSqlParser.Net.Parser;

/// <summary>
/// Interface for classes that can provide access to their underlying AST node.
/// </summary>
public interface ASTNodeAccess
{
    SimpleNode? GetASTNode();
    void SetASTNode(SimpleNode node);
}
