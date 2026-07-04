using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Delete;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Lock;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Drop;
using Azrng.JSqlParser.Statement.Truncate;
using Azrng.JSqlParser.Statement.CreateView;
using Azrng.JSqlParser.Statement.CreateIndex;
using Azrng.JSqlParser.Statement.Merge;
using Azrng.JSqlParser.Statement.Piped;

namespace Azrng.JSqlParser.Parser.ANTLR4;

/// <summary>
/// Converts ANTLR4 parse tree to native C# AST nodes.
/// </summary>
public class AstBuilderVisitor : JSqlParserGrammarBaseVisitor<object>
{
    // ── Statements ─────────────────────────────

    public override object VisitStatements(JSqlParserGrammar.StatementsContext context)
    {
        var statements = new Statements();
        foreach (var stmtCtx in context.statement())
        {
            var stmt = (Statement.Statement)Visit(stmtCtx);
            statements.StatementList.Add(stmt);
        }
        return statements;
    }

    public override object VisitStatement(JSqlParserGrammar.StatementContext context)
    {
        if (context.selectStatement() != null) return Visit(context.selectStatement());
        if (context.insertStatement() != null) return Visit(context.insertStatement());
        if (context.updateStatement() != null) return Visit(context.updateStatement());
        if (context.deleteStatement() != null) return Visit(context.deleteStatement());
        if (context.createTable() != null) return Visit(context.createTable());
        if (context.createView() != null) return Visit(context.createView());
        if (context.createIndex() != null) return Visit(context.createIndex());
        if (context.alterStatement() != null) return Visit(context.alterStatement());
        if (context.dropStatement() != null) return Visit(context.dropStatement());
        if (context.truncateStatement() != null) return Visit(context.truncateStatement());
        if (context.commitStatement() != null) return Visit(context.commitStatement());
        if (context.rollbackStatement() != null) return Visit(context.rollbackStatement());
        if (context.savepointStatement() != null) return Visit(context.savepointStatement());
        if (context.useStatement() != null) return Visit(context.useStatement());
        if (context.setStatement() != null) return Visit(context.setStatement());
        if (context.mergeStatement() != null) return Visit(context.mergeStatement());
        if (context.describeStatement() != null) return Visit(context.describeStatement());
        if (context.showStatement() != null) return Visit(context.showStatement());
        if (context.explainStatement() != null) return Visit(context.explainStatement());
        if (context.grantStatement() != null) return Visit(context.grantStatement());
        if (context.sessionStatement() != null) return Visit(context.sessionStatement());
        if (context.lockStatement() != null) return Visit(context.lockStatement());

        return new UnsupportedStatement();
    }

    // ── SELECT ─────────────────────────────────

    public override object VisitSelectStatement(JSqlParserGrammar.SelectStatementContext context)
    {
        Select select;

        if (context.fromQuery() != null)
        {
            select = (FromQuery)Visit(context.fromQuery());
        }
        else
        {
            select = (Select)Visit(context.selectBody());
        }

        if (context.withClause() != null)
        {
            select.WithItemsList = (List<WithItem>)Visit(context.withClause());
        }

        if (context.orderByClause() != null)
        {
            select.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        }

        if (context.limitClause() != null)
        {
            select.Limit = (Limit)Visit(context.limitClause());
        }

        if (context.offsetClause() != null)
        {
            select.Offset = (Offset)Visit(context.offsetClause());
        }

        if (context.fetchClause() != null)
        {
            select.Fetch = (Fetch)Visit(context.fetchClause());
        }

        if (context.forUpdateClause() != null)
        {
            VisitForUpdateClause(context.forUpdateClause(), select);
        }

        return select;
    }

    public override object VisitWithClause(JSqlParserGrammar.WithClauseContext context)
    {
        var items = new List<WithItem>();
        foreach (var withItemCtx in context.withItem())
        {
            items.Add((WithItem)Visit(withItemCtx));
        }
        return items;
    }

    public override object VisitWithItem(JSqlParserGrammar.WithItemContext context)
    {
        var withItem = new WithItem();
        withItem.Alias = new Alias(context.identifier().GetText());

        if (context.identifierList() != null)
        {
            withItem.WithItemList = new List<SelectItem>();
            foreach (var id in context.identifierList().identifier())
            {
                withItem.WithItemList.Add(new SelectItem(new Column { ColumnName = id.GetText() }));
            }
        }

        // MATERIALIZED hint
        if (context.MATERIALIZED() != null && context.NOT() == null)
            withItem.Materialized = true;
        else if (context.MATERIALIZED() != null && context.NOT() != null)
            withItem.Materialized = false;

        // CTE body: SELECT, INSERT, UPDATE, or DELETE
        if (context.selectStatement() != null)
        {
            withItem.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.insertStatement() != null)
        {
            var insert = (Statement.Insert.Insert)Visit(context.insertStatement());
            withItem.ParenthesedInsert = new ParenthesedInsert { Insert = insert };
        }
        else if (context.updateStatement() != null)
        {
            var update = (Statement.Update.Update)Visit(context.updateStatement());
            withItem.ParenthesedUpdate = new ParenthesedUpdate { Update = update };
        }
        else if (context.deleteStatement() != null)
        {
            var delete = (Statement.Delete.Delete)Visit(context.deleteStatement());
            withItem.ParenthesedDelete = new ParenthesedDelete { Delete = delete };
        }

        return withItem;
    }

    public override object VisitSelectBody(JSqlParserGrammar.SelectBodyContext context)
    {
        var plainSelects = context.plainSelect();
        if (plainSelects.Length == 1)
        {
            return Visit(plainSelects[0]);
        }

        var setOpList = new SetOperationList();
        setOpList.Selects = new List<Select>();

        setOpList.Selects.Add((Select)Visit(plainSelects[0]));

        for (int i = 1; i < plainSelects.Length; i++)
        {
            setOpList.Selects.Add((Select)Visit(plainSelects[i]));
            var setOp = context.setOperator(i - 1);
            setOpList.Operations.Add(CreateSetOperation(setOp));
        }

        return setOpList;
    }

    public override object VisitPlainSelect(JSqlParserGrammar.PlainSelectContext context)
    {
        var select = new PlainSelect();

        if (context.DISTINCT() != null)
        {
            select.Distinct = new Distinct();
        }
        else if (context.ALL() != null)
        {
            select.All = true;
        }

        select.SelectItems = new List<SelectItem>();
        foreach (var itemCtx in context.selectColumnList().selectItem())
        {
            select.SelectItems.Add((SelectItem)Visit(itemCtx));
        }

        if (context.fromClause() != null)
        {
            var fromItems = context.fromClause().fromItem();
            if (fromItems.Length > 0)
            {
                var firstFrom = fromItems[0];
                select.FromItem = (FromItem)Visit(firstFrom.tableOrSubquery());
                select.Joins = new List<Join>();

                if (firstFrom.joinClause().Length > 0)
                {
                    foreach (var joinCtx in firstFrom.joinClause())
                    {
                        select.Joins.Add((Join)Visit(joinCtx));
                    }
                }

                for (int i = 1; i < fromItems.Length; i++)
                {
                    var join = new Join
                    {
                        Simple = true,
                        RightItem = (FromItem)Visit(fromItems[i].tableOrSubquery())
                    };
                    select.Joins.Add(join);

                    foreach (var joinCtx in fromItems[i].joinClause())
                    {
                        select.Joins.Add((Join)Visit(joinCtx));
                    }
                }

                if (select.Joins.Count == 0)
                {
                    select.Joins = null;
                }
            }
        }

        if (context.whereClause() != null)
        {
            select.Where = (Expression.Expression)Visit(context.whereClause().expression());
        }

        if (context.preferringClause() != null)
        {
            select.Preferring = (PreferringClause)Visit(context.preferringClause());
        }

        if (context.groupByClause() != null)
        {
            select.GroupBy = new GroupByElement();
            var exprs = context.groupByClause().expression();
            select.GroupBy.GroupByExpressions = new List<Expression.Expression>();
            foreach (var expr in exprs)
            {
                select.GroupBy.GroupByExpressions.Add((Expression.Expression)Visit(expr));
            }
        }

        if (context.havingClause() != null)
        {
            select.Having = (Expression.Expression)Visit(context.havingClause().expression());
        }

        return select;
    }

