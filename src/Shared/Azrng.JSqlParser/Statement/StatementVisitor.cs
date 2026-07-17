namespace Azrng.JSqlParser.Statement;

/// <summary>
/// Visitor interface for traversing SQL statement trees.
/// Generic version (JSqlParser 5.0+): returns T, accepts context S.
/// </summary>
public interface IStatementVisitor<T>
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
    T Visit<S>(Alter.RenameTableStatement rename, S context);
    T Visit<S>(Analyze.Analyze analyze, S context);
    T Visit<S>(Comment.Comment comment, S context);
    T Visit<S>(Execute.Execute execute, S context);
    T Visit<S>(PurgeStatement purge, S context);
    T Visit<S>(Alter.AlterView alterView, S context);
    T Visit<S>(Alter.AlterSession alterSession, S context);
    T Visit<S>(Alter.AlterSystemStatement alterSystem, S context);
    T Visit<S>(Alter.AlterSequence alterSequence, S context);
    T Visit<S>(Create.Synonym.CreateSynonym createSynonym, S context);
    T Visit<S>(Block block, S context);
    T Visit<S>(DeclareStatement declare, S context);
    T Visit<S>(IfElseStatement ifElse, S context);
    T Visit<S>(Create.Function.CreateFunction createFunction, S context);
    T Visit<S>(Create.Procedure.CreateProcedure createProcedure, S context);
    T Visit<S>(Drop.Drop drop, S context);
    T Visit<S>(Truncate.Truncate truncate, S context);
    T Visit<S>(CommitStatement commitStatement, S context);
    T Visit<S>(RollbackStatement rollbackStatement, S context);
    T Visit<S>(SavepointStatement savepointStatement, S context);
    T Visit<S>(UseStatement use, S context);
    T Visit<S>(SetStatement set, S context);
    T Visit<S>(ResetStatement reset, S context);
    T Visit<S>(ShowStatement show, S context);
    T Visit<S>(Show.ShowColumnsStatement showColumns, S context);
    T Visit<S>(Show.ShowIndexStatement showIndex, S context);
    T Visit<S>(Show.ShowTablesStatement showTables, S context);
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

    // JSqlParser 5.4+ - Oracle INSERT ALL/FIRST (上游 commit 4f982e74)
    T Visit<S>(Insert.MultiInsert multiInsert, S context);

    // JSqlParser 5.4+ - CREATE SCHEMA (上游 commit ac46c434)
    T Visit<S>(Create.Schema.CreateSchema createSchema, S context);

    // REFRESH MATERIALIZED VIEW (T091 P1-6)
    T Visit<S>(Refresh.RefreshMaterializedViewStatement refreshMaterializedView, S context);

    // UPSERT / REPLACE (T091 P1-8)
    T Visit<S>(Insert.UpsertStatement upsert, S context);

    // BEGIN TRANSACTION (BL-20 增强，上游不支持)
    T Visit<S>(BeginTransactionStatement beginTransaction, S context);

    // T096 P4 方言补全
    T Visit<S>(Select.TableStatement tableStatement, S context);
    T Visit<S>(Export.ExportStatement export, S context);
    T Visit<S>(Import.ImportStatement import, S context);

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
    void Visit(Alter.RenameTableStatement rename) => Visit<object?>(rename, default);
    void Visit(Analyze.Analyze analyze) => Visit<object?>(analyze, default);
    void Visit(Comment.Comment comment) => Visit<object?>(comment, default);
    void Visit(Execute.Execute execute) => Visit<object?>(execute, default);
    void Visit(PurgeStatement purge) => Visit<object?>(purge, default);
    void Visit(Alter.AlterView alterView) => Visit<object?>(alterView, default);
    void Visit(Alter.AlterSession alterSession) => Visit<object?>(alterSession, default);
    void Visit(Alter.AlterSystemStatement alterSystem) => Visit<object?>(alterSystem, default);
    void Visit(Alter.AlterSequence alterSequence) => Visit<object?>(alterSequence, default);
    void Visit(Create.Synonym.CreateSynonym createSynonym) => Visit<object?>(createSynonym, default);
    void Visit(Block block) => Visit<object?>(block, default);
    void Visit(DeclareStatement declare) => Visit<object?>(declare, default);
    void Visit(IfElseStatement ifElse) => Visit<object?>(ifElse, default);
    void Visit(Create.Function.CreateFunction createFunction) => Visit<object?>(createFunction, default);
    void Visit(Create.Procedure.CreateProcedure createProcedure) => Visit<object?>(createProcedure, default);
    void Visit(Drop.Drop drop) => Visit<object?>(drop, default);
    void Visit(Truncate.Truncate truncate) => Visit<object?>(truncate, default);
    void Visit(CommitStatement commitStatement) => Visit<object?>(commitStatement, default);
    void Visit(RollbackStatement rollbackStatement) => Visit<object?>(rollbackStatement, default);
    void Visit(SavepointStatement savepointStatement) => Visit<object?>(savepointStatement, default);
    void Visit(UseStatement use) => Visit<object?>(use, default);
    void Visit(SetStatement set) => Visit<object?>(set, default);
    void Visit(ResetStatement reset) => Visit<object?>(reset, default);
    void Visit(ShowStatement show) => Visit<object?>(show, default);
    void Visit(Show.ShowColumnsStatement showColumns) => Visit<object?>(showColumns, default);
    void Visit(Show.ShowIndexStatement showIndex) => Visit<object?>(showIndex, default);
    void Visit(Show.ShowTablesStatement showTables) => Visit<object?>(showTables, default);
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
    void Visit(Insert.MultiInsert multiInsert) => Visit<object?>(multiInsert, default);
    void Visit(Create.Schema.CreateSchema createSchema) => Visit<object?>(createSchema, default);
    void Visit(Refresh.RefreshMaterializedViewStatement refreshMaterializedView) => Visit<object?>(refreshMaterializedView, default);
    void Visit(Insert.UpsertStatement upsert) => Visit<object?>(upsert, default);
    void Visit(BeginTransactionStatement beginTransaction) => Visit<object?>(beginTransaction, default);
    void Visit(Select.TableStatement tableStatement) => Visit<object?>(tableStatement, default);
    void Visit(Export.ExportStatement export) => Visit<object?>(export, default);
    void Visit(Import.ImportStatement import) => Visit<object?>(import, default);
}
