namespace Azrng.JSqlParser.Statement;

/// <summary>
/// Visitor interface for traversing SQL statement trees.
/// Generic version (JSqlParser 5.0+): returns T, accepts context S.
/// </summary>
public interface StatementVisitor<T>
{
    T Visit<S>(Statements stmts, S context);
    T Visit<S>(Select.Select select, S context);
    T Visit<S>(Insert.Insert insert, S context);
    T Visit<S>(Update.Update update, S context);
    T Visit<S>(Delete.Delete delete, S context);
    T Visit<S>(Merge.Merge merge, S context);
    T Visit<S>(CreateTable.CreateTable createTable, S context);
    T Visit<S>(CreateView.CreateView createView, S context);
    T Visit<S>(CreateIndex.CreateIndex createIndex, S context);
    T Visit<S>(Alter.Alter alter, S context);
    T Visit<S>(Drop.Drop drop, S context);
    T Visit<S>(Truncate.Truncate truncate, S context);
    T Visit<S>(CommitStatement commitStatement, S context);
    T Visit<S>(RollbackStatement rollbackStatement, S context);
    T Visit<S>(SavepointStatement savepointStatement, S context);
    T Visit<S>(UseStatement use, S context);
    T Visit<S>(SetStatement set, S context);
    T Visit<S>(ShowStatement show, S context);
    T Visit<S>(DescribeStatement describe, S context);
    T Visit<S>(ExplainStatement explain, S context);
    T Visit<S>(GrantStatement grant, S context);
    T Visit<S>(UnsupportedStatement unsupportedStatement, S context);

    // JSqlParser 5.1 - Parenthesized DML for CTEs
    T Visit<S>(Select.ParenthesedInsert parenthesedInsert, S context);
    T Visit<S>(Select.ParenthesedUpdate parenthesedUpdate, S context);
    T Visit<S>(Select.ParenthesedDelete parenthesedDelete, S context);

    // JSqlParser 5.4
    T Visit<S>(SessionStatement sessionStatement, S context);

    // JSqlParser 5.4+ - LOCK TABLE
    T Visit<S>(Lock.LockStatement lockStatement, S context);

    // JSqlParser 5.4+ - CREATE POLICY (PostgreSQL RLS)
    T Visit<S>(Create.Policy.CreatePolicy createPolicy, S context);

    // JSqlParser 5.4+ - CREATE SEQUENCE
    T Visit<S>(Create.Sequence.CreateSequence createSequence, S context);

    // Convenience overloads (no context)
    void Visit(Statements stmts) => Visit<object?>(stmts, default);
    void Visit(Select.Select select) => Visit<object?>(select, default);
    void Visit(Insert.Insert insert) => Visit<object?>(insert, default);
    void Visit(Update.Update update) => Visit<object?>(update, default);
    void Visit(Delete.Delete delete) => Visit<object?>(delete, default);
    void Visit(Merge.Merge merge) => Visit<object?>(merge, default);
    void Visit(CreateTable.CreateTable createTable) => Visit<object?>(createTable, default);
    void Visit(CreateView.CreateView createView) => Visit<object?>(createView, default);
    void Visit(CreateIndex.CreateIndex createIndex) => Visit<object?>(createIndex, default);
    void Visit(Alter.Alter alter) => Visit<object?>(alter, default);
    void Visit(Drop.Drop drop) => Visit<object?>(drop, default);
    void Visit(Truncate.Truncate truncate) => Visit<object?>(truncate, default);
    void Visit(CommitStatement commitStatement) => Visit<object?>(commitStatement, default);
    void Visit(RollbackStatement rollbackStatement) => Visit<object?>(rollbackStatement, default);
    void Visit(SavepointStatement savepointStatement) => Visit<object?>(savepointStatement, default);
    void Visit(UseStatement use) => Visit<object?>(use, default);
    void Visit(SetStatement set) => Visit<object?>(set, default);
    void Visit(ShowStatement show) => Visit<object?>(show, default);
    void Visit(DescribeStatement describe) => Visit<object?>(describe, default);
    void Visit(ExplainStatement explain) => Visit<object?>(explain, default);
    void Visit(GrantStatement grant) => Visit<object?>(grant, default);
    void Visit(UnsupportedStatement unsupportedStatement) => Visit<object?>(unsupportedStatement, default);
    void Visit(Select.ParenthesedInsert parenthesedInsert) => Visit<object?>(parenthesedInsert, default);
    void Visit(Select.ParenthesedUpdate parenthesedUpdate) => Visit<object?>(parenthesedUpdate, default);
    void Visit(Select.ParenthesedDelete parenthesedDelete) => Visit<object?>(parenthesedDelete, default);
    void Visit(Lock.LockStatement lockStatement) => Visit<object?>(lockStatement, default);
    void Visit(Create.Policy.CreatePolicy createPolicy) => Visit<object?>(createPolicy, default);
    void Visit(Create.Sequence.CreateSequence createSequence) => Visit<object?>(createSequence, default);
}