    /// <summary>
    /// 解析 FOR UPDATE / FOR SHARE 子句，填充 Select 基类的 ForMode/ForUpdateTables/Wait/NoWait/SkipLocked
    /// 以及可选的 OrderByElements + ForUpdateBeforeOrderBy。
    /// </summary>
    private void VisitForUpdateClause(JSqlParserGrammar.ForUpdateClauseContext context, Select select)
    {
        select.ForMode = (ForMode)Visit(context.forMode());

        if (context.OF() != null)
        {
            var tables = context.table();
            select.ForUpdateTables = new List<Table>();
            foreach (var tableCtx in tables)
            {
                select.ForUpdateTables.Add((Table)Visit(tableCtx));
            }
        }

        if (context.WAIT() != null)
        {
            select.Wait = new Wait { Timeout = long.Parse(context.LONG_VALUE().GetText()) };
        }

        if (context.NOWAIT() != null)
        {
            select.NoWait = true;
        }
        else if (context.SKIP_KW() != null)
        {
            select.SkipLocked = true;
        }

        // FOR UPDATE 内含 ORDER BY（Oracle 风格：FOR UPDATE ... ORDER BY ...）
        if (context.orderByClause() != null)
        {
            select.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
            select.ForUpdateBeforeOrderBy = true;
        }
    }

    public override object VisitFetchClause(JSqlParserGrammar.FetchClauseContext context)
    {
        var fetch = new Fetch
        {
            FetchFirst = context.FIRST() != null,
            RowOrRows = context.ROWS() != null,
            WithTies = context.TIES() != null
        };

        if (context.expression() != null)
        {
            fetch.FetchExpression = (Expression.Expression)Visit(context.expression());
        }

        if (context.PERCENT() != null)
        {
            fetch.Percent = true;
        }

        return fetch;
    }

    public override object VisitForMode(JSqlParserGrammar.ForModeContext context)
    {
        // NO KEY UPDATE / KEY SHARE / READ ONLY / FETCH ONLY 优先判断（多 token 组合）
        if (context.NO() != null) return ForMode.NO_KEY_UPDATE;
        if (context.KEY() != null) return ForMode.KEY_SHARE;
        if (context.READ() != null) return ForMode.READ_ONLY;
        if (context.FETCH() != null) return ForMode.FETCH_ONLY;
        if (context.UPDATE() != null) return ForMode.UPDATE;
        return ForMode.SHARE;
    }

    public override object VisitSelectItem(JSqlParserGrammar.SelectItemContext context)
    {
        if (context.identifier() != null && context.DOT() != null)
        {
            var allTableCols = new AllTableColumns();
            allTableCols.Table = new Table { Name = context.identifier().GetText() };
            return new SelectItem(allTableCols);
        }

        if (context.MULTIPLY() != null)
        {
            return new SelectItem(new AllColumns());
        }

        var expr = (Expression.Expression)Visit(context.expression());
        var item = new SelectItem(expr);

        if (context.alias() != null)
        {
            item.Alias = new Alias(context.alias().identifier().GetText(),
                context.AS() != null || context.alias().AS() != null);
        }

        return item;
    }

    public override object VisitJoinClause(JSqlParserGrammar.JoinClauseContext context)
    {
        var join = new Join();

        if (context.CROSS() != null)
        {
            join.Cross = true;
        }
        else if (context.NATURAL() != null)
        {
            join.Natural = true;
        }
        else if (context.joinType() != null)
        {
            SetJoinType(join, context.joinType());
        }

        if (context.FETCH() != null)
        {
            join.Fetch = true;
        }

        join.RightItem = (FromItem)Visit(context.tableOrSubquery());

        if (context.joinCondition() != null)
        {
            var cond = context.joinCondition();
            if (cond.ON() != null)
            {
                join.OnExpression = (Expression.Expression)Visit(cond.expression());
            }
            else if (cond.USING() != null)
            {
                join.UsingColumns = cond.identifierList().identifier()
                    .Select(id => new Column { ColumnName = id.GetText() }).ToList();
            }
        }

        return join;
    }

    public override object VisitTableOrSubquery(JSqlParserGrammar.TableOrSubqueryContext context)
    {
        if (context.table() != null)
        {
            var table = (Table)Visit(context.table());
            if (context.alias() != null)
            {
                table.Alias = new Alias(context.alias().identifier().GetText());
            }
            return table;
        }

        if (context.subSelect() != null)
        {
            return Visit(context.subSelect());
        }

        return Visit(context.GetChild(0));
    }

    public override object VisitSubSelect(JSqlParserGrammar.SubSelectContext context)
    {
        var select = (Select)Visit(context.selectStatement());
        var parenSelect = new ParenthesedSelect();
        parenSelect.Select = select;

        if (context.alias() != null)
        {
            parenSelect.Alias = new Alias(context.alias().identifier().GetText());
        }

        return parenSelect;
    }

    public override object VisitOrderByClause(JSqlParserGrammar.OrderByClauseContext context)
    {
        var items = new List<OrderByElement>();
        foreach (var itemCtx in context.orderByItem())
        {
            items.Add((OrderByElement)Visit(itemCtx));
        }
        return items;
    }

    public override object VisitOrderByItem(JSqlParserGrammar.OrderByItemContext context)
    {
        var item = new OrderByElement();
        item.Expression = (Expression.Expression)Visit(context.expression());
        item.Asc = context.DESC() == null;
        item.AscDescPresent = context.ASC() != null || context.DESC() != null;
        return item;
    }

    public override object VisitLimitClause(JSqlParserGrammar.LimitClauseContext context)
    {
        var limit = new Limit();
        var exprs = context.expression();
        if (exprs.Length == 1)
        {
            limit.RowCount = (Expression.Expression)Visit(exprs[0]);
        }
        else if (exprs.Length == 2)
        {
            // LIMIT offset,rowCount (MySQL syntax) — swap: first is offset, second is rowCount
            limit.Offset = (Expression.Expression)Visit(exprs[0]);
            limit.RowCount = (Expression.Expression)Visit(exprs[1]);
        }
        return limit;
    }

    public override object VisitOffsetClause(JSqlParserGrammar.OffsetClauseContext context)
    {
        var offset = new Offset();
        offset.OffsetExpression = (Expression.Expression)Visit(context.expression());
        return offset;
    }

    // ── INSERT ─────────────────────────────────

