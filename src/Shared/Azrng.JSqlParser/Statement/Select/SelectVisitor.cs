namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Visitor interface for traversing SELECT statement trees.
/// Generic version (JSqlParser 5.0+): returns T, accepts context S.
/// </summary>
public interface SelectVisitor<T>
{
    T Visit<S>(PlainSelect plainSelect, S context);
    T Visit<S>(SetOperationList setOpList, S context);
    T Visit<S>(WithItem withItem, S context);
    T Visit<S>(Piped.FromQuery fromQuery, S context);
    T Visit<S>(TableStatement tableStatement, S context);

    // Convenience overloads (no context)
    void Visit(PlainSelect plainSelect) => Visit<object?>(plainSelect, default);
    void Visit(SetOperationList setOpList) => Visit<object?>(setOpList, default);
    void Visit(WithItem withItem) => Visit<object?>(withItem, default);
    void Visit(Piped.FromQuery fromQuery) => Visit<object?>(fromQuery, default);
    void Visit(TableStatement tableStatement) => Visit<object?>(tableStatement, default);
}
