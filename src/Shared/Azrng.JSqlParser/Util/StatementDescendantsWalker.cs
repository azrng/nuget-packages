namespace Azrng.JSqlParser.Statement;

/// <summary>
/// 内部语句遍历引擎：把 visitor 的 push 模型桥接为 <see cref="Azrng.JSqlParser.StatementExtension.Descendants{T}"/> 所需的拉取式收集。
/// 继承 <see cref="StatementVisitorAdapter{T}"/>（默认 no-op），覆盖全部语句类型以对每个被访问节点回调；
/// 含子语句的复合类型（Statements/Block/IfElse/Parenthesed*/MultiInsert）额外递归其子语句。
/// </summary>
/// <remarks>
/// 不对外公开。Select body 内的表达式收集请用 <see cref="Azrng.JSqlParser.ExpressionExtension.Descendants{T}"/>；
/// 本 walker 仅遍历语句层（含嵌套子语句），不进入 WHERE 等 Select 内部表达式
/// （这些属于表达式层，由 <c>ExpressionDescendantsWalker</c> 处理）。
/// </remarks>
internal sealed class StatementDescendantsWalker : StatementVisitorAdapter<object?>
{
    private readonly Action<Statement> _onVisit;

    private StatementDescendantsWalker(Action<Statement> onVisit)
    {
        _onVisit = onVisit;
    }

    /// <summary>从根语句出发，按深度优先顺序回调每一个被访问到的语句节点。</summary>
    public static void Walk(Statement root, Action<Statement> onVisit)
    {
        var walker = new StatementDescendantsWalker(onVisit);
        root.Accept(walker);
    }

    // 复合语句：回调 + 递归子语句
    public override object? Visit<S>(Statements stmts, S context)
    {
        _onVisit(stmts);
        if (stmts.StatementList != null)
            foreach (var s in stmts.StatementList) s.Accept(this, context);
        return default;
    }

    public override object? Visit<S>(Block block, S context)
    {
        _onVisit(block);
        if (block.Statements?.StatementList != null)
            foreach (var s in block.Statements.StatementList) s.Accept(this, context);
        return default;
    }

    public override object? Visit<S>(IfElseStatement ifElse, S context)
    {
        _onVisit(ifElse);
        ifElse.IfStatement?.Accept(this, context);
        ifElse.ElseStatement?.Accept(this, context);
        return default;
    }

    public override object? Visit<S>(Select.ParenthesedInsert parenthesedInsert, S context)
    {
        _onVisit(parenthesedInsert);
        parenthesedInsert.Insert?.Accept(this, context);
        return default;
    }

    public override object? Visit<S>(Select.ParenthesedUpdate parenthesedUpdate, S context)
    {
        _onVisit(parenthesedUpdate);
        parenthesedUpdate.Update?.Accept(this, context);
        return default;
    }

    public override object? Visit<S>(Select.ParenthesedDelete parenthesedDelete, S context)
    {
        _onVisit(parenthesedDelete);
        parenthesedDelete.Delete?.Accept(this, context);
        return default;
    }

    public override object? Visit<S>(Insert.MultiInsert multiInsert, S context)
    {
        _onVisit(multiInsert);
        if (multiInsert.Branches != null)
            foreach (var branch in multiInsert.Branches)
                foreach (var clause in branch.Clauses) clause.Select?.Accept(this, context);
        return default;
    }