    public override object VisitInsertStatement(JSqlParserGrammar.InsertStatementContext context)
    {
        var insert = new Insert();
        insert.Table = (Table)Visit(context.table());

        if (context.identifierList() != null)
        {
            insert.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
            {
                insert.Columns.Add(new Column { ColumnName = id.GetText() });
            }
        }

        if (context.selectStatement() != null)
        {
            insert.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.valuesList() != null)
        {
            insert.UseValues = true;
        }

        if (context.onDuplicateKey() != null)
        {
            var dupCtx = context.onDuplicateKey();
            if (dupCtx.NOTHING() != null)
            {
                insert.DuplicateUpdateNothing = true;
            }
            else
            {
                insert.DuplicateUpdateSets = new List<UpdateSet>();
                foreach (var assignment in dupCtx.assignmentItem())
                {
                    var updateSet = new UpdateSet();
                    updateSet.Columns = new List<Column>();
                    foreach (var target in assignment.assignmentTarget())
                    {
                        updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                    }
                    updateSet.Values = new List<Expression.Expression>();
                    updateSet.Values.Add((Expression.Expression)Visit(assignment.expression()));
                    insert.DuplicateUpdateSets.Add(updateSet);
                }
            }
        }

        if (context.returningClause() != null)
        {
            insert.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return insert;
    }

    // ── UPDATE ─────────────────────────────────

    public override object VisitUpdateStatement(JSqlParserGrammar.UpdateStatementContext context)
    {
        var update = new Update();
        update.Table = (Table)Visit(context.table());

        var joinClauses = context.joinClause();
        if (joinClauses.Length > 0)
        {
            update.Joins = new List<Join>();
            foreach (var joinCtx in joinClauses)
            {
                update.Joins.Add((Join)Visit(joinCtx));
            }
        }

        update.UpdateSets = new List<UpdateSet>();
        foreach (var assignment in context.assignmentItem())
        {
            var updateSet = new UpdateSet();
            updateSet.Columns = new List<Column>();
            foreach (var target in assignment.assignmentTarget())
            {
                updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
            }
            updateSet.Values = new List<Expression.Expression>();
            updateSet.Values.Add((Expression.Expression)Visit(assignment.expression()));
            update.UpdateSets.Add(updateSet);
        }

        if (context.whereClause() != null)
        {
            update.Where = (Expression.Expression)Visit(context.whereClause().expression());
        }

        if (context.returningClause() != null)
        {
            update.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return update;
    }

    // ── DELETE ─────────────────────────────────

    public override object VisitDeleteStatement(JSqlParserGrammar.DeleteStatementContext context)
    {
        var delete = new Delete();
        delete.Table = (Table)Visit(context.table());

        if (context.whereClause() != null)
        {
            delete.Where = (Expression.Expression)Visit(context.whereClause().expression());
        }

        if (context.returningClause() != null)
        {
            delete.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return delete;
    }

    // ── CREATE TABLE ───────────────────────────

    public override object VisitCreateTable(JSqlParserGrammar.CreateTableContext context)
    {
        var create = new CreateTable();
        create.Table = (Table)Visit(context.table());

        create.ColumnDefinitions = new List<ColumnDefinition>();
        create.Constraints = new List<Constraint>();

        foreach (var defCtx in context.createTableDefinition())
        {
            if (defCtx.columnDefinition() != null)
            {
                create.ColumnDefinitions.Add((ColumnDefinition)Visit(defCtx.columnDefinition()));
            }
            else if (defCtx.tableConstraint() != null)
            {
                create.Constraints.Add((Constraint)Visit(defCtx.tableConstraint()));
            }
        }

        return create;
    }

    public override object VisitColumnDefinition(JSqlParserGrammar.ColumnDefinitionContext context)
    {
        var colDef = new ColumnDefinition();
        colDef.ColumnName = context.identifier().GetText();
        colDef.DataType = context.dataType().GetText();
        return colDef;
    }

    public override object VisitTableConstraint(JSqlParserGrammar.TableConstraintContext context)
    {
        var constraint = new Constraint();
        if (context.identifier() != null)
        {
            constraint.Name = context.identifier().GetText();
        }
        return constraint;
    }

    // ── ALTER TABLE ────────────────────────────

    public override object VisitAlterStatement(JSqlParserGrammar.AlterStatementContext context)
    {
        var alter = new Alter();
        alter.Table = (Table)Visit(context.table());
        return alter;
    }

    // ── DROP TABLE ─────────────────────────────

    public override object VisitDropStatement(JSqlParserGrammar.DropStatementContext context)
    {
        var drop = new Drop();
        drop.Name = (Table)Visit(context.table(0));
        drop.Type = context.TABLE() != null ? "TABLE" :
                    context.VIEW() != null ? "VIEW" : "INDEX";
        drop.IfExists = context.IF() != null;
        return drop;
    }

    // ── TRUNCATE ───────────────────────────────

    public override object VisitTruncateStatement(JSqlParserGrammar.TruncateStatementContext context)
    {
        var truncate = new Truncate();
        truncate.Table = (Table)Visit(context.table());
        return truncate;
    }

    // ── Transaction control ────────────────────

    public override object VisitCommitStatement(JSqlParserGrammar.CommitStatementContext context)
    {
        return new CommitStatement();
    }

    public override object VisitRollbackStatement(JSqlParserGrammar.RollbackStatementContext context)
    {
        var rollback = new RollbackStatement();
        var identifier = context.identifier();
        if (identifier != null)
        {
            rollback.Savepoint = identifier.GetText();
        }
        return rollback;
    }

    public override object VisitSavepointStatement(JSqlParserGrammar.SavepointStatementContext context)
    {
        return new SavepointStatement { Name = context.identifier().GetText() };
    }

    public override object VisitUseStatement(JSqlParserGrammar.UseStatementContext context)
    {
        return new UseStatement { Name = context.identifier().GetText() };
    }

    public override object VisitSetStatement(JSqlParserGrammar.SetStatementContext context)
    {
        var stmt = new SetStatement();
        if (context.identifier() != null)
            stmt.Name = context.identifier().GetText();
        else if (context.S_AT_IDENTIFIER() != null)
            stmt.Name = context.S_AT_IDENTIFIER().GetText();
        else if (context.SINGLE_AT_IDENTIFIER() != null)
            stmt.Name = context.SINGLE_AT_IDENTIFIER().GetText();
        stmt.Value = (Expression.Expression)Visit(context.expression());
        return stmt;
    }

    // ── MERGE ──────────────────────────────────

    public override object VisitMergeStatement(JSqlParserGrammar.MergeStatementContext context)
    {
        var merge = new Statement.Merge.Merge();
        merge.Table = (Table)Visit(context.table());
        merge.OnCondition = (Expression.Expression)Visit(context.expression());

        foreach (var whenCtx in context.mergeWhenClause())
        {
            if (whenCtx.UPDATE() != null)
            {
                var op = new Statement.Merge.MergeUpdate();
                if (whenCtx.NOT() != null) op.Not = true;
                foreach (var assignment in whenCtx.assignmentItem())
                {
                    var updateSet = new UpdateSet();
                    updateSet.Columns = new List<Column>();
                    foreach (var target in assignment.assignmentTarget())
                    {
                        updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                    }
                    updateSet.Values = new List<Expression.Expression>();
                    updateSet.Values.Add((Expression.Expression)Visit(assignment.expression()));
                    op.UpdateSets.Add(updateSet);
                }
                merge.Operations.Add(op);
            }
            else if (whenCtx.DELETE() != null)
            {
                var op = new Statement.Merge.MergeDelete();
                if (whenCtx.NOT() != null) op.Not = true;
                merge.Operations.Add(op);
            }
            else if (whenCtx.INSERT() != null)
            {
                var op = new Statement.Merge.MergeInsert();
                if (whenCtx.NOT() != null) op.Not = true;
                if (whenCtx.identifierList() != null)
                {
                    op.Columns = new List<Column>();
                    foreach (var id in whenCtx.identifierList().identifier())
                    {
                        op.Columns.Add(new Column { ColumnName = id.GetText() });
                    }
                }
                merge.Operations.Add(op);
            }
        }

        return merge;
    }

    // ── SESSION ────────────────────────────────

    public override object VisitSessionStatement(JSqlParserGrammar.SessionStatementContext context)
    {
        var actionText = context.GetChild(1).GetText();
        var action = Enum.Parse<SessionStatement.Action>(actionText, ignoreCase: true);
        var id = context.identifier()?.GetText();
        var session = new SessionStatement(action, id);

        foreach (var optCtx in context.sessionOption())
        {
            var key = optCtx.identifier(0).GetText();
            var value = optCtx.identifier(1).GetText();
            session.PutOption(key, value);
        }

        return session;
    }

    public override object VisitLockStatement(JSqlParserGrammar.LockStatementContext context)
    {
        var table = (Table)Visit(context.table());
        var lockMode = (LockMode)Visit(context.lockMode());
        var stmt = new LockStatement(table, lockMode);

        if (context.NOWAIT() != null)
        {
            stmt.NoWait = true;
        }
        else if (context.WAIT() != null)
        {
            stmt.WaitSeconds = long.Parse(context.LONG_VALUE().GetText());
        }

        return stmt;
    }

    public override object VisitLockMode(JSqlParserGrammar.LockModeContext context)
    {
        // 按 token 序列判断（与文法 lockMode 分支对应）
        // 文法分支顺序：ROW SHARE | ROW EXCLUSIVE | SHARE ROW EXCLUSIVE | SHARE UPDATE | SHARE | EXCLUSIVE
        var tokens = context.children.OfType<ITerminalNode>().Select(t => t.Symbol.Type).ToList();

        if (tokens.SequenceEqual(new[] { JSqlParserGrammarLexer.ROW, JSqlParserGrammarLexer.SHARE }))
            return LockMode.RowShare;
        if (tokens.SequenceEqual(new[] { JSqlParserGrammarLexer.ROW, JSqlParserGrammarLexer.EXCLUSIVE }))
            return LockMode.RowExclusive;
        if (tokens.SequenceEqual(new[] { JSqlParserGrammarLexer.SHARE, JSqlParserGrammarLexer.ROW, JSqlParserGrammarLexer.EXCLUSIVE }))
            return LockMode.ShareRowExclusive;
        if (tokens.SequenceEqual(new[] { JSqlParserGrammarLexer.SHARE, JSqlParserGrammarLexer.UPDATE }))
            return LockMode.ShareUpdate;
        if (tokens.Count == 1 && tokens[0] == JSqlParserGrammarLexer.SHARE)
            return LockMode.Share;
        return LockMode.Exclusive;
    }

    public override object VisitReturningClause(JSqlParserGrammar.ReturningClauseContext context)
    {
        var keyword = context.RETURNING() != null
            ? ReturningClause.Keyword.RETURNING
            : ReturningClause.Keyword.RETURN;

        List<ReturningOutputAlias>? outputAliases = null;
        if (context.WITH() != null)
        {
            outputAliases = new List<ReturningOutputAlias>();
            foreach (var aliasCtx in context.returningOutputAlias())
            {
                outputAliases.Add((ReturningOutputAlias)Visit(aliasCtx));
            }
        }

        var selectItems = new List<SelectItem>();
        foreach (var itemCtx in context.selectColumnList().selectItem())
        {
            selectItems.Add((SelectItem)Visit(itemCtx));
        }

        var clause = new ReturningClause(keyword, selectItems, outputAliases);

        // PostgreSQL 18 OLD/NEW 引用归一化：将 old.price / new.* 的限定符
        // 从 Table 迁移到 ReturningReferenceType + ReturningQualifier。
        // 若指定了 WITH 别名，按别名映射；否则默认 "old"->OLD, "new"->NEW。
        NormalizeReturningReferences(clause);

        return clause;
    }

    public override object VisitReturningOutputAlias(JSqlParserGrammar.ReturningOutputAliasContext context)
    {
        var ids = context.identifier();
        var refType = ReturningReferenceTypeExtensions.From(ids[0].GetText())
            ?? throw new InvalidOperationException(
                $"Expected OLD or NEW but found: {ids[0].GetText()}");
        return new ReturningOutputAlias(refType, ids[1].GetText());
    }

    /// <summary>
    /// 将 RETURNING 列表中的 old./new. 限定符（或 WITH 别名定义的别名前缀）
    /// 从 Table 迁移到 ReturningReferenceType + ReturningQualifier，并清空 Table。
    /// </summary>
    private static void NormalizeReturningReferences(ReturningClause clause)
    {
        // 构建限定符 -> 引用类型 的映射
        var qualifierMap = new Dictionary<string, ReturningReferenceType>(StringComparer.OrdinalIgnoreCase);
        if (clause.OutputAliases != null && clause.OutputAliases.Count > 0)
        {
            foreach (var alias in clause.OutputAliases)
            {
                if (alias.Alias != null)
                {
                    qualifierMap[alias.Alias] = alias.ReferenceType;
                }
            }
        }
        else
        {
            qualifierMap["old"] = ReturningReferenceType.OLD;
            qualifierMap["new"] = ReturningReferenceType.NEW;
        }

        foreach (var item in clause.SelectItems)
        {
            if (item.Expression is Column col)
            {
                NormalizeColumnReference(col, qualifierMap);
            }
            else if (item.Expression is AllTableColumns allCols)
            {
                NormalizeAllTableColumnsReference(allCols, qualifierMap);
            }
        }
    }

    private static void NormalizeColumnReference(Column col, Dictionary<string, ReturningReferenceType> qualifierMap)
    {
        var table = col.Table;
        // 仅当限定符是简单表名（无 schema/database）时才识别为 OLD/NEW 引用
        if (table == null || table.SchemaName != null || table.Database != null) return;

        var qualifier = table.Name;
        if (qualifier == null || qualifier.Contains('@')) return;

        if (qualifierMap.TryGetValue(qualifier, out var refType))
        {
            col.ReturningReferenceType = refType;
            col.ReturningQualifier = qualifier;
            col.Table = null;
        }
    }

    private static void NormalizeAllTableColumnsReference(AllTableColumns allCols, Dictionary<string, ReturningReferenceType> qualifierMap)
    {
        var table = allCols.Table;
        if (table == null || table.SchemaName != null || table.Database != null) return;

        var qualifier = table.Name;
        if (qualifier == null || qualifier.Contains('@')) return;

        if (qualifierMap.TryGetValue(qualifier, out var refType))
        {
            allCols.ReturningReferenceType = refType;
            allCols.ReturningQualifier = qualifier;
            allCols.Table = null!;
        }
    }

    public override object VisitDescribeStatement(JSqlParserGrammar.DescribeStatementContext context)
    {
        var desc = new DescribeStatement();
        var table = (Table)Visit(context.table());
        desc.Name = table.Name;
        return desc;
    }

    public override object VisitShowStatement(JSqlParserGrammar.ShowStatementContext context)
    {
        var show = new ShowStatement();
        if (context.TABLES() != null)
            show.Name = "TABLES";
        else if (context.identifier()?.Length > 0)
            show.Name = string.Join(" ", context.identifier().Select(id => id.GetText()));
        return show;
    }

    public override object VisitExplainStatement(JSqlParserGrammar.ExplainStatementContext context)
    {
        var explain = new ExplainStatement();
        explain.Statement = (Statement.Statement)Visit(context.statement());
        return explain;
    }

    public override object VisitGrantStatement(JSqlParserGrammar.GrantStatementContext context)
    {
        var grant = new GrantStatement();
        grant.Table = (Table)Visit(context.table());
        grant.Grantee = context.grantee().GetText();
        grant.WithGrantOption = context.OPTION() != null;

        var privilegeList = context.privilegeList();
        if (privilegeList.ALL() != null)
        {
            grant.Privileges.Add(privilegeList.PRIVILEGES() != null ? "ALL PRIVILEGES" : "ALL");
        }
        else
        {
            foreach (var privilege in privilegeList.privilegeName())
            {
                grant.Privileges.Add(privilege.GetText());
            }
        }

        return grant;
    }

    // ── CREATE VIEW / INDEX ────────────────────

    public override object VisitCreateView(JSqlParserGrammar.CreateViewContext context)
    {
        var createView = new CreateView();
        createView.View = (Table)Visit(context.table());
        createView.OrReplace = context.REPLACE() != null;
        createView.IfNotExists = context.EXISTS() != null;
        createView.Select = (Select)Visit(context.selectStatement());
        return createView;
    }

    public override object VisitCreateIndex(JSqlParserGrammar.CreateIndexContext context)
    {
        var createIndex = new CreateIndex();
        createIndex.Index = new Schema.Index { Name = context.identifier().GetText() };
        createIndex.Table = (Table)Visit(context.table());
        createIndex.Unique = context.UNIQUE() != null;
        return createIndex;
    }

    // ── EXPRESSIONS ────────────────────────────

    public override object VisitExpressionEntry(JSqlParserGrammar.ExpressionEntryContext context)
    {
        return Visit(context.expression());
    }

    public override object VisitExpression(JSqlParserGrammar.ExpressionContext context)
    {
        return Visit(context.orExpression());
    }

    public override object VisitOrExpression(JSqlParserGrammar.OrExpressionContext context)
    {
        var andExprs = context.andExpression();
        if (andExprs.Length == 1)
        {
            return Visit(andExprs[0]);
        }

        Expression.Expression result = (Expression.Expression)Visit(andExprs[0]);
        for (int i = 1; i < andExprs.Length; i++)
        {
            var or = new OrExpression();
            or.LeftExpression = result;
            or.RightExpression = (Expression.Expression)Visit(andExprs[i]);
            result = or;
        }
        return result;
    }

    public override object VisitAndExpression(JSqlParserGrammar.AndExpressionContext context)
    {
        var notExprs = context.notExpression();
        if (notExprs.Length == 1)
        {
            return Visit(notExprs[0]);
        }

        Expression.Expression result = (Expression.Expression)Visit(notExprs[0]);
        for (int i = 1; i < notExprs.Length; i++)
        {
            var and = new AndExpression();
            and.LeftExpression = result;
            and.RightExpression = (Expression.Expression)Visit(notExprs[i]);
            result = and;
        }
        return result;
    }

    public override object VisitNotExpression(JSqlParserGrammar.NotExpressionContext context)
    {
        if (context.NOT() != null)
        {
            var not = new NotExpression();
            not.Expression = (Expression.Expression)Visit(context.notExpression());
            return not;
        }
        return Visit(context.predicate());
    }

    public override object VisitPredicate(JSqlParserGrammar.PredicateContext context)
    {
        if (context.EXISTS() != null)
        {
            var exists = new ExistsExpression();
            exists.RightExpression = (Expression.Expression)Visit(context.selectStatement());
            return exists;
        }

        var concat = (Expression.Expression)Visit(context.concatenationExpr());

        if (context.predicateSuffix() == null)
        {
            return concat;
        }

        var suffix = context.predicateSuffix();

        if (suffix.comparisonOperator() != null)
        {
            var op = suffix.comparisonOperator();
            Expression.Expression right = (Expression.Expression)Visit(suffix.concatenationExpr(0));

            if (op.EQUALS() != null) return CreateBinary<EqualsTo>(concat, right);
            if (op.NOT_EQUALS() != null || op.NOT_EQUALS2() != null || op.NOT_EQUALS3() != null)
                return CreateBinary<NotEqualsTo>(concat, right);
            if (op.GREATER_THAN() != null) return CreateBinary<GreaterThan>(concat, right);
            if (op.GREATER_THAN_EQUALS() != null) return CreateBinary<GreaterThanEquals>(concat, right);
            if (op.MINOR_THAN() != null) return CreateBinary<MinorThan>(concat, right);
            if (op.MINOR_THAN_EQUALS() != null) return CreateBinary<MinorThanEquals>(concat, right);

            return CreateBinary<EqualsTo>(concat, right);
        }

        if (suffix.IN() != null)
        {
            var inExpr = new InExpression();
            inExpr.LeftExpression = concat;
            if (suffix.selectStatement() != null)
            {
                inExpr.RightExpression = (Expression.Expression)Visit(suffix.selectStatement());
            }
            else if (suffix.expressionList() != null)
            {
                inExpr.RightExpression = (Expression.Expression)Visit(suffix.expressionList());
            }
            if (suffix.NOT() != null) inExpr.Not = true;
            return inExpr;
        }

        if (suffix.BETWEEN() != null)
        {
            var between = new Between();
            between.LeftExpression = concat;
            between.BetweenExpressionStart = (Expression.Expression)Visit(suffix.concatenationExpr(0));
            between.BetweenExpressionEnd = (Expression.Expression)Visit(suffix.concatenationExpr(1));
            if (suffix.NOT() != null) between.Not = true;
            return between;
        }

        if (suffix.REGEXP() != null || suffix.RLIKE() != null || suffix.REGEXP_LIKE() != null)
        {
            var regexp = new RegExpMatchOperator();
            regexp.LeftExpression = concat;
            regexp.RightExpression = (Expression.Expression)Visit(suffix.concatenationExpr(0));
            regexp.Operator = suffix.REGEXP() != null ? "REGEXP" : suffix.RLIKE() != null ? "RLIKE" : "REGEXP_LIKE";
            if (suffix.NOT() != null) regexp.Not = true;
            return regexp;
        }

        if (suffix.LIKE() != null || suffix.ILIKE() != null
            || suffix.MATCH_ANY() != null || suffix.MATCH_ALL() != null
            || suffix.MATCH_PHRASE() != null || suffix.MATCH_PHRASE_PREFIX() != null || suffix.MATCH_REGEXP() != null)
        {
            var like = new LikeExpression();
            like.LeftExpression = concat;
            like.RightExpression = (Expression.Expression)Visit(suffix.concatenationExpr(0));
            if (suffix.NOT() != null) like.Not = true;
            return like;
        }

        if (suffix.IS() != null)
        {
            if (suffix.DISTINCT() != null)
            {
                var isDistinct = new IsDistinctExpression();
                isDistinct.LeftExpression = concat;
                isDistinct.RightExpression = (Expression.Expression)Visit(suffix.concatenationExpr(0));
                if (suffix.NOT() != null) isDistinct.Not = true;
                return isDistinct;
            }

            if (suffix.TRUE() != null)
            {
                var isBool = new IsBooleanExpression();
                isBool.LeftExpression = concat;
                isBool.IsTrue = true;
                if (suffix.NOT() != null) isBool.Not = true;
                return isBool;
            }

            if (suffix.FALSE() != null)
            {
                var isBool = new IsBooleanExpression();
                isBool.LeftExpression = concat;
                isBool.IsTrue = false;
                if (suffix.NOT() != null) isBool.Not = true;
                return isBool;
            }

            if (suffix.UNKNOWN() != null)
            {
                var isUnknown = new IsUnknownExpression();
                isUnknown.LeftExpression = concat;
                if (suffix.NOT() != null) isUnknown.Not = true;
                return isUnknown;
            }

            var isNull = new IsNullExpression();
            isNull.LeftExpression = concat;
            if (suffix.NOT() != null) isNull.Not = true;
            return isNull;
        }

        if (suffix.ISNULL() != null)
        {
            var isNull = new IsNullExpression();
            isNull.LeftExpression = concat;
            return isNull;
        }

        if (suffix.NOTNULL() != null)
        {
            var isNull = new IsNullExpression();
            isNull.LeftExpression = concat;
            isNull.Not = true;
            return isNull;
        }

        if (suffix.EXCLUDES() != null)
        {
            var excludes = new ExcludesExpression();
            excludes.LeftExpression = concat;
            excludes.RightExpression = (Expression.Expression)Visit(suffix.expressionList());
            return excludes;
        }

        if (suffix.INCLUDES() != null)
        {
            var includes = new IncludesExpression();
            includes.LeftExpression = concat;
            includes.RightExpression = (Expression.Expression)Visit(suffix.expressionList());
            return includes;
        }

        return concat;
    }

    public override object VisitConcatenationExpr(JSqlParserGrammar.ConcatenationExprContext context)
    {
        var additiveExprs = context.additiveExpr();
        if (additiveExprs.Length == 1)
        {
            return Visit(additiveExprs[0]);
        }

        Expression.Expression result = (Expression.Expression)Visit(additiveExprs[0]);
        for (int i = 1; i < additiveExprs.Length; i++)
        {
            var concat = new Concat();
            concat.LeftExpression = result;
            concat.RightExpression = (Expression.Expression)Visit(additiveExprs[i]);
            result = concat;
        }
        return result;
    }

    public override object VisitAdditiveExpr(JSqlParserGrammar.AdditiveExprContext context)
    {
        var multiplicativeExprs = context.multiplicativeExpr();
        if (multiplicativeExprs.Length == 1)
        {
            return Visit(multiplicativeExprs[0]);
        }

        Expression.Expression result = (Expression.Expression)Visit(multiplicativeExprs[0]);
        for (int i = 1; i < multiplicativeExprs.Length; i++)
        {
            var op = context.GetChild(2 * i - 1);
            Expression.Expression right = (Expression.Expression)Visit(multiplicativeExprs[i]);

            if (op is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.PLUS)
                {
                    var add = new Addition();
                    add.LeftExpression = result;
                    add.RightExpression = right;
                    result = add;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.MINUS)
                {
                    var sub = new Subtraction();
                    sub.LeftExpression = result;
                    sub.RightExpression = right;
                    result = sub;
                }
            }
        }
        return result;
    }

    public override object VisitMultiplicativeExpr(JSqlParserGrammar.MultiplicativeExprContext context)
    {
        var unaryExprs = context.unaryExpr();
        if (unaryExprs.Length == 1)
        {
            return Visit(unaryExprs[0]);
        }

        Expression.Expression result = (Expression.Expression)Visit(unaryExprs[0]);
        for (int i = 1; i < unaryExprs.Length; i++)
        {
            var op = context.GetChild(2 * i - 1);
            Expression.Expression right = (Expression.Expression)Visit(unaryExprs[i]);

            if (op is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.MULTIPLY)
                {
                    var mul = new Multiplication();
                    mul.LeftExpression = result;
                    mul.RightExpression = right;
                    result = mul;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.DIVIDE)
                {
                    var div = new Division();
                    div.LeftExpression = result;
                    div.RightExpression = right;
                    result = div;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.MODULO)
                {
                    var mod = new Modulo();
                    mod.LeftExpression = result;
                    mod.RightExpression = right;
                    result = mod;
                }
            }
        }
        return result;
    }

    public override object VisitUnaryExpr(JSqlParserGrammar.UnaryExprContext context)
    {
        if (context.postfixExpr() != null)
        {
            return Visit(context.postfixExpr());
        }

        var expr = (Expression.Expression)Visit(context.unaryExpr());
        if (context.MINUS() != null)
        {
            var signed = new SignedExpression();
            signed.Sign = '-';
            signed.Expression = expr;
            return signed;
        }

        return expr;
    }

    public override object VisitPostfixExpr(JSqlParserGrammar.PostfixExprContext context)
    {
        var expr = (Expression.Expression)Visit(context.primaryExpr());

        // 按出现顺序处理后缀操作符：::dataType（cast）、.identifier（字段访问）。
        // ANTLR 的 postfixExpr 文法将这些操作符以 * 循环混合，需顺序遍历子节点。
        var dataTypes = context.dataType();
        int dataTypeIdx = 0;
        var identifiers = context.identifier();
        int identifierIdx = 0;

        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.DOUBLE_COLON && dataTypeIdx < dataTypes.Length)
                {
                    var cast = new CastExpression();
                    cast.Expression = expr;
                    cast.DataType = dataTypes[dataTypeIdx].GetText();
                    cast.UseCastKeyword = false;
                    dataTypeIdx++;
                    expr = cast;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.DOT && identifierIdx < identifiers.Length)
                {
                    // PostgreSQL 复合类型字段访问：(expr).field 或多层 (expr).field1.field2
                    expr = new RowGetExpression(expr, identifiers[identifierIdx].GetText());
                    identifierIdx++;
                }
            }
        }

        return expr;
    }

    public override object VisitPrimaryExpr(JSqlParserGrammar.PrimaryExprContext context)
    {
        if (context.literal() != null) return Visit(context.literal());
        if (context.parameter() != null) return Visit(context.parameter());
        if (context.caseExpr() != null) return Visit(context.caseExpr());
        if (context.castExpr() != null) return Visit(context.castExpr());
        if (context.extractExpr() != null) return Visit(context.extractExpr());
        if (context.intervalExpr() != null) return Visit(context.intervalExpr());
        if (context.functionExpr() != null) return Visit(context.functionExpr());
        if (context.subSelect() != null) return Visit(context.subSelect());
        if (context.structType() != null) return Visit(context.structType());
        if (context.lambdaExpression() != null) return Visit(context.lambdaExpression());
        if (context.keyExpression() != null) return Visit(context.keyExpression());
        if (context.fullTextSearch() != null) return Visit(context.fullTextSearch());
        if (context.columnRef() != null) return Visit(context.columnRef());
        if (context.MULTIPLY() != null) return new AllColumns();

        if (context.OPENING_PAREN() != null && context.expression() != null)
        {
            var paren = new Parenthesis();
            paren.Expression = (Expression.Expression)Visit(context.expression());
            return paren;
        }

        return new NullValue();
    }

    public override object VisitKeyExpression(JSqlParserGrammar.KeyExpressionContext context)
    {
        var inner = (Expression.Expression)Visit(context.columnRef());
        return new KeyExpression(inner);
    }

    public override object VisitFullTextSearch(JSqlParserGrammar.FullTextSearchContext context)
    {
        var fts = new FullTextSearch();
        foreach (var colCtx in context.columnRef())
        {
            fts.Columns.Add(((Column)Visit(colCtx)).GetFullyQualifiedName());
        }
        fts.MatchExpression = (Expression.Expression)Visit(context.expression());

        if (context.searchModifier() != null)
        {
            // 按 token 顺序用空格拼接修饰符文本（如 "IN BOOLEAN MODE"）
            var modifierCtx = context.searchModifier();
            fts.SearchModifier = string.Join(' ', modifierCtx.children
                .OfType<ITerminalNode>().Select(t => t.GetText()));
        }

        return fts;
    }

    public override object VisitLiteral(JSqlParserGrammar.LiteralContext context)
    {
        if (context.LONG_VALUE() != null)
            return new LongValue(long.Parse(context.LONG_VALUE().GetText()));
        if (context.S_DOUBLE() != null)
            return new DoubleValue(double.Parse(context.S_DOUBLE().GetText()));
        if (context.S_CHAR_LITERAL() != null)
        {
            var text = context.S_CHAR_LITERAL().GetText();
            return new StringValue(text[1..^1].Replace("''", "'"));
        }
        if (context.S_HEX() != null)
            return new HexValue { Value = context.S_HEX().GetText() };
        if (context.NULL() != null)
            return new NullValue();
        if (context.TRUE() != null)
            return new BooleanValue(true);
        if (context.FALSE() != null)
            return new BooleanValue(false);

        return new NullValue();
    }

    public override object VisitParameter(JSqlParserGrammar.ParameterContext context)
    {
        if (context.S_JDBC_NAMED_PARAM() != null)
        {
            return new JdbcNamedParameter { Name = context.S_JDBC_NAMED_PARAM().GetText()[1..] };
        }

        var param = new JdbcParameter();
        if (context.S_PARAMETER() != null)
        {
            param.Index = int.Parse(context.S_PARAMETER().GetText()[1..]);
        }
        return param;
    }

    public override object VisitCaseExpr(JSqlParserGrammar.CaseExprContext context)
    {
        var caseExpr = new CaseExpression();

        if (context.expression().Length > 0 && context.whenExpr().Length > 0)
        {
            caseExpr.SwitchExpression = (Expression.Expression)Visit(context.expression(0));
        }

        caseExpr.WhenClauses = new List<WhenClause>();
        int exprOffset = caseExpr.SwitchExpression != null ? 1 : 0;

        foreach (var whenCtx in context.whenExpr())
        {
            var whenClause = new WhenClause();
            whenClause.WhenExpression = (Expression.Expression)Visit(whenCtx.expression(0));
            whenClause.ThenExpression = (Expression.Expression)Visit(whenCtx.expression(1));
            caseExpr.WhenClauses.Add(whenClause);
        }

        if (context.ELSE() != null)
        {
            var elseExprs = context.expression();
            caseExpr.ElseExpression = (Expression.Expression)Visit(elseExprs[^1]);
        }

        return caseExpr;
    }

    public override object VisitCastExpr(JSqlParserGrammar.CastExprContext context)
    {
        var cast = new CastExpression();
        cast.Expression = (Expression.Expression)Visit(context.expression());
        cast.DataType = context.dataType().GetText();
        return cast;
    }

    public override object VisitExtractExpr(JSqlParserGrammar.ExtractExprContext context)
    {
        var extract = new ExtractExpression();
        extract.Name = context.extractField().GetText();
        extract.Expression = (Expression.Expression)Visit(context.expression());
        return extract;
    }

    public override object VisitIntervalExpr(JSqlParserGrammar.IntervalExprContext context)
    {
        var interval = new IntervalExpression();
        interval.IntervalKeyword = true;
        interval.Expression = (Expression.Expression)Visit(context.expression());

        if (context.YEAR() != null) interval.IntervalType = "YEAR";
        else if (context.MONTH() != null) interval.IntervalType = "MONTH";
        else if (context.DAY() != null) interval.IntervalType = "DAY";
        else if (context.HOUR() != null) interval.IntervalType = "HOUR";
        else if (context.MINUTE() != null) interval.IntervalType = "MINUTE";
        else if (context.SECOND() != null) interval.IntervalType = "SECOND";

        return interval;
    }

    public override object VisitFunctionExpr(JSqlParserGrammar.FunctionExprContext context)
    {
        var funcName = context.identifier()?.GetText() ?? context.NEXTVAL()?.GetText() ?? "";

        ExpressionList? parameters = null;
        if (context.expressionList() != null)
        {
            parameters = new ExpressionList();
            parameters.Expressions = new List<Expression.Expression>();
            foreach (var expr in context.expressionList().expression())
            {
                parameters.Expressions.Add((Expression.Expression)Visit(expr));
            }
        }

        if (context.overClause() != null)
        {
            var analytic = new AnalyticExpression();
            analytic.Name = funcName;
            ApplyFunctionClauses(context, analytic);

            if (context.MULTIPLY() != null ||
                (parameters != null && parameters.Expressions.Count == 1 && parameters.Expressions[0] is AllColumns))
            {
                analytic.AllColumns = true;
            }
            else if (context.DISTINCT() != null && parameters != null)
            {
                analytic.Distinct = true;
                analytic.Expression = parameters.Expressions.Count > 0 ? parameters.Expressions[0] : null;
            }
            else if (parameters != null && parameters.Expressions.Count > 0)
            {
                analytic.Expression = parameters.Expressions[0];
                if (parameters.Expressions.Count > 1) analytic.Offset = parameters.Expressions[1];
                if (parameters.Expressions.Count > 2) analytic.DefaultValue = parameters.Expressions[2];
            }

            var overCtx = context.overClause();
            if (overCtx.identifier() != null)
            {
                analytic.WindowName = overCtx.identifier().GetText();
            }
            else if (overCtx.windowSpecification() != null)
            {
                var winSpec = overCtx.windowSpecification();
                if (winSpec.PARTITION() != null)
                {
                    analytic.PartitionExpressionList = new List<Expression.Expression>();
                    foreach (var partExpr in winSpec.expression())
                    {
                        analytic.PartitionExpressionList.Add((Expression.Expression)Visit(partExpr));
                    }
                }
                if (winSpec.orderByClause() != null)
                {
                    analytic.OrderByElements = (List<OrderByElement>)Visit(winSpec.orderByClause());
                }
            }

            return analytic;
        }

        var func = new Function();
        func.Name = funcName;
        func.Parameters = parameters;
        func.AllColumns = context.MULTIPLY() != null ||
            (parameters != null && parameters.Expressions.Count == 1 && parameters.Expressions[0] is AllColumns);
        ApplyFunctionClauses(context, func);
        return func;
    }

    private void ApplyFunctionClauses(JSqlParserGrammar.FunctionExprContext context, Function function)
    {
        if (context.withinGroupClause() != null)
        {
            function.WithinGroupOrderByElements =
                (List<OrderByElement>)Visit(context.withinGroupClause().orderByClause());
        }

        if (context.filterClause() != null)
        {
            function.FilterExpression =
                (Expression.Expression)Visit(context.filterClause().whereClause().expression());
        }
    }

    private void ApplyFunctionClauses(JSqlParserGrammar.FunctionExprContext context, AnalyticExpression analytic)
    {
        if (context.withinGroupClause() != null)
        {
            analytic.WithinGroupOrderByElements =
                (List<OrderByElement>)Visit(context.withinGroupClause().orderByClause());
        }

        if (context.filterClause() != null)
        {
            analytic.FilterExpression =
                (Expression.Expression)Visit(context.filterClause().whereClause().expression());
        }
    }

    public override object VisitExpressionList(JSqlParserGrammar.ExpressionListContext context)
    {
        var list = new ExpressionList();
        list.Expressions = new List<Expression.Expression>();
        foreach (var expr in context.expression())
        {
            list.Expressions.Add((Expression.Expression)Visit(expr));
        }
        return list;
    }

    public override object VisitColumnRef(JSqlParserGrammar.ColumnRefContext context)
    {
        var identifiers = context.identifier();
        if (identifiers.Length == 1)
        {
            return new Column { ColumnName = identifiers[0].GetText() };
        }

        if (identifiers.Length == 2)
        {
            var table = new Table { Name = identifiers[0].GetText() };
            return new Column { Table = table, ColumnName = identifiers[1].GetText() };
        }

        return new Column { ColumnName = context.GetText() };
    }

    public override object VisitTable(JSqlParserGrammar.TableContext context)
    {
        var identifiers = context.identifier();
        if (identifiers.Length == 1)
        {
            return new Table { Name = identifiers[0].GetText() };
        }

        if (identifiers.Length == 2)
        {
            return new Table
            {
                SchemaName = identifiers[0].GetText(),
                Name = identifiers[1].GetText()
            };
        }

        return new Table { Name = identifiers[^1].GetText() };
    }

    public override object VisitLambdaExpression(JSqlParserGrammar.LambdaExpressionContext context)
    {
        var lambda = new LambdaExpression();

        if (context.identifierList() != null)
        {
            foreach (var id in context.identifierList().identifier())
                lambda.Identifiers.Add(id.GetText());
        }
        else if (context.identifier() != null)
        {
            lambda.Identifiers.Add(context.identifier().GetText());
        }

        lambda.Expression = (Expression.Expression)Visit(context.expression());
        return lambda;
    }

    public override object VisitStructType(JSqlParserGrammar.StructTypeContext context)
    {
        var structType = new StructType();

        // DuckDB syntax: { a::expr, b::expr } [::STRUCT(...)]
        if (context.LBRACE() != null)
        {
            structType.StructDialect = StructType.Dialect.DuckDB;
            structType.Arguments = new List<SelectItem>();
            foreach (var argCtx in context.structArgument())
            {
                var id = argCtx.identifier()?.GetText() ?? argCtx.S_CHAR_LITERAL()?.GetText().Trim('\'');
                var expr = (Expression.Expression)Visit(argCtx.expression());
                var item = new SelectItem { Expression = expr };
                if (id != null) item.Alias = new Alias { Name = id };
                structType.Arguments.Add(item);
            }

            var structParams = context.structParameters();
            if (structParams != null)
            {
                structType.Parameters = new List<KeyValuePair<string, string>>();
                foreach (var paramCtx in structParams.structParameter())
                {
                    var paramName = paramCtx.identifier()?.GetText() ?? "";
                    var paramType = paramCtx.dataType().GetText();
                    structType.Parameters.Add(new KeyValuePair<string, string>(paramName, paramType));
                }
            }

            return structType;
        }

        // BigQuery syntax: STRUCT<params>(args) or STRUCT(args)
        structType.StructDialect = StructType.Dialect.BigQuery;
        structType.Keyword = context.STRUCT().GetText();

        var parameters = context.structParameters();
        if (parameters != null)
        {
            structType.Parameters = new List<KeyValuePair<string, string>>();
            foreach (var paramCtx in parameters.structParameter())
            {
                var paramName = paramCtx.identifier()?.GetText() ?? "";
                var paramType = paramCtx.dataType().GetText();
                structType.Parameters.Add(new KeyValuePair<string, string>(paramName, paramType));
            }
        }

        if (context.selectColumnList() != null)
        {
            structType.Arguments = new List<SelectItem>();
            foreach (var itemCtx in context.selectColumnList().selectItem())
            {
                var item = (SelectItem)Visit(itemCtx);
                structType.Arguments.Add(item);
            }
        }

        return structType;
    }

    public override object VisitPreferringClause(JSqlParserGrammar.PreferringClauseContext context)
    {
        var preferring = new PreferringClause();
        preferring.Preferring = (Expression.Expression)Visit(context.preferenceTerm());

        if (context.expressionList() != null)
        {
            preferring.PartitionBy = (ExpressionList)Visit(context.expressionList());
        }

        return preferring;
    }

    public override object VisitPreferenceTerm(JSqlParserGrammar.PreferenceTermContext context)
    {
        if (context.HIGH() != null)
        {
            return new HighExpression((Expression.Expression)Visit(context.expression()));
        }
        if (context.LOW() != null)
        {
            return new LowExpression((Expression.Expression)Visit(context.expression()));
        }
        if (context.INVERSE_KW() != null)
        {
            return new Inverse((Expression.Expression)Visit(context.expression()));
        }
        if (context.PLUS_KW() != null)
        {
            var inner = (Expression.Expression)Visit(context.preferenceTerm());
            return inner; // Plus is a binary operator, but here it's unary prefix
        }
        if (context.PRIOR() != null)
        {
            var inner = (Expression.Expression)Visit(context.preferenceTerm());
            return inner; // PriorTo is binary, but here it's unary prefix
        }
        if (context.expression() != null)
        {
            return (Expression.Expression)Visit(context.expression());
        }

        return new NullValue();
    }

    public override object VisitConnectByPriorOperator(JSqlParserGrammar.ConnectByPriorOperatorContext context)
    {
        return new ConnectByPriorOperator((Expression.Expression)Visit(context.expression()));
    }

    // ── Helpers ────────────────────────────────

    // ── Pipe SQL (fromQuery) ────────────────────

    public override object VisitFromQuery(JSqlParserGrammar.FromQueryContext context)
    {
        var fromQuery = new FromQuery();
        fromQuery.UsingFromKeyword = context.FROM() != null;
        fromQuery.FromItem = (FromItem)Visit(context.fromItem());

        if (context.joinClause() != null)
        {
            fromQuery.Joins = new List<Join>();
            foreach (var joinCtx in context.joinClause())
            {
                fromQuery.Joins.Add((Join)Visit(joinCtx));
            }
        }

        foreach (var pipeOpCtx in context.pipeOperator())
        {
            fromQuery.PipeOperators.Add((PipeOperator)Visit(pipeOpCtx));
        }

        return fromQuery;
    }

    public override object VisitSelectPipeOp(JSqlParserGrammar.SelectPipeOpContext context)
    {
        var op = new SelectPipeOperator();
        op.SelectItems = new List<SelectItem>();
        foreach (var itemCtx in context.selectColumnList().selectItem())
        {
            op.SelectItems.Add((SelectItem)Visit(itemCtx));
        }
        return op;
    }

    public override object VisitWherePipeOp(JSqlParserGrammar.WherePipeOpContext context)
    {
        var op = new WherePipeOperator();
        op.Expression = (Expression.Expression)Visit(context.expression());
        return op;
    }

    public override object VisitAggregatePipeOp(JSqlParserGrammar.AggregatePipeOpContext context)
    {
        var op = new AggregatePipeOperator();
        op.SelectItems = new List<SelectItem>();
        foreach (var itemCtx in context.selectColumnList().selectItem())
        {
            op.SelectItems.Add((SelectItem)Visit(itemCtx));
        }

        var groupByExprs = context.GROUP();
        if (groupByExprs != null)
        {
            op.GroupBy = new List<Expression.Expression>();
            // GROUP BY expressions are after the GROUP BY keywords
            var expressions = context.expression();
            // The first expressions in the list are GROUP BY ones (before HAVING)
            int groupByCount = 0;
            for (int i = 0; i < expressions.Length; i++)
            {
                if (context.HAVING() != null && i == expressions.Length - 1)
                    break;
                op.GroupBy.Add((Expression.Expression)Visit(expressions[i]));
                groupByCount++;
            }

            if (context.HAVING() != null)
            {
                op.Having = (Expression.Expression)Visit(expressions[expressions.Length - 1]);
            }
        }
        else if (context.HAVING() != null)
        {
            var expressions = context.expression();
            op.Having = (Expression.Expression)Visit(expressions[expressions.Length - 1]);
        }

        return op;
    }

    public override object VisitOrderByPipeOp(JSqlParserGrammar.OrderByPipeOpContext context)
    {
        var op = new OrderByPipeOperator();
        op.OrderByElements = new List<OrderByElement>();
        foreach (var itemCtx in context.orderByItem())
        {
            op.OrderByElements.Add((OrderByElement)Visit(itemCtx));
        }
        return op;
    }

    public override object VisitLimitPipeOp(JSqlParserGrammar.LimitPipeOpContext context)
    {
        var op = new LimitPipeOperator();
        var expressions = context.expression();
        op.Expression = (Expression.Expression)Visit(expressions[0]);
        if (expressions.Length > 1)
            op.Offset = (Expression.Expression)Visit(expressions[1]);
        return op;
    }

    public override object VisitJoinPipeOp(JSqlParserGrammar.JoinPipeOpContext context)
    {
        var join = new Join();
        if (context.joinType() != null)
            SetJoinType(join, context.joinType());
        join.RightItem = (FromItem)Visit(context.tableOrSubquery());
        if (context.joinCondition() != null)
        {
            var condCtx = context.joinCondition();
            if (condCtx.ON() != null)
                join.OnExpression = (Expression.Expression)Visit(condCtx.expression());
            else if (condCtx.USING() != null)
                join.UsingColumns = condCtx.identifierList().identifier()
                    .Select(id => new Column { ColumnName = id.GetText() }).ToList();
        }
        return new JoinPipeOperator { Join = join };
    }

    public override object VisitAsPipeOp(JSqlParserGrammar.AsPipeOpContext context)
    {
        return new AsPipeOperator { Alias = (Alias)Visit(context.alias()) };
    }

    public override object VisitCallPipeOp(JSqlParserGrammar.CallPipeOpContext context)
    {
        var op = new CallPipeOperator();
        op.FunctionName = context.identifier().GetText();
        if (context.expressionList() != null)
            op.Parameters = (ExpressionList)Visit(context.expressionList());
        return op;
    }

    public override object VisitDropPipeOp(JSqlParserGrammar.DropPipeOpContext context)
    {
        var op = new DropPipeOperator();
        op.ColumnNames = context.identifier().Select(id => id.GetText()).ToList();
        return op;
    }

    public override object VisitExtendPipeOp(JSqlParserGrammar.ExtendPipeOpContext context)
    {
        var op = new ExtendPipeOperator();
        op.Expression = (Expression.Expression)Visit(context.expression());
        if (context.alias() != null)
            op.Alias = (Alias)Visit(context.alias());
        return op;
    }

    public override object VisitRenamePipeOp(JSqlParserGrammar.RenamePipeOpContext context)
    {
        var op = new RenamePipeOperator();
        op.Renames = new Dictionary<string, string>();
        var identifiers = context.identifier();
        for (int i = 0; i < identifiers.Length; i += 2)
        {
            op.Renames[identifiers[i].GetText()] = identifiers[i + 1].GetText();
        }
        return op;
    }

    public override object VisitSetPipeOp(JSqlParserGrammar.SetPipeOpContext context)
    {
        var op = new SetPipeOperator();
        op.SetItems = new List<SelectItem>();
        foreach (var itemCtx in context.assignmentItem())
        {
            var target = itemCtx.assignmentTarget(0).GetText();
            var expr = (Expression.Expression)Visit(itemCtx.expression());
            var selectExpr = new EqualsTo
            {
                LeftExpression = new Column { ColumnName = target },
                RightExpression = expr
            };
            op.SetItems.Add(new SelectItem(selectExpr));
        }
        return op;
    }

    public override object VisitPivotPipeOp(JSqlParserGrammar.PivotPipeOpContext context)
    {
        var op = new PivotPipeOperator();
        if (context.identifier() != null)
            op.FunctionName = context.identifier().GetText();

        op.SelectItems = new List<SelectItem>();
        if (context.selectColumnList() != null)
        {
            foreach (var itemCtx in context.selectColumnList().selectItem())
                op.SelectItems.Add((SelectItem)Visit(itemCtx));
        }

        op.InExpressions = new List<Expression.Expression>();
        if (context.expressionList() != null)
        {
            foreach (var exprCtx in context.expressionList().expression())
                op.InExpressions.Add((Expression.Expression)Visit(exprCtx));
        }
        return op;
    }

    public override object VisitUnpivotPipeOp(JSqlParserGrammar.UnpivotPipeOpContext context)
    {
        var op = new UnPivotPipeOperator();
        if (context.identifier() != null)
            op.FunctionName = context.identifier().GetText();

        op.SelectItems = new List<SelectItem>();
        if (context.selectColumnList() != null)
        {
            foreach (var itemCtx in context.selectColumnList().selectItem())
                op.SelectItems.Add((SelectItem)Visit(itemCtx));
        }

        op.InExpressions = new List<Expression.Expression>();
        if (context.expressionList() != null)
        {
            foreach (var exprCtx in context.expressionList().expression())
                op.InExpressions.Add((Expression.Expression)Visit(exprCtx));
        }
        return op;
    }

    public override object VisitTableSamplePipeOp(JSqlParserGrammar.TableSamplePipeOpContext context)
    {
        return new TableSamplePipeOperator
        {
            SampleSize = (Expression.Expression)Visit(context.expression())
        };
    }

    public override object VisitWindowPipeOp(JSqlParserGrammar.WindowPipeOpContext context)
    {
        var op = new WindowPipeOperator();
        op.WindowName = context.identifier().GetText();
        // Window expression is the full specification
        op.WindowExpression = new Column { ColumnName = context.windowSpecification().GetText() };
        return op;
    }

    public override object VisitSetOperationPipeOp(JSqlParserGrammar.SetOperationPipeOpContext context)
    {
        var op = new SetOperationPipeOperator();
        var setOp = CreateSetOperation(context.setOperator());
        op.OperationType = setOp.Type;
        op.All = setOp.All;
        return op;
    }

    private static T CreateBinary<T>(Expression.Expression left, Expression.Expression right) where T : BinaryExpression, new()
    {
        var expr = new T();
        expr.LeftExpression = left;
        expr.RightExpression = right;
        return expr;
    }

    private static SetOperation CreateSetOperation(JSqlParserGrammar.SetOperatorContext context)
    {
        bool all = context.ALL() != null;
        bool distinct = context.DISTINCT() != null;

        if (context.UNION() != null)
            return new SetOperation(SetOperation.OperationType.UNION, all, distinct);
        if (context.INTERSECT() != null)
            return new SetOperation(SetOperation.OperationType.INTERSECT, all, distinct);
        if (context.EXCEPT() != null)
            return new SetOperation(SetOperation.OperationType.EXCEPT, all, distinct);
        return new SetOperation(SetOperation.OperationType.MINUS, all, distinct);
    }

    private static void SetJoinType(Join join, JSqlParserGrammar.JoinTypeContext context)
    {
        if (context.INNER() != null) { join.Inner = true; return; }
        if (context.LEFT() != null) { join.Left = true; if (context.OUTER() != null) join.Outer = true; return; }
        if (context.RIGHT() != null) { join.Right = true; if (context.OUTER() != null) join.Outer = true; return; }
        if (context.FULL() != null) { join.Full = true; if (context.OUTER() != null) join.Outer = true; return; }
        if (context.SEMI() != null) { join.Semi = true; return; }
        join.Inner = true;
    }
}
