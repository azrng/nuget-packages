namespace Azrng.JSqlParser.Statement;

/// <summary>
/// Default implementation of StatementVisitor with empty visit methods.
/// Override only the methods you need.
/// </summary>
public class StatementVisitorAdapter<T> : StatementVisitor<T>
{
    public virtual T Visit<S>(Statements stmts, S context) => default!;
    public virtual T Visit<S>(Select.Select select, S context) => default!;
    public virtual T Visit<S>(Insert.Insert insert, S context) => default!;
    public virtual T Visit<S>(Update.Update update, S context) => default!;
    public virtual T Visit<S>(Delete.Delete delete, S context) => default!;
    public virtual T Visit<S>(Merge.Merge merge, S context) => default!;
    public virtual T Visit<S>(CreateTable.CreateTable createTable, S context) => default!;
    public virtual T Visit<S>(CreateView.CreateView createView, S context) => default!;
    public virtual T Visit<S>(CreateIndex.CreateIndex createIndex, S context) => default!;
    public virtual T Visit<S>(Alter.Alter alter, S context) => default!;
    public virtual T Visit<S>(Alter.RenameTableStatement rename, S context) => default!;
    public virtual T Visit<S>(Drop.Drop drop, S context) => default!;
    public virtual T Visit<S>(Truncate.Truncate truncate, S context) => default!;
    public virtual T Visit<S>(CommitStatement commitStatement, S context) => default!;
    public virtual T Visit<S>(RollbackStatement rollbackStatement, S context) => default!;
    public virtual T Visit<S>(SavepointStatement savepointStatement, S context) => default!;
    public virtual T Visit<S>(UseStatement use, S context) => default!;
    public virtual T Visit<S>(SetStatement set, S context) => default!;
    public virtual T Visit<S>(ResetStatement reset, S context) => default!;
    public virtual T Visit<S>(ShowStatement show, S context) => default!;
    public virtual T Visit<S>(DescribeStatement describe, S context) => default!;
    public virtual T Visit<S>(ExplainStatement explain, S context) => default!;
    public virtual T Visit<S>(GrantStatement grant, S context) => default!;
    public virtual T Visit<S>(UnsupportedStatement unsupportedStatement, S context) => default!;

    // JSqlParser 5.1 - Parenthesized DML for CTEs
    public virtual T Visit<S>(Select.ParenthesedInsert parenthesedInsert, S context) => default!;
    public virtual T Visit<S>(Select.ParenthesedUpdate parenthesedUpdate, S context) => default!;
    public virtual T Visit<S>(Select.ParenthesedDelete parenthesedDelete, S context) => default!;

    // JSqlParser 5.4
    public virtual T Visit<S>(SessionStatement sessionStatement, S context) => default!;

    // JSqlParser 5.4+ - LOCK TABLE
    public virtual T Visit<S>(Lock.LockStatement lockStatement, S context) => default!;

    // JSqlParser 5.4+ - CREATE POLICY (PostgreSQL RLS)
    public virtual T Visit<S>(Create.Policy.CreatePolicy createPolicy, S context) => default!;

    // JSqlParser 5.4+ - CREATE SEQUENCE
    public virtual T Visit<S>(Create.Sequence.CreateSequence createSequence, S context) => default!;

    // JSqlParser 5.4+ - Oracle INSERT ALL/FIRST (上游 commit 4f982e74)
    public virtual T Visit<S>(Insert.MultiInsert multiInsert, S context) => default!;

    // JSqlParser 5.4+ - CREATE SCHEMA (上游 commit ac46c434)
    public virtual T Visit<S>(Create.Schema.CreateSchema createSchema, S context) => default!;
}