    // Select：回调当前节点；SetOperationList 额外递归其各分支（PlainSelect 等）
    public override object? Visit<S>(Select.Select select, S context)
    {
        _onVisit(select);
        if (select is Select.SetOperationList setOpList && setOpList.Selects != null)
        {
            foreach (var branch in setOpList.Selects) branch.Accept(this, context);
        }
        return default;
    }
    public override object? Visit<S>(Insert.Insert insert, S context) { _onVisit(insert); return default; }
    public override object? Visit<S>(Update.Update update, S context) { _onVisit(update); return default; }
    public override object? Visit<S>(Delete.Delete delete, S context) { _onVisit(delete); return default; }
    public override object? Visit<S>(Merge.Merge merge, S context) { _onVisit(merge); return default; }
    public override object? Visit<S>(CreateTable.CreateTable createTable, S context) { _onVisit(createTable); return default; }
    public override object? Visit<S>(CreateView.CreateView createView, S context) { _onVisit(createView); return default; }
    public override object? Visit<S>(CreateIndex.CreateIndex createIndex, S context) { _onVisit(createIndex); return default; }
    public override object? Visit<S>(Alter.Alter alter, S context) { _onVisit(alter); return default; }
    public override object? Visit<S>(Alter.RenameTableStatement rename, S context) { _onVisit(rename); return default; }
    public override object? Visit<S>(Analyze.Analyze analyze, S context) { _onVisit(analyze); return default; }
    public override object? Visit<S>(Comment.Comment comment, S context) { _onVisit(comment); return default; }
    public override object? Visit<S>(Execute.Execute execute, S context) { _onVisit(execute); return default; }
    public override object? Visit<S>(PurgeStatement purge, S context) { _onVisit(purge); return default; }
    public override object? Visit<S>(Alter.AlterView alterView, S context) { _onVisit(alterView); return default; }
    public override object? Visit<S>(Alter.AlterSession alterSession, S context) { _onVisit(alterSession); return default; }
    public override object? Visit<S>(Alter.AlterSystemStatement alterSystem, S context) { _onVisit(alterSystem); return default; }
    public override object? Visit<S>(Alter.AlterSequence alterSequence, S context) { _onVisit(alterSequence); return default; }
    public override object? Visit<S>(Create.Synonym.CreateSynonym createSynonym, S context) { _onVisit(createSynonym); return default; }
    public override object? Visit<S>(DeclareStatement declare, S context) { _onVisit(declare); return default; }
    public override object? Visit<S>(Create.Function.CreateFunction createFunction, S context) { _onVisit(createFunction); return default; }
    public override object? Visit<S>(Create.Procedure.CreateProcedure createProcedure, S context) { _onVisit(createProcedure); return default; }
    public override object? Visit<S>(Drop.Drop drop, S context) { _onVisit(drop); return default; }
    public override object? Visit<S>(Truncate.Truncate truncate, S context) { _onVisit(truncate); return default; }
    public override object? Visit<S>(CommitStatement commitStatement, S context) { _onVisit(commitStatement); return default; }
    public override object? Visit<S>(RollbackStatement rollbackStatement, S context) { _onVisit(rollbackStatement); return default; }
    public override object? Visit<S>(SavepointStatement savepointStatement, S context) { _onVisit(savepointStatement); return default; }
    public override object? Visit<S>(UseStatement use, S context) { _onVisit(use); return default; }
    public override object? Visit<S>(SetStatement set, S context) { _onVisit(set); return default; }
    public override object? Visit<S>(ResetStatement reset, S context) { _onVisit(reset); return default; }
    public override object? Visit<S>(ShowStatement show, S context) { _onVisit(show); return default; }
    public override object? Visit<S>(Show.ShowColumnsStatement showColumns, S context) { _onVisit(showColumns); return default; }
    public override object? Visit<S>(Show.ShowIndexStatement showIndex, S context) { _onVisit(showIndex); return default; }
    public override object? Visit<S>(Show.ShowTablesStatement showTables, S context) { _onVisit(showTables); return default; }
    public override object? Visit<S>(DescribeStatement describe, S context) { _onVisit(describe); return default; }
    public override object? Visit<S>(ExplainStatement explain, S context) { _onVisit(explain); return default; }
    public override object? Visit<S>(GrantStatement grant, S context) { _onVisit(grant); return default; }
    public override object? Visit<S>(UnsupportedStatement unsupportedStatement, S context) { _onVisit(unsupportedStatement); return default; }
    public override object? Visit<S>(SessionStatement sessionStatement, S context) { _onVisit(sessionStatement); return default; }
    public override object? Visit<S>(Lock.LockStatement lockStatement, S context) { _onVisit(lockStatement); return default; }
    public override object? Visit<S>(Create.Policy.CreatePolicy createPolicy, S context) { _onVisit(createPolicy); return default; }
    public override object? Visit<S>(Create.Sequence.CreateSequence createSequence, S context) { _onVisit(createSequence); return default; }
    public override object? Visit<S>(Create.Schema.CreateSchema createSchema, S context) { _onVisit(createSchema); return default; }
    public override object? Visit<S>(Refresh.RefreshMaterializedViewStatement refreshMaterializedView, S context) { _onVisit(refreshMaterializedView); return default; }
    public override object? Visit<S>(Insert.UpsertStatement upsert, S context) { _onVisit(upsert); return default; }
    public override object? Visit<S>(BeginTransactionStatement beginTransaction, S context) { _onVisit(beginTransaction); return default; }
    public override object? Visit<S>(Select.TableStatement tableStatement, S context) { _onVisit(tableStatement); return default; }
    public override object? Visit<S>(Export.ExportStatement export, S context) { _onVisit(export); return default; }
    public override object? Visit<S>(Import.ImportStatement import, S context) { _onVisit(import); return default; }
}
