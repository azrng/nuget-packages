using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Show;
using Azrng.JSqlParser.Statement.Delete;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Lock;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Create.Policy;
using Azrng.JSqlParser.Statement.Create.Schema;
using Azrng.JSqlParser.Statement.Create.Sequence;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Analyze;
using Azrng.JSqlParser.Statement.Comment;
using Azrng.JSqlParser.Statement.Execute;
using Azrng.JSqlParser.Statement.Create.Synonym;
using Azrng.JSqlParser.Statement.Create.Function;
using Azrng.JSqlParser.Statement.Create.Procedure;
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
        if (context.multiInsertStatement() != null) return Visit(context.multiInsertStatement());
        if (context.updateStatement() != null) return Visit(context.updateStatement());
        if (context.deleteStatement() != null) return Visit(context.deleteStatement());
        if (context.createTable() != null) return Visit(context.createTable());
        if (context.createView() != null) return Visit(context.createView());
        if (context.createIndex() != null) return Visit(context.createIndex());
        if (context.alterStatement() != null) return Visit(context.alterStatement());
        if (context.renameTableStatement() != null) return Visit(context.renameTableStatement());
        if (context.analyzeStatement() != null) return Visit(context.analyzeStatement());
        if (context.commentStatement() != null) return Visit(context.commentStatement());
        if (context.executeStatement() != null) return Visit(context.executeStatement());
        if (context.purgeStatement() != null) return Visit(context.purgeStatement());
        if (context.alterViewStatement() != null) return Visit(context.alterViewStatement());
        if (context.alterSessionStatement() != null) return Visit(context.alterSessionStatement());
        if (context.alterSystemStatement() != null) return Visit(context.alterSystemStatement());
        if (context.alterSequenceStatement() != null) return Visit(context.alterSequenceStatement());
        if (context.createSynonymStatement() != null) return Visit(context.createSynonymStatement());
        if (context.blockStatement() != null) return Visit(context.blockStatement());
        if (context.declareStatement() != null) return Visit(context.declareStatement());
        if (context.ifElseStatement() != null) return Visit(context.ifElseStatement());
        if (context.createFunctionStatement() != null) return Visit(context.createFunctionStatement());
        if (context.dropStatement() != null) return Visit(context.dropStatement());
        if (context.truncateStatement() != null) return Visit(context.truncateStatement());
        if (context.commitStatement() != null) return Visit(context.commitStatement());
        if (context.rollbackStatement() != null) return Visit(context.rollbackStatement());
        if (context.savepointStatement() != null) return Visit(context.savepointStatement());
        if (context.useStatement() != null) return Visit(context.useStatement());
        if (context.setStatement() != null) return Visit(context.setStatement());
        if (context.resetStatement() != null) return Visit(context.resetStatement());
        if (context.mergeStatement() != null) return Visit(context.mergeStatement());
        if (context.describeStatement() != null) return Visit(context.describeStatement());
        if (context.showStatement() != null) return Visit(context.showStatement());
        if (context.explainStatement() != null) return Visit(context.explainStatement());
        if (context.grantStatement() != null) return Visit(context.grantStatement());
        if (context.sessionStatement() != null) return Visit(context.sessionStatement());
        if (context.lockStatement() != null) return Visit(context.lockStatement());
        if (context.createPolicy() != null) return Visit(context.createPolicy());
        if (context.createSequence() != null) return Visit(context.createSequence());
        if (context.createSchema() != null) return Visit(context.createSchema());
        if (context.refreshStatement() != null) return Visit(context.refreshStatement());
        if (context.upsertStatement() != null) return Visit(context.upsertStatement());
        if (context.beginTransactionStatement() != null) return Visit(context.beginTransactionStatement());

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

        // SQL Server FOR XML PATH（selectStatement 层，ORDER BY 之后）
        if (context.XML() != null && select is PlainSelect plainSelect)
        {
            plainSelect.ForXmlPath = context.S_CHAR_LITERAL() != null
                ? context.S_CHAR_LITERAL().GetText() : "";
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

        // Oracle Hint：SELECT 关键字后紧跟的 /*+ ... */ 或 --+ ... 注释
        if (context.ORACLE_HINT() != null || context.ORACLE_HINT_ML() != null)
        {
            var hintComment = context.ORACLE_HINT()?.GetText() ?? context.ORACLE_HINT_ML()?.GetText() ?? "";
            select.OracleHint = new OracleHint(hintComment);
        }

        // SELECT TOP n [PERCENT] [WITH TIES]
        if (context.topClause() != null)
        {
            var topCtx = context.topClause();
            var top = new Top();
            if (topCtx.OPENING_PAREN() != null)
            {
                top.HasParenthesis = true;
                top.Expression = (Expression.Expression)Visit(topCtx.expression());
            }
            else
            {
                top.Expression = new LongValue(topCtx.LONG_VALUE().GetText());
            }
            if (topCtx.PERCENT() != null) top.IsPercentage = true;
            if (topCtx.TIES() != null) top.IsWithTies = true;
            select.Top = top;
        }

        // Informix SKIP n / FIRST n 量词
        if (context.informixSkipFirstClause() != null)
        {
            var sfc = context.informixSkipFirstClause();
            if (sfc.SKIP_KW() != null && sfc.expression().Length > 0)
                select.Skip = (Expression.Expression)Visit(sfc.expression(0));
            // FIRST 可能在 SKIP 后（expression(1)）或单独（expression(0)）
            if (sfc.FIRST() != null)
            {
                var firstExpr = sfc.SKIP_KW() != null && sfc.expression().Length > 1
                    ? sfc.expression(1) : sfc.expression(0);
                select.First = (Expression.Expression)Visit(firstExpr);
            }
        }

        if (context.DISTINCT() != null || context.DISTINCTROW() != null)
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

        // Oracle 层次查询（START WITH ... CONNECT BY ...）
        if (context.connectByClause() is { } connectByCtx)
        {
            var hier = new OracleHierarchicalExpression();
            var expressions = connectByCtx.expression();
            // 判断 START 与 CONNECT 的相对顺序：遍历子节点找首个 START/CONNECT token
            bool startBeforeConnect = false;
            for (int i = 0; i < connectByCtx.ChildCount; i++)
            {
                if (connectByCtx.GetChild(i) is Antlr4.Runtime.Tree.ITerminalNode tn)
                {
                    if (tn.Symbol.Type == JSqlParserGrammarLexer.START) { startBeforeConnect = true; break; }
                    if (tn.Symbol.Type == JSqlParserGrammarLexer.CONNECT) { break; }
                }
            }

            if (startBeforeConnect)
            {
                // START WITH expr CONNECT BY expr
                hier.StartExpression = (Expression.Expression)Visit(expressions[0]);
                hier.ConnectExpression = (Expression.Expression)Visit(expressions[1]);
            }
            else
            {
                // CONNECT BY expr [START WITH expr]
                hier.ConnectFirst = true;
                hier.ConnectExpression = (Expression.Expression)Visit(expressions[0]);
                if (expressions.Length > 1)
                    hier.StartExpression = (Expression.Expression)Visit(expressions[1]);
            }
            if (connectByCtx.NOCYCLE() != null) hier.NoCycle = true;
            select.OracleHierarchical = hier;
        }

        if (context.groupByClause() != null)
        {
            select.GroupBy = BuildGroupByElement(context.groupByClause());
        }

        if (context.havingClause() != null)
        {
            select.Having = (Expression.Expression)Visit(context.havingClause().expression());
        }

        // MySQL INTO OUTFILE/DUMPFILE（前置位置：SELECT ... INTO OUTFILE ... FROM）
        var intoClause = context.intoClause();
        if (intoClause != null && intoClause.OUTFILE() != null)
        {
            select.MySqlIntoOutfile = new MySqlIntoOutfile
            {
                Type = MySqlIntoOutfile.OutfileType.OUTFILE,
                FileName = intoClause.S_CHAR_LITERAL().GetText(),
                BeforeFrom = true
            };
            FillOutfileTail(select.MySqlIntoOutfile, intoClause.outfileTail());
        }
        else if (intoClause != null && intoClause.DUMPFILE() != null)
        {
            select.MySqlIntoOutfile = new MySqlIntoOutfile
            {
                Type = MySqlIntoOutfile.OutfileType.DUMPFILE,
                FileName = intoClause.S_CHAR_LITERAL().GetText(),
                BeforeFrom = true
            };
        }

        // MySQL INTO OUTFILE/DUMPFILE（尾部位置：SELECT ... FROM ... INTO OUTFILE ...）
        // plainSelect 文法末尾的可选 INTO 分支
        if (context.INTO() != null)
        {
            select.MySqlIntoOutfile = new MySqlIntoOutfile
            {
                Type = context.OUTFILE() != null
                    ? MySqlIntoOutfile.OutfileType.OUTFILE
                    : MySqlIntoOutfile.OutfileType.DUMPFILE,
                FileName = context.S_CHAR_LITERAL().GetText(),
                BeforeFrom = false
            };
            FillOutfileTail(select.MySqlIntoOutfile, context.outfileTail());
        }

        // DB2 OPTIMIZE FOR n ROWS
        if (context.optimizeForClause() != null)
        {
            select.OptimizeFor = long.Parse(context.optimizeForClause().LONG_VALUE().GetText());
        }

        // WINDOW 命名窗口定义：透传 windowItem 原始文本保 round-trip（对齐上游 windowDefinitions）
        if (context.windowClause() is { } windowCtx)
        {
            select.WindowDefinitions = windowCtx.windowItem().Select(GetOriginalText).ToList();
        }

        // QUALIFY 过滤表达式（Snowflake/Teradata）
        if (context.qualifyClause() is { } qualifyCtx)
        {
            select.Qualify = (Expression.Expression)Visit(qualifyCtx.expression());
        }

        return select;
    }

    /// <summary>
    /// 从 groupByClause 构造 GroupByElement，支持普通表达式 / GROUPING SETS / ROLLUP / CUBE / WITH ROLLUP。
    /// 对齐上游 GroupByColumnReferences。
    /// </summary>
    private GroupByElement BuildGroupByElement(JSqlParserGrammar.GroupByClauseContext ctx)
    {
        var gb = new GroupByElement();

        // MySQL WITH ROLLUP（普通表达式分支末尾）
        if (ctx.WITH() != null && ctx.ROLLUP() != null)
            gb.MySqlWithRollup = true;

        // GROUPING SETS ((a,b), (c))
        if (ctx.GROUPING() != null)
        {
            gb.GroupingSets = ctx.groupingSetItem().Select(GetOriginalText).ToList();
            return gb;
        }

        // ROLLUP(a,b) / CUBE(a,b) 与普通表达式混用
        if (ctx.groupByRollupCubeList() is { } rcList)
        {
            foreach (var item in rcList.groupByRollupCubeItem())
            {
                var rollupExprs = item.expression();
                if (item.ROLLUP() != null)
                {
                    gb.RollupExpressions = rollupExprs.Select(GetOriginalText).ToList();
                }
                else if (item.CUBE() != null)
                {
                    gb.CubeExpressions = rollupExprs.Select(GetOriginalText).ToList();
                }
                else if (rollupExprs.Length > 0)
                {
                    // 普通表达式
                    gb.GroupByExpressions.Add((Expression.Expression)Visit(rollupExprs[0]));
                }
            }
            return gb;
        }

        // 普通表达式列表
        gb.GroupByExpressions = ctx.expression().Select(e => (Expression.Expression)Visit(e)).ToList();
        return gb;
    }

    /// <summary>
    /// 把 outfileTail/fieldsClause/linesClause 的解析结果填充到 MySqlIntoOutfile。
    /// 前置与尾部两种位置共用，避免重复逻辑。
    /// </summary>
    private void FillOutfileTail(MySqlIntoOutfile outfile, JSqlParserGrammar.OutfileTailContext? tail)
    {
        if (tail == null) return;

        if (tail.CHARACTER() != null)
        {
            outfile.CharacterSet = tail.identifier()?.GetText() ?? (tail.BINARY() != null ? "BINARY" : null);
        }

        if (tail.outfileFieldsClause() != null)
        {
            var fields = tail.outfileFieldsClause();
            outfile.FieldsKeywordValue = fields.FIELDS() != null
                ? MySqlIntoOutfile.FieldsKeyword.FIELDS
                : MySqlIntoOutfile.FieldsKeyword.COLUMNS;
            // S_CHAR_LITERAL 按 TERMINATED / ENCLOSED / ESCAPED 的出现顺序填充
            var literals = fields.S_CHAR_LITERAL();
            int idx = 0;
            if (fields.TERMINATED() != null && literals.Length > idx)
            {
                outfile.FieldsTerminatedBy = literals[idx++].GetText();
            }
            outfile.FieldsOptionallyEnclosed = fields.OPTIONALLY() != null;
            if (fields.ENCLOSED() != null && literals.Length > idx)
            {
                outfile.FieldsEnclosedBy = literals[idx++].GetText();
            }
            if (fields.ESCAPED() != null && literals.Length > idx)
            {
                outfile.FieldsEscapedBy = literals[idx++].GetText();
            }
        }

        if (tail.outfileLinesClause() != null)
        {
            var lines = tail.outfileLinesClause();
            var literals = lines.S_CHAR_LITERAL();
            int idx = 0;
            if (lines.STARTING() != null && literals.Length > idx)
            {
                outfile.LinesStartingBy = literals[idx++].GetText();
            }
            if (lines.TERMINATED() != null && literals.Length > idx)
            {
                outfile.LinesTerminatedBy = literals[idx++].GetText();
            }
        }
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

        // STRAIGHT_JOIN（ClickHouse/MySQL 强制连接顺序）
        if (context.STRAIGHT_JOIN() != null)
        {
            join.Straight = true;
        }
        else if (context.CROSS() != null)
        {
            join.Cross = true;
        }
        else if (context.NATURAL() != null)
        {
            join.Natural = true;
        }
        else
        {
            // ClickHouse 修饰：GLOBAL / ANY|ALL（仅普通 JOIN 分支支持）
            if (context.GLOBAL() != null) join.Global = true;
            if (context.ANY() != null) join.Any = true;
            else if (context.ALL() != null) join.All = true;
            if (context.joinType() != null)
            {
                SetJoinType(join, context.joinType());
            }
            // SQL Server Join 提示（LOOP/HASH/MERGE）
            if (context.joinHint() is { } hintCtx)
                join.JoinHint = hintCtx.GetText().ToUpperInvariant();
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
                join.OnExpressions.Add((Expression.Expression)Visit(cond.expression()));
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
        // 表函数（FROM func(...) AS alias）
        if (context.tableFunction() != null)
        {
            var fn = BuildFunctionFromTableFunction(context.tableFunction());
            var tableFn = new TableFunction { Function = fn };
            if (context.alias() != null)
                tableFn.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            return tableFn;
        }

        if (context.table() != null)
        {
            var table = (Table)Visit(context.table());
            if (context.alias() != null)
            {
                table.Alias = new Alias(context.alias().identifier().GetText());
            }
            // SQL Server 表提示：WITH (INDEX(name) | NOLOCK | ...)
            if (context.sqlServerHints() != null)
            {
                table.SqlServerHints = (SQLServerHints)Visit(context.sqlServerHints());
            }
            // MySQL 索引提示：USE|IGNORE|FORCE INDEX|KEY (idx1, ...)
            if (context.mySqlIndexHint() != null)
            {
                table.MySqlIndexHint = (MySQLIndexHint)Visit(context.mySqlIndexHint());
            }
            // TABLESAMPLE 子句
            if (context.tableSampleClause() != null)
            {
                table.TableSample = BuildTableSample(context.tableSampleClause());
            }
            // PIVOT / UNPIVOT 子句
            if (context.pivotClause() != null)
            {
                BuildPivotClause(table, context.pivotClause());
            }
            // Snowflake 时间旅行（AT/BEFORE）
            if (context.timeTravelClause() != null)
            {
                table.TimeTravel = BuildTimeTravel(context.timeTravelClause());
            }
            return table;
        }

        if (context.jsonTable() != null)
        {
            var jsonTable = (JsonTable)Visit(context.jsonTable());
            if (context.alias() != null)
            {
                jsonTable.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            }
            return jsonTable;
        }

        if (context.LATERAL() != null)
        {
            // LATERAL 子查询：保留 LateralSubSelect 类型，避免退化为 ParenthesedSelect 导致前缀丢失
            // 注意：subSelect 规则自身包含 alias（g4:170），需从 subSelect 上下文取
            var subSelectCtx = context.subSelect();
            var lateral = new LateralSubSelect
            {
                Select = (Select)Visit(subSelectCtx.selectStatement())
            };
            if (subSelectCtx.alias() != null)
            {
                lateral.Alias = new Alias(subSelectCtx.alias().identifier().GetText());
            }
            return lateral;
        }

        if (context.subSelect() != null)
        {
            return Visit(context.subSelect());
        }

        return Visit(context.GetChild(0));
    }

    /// <summary>构建 TableSample（FROM 子句采样）。</summary>
    private TableSample BuildTableSample(JSqlParserGrammar.TableSampleClauseContext context)
    {
        var sample = new TableSample
        {
            SampleSize = (Expression.Expression)Visit(context.expression())
        };
        if (context.BERNOULLI() != null) sample.SamplingMethod = "BERNOULLI";
        else if (context.SYSTEM() != null) sample.SamplingMethod = "SYSTEM";
        if (context.PERCENT() != null) sample.Percentage = true;
        return sample;
    }

    /// <summary>
    /// 构建 TimeTravelClause（Snowflake 时间旅行 AT/BEFORE）。
    /// 形式：AT (TIMESTAMP|OFFSET|STATEMENT =&gt; expr) 或 BEFORE (STATEMENT =&gt; expr)。
    /// </summary>
    private TimeTravelClause BuildTimeTravel(JSqlParserGrammar.TimeTravelClauseContext context)
    {
        var clause = new TimeTravelClause
        {
            IsBefore = context.BEFORE() != null
        };
        if (context.TIMESTAMP() != null) clause.TravelType = "TIMESTAMP";
        else if (context.OFFSET() != null) clause.TravelType = "OFFSET";
        else if (context.STATEMENT() != null) clause.TravelType = "STATEMENT";
        clause.Expression = (Expression.Expression)Visit(context.expression());
        return clause;
    }

    /// <summary>从 tableFunction 上下文构建 Function（FROM 子句的表函数）。</summary>
    private Function BuildFunctionFromTableFunction(JSqlParserGrammar.TableFunctionContext context)
    {
        var func = new Function
        {
            Name = context.identifier().GetText(),
            AllColumns = context.MULTIPLY() != null
        };
        if (context.expressionList() != null)
        {
            var exprList = (ExpressionList)Visit(context.expressionList());
            func.Parameters = exprList;
        }
        return func;
    }

    /// <summary>构建 PIVOT/UNPIVOT 子句并挂到 Table。</summary>
    private void BuildPivotClause(Table table, JSqlParserGrammar.PivotClauseContext context)
    {
        // PIVOT (func FOR cols IN (vals)) [AS alias]
        if (context.PIVOT() != null)
        {
            var pivot = new Pivot
            {
                Function = (Function)Visit(context.functionExpr()),
                ForColumns = context.columnList(0).identifier()
                    .Select(i => new Column { ColumnName = i.GetText() }).ToList(),
                InItems = ((ExpressionList)Visit(context.expressionList())).Expressions
            };
            if (context.alias() != null)
                pivot.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            table.Pivot = pivot;
        }
        // UNPIVOT [INCLUDE NULLS] (cols FOR cols IN (vals)) [AS alias]
        else if (context.UNPIVOT() != null)
        {
            var unpivot = new UnPivot
            {
                IncludeNulls = context.INCLUDE() != null && context.NULLS() != null,
                UnpivotClause = context.columnList(0).identifier()
                    .Select(i => new Column { ColumnName = i.GetText() }).ToList(),
                UnpivotForClause = context.columnList(1).identifier()
                    .Select(i => new Column { ColumnName = i.GetText() }).ToList(),
                UnpivotInClause = ((ExpressionList)Visit(context.expressionList())).Expressions
            };
            if (context.alias() != null)
                unpivot.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            table.UnPivot = unpivot;
        }
    }

    public override object VisitJsonTable(JSqlParserGrammar.JsonTableContext context)
    {
        var jsonTable = new JsonTable
        {
            JsonExpression = (Expression.Expression)Visit(context.expression())
        };

        // 输入 FORMAT JSON（Oracle）
        if (context.FORMAT() != null) jsonTable.InputFormatJson = true;

        // path：第二个参数的 S_CHAR_LITERAL（在 expression 之后、PASSING 之前）
        if (context.COMMA().Length > 0 && context.S_CHAR_LITERAL() != null)
        {
            jsonTable.PathExpression = context.S_CHAR_LITERAL().GetText();
        }

        // PASSING 子句
        foreach (var p in context.jsonTablePassingItem())
        {
            jsonTable.PassingClauses.Add((JsonTablePassingClause)Visit(p));
        }

        // TYPE (STRICT|LAX)
        if (context.TYPE() != null)
        {
            jsonTable.ParsingType = context.STRICT() != null ? "STRICT" : "LAX";
        }

        // ON EMPTY / ON ERROR（jsonTableBehavior + ON EMPTY_KW/ON ERROR）
        var behaviors = context.jsonTableBehavior();
        bool hasEmpty = context.EMPTY_KW() != null;
        if (behaviors.Length >= 1 && hasEmpty)
        {
            jsonTable.OnEmptyBehavior = ParseJsonTableBehavior(behaviors[0]);
        }
        if (behaviors.Length >= 2 || (behaviors.Length >= 1 && !hasEmpty))
        {
            // 无 ON EMPTY 时，唯一 behavior 是 ON ERROR；有 ON EMPTY 时第二个是 ON ERROR
            jsonTable.OnErrorBehavior = ParseJsonTableBehavior(hasEmpty ? behaviors[1] : behaviors[0]);
        }

        foreach (var colCtx in context.jsonTableColumn())
        {
            jsonTable.Columns.Add((JsonTableColumn)Visit(colCtx));
        }

        // PLAN [DEFAULT] (plan_expr)
        if (context.jsonTablePlanClause() != null)
        {
            jsonTable.Plan = context.jsonTablePlanClause().GetText();
        }

        return jsonTable;
    }

    /// <summary>将 jsonTableBehavior 上下文解析为 JsonOnResponseBehavior。</summary>
    private JsonFunction.JsonOnResponseBehavior ParseJsonTableBehavior(JSqlParserGrammar.JsonTableBehaviorContext ctx)
    {
        if (ctx.ERROR() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.ERROR);
        if (ctx.NULL() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
        if (ctx.TRUE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.TRUE);
        if (ctx.FALSE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.FALSE);
        if (ctx.EMPTY_KW() != null)
        {
            return ctx.ARRAY() != null
                ? new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_ARRAY)
                : ctx.OBJECT() != null
                    ? new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_OBJECT)
                    : new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY);
        }
        if (ctx.DEFAULT() != null)
        {
            return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.DEFAULT,
                (Expression.Expression)Visit(ctx.expression()));
        }
        return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
    }

    public override object VisitJsonTablePassingItem(JSqlParserGrammar.JsonTablePassingItemContext context)
    {
        return new JsonTablePassingClause
        {
            ValueExpression = (Expression.Expression)Visit(context.expression()),
            ParameterName = context.identifier().GetText()
        };
    }

    public override object VisitJsonTableColumn(JSqlParserGrammar.JsonTableColumnContext context)
    {
        // NESTED PATH 分支
        if (context.NESTED() != null)
        {
            var nested = new JsonTableColumn
            {
                Path = context.S_CHAR_LITERAL().GetText(),
                NestedColumns = new List<JsonTableColumn>()
            };
            foreach (var inner in context.jsonTableColumn())
            {
                nested.NestedColumns.Add((JsonTableColumn)Visit(inner));
            }
            return nested;
        }

        // 列名是首个 identifier（grammar 含列名 + 可选 ENCODING name 两个 identifier）
        var colIdents = context.identifier();
        var column = new JsonTableColumn { Name = colIdents[0].GetText() };

        if (context.FOR() != null)
        {
            column.ForOrdinality = true;
        }
        else
        {
            // dataType 是组合 token，取原始文本
            column.DataType = context.dataType().GetText();
            if (context.EXISTS() != null) column.Exists = true;
            if (context.PATH() != null)
            {
                column.Path = context.S_CHAR_LITERAL().GetText();
            }
            // FORMAT JSON [ENCODING name]
            if (context.FORMAT() != null)
            {
                column.FormatJson = true;
                if (context.ENCODING() != null) column.Encoding = colIdents.Length > 1 ? colIdents[1].GetText() : "";
            }
            // WRAPPER
            if (context.jsonWrapperClause() != null)
            {
                var w = context.jsonWrapperClause();
                column.Wrapper = w.WITHOUT() != null
                    ? JsonFunction.WrapperType.WITHOUT
                    : JsonFunction.WrapperType.WITH;
                if (w.CONDITIONAL() != null) column.WrapperMode = JsonFunction.WrapperMode.CONDITIONAL;
                if (w.UNCONDITIONAL() != null) column.WrapperMode = JsonFunction.WrapperMode.UNCONDITIONAL;
                if (w.ARRAY() != null) column.WrapperArray = true;
            }
            // QUOTES
            if (context.jsonQuotesClause() != null)
            {
                var q = context.jsonQuotesClause();
                column.Quotes = q.KEEP() != null ? JsonFunction.QuotesType.KEEP : JsonFunction.QuotesType.OMIT;
                if (q.SCALAR() != null) column.QuotesOnScalarString = true;
            }
            // SCALARS
            if (context.ALLOW() != null) column.Scalars = JsonFunction.ScalarsType.ALLOW;
            else if (context.DISALLOW() != null) column.Scalars = JsonFunction.ScalarsType.DISALLOW;
            // 列级 ON EMPTY / ON ERROR
            var colBehaviors = context.jsonTableBehavior();
            bool colHasEmpty = context.EMPTY_KW() != null;
            if (colBehaviors.Length >= 1 && colHasEmpty)
            {
                column.OnEmptyBehavior = ParseJsonTableBehavior(colBehaviors[0]);
            }
            if (colBehaviors.Length >= 2 || (colBehaviors.Length >= 1 && !colHasEmpty))
            {
                column.OnErrorBehavior = ParseJsonTableBehavior(colHasEmpty ? colBehaviors[1] : colBehaviors[0]);
            }
        }

        return column;
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
        var expr = (Expression.Expression)Visit(context.expression());

        // 兼容：当 COLLATE 由 concatenationExpr 解析后，从表达式层提取 Collate 信息
        if (expr is CollateExpression collateExpr)
        {
            item.CollateName = collateExpr.Collate;
            expr = collateExpr.LeftExpression!;
        }

        // 兼容：当 ORDER BY 文法直接消费 COLLATE 时（向后兼容）
        if (context.COLLATE() != null)
        {
            item.CollateName = context.S_CHAR_LITERAL()?.GetText() ?? context.QUOTED_IDENTIFIER()?.GetText();
        }

        item.Expression = expr;
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

        // MySQL INSERT 修饰符：LOW_PRIORITY / DELAYED / HIGH_PRIORITY / IGNORE
        if (context.LOW_PRIORITY() != null) insert.ModifierPriority = InsertModifierPriority.LowPriority;
        else if (context.DELAYED() != null) insert.ModifierPriority = InsertModifierPriority.Delayed;
        else if (context.HIGH_PRIORITY() != null) insert.ModifierPriority = InsertModifierPriority.HighPriority;
        if (context.IGNORE() != null) insert.ModifierIgnore = true;

        if (context.identifierList() != null)
        {
            insert.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
            {
                insert.Columns.Add(new Column { ColumnName = id.GetText() });
            }
        }

        // MSSQL OUTPUT 子句（透传原始文本保 round-trip）
        if (context.outputClause() is { } outputCtx)
            insert.OutputClause = GetOriginalText(outputCtx);

        if (context.selectStatement() != null)
        {
            insert.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.valuesList() != null)
        {
            insert.UseValues = true;
            insert.ValuesItems = new List<ExpressionList>();
            foreach (var itemCtx in context.valuesList().valuesItem())
            {
                var exprList = new ExpressionList
                {
                    Expressions = new List<Expression.Expression>()
                };
                foreach (var exprCtx in itemCtx.expression())
                {
                    exprList.Expressions.Add((Expression.Expression)Visit(exprCtx));
                }
                insert.ValuesItems.Add(exprList);
            }
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

        if (context.onConflictClause() != null)
        {
            var onConflictCtx = context.onConflictClause();
            if (onConflictCtx.insertConflictTarget() != null)
            {
                insert.ConflictTarget = (InsertConflictTarget)Visit(onConflictCtx.insertConflictTarget());
            }
            insert.ConflictAction = (InsertConflictAction)Visit(onConflictCtx.insertConflictAction());
        }

        if (context.returningClause() != null)
        {
            insert.Returning = (ReturningClause)Visit(context.returningClause());
        }

        return insert;
    }

    // PostgreSQL ON CONFLICT 冲突目标：(col1, col2) [WHERE pred] 或 ON CONSTRAINT name
    public override object VisitInsertConflictTarget(JSqlParserGrammar.InsertConflictTargetContext context)
    {
        var target = new InsertConflictTarget();
        var identifiers = context.identifier();

        if (context.CONSTRAINT() != null && identifiers.Length > 0)
        {
            // ON CONSTRAINT name 形式：取第一个（也是唯一的）identifier
            target.ConstraintName = identifiers[0].GetText();
        }
        else
        {
            // (col1, col2, ...) [WHERE pred] 索引列形式
            foreach (var id in identifiers)
            {
                target.IndexColumnNames.Add(id.GetText());
            }
            if (context.whereClause() != null)
            {
                target.WhereExpression = (Expression.Expression)Visit(context.whereClause().expression());
            }
        }

        return target;
    }

    // PostgreSQL ON CONFLICT 冲突动作：DO NOTHING | DO UPDATE SET ... [WHERE ...]
    public override object VisitInsertConflictAction(JSqlParserGrammar.InsertConflictActionContext context)
    {
        var action = new InsertConflictAction();

        if (context.NOTHING() != null)
        {
            action.ConflictActionType = ConflictActionType.DoNothing;
        }
        else if (context.UPDATE() != null)
        {
            action.ConflictActionType = ConflictActionType.DoUpdate;
            action.UpdateSets = new List<UpdateSet>();
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
                action.UpdateSets.Add(updateSet);
            }
            if (context.whereClause() != null)
            {
                action.WhereExpression = (Expression.Expression)Visit(context.whereClause().expression());
            }
        }

        return action;
    }

    // ── MULTI INSERT (Oracle INSERT ALL/FIRST) ─

    public override object VisitMultiInsertStatement(JSqlParserGrammar.MultiInsertStatementContext context)
    {
        var multiInsert = new MultiInsert
        {
            // INSERT FIRST 命中即停；INSERT ALL 评估全部 WHEN
            IsFirst = context.FIRST() != null
        };

        foreach (var branchCtx in context.multiInsertBranch())
        {
            multiInsert.Branches.Add((MultiInsertBranch)Visit(branchCtx));
        }

        if (context.selectStatement() != null)
        {
            multiInsert.Select = (Select)Visit(context.selectStatement());
        }

        return multiInsert;
    }

    public override object VisitMultiInsertBranch(JSqlParserGrammar.MultiInsertBranchContext context)
    {
        var branch = new MultiInsertBranch();

        // WHEN expression THEN / ELSE / 无条件 INTO
        if (context.WHEN() != null && context.expression() != null)
        {
            branch.WhenCondition = (Expression.Expression)Visit(context.expression());
        }
        else if (context.ELSE() != null)
        {
            branch.IsElse = true;
        }

        // 收集分支下的所有 INTO 目标子句（一个分支可含多个目标）
        foreach (var clauseCtx in context.multiInsertClause())
        {
            branch.Clauses.Add((MultiInsertClause)Visit(clauseCtx));
        }

        return branch;
    }

    public override object VisitMultiInsertClause(JSqlParserGrammar.MultiInsertClauseContext context)
    {
        var clause = new MultiInsertClause
        {
            Table = (Table)Visit(context.table())
        };

        if (context.identifierList() != null)
        {
            clause.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
            {
                clause.Columns.Add(new Column { ColumnName = id.GetText() });
            }
        }

        // VALUES valuesList | selectStatement
        if (context.valuesList() != null)
        {
            clause.UseValues = true;
            clause.ValuesItems = new List<ExpressionList>();
            foreach (var itemCtx in context.valuesList().valuesItem())
            {
                var exprList = new ExpressionList
                {
                    Expressions = new List<Expression.Expression>()
                };
                foreach (var exprCtx in itemCtx.expression())
                {
                    exprList.Expressions.Add((Expression.Expression)Visit(exprCtx));
                }
                clause.ValuesItems.Add(exprList);
            }
        }
        else if (context.selectStatement() != null)
        {
            clause.UseValues = false;
            clause.Select = (Select)Visit(context.selectStatement());
        }

        return clause;
    }

    // ── UPDATE ─────────────────────────────────

    public override object VisitUpdateStatement(JSqlParserGrammar.UpdateStatementContext context)
    {
        var update = new Update();
        // MySQL 修饰符：LOW_PRIORITY / IGNORE
        if (context.LOW_PRIORITY() != null) update.ModifierLowPriority = true;
        if (context.IGNORE() != null) update.ModifierIgnore = true;
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
        // MySQL 修饰符：LOW_PRIORITY / QUICK / IGNORE
        if (context.LOW_PRIORITY() != null) delete.ModifierLowPriority = true;
        if (context.QUICK() != null) delete.ModifierQuick = true;
        if (context.IGNORE() != null) delete.ModifierIgnore = true;
        delete.Table = (Table)Visit(context.table());

        // DELETE ... USING fromItem (COMMA fromItem)*
        // PostgreSQL/SQL Server 风格的 DELETE USING 子句，允许引用附加表参与 WHERE 连接
        var usingItems = context.fromItem();
        if (context.USING() != null && usingItems.Length > 0)
        {
            delete.UsingItems = new List<FromItem>();
            foreach (var fromCtx in usingItems)
            {
                delete.UsingItems.Add((FromItem)Visit(fromCtx.tableOrSubquery()));
            }
        }

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
        // createTable 产生式含多个 table 引用（主表 / LIKE 表 / spannerInterleaveIn 表），第一个为主表
        var tables = context.table();
        create.Table = tables.Length > 0 ? (Table)Visit(tables[0]) : null;

        // CREATE 子句选项
        create.OrReplace = context.OR() != null;
        create.Unlogged = context.UNLOGGED() != null;
        if (context.IF() != null) create.IfNotExists = true;
        if (context.createOption() is { Length: > 0 } opts)
            create.CreateOptions = opts.Select(o => o.GetText()).ToList();

        // 括号内定义：simpleColumnNames（仅列名）或 createTableDefinition 列表
        if (context.simpleColumnNames() != null)
        {
            create.Columns = context.simpleColumnNames().identifier().Select(i => i.GetText()).ToList();
        }
        else
        {
            create.ColumnDefinitions = new List<ColumnDefinition>();
            create.Constraints = new List<Constraint>();
            foreach (var defCtx in context.createTableDefinition())
            {
                if (defCtx.columnDefinition() != null)
                    create.ColumnDefinitions.Add((ColumnDefinition)Visit(defCtx.columnDefinition()));
                else if (defCtx.tableConstraint() != null)
                    create.Constraints.Add((Constraint)Visit(defCtx.tableConstraint()));
            }
        }

        // 表级选项（ENGINE/CHARSET/PARTITION BY/ORDER BY/SAMPLE BY 等），createParameter 透传为字符串
        create.TableOptions = CollectCreateParameters(context.createParameter());

        // Oracle ROW MOVEMENT
        if (context.rowMovementClause() is { } rmCtx)
        {
            var mode = rmCtx.ENABLE() != null ? RowMovementMode.Enable : RowMovementMode.Disable;
            create.RowMovement = new RowMovement { Mode = mode };
        }

        // AS SELECT (CTAS)
        if (context.selectStatement() != null)
            create.Select = (Statement.Select.Select)Visit(context.selectStatement());

        // LIKE table
        if (context.LIKE() != null && tables.Length > 1)
            create.LikeTable = (Table)Visit(tables[1]);

        // Spanner INTERLEAVE IN PARENT
        if (context.spannerInterleaveIn() is { } interleaveCtx)
        {
            create.InterleaveIn = new SpannerInterleaveIn
            {
                Table = interleaveCtx.table() is { } parentTable ? (Table)Visit(parentTable) : null,
                OnDelete = interleaveCtx.ON() != null && interleaveCtx.DELETE() != null
                    ? new ReferentialAction
                    {
                        Type = ReferentialActionType.Delete,
                        Action = interleaveCtx.CASCADE() != null ? ReferentialActionMode.Cascade : ReferentialActionMode.NoAction
                    }
                    : null
            };
        }

        return create;
    }

    public override object VisitColumnDefinition(JSqlParserGrammar.ColumnDefinitionContext context)
    {
        var colDef = new ColumnDefinition();
        colDef.ColumnName = context.identifier().GetText();
        colDef.ColDataType = BuildColDataType(context.colDataType());
        // 列规格透传为字符串（NOT NULL / DEFAULT expr / MATERIALIZED / COMMENT '...' 等），对齐上游 columnSpecs
        // 结构化 columnConstraint + 兜底 createParameter 均收集原始文本，保 round-trip
        var specs = new List<string>();
        foreach (var cc in context.columnConstraint())
            specs.Add(GetOriginalText(cc));
        specs.AddRange(CollectCreateParameters(context.createParameter()));
        colDef.ColumnSpecs = specs;
        return colDef;
    }

    /// <summary>
    /// 从 colDataType 产生式构造结构化 <see cref="ColDataType"/>（类型名/参数/数组维度），对齐上游。
    /// 支持三路：ARRAY&lt;T&gt;（扁平化整体存 DataType，对齐上游 DataType() ARRAY 分支）、
    /// STRUCT(fld type, ...)（DataType="STRUCT"，字段进 ArgumentsStringList，对齐上游 ColDataType() STRUCT 分支）、
    /// 普通 dataType + 可选 PostgreSQL [] 数组维度。
    /// </summary>
    private static ColDataType BuildColDataType(JSqlParserGrammar.ColDataTypeContext? ctx)
    {
        var result = new ColDataType();
        if (ctx == null) return result;

        // ARRAY<T>：整体扁平化为 "ARRAY<...>"（含嵌套），对齐上游 setDataType("ARRAY<" + inner + ">")
        if (ctx.ARRAY() != null)
        {
            result.DataType = ctx.GetText();
            return result;
        }

        // STRUCT(fld type, ...)：DataType="STRUCT"，字段列表进 ArgumentsStringList
        if (ctx.STRUCT() != null)
        {
            result.DataType = "STRUCT";
            result.ArgumentsStringList = ctx.structColField()
                .Select(f => $"{f.identifier().GetText()} {BuildColDataType(f.colDataType())}")
                .ToList();
            return result;
        }

        // 普通类型：dataType 原始文本（保留 CHARACTER VARYING / set('a','b') 等空格）+ 可选 TIME ZONE 后缀
        // 数组维度 int[5] 不进 DataType，由 ArrayData 单独表示（避免 ToString 重复输出）
        var dtCtx = ctx.dataType();
        if (dtCtx != null)
        {
            var typeText = GetOriginalText(dtCtx);
            if (ctx.timeZoneSuffix() is { } tz)
                typeText += " " + GetOriginalText(tz);
            result.DataType = typeText;
        }
        // 数组维度 int[5]：填充 ArrayData（带尺寸），对齐上游 arrayData
        var dims = ctx.arrayDimension();
        if (dims is { Length: > 0 })
        {
            result.ArrayData = dims.Select(d =>
            {
                var n = d.LONG_VALUE();
                return n != null ? int.Parse(n.GetText()) : (int?)null;
            }).ToList();
        }
        return result;
    }

    /// <summary>
    /// 收集 createParameter 产生式的原始文本为字符串列表（保 round-trip），对齐上游 CreateParameter + tableOptionsStrings/columnSpecs。
    /// 使用 InputStream 区间获取原始字符流文本，保留 token 间的空格/等号风格。
    /// </summary>
    private static List<string> CollectCreateParameters(JSqlParserGrammar.CreateParameterContext[]? parameters)
    {
        var result = new List<string>();
        if (parameters == null) return result;
        foreach (var p in parameters)
            AddOriginalText(result, p);
        return result;
    }

    /// <summary>获取解析树节点的原始字符流文本（保留 token 间空格），用于 round-trip。</summary>
    private static string GetOriginalText(Antlr4.Runtime.ParserRuleContext ctx)
    {
        if (ctx.Start == null) return ctx.GetText();
        var stop = ctx.Stop ?? ctx.Start;
        var interval = new Antlr4.Runtime.Misc.Interval(ctx.Start.StartIndex, stop.StopIndex);
        return ctx.Start.InputStream?.GetText(interval) ?? ctx.GetText();
    }

    /// <summary>将解析树节点的原始文本加入列表（非空才加）。</summary>
    private static void AddOriginalText(List<string> list, Antlr4.Runtime.ParserRuleContext ctx)
    {
        var text = GetOriginalText(ctx);
        if (!string.IsNullOrEmpty(text)) list.Add(text);
    }

    public override object VisitTableConstraint(JSqlParserGrammar.TableConstraintContext context)
    {
        // CONSTRAINT name 中的 name 是第一个 identifier（若有 CONSTRAINT 关键字）
        var identifiers = context.identifier();
        string? name = null;
        if (context.CONSTRAINT() != null && identifiers.Length > 0)
            name = identifiers[0].GetText();

        // EXCLUDE WHERE (expr) — PostgreSQL
        if (context.EXCLUDE() != null)
        {
            var exclude = new ExcludeConstraint { Name = name };
            if (context.expression() != null)
                exclude.Expression = (Expression.Expression)Visit(context.expression());
            return exclude;
        }

        // CHECK (expr) — 持有表达式
        if (context.CHECK() != null)
        {
            var check = new CheckConstraint { Name = name };
            if (context.expression() != null)
                check.Expression = (Expression.Expression)Visit(context.expression());
            return check;
        }

        // FOREIGN KEY — 持有引用表/引用列/ON DELETE/UPDATE 动作
        if (context.FOREIGN() != null)
        {
            var identifierLists = context.identifierList();
            var localList = identifierLists.Length > 0 ? identifierLists[0] : null;
            var fk = new ForeignKeyIndex
            {
                Name = name,
                Type = "FOREIGN KEY",
                Columns = ExtractIdentifierList(localList)
            };
            // REFERENCES table (cols)
            var refTable = context.table();
            if (refTable != null)
                fk.ReferencedTable = (Table)Visit(refTable);
            if (identifierLists.Length > 1)
                fk.ReferencedColumnNames = ExtractIdentifierList(identifierLists[1]);
            // ON DELETE / ON UPDATE 动作
            var (onDelete, onUpdate) = ExtractReferentialActions(context.ON(), context.DELETE(), context.UPDATE(), context.referentialAction());
            fk.OnDelete = onDelete;
            fk.OnUpdate = onUpdate;
            return fk;
        }

        // 其余类型（PRIMARY KEY / UNIQUE / KEY|INDEX）沿用扁平 Constraint
        var constraint = new Constraint { Name = name };
        var identifierLists2 = context.identifierList();
        JSqlParserGrammar.IdentifierListContext? firstList = identifierLists2.Length > 0 ? identifierLists2[0] : null;

        if (context.PRIMARY() != null)
        {
            constraint.Type = "PRIMARY KEY";
            constraint.Columns = ExtractIdentifierList(firstList);
            FillUsingIndex(constraint, context);
            CollectIndexOptions(constraint, context);
        }
        else if (context.UNIQUE() != null && context.KEY() == null && context.INDEX() == null)
        {
            constraint.Type = "UNIQUE";
            constraint.Columns = ExtractIdentifierList(firstList);
            FillUsingIndex(constraint, context);
            CollectIndexOptions(constraint, context);
        }
        else if (context.KEY() != null || context.INDEX() != null)
        {
            // [UNIQUE | FULLTEXT | SPATIAL] (KEY|INDEX) [name] (cols [ASC|DESC]) — MySQL 索引定义
            var prefix = context.UNIQUE() != null ? "UNIQUE"
                : context.FULLTEXT() != null ? "FULLTEXT"
                : context.SPATIAL() != null ? "SPATIAL" : "";
            var kw = context.INDEX() != null ? "INDEX" : "KEY";
            constraint.Type = string.IsNullOrEmpty(prefix) ? kw : $"{prefix} {kw}";
            int nameIdx = context.CONSTRAINT() != null ? 1 : 0;
            if (identifiers.Length > nameIdx)
                constraint.Name = identifiers[nameIdx].GetText();
            var (colParams, colNames) = ExtractIndexColumnList(context.indexColumnList());
            constraint.IndexColumnParams = colParams;
            constraint.Columns = colNames;
            CollectIndexOptions(constraint, context);
        }
        return constraint;
    }

    /// <summary>
    /// 从 ON DELETE/UPDATE + referentialAction 节点提取引用动作，返回 (OnDelete, OnUpdate)。
    /// </summary>
    private static (ReferentialAction? onDelete, ReferentialAction? onUpdate) ExtractReferentialActions(
        Antlr4.Runtime.Tree.ITerminalNode[]? onNodes,
        Antlr4.Runtime.Tree.ITerminalNode[]? deleteNodes,
        Antlr4.Runtime.Tree.ITerminalNode[]? updateNodes,
        JSqlParserGrammar.ReferentialActionContext[]? actionContexts)
    {
        ReferentialAction? onDelete = null;
        ReferentialAction? onUpdate = null;
        if (onNodes == null || onNodes.Length == 0) return (onDelete, onUpdate);

        // ON 节点按顺序与 DELETE/UPDATE 配对
        int actionIdx = 0;
        var delCount = deleteNodes?.Length ?? 0;
        var updCount = updateNodes?.Length ?? 0;
        int delUsed = 0, updUsed = 0;
        for (int i = 0; i < onNodes.Length; i++)
        {
            ReferentialActionType type;
            // 判断当前 ON 后跟 DELETE 还是 UPDATE
            if (delUsed < delCount && updUsed < updCount)
            {
                // 两者都还有：按源码顺序判断（ON 节点后紧跟的 terminal）
                // 简化：DELETE 优先匹配（实际 SQL 中 ON DELETE 通常在 ON UPDATE 前）
                type = ReferentialActionType.Delete;
                delUsed++;
            }
            else if (delUsed < delCount)
            {
                type = ReferentialActionType.Delete;
                delUsed++;
            }
            else
            {
                type = ReferentialActionType.Update;
                updUsed++;
            }
            var action = actionIdx < (actionContexts?.Length ?? 0)
                ? ParseReferentialActionMode(actionContexts![actionIdx])
                : ReferentialActionMode.NoAction;
            actionIdx++;
            var ra = new ReferentialAction { Type = type, Action = action };
            if (type == ReferentialActionType.Delete) onDelete = ra;
            else onUpdate = ra;
        }
        return (onDelete, onUpdate);
    }

    /// <summary>解析 referentialAction 产生式为动作模式枚举。</summary>
    private static ReferentialActionMode ParseReferentialActionMode(JSqlParserGrammar.ReferentialActionContext ctx)
    {
        if (ctx.CASCADE() != null) return ReferentialActionMode.Cascade;
        if (ctx.SET() != null && ctx.NULL() != null) return ReferentialActionMode.SetNull;
        if (ctx.SET() != null && ctx.DEFAULT() != null) return ReferentialActionMode.SetDefault;
        if (ctx.RESTRICT() != null) return ReferentialActionMode.Restrict;
        return ReferentialActionMode.NoAction; // NO ACTION
    }

    /// <summary>
    /// 解析约束的 USING INDEX [name] 子句（Oracle/DB2，commit c7b3bdbd）。
    /// </summary>
    private static void FillUsingIndex(Constraint constraint, JSqlParserGrammar.TableConstraintContext context)
    {
        var usingCtx = context.usingIndexClause();
        if (usingCtx == null) return;
        constraint.HasUsingIndex = true;
        var nameId = usingCtx.identifier();
        if (nameId != null)
        {
            constraint.UsingIndex = nameId.GetText();
        }
    }

    /// <summary>
    /// 收集 MySQL 索引尾选项（USING BTREE/HASH、COMMENT '...'、KEY_BLOCK_SIZE n、VISIBLE/INVISIBLE 等），
    /// 用 InputStream 区间取原始文本保 round-trip，对齐上游 IndexOption/idxSpec。
    /// </summary>
    private static void CollectIndexOptions(Constraint constraint, JSqlParserGrammar.TableConstraintContext context)
    {
        var opts = context.indexOption();
        if (opts is { Length: > 0 })
            constraint.IndexOptions = opts.Select(GetOriginalText).ToList();
    }

    private static List<string> ExtractIdentifierList(JSqlParserGrammar.IdentifierListContext? list)
    {
        var result = new List<string>();
        if (list == null) return result;
        foreach (var id in list.identifier())
        {
            result.Add(id.GetText());
        }
        return result;
    }

    /// <summary>
    /// 解析 MySQL 索引列列表（含 ASC/DESC 排序方向，支持表达式索引 (expr)）。
    /// 返回 (含方向的列参数如 "col ASC" 或 "(expr) DESC", 纯列名/表达式文本列表)。
    /// </summary>
    private static (List<string> colParams, List<string> colNames) ExtractIndexColumnList(
        JSqlParserGrammar.IndexColumnListContext? list)
    {
        var colParams = new List<string>();
        var colNames = new List<string>();
        if (list == null) return (colParams, colNames);
        foreach (var col in list.indexColumn())
        {
            // 普通列名或表达式索引 (expr)：identifier 为 null 时取表达式原始文本并补括号
            string name;
            if (col.identifier() != null)
                name = col.identifier().GetText();
            else if (col.expression() != null)
                name = $"({GetOriginalText(col.expression())})";
            else
                name = GetOriginalText(col);
            colNames.Add(name);
            if (col.ASC() != null)
                colParams.Add($"{name} ASC");
            else if (col.DESC() != null)
                colParams.Add($"{name} DESC");
            else
                colParams.Add(name);
        }
        return (colParams, colNames);
    }

    // ── ALTER TABLE ────────────────────────────

    public override object VisitAlterStatement(JSqlParserGrammar.AlterStatementContext context)
    {
        var alter = new Alter();
        alter.Table = (Table)Visit(context.table());

        foreach (var opCtx in context.alterOperation())
        {
            alter.AlterExpressions.Add(BuildAlterExpression(opCtx));
        }
        return alter;
    }

    public override object VisitRenameTableStatement(JSqlParserGrammar.RenameTableStatementContext context)
    {
        var stmt = new RenameTableStatement
        {
            UsingTableKeyword = context.TABLE() != null,
            UsingIfExistsKeyword = context.IF() != null && context.EXISTS() != null
        };

        // WAIT/NOWAIT 指令
        if (context.WAIT() != null && context.LONG_VALUE() != null)
            stmt.WaitDirective = $"WAIT {context.LONG_VALUE().GetText()}";
        else if (context.NOWAIT() != null)
            stmt.WaitDirective = "NOWAIT";

        // 首对 old TO new（grammar 里 table 的索引：[0]=old, [1]=new, [2..]=后续对的 old/new）
        var tables = context.table();
        stmt.AddTableNames((Table)Visit(tables[0]), (Table)Visit(tables[1]));

        // 后续多对（grammar: COMMA table TO table，每对占 2 个 table 节点）
        for (var i = 2; i + 1 < tables.Length; i += 2)
        {
            stmt.AddTableNames((Table)Visit(tables[i]), (Table)Visit(tables[i + 1]));
        }

        return stmt;
    }

    public override object VisitAnalyzeStatement(JSqlParserGrammar.AnalyzeStatementContext context)
    {
        return new Analyze { Table = (Table)Visit(context.table()) };
    }

    public override object VisitCommentStatement(JSqlParserGrammar.CommentStatementContext context)
    {
        var comment = new Comment();
        // grammar: COMMENT ON (TABLE table | COLUMN identifier | VIEW table)
        if (context.VIEW() != null && context.table() is { } viewCtx)
            comment.View = (Table)Visit(viewCtx);
        else if (context.table() is { } tableCtx)
            comment.Table = (Table)Visit(tableCtx);
        else if (context.columnRef() is { } colCtx)
            comment.Column = new Column { ColumnName = colCtx.GetText() };
        comment.CommentText = new StringValue(context.S_CHAR_LITERAL().GetText());
        return comment;
    }

    public override object VisitExecuteStatement(JSqlParserGrammar.ExecuteStatementContext context)
    {
        var execType = context.CALL() != null ? ExecType.CALL
            : context.EXEC() != null ? ExecType.EXEC : ExecType.EXECUTE;
        var exec = new Execute { ExecType = execType, Name = context.identifier().GetText() };
        if (context.expressionList() != null)
        {
            var list = (ExpressionList)Visit(context.expressionList());
            exec.ExprList = list;
        }
        return exec;
    }

    public override object VisitPurgeStatement(JSqlParserGrammar.PurgeStatementContext context)
    {
        var purge = new PurgeStatement();
        if (context.TABLE() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.TABLE;
            purge.Table = (Table)Visit(context.table());
        }
        else if (context.INDEX() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.INDEX;
            // INDEX 形式：table.identifier，取 table 作为宿主
            purge.Table = (Table)Visit(context.table());
        }
        else if (context.RECYCLEBIN() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.RECYCLEBIN;
        }
        else if (context.DBA_RECYCLEBIN() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.DBA_RECYCLEBIN;
        }
        else if (context.TABLESPACE() != null)
        {
            purge.PurgeObjectType = PurgeObjectType.TABLESPACE;
            var ids = context.identifier();
            if (ids.Length > 0) purge.TableSpaceName = ids[0].GetText();
            if (context.USER() != null && ids.Length > 1) purge.UserName = ids[1].GetText();
        }
        return purge;
    }

    public override object VisitAlterViewStatement(JSqlParserGrammar.AlterViewStatementContext context)
    {
        var view = new AlterView
        {
            UseReplace = context.REPLACE() != null,
            View = (Table)Visit(context.table()),
            Select = (Select)Visit(context.selectStatement())
        };
        return view;
    }

    public override object VisitAlterSessionStatement(JSqlParserGrammar.AlterSessionStatementContext context)
    {
        var stmt = new AlterSession();
        var ids = context.identifier();
        if (context.SET() != null)
        {
            stmt.Operation = "SET";
            foreach (var id in ids) stmt.Parameters.Add(id.GetText());
        }
        else if (ids.Length > 0)
        {
            stmt.Operation = ids[0].GetText();
            for (var i = 1; i < ids.Length; i++) stmt.Parameters.Add(ids[i].GetText());
        }
        return stmt;
    }

    public override object VisitAlterSystemStatement(JSqlParserGrammar.AlterSystemStatementContext context)
    {
        var stmt = new AlterSystemStatement();
        var ids = context.identifier();
        if (context.SET() != null)
        {
            stmt.Operation = "SET";
            foreach (var id in ids) stmt.Parameters.Add(id.GetText());
        }
        else if (ids.Length > 0)
        {
            stmt.Operation = ids[0].GetText();
            for (var i = 1; i < ids.Length; i++) stmt.Parameters.Add(ids[i].GetText());
        }
        return stmt;
    }

    public override object VisitAlterSequenceStatement(JSqlParserGrammar.AlterSequenceStatementContext context)
    {
        var table = (Table)Visit(context.table());
        var sequence = new Sequence { Name = table.Name, SchemaName = table.SchemaName, Database = table.Database };
        var stmt = new AlterSequence { Sequence = sequence };
        foreach (var optCtx in context.alterSequenceOption())
        {
            sequence.Parameters ??= new List<SequenceParameter>();
            var param = ParseAlterSequenceOption(optCtx);
            if (param != null) sequence.Parameters.Add(param);
        }
        return stmt;
    }

    /// <summary>
    /// 将单个 alterSequenceOption 上下文解析为结构化 <see cref="SequenceParameter"/>。
    /// </summary>
    private static SequenceParameter? ParseAlterSequenceOption(JSqlParserGrammar.AlterSequenceOptionContext ctx)
    {
        if (ctx.RESTART() != null)
        {
            var p = new SequenceParameter(SequenceParameterType.RESTART_WITH);
            if (ctx.WITH() != null && ctx.LONG_VALUE() != null)
                p.Value = long.Parse(ctx.LONG_VALUE().GetText());
            return p;
        }
        if (ctx.INCREMENT() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.INCREMENT_BY).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));
        if (ctx.NOMINVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMINVALUE);
        if (ctx.MINVALUE() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.MINVALUE).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));
        if (ctx.NOMAXVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMAXVALUE);
        if (ctx.MAXVALUE() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.MAXVALUE).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));
        if (ctx.NOCACHE() != null) return new SequenceParameter(SequenceParameterType.NOCACHE);
        if (ctx.CACHE() != null && ctx.LONG_VALUE() != null)
            return new SequenceParameter(SequenceParameterType.CACHE).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));
        if (ctx.NOCYCLE() != null) return new SequenceParameter(SequenceParameterType.NOCYCLE);
        if (ctx.CYCLE() != null) return new SequenceParameter(SequenceParameterType.CYCLE);
        if (ctx.NOORDER() != null) return new SequenceParameter(SequenceParameterType.NOORDER);
        if (ctx.ORDER() != null) return new SequenceParameter(SequenceParameterType.ORDER);
        return null;
    }

    public override object VisitCreateSynonymStatement(JSqlParserGrammar.CreateSynonymStatementContext context)
    {
        var stmt = new CreateSynonym
        {
            OrReplace = context.OR() != null && context.REPLACE() != null,
            PublicSynonym = context.PUBLIC() != null,
            Name = context.identifier(0).GetText()
        };
        // FOR target：grammar FOR identifier (DOT identifier)?
        if (context.FOR() != null)
        {
            var forIds = context.identifier();
            // 第 0 个是 synonym 名，FOR 后的从 index 1 开始
            if (forIds.Length > 1)
            {
                var target = forIds[1].GetText();
                if (forIds.Length > 2) target += $".{forIds[2].GetText()}";
                stmt.ForList.Add(target);
            }
        }
        return stmt;
    }

    public override object VisitBlockStatement(JSqlParserGrammar.BlockStatementContext context)
    {
        var block = new Block();
        foreach (var stmtCtx in context.statement())
        {
            block.Statements.StatementList.Add((Azrng.JSqlParser.Statement.Statement)Visit(stmtCtx));
        }
        return block;
    }

    public override object VisitDeclareStatement(JSqlParserGrammar.DeclareStatementContext context)
    {
        var stmt = new DeclareStatement();
        foreach (var itemCtx in context.declareItem())
        {
            // 变量名可能来自 identifier、SINGLE_AT_IDENTIFIER 或 S_AT_IDENTIFIER
            var varName = itemCtx.identifier()?.GetText()
                ?? itemCtx.SINGLE_AT_IDENTIFIER()?.GetText()
                ?? itemCtx.S_AT_IDENTIFIER()?.GetText() ?? "";
            var item = new TypeDefExpr
            {
                UserVariable = varName,
                Type = itemCtx.dataType().GetText()
            };
            if (itemCtx.expression() != null)
                item.DefaultExpression = (Expression.Expression)Visit(itemCtx.expression());
            stmt.TypeDefExprList.Add(item);
        }
        return stmt;
    }

    public override object VisitIfElseStatement(JSqlParserGrammar.IfElseStatementContext context)
    {
        var stmt = new IfElseStatement
        {
            Condition = (Expression.Expression)Visit(context.expression()),
            IfStatement = (Azrng.JSqlParser.Statement.Statement)Visit(context.statement(0))
        };
        if (context.statement().Length > 1)
            stmt.ElseStatement = (Azrng.JSqlParser.Statement.Statement)Visit(context.statement(1));
        return stmt;
    }

    public override object VisitCreateFunctionStatement(JSqlParserGrammar.CreateFunctionStatementContext context)
    {
        var orReplace = context.OR() != null && context.REPLACE() != null;
        // 收集 functionBodyTokens 的原始文本（对齐上游 captureFunctionBody 容器式行为）
        var parts = new List<string> { context.identifier().GetText() };
        parts.AddRange(context.functionBodyTokens().GetText().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (context.FUNCTION() != null)
        {
            return new CreateFunction(parts) { OrReplace = orReplace };
        }
        return new CreateProcedure(parts) { OrReplace = orReplace };
    }

    /// <summary>
    /// 将 alterOperation 文法节点转换为 AlterExpression。修复此前 alterOperation 完全未解析的缺陷。
    /// </summary>
    private AlterExpression BuildAlterExpression(JSqlParserGrammar.AlterOperationContext context)
    {
        var expr = new AlterExpression();
        var identifiers = context.identifier();

        // 分区操作优先识别（ADD/DROP/... PARTITION 与列操作共享 ADD/DROP token，须先分流）
        if (context.PARTITION() != null)
        {
            BuildPartitionOperation(expr, context);
            return expr;
        }

        // ADD COLUMN? columnDefinition | ADD tableConstraint
        if (context.ADD() != null)
        {
            expr.Operation = AlterOperation.ADD;
            if (context.tableConstraint() != null)
            {
                // ADD 约束：约束类型/列/USING INDEX 写入结构化字段
                var constraint = (Constraint)Visit(context.tableConstraint());
                expr.ConstraintType = constraint.Type;
                if (constraint.Name != null) expr.ConstraintSymbol = constraint.Name;
                if (constraint.Type == "PRIMARY KEY" && constraint.Columns.Count > 0)
                    expr.PkColumns = constraint.Columns;
                else if (constraint.Type == "UNIQUE" && constraint.Columns.Count > 0)
                    expr.UkColumns = constraint.Columns;
                else if (!string.IsNullOrEmpty(constraint.Type))
                {
                    // KEY/INDEX 等 MySQL 索引约束：用 Constraint.ToString 作为可选说明符输出
                    expr.OptionalSpecifier = constraint.ToString();
                    // 清空 ConstraintType 避免 ToString 重复输出约束关键字
                    expr.ConstraintType = null;
                }
                // USING INDEX 子句
                expr.HasUsingIndex = constraint.HasUsingIndex;
                expr.UsingIndex = constraint.UsingIndex;
            }
            else if (context.columnDefinition() != null)
            {
                var colDef = (ColumnDefinition)Visit(context.columnDefinition());
                expr.ColDataTypeList = new List<AlterExpression.ColumnDataType>
                {
                    new() { ColumnName = colDef.ColumnName, DataType = colDef.ColDataType.ToString() }
                };
            }
            return expr;
        }

        // DROP 分支：DROP COLUMN / DROP PRIMARY KEY / DROP UNIQUE / DROP FOREIGN KEY / DROP CONSTRAINT
        if (context.DROP() != null)
        {
            if (context.PRIMARY() != null)
            {
                expr.Operation = AlterOperation.DROP_PRIMARY_KEY;
            }
            else if (context.UNIQUE() != null)
            {
                expr.Operation = AlterOperation.DROP_UNIQUE;
                if (identifiers.Length > 0) expr.ConstraintSymbol = identifiers[0].GetText();
            }
            else if (context.FOREIGN() != null)
            {
                expr.Operation = AlterOperation.DROP_FOREIGN_KEY;
                if (identifiers.Length > 0) expr.ConstraintSymbol = identifiers[0].GetText();
            }
            else if (context.CONSTRAINT() != null)
            {
                expr.Operation = AlterOperation.DROP;
                if (identifiers.Length > 0) expr.ConstraintSymbol = identifiers[0].GetText();
            }
            else
            {
                expr.Operation = AlterOperation.DROP;
                if (identifiers.Length > 0)
                    expr.ColumnName = identifiers[0].GetText();
            }
            return expr;
        }

        // MODIFY COLUMN? columnDefinition
        if (context.MODIFY() != null)
        {
            expr.Operation = AlterOperation.MODIFY;
            if (context.columnDefinition() != null)
            {
                var colDef = (ColumnDefinition)Visit(context.columnDefinition());
                expr.ColDataTypeList = new List<AlterExpression.ColumnDataType>
                {
                    new() { ColumnName = colDef.ColumnName, DataType = colDef.ColDataType.ToString() }
                };
            }
            return expr;
        }

        // CHANGE COLUMN? identifier columnDefinition
        if (context.CHANGE() != null)
        {
            expr.Operation = AlterOperation.CHANGE;
            if (identifiers.Length > 0)
                expr.ColumnOldName = identifiers[0].GetText();
            if (context.columnDefinition() != null)
            {
                var colDef = (ColumnDefinition)Visit(context.columnDefinition());
                expr.ColDataTypeList = new List<AlterExpression.ColumnDataType>
                {
                    new() { ColumnName = colDef.ColumnName, DataType = colDef.ColDataType.ToString() }
                };
            }
            return expr;
        }

        // ALTER COLUMN? identifier (SET DEFAULT ... | DROP DEFAULT | SET NOT NULL | DROP NOT NULL | TYPE ...)
        if (context.ALTER() != null)
        {
            expr.Operation = AlterOperation.ALTER;
            if (identifiers.Length > 0)
                expr.ColumnName = identifiers[0].GetText();
            return expr;
        }

        // RENAME 分支：RENAME COLUMN? old TO new / RENAME TO table / RENAME INDEX/KEY/CONSTRAINT old TO new
        if (context.RENAME() != null)
        {
            if (context.INDEX() != null)
            {
                expr.Operation = AlterOperation.RENAME_INDEX;
                if (identifiers.Length >= 2)
                {
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
            }
            else if (context.KEY() != null)
            {
                expr.Operation = AlterOperation.RENAME_KEY;
                if (identifiers.Length >= 2)
                {
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
            }
            else if (context.CONSTRAINT() != null)
            {
                expr.Operation = AlterOperation.RENAME_CONSTRAINT;
                if (identifiers.Length >= 2)
                {
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
            }
            else if (context.TO() != null)
            {
                if (identifiers.Length >= 2)
                {
                    // RENAME COLUMN? old TO new
                    expr.Operation = AlterOperation.RENAME;
                    expr.ColumnOldName = identifiers[0].GetText();
                    expr.ColumnName = identifiers[1].GetText();
                }
                else if (identifiers.Length == 1)
                {
                    // RENAME TO new_table
                    expr.Operation = AlterOperation.RENAME_TABLE;
                    expr.NewTableName = identifiers[0].GetText();
                }
            }
            return expr;
        }

        // ROW LEVEL SECURITY 分支
        if (context.ENABLE() != null)
        {
            expr.Operation = AlterOperation.ENABLE_ROW_LEVEL_SECURITY;
            return expr;
        }
        if (context.DISABLE() != null)
        {
            expr.Operation = AlterOperation.DISABLE_ROW_LEVEL_SECURITY;
            return expr;
        }
        if (context.FORCE() != null)
        {
            expr.Operation = AlterOperation.FORCE_ROW_LEVEL_SECURITY;
            return expr;
        }
        if (context.NO() != null)
        {
            expr.Operation = AlterOperation.NO_FORCE_ROW_LEVEL_SECURITY;
            return expr;
        }

        // ENGINE [=] name
        if (context.ENGINE() != null)
        {
            expr.Operation = AlterOperation.ENGINE;
            expr.UseEqualsForEngine = context.EQUALS() != null;
            if (identifiers.Length > 0) expr.OptionalSpecifier = identifiers[0].GetText();
            return expr;
        }

        // COMMENT [=] 'xxx'
        if (context.COMMENT() != null)
        {
            expr.Operation = context.EQUALS() != null ? AlterOperation.COMMENT_WITH_EQUAL_SIGN : AlterOperation.COMMENT;
            expr.UseEqualsForComment = context.EQUALS() != null;
            if (context.S_CHAR_LITERAL() != null) expr.OptionalSpecifier = context.S_CHAR_LITERAL().GetText();
            return expr;
        }

        if (context.REMOVE() != null)
        {
            expr.Operation = AlterOperation.REMOVE_PARTITIONING;
            return expr;
        }

        expr.Operation = AlterOperation.UNSPECIFIC;
        return expr;
    }

    /// <summary>
    /// 识别分区操作并填充 <paramref name="expr"/> 的结构与 Operation 字段。
    /// </summary>
    private void BuildPartitionOperation(AlterExpression expr, JSqlParserGrammar.AlterOperationContext context)
    {
        if (context.ADD() != null)
        {
            expr.Operation = AlterOperation.ADD_PARTITION;
            var partDefs = context.partitionDef();
            if (partDefs != null && partDefs.Length > 0)
            {
                expr.PartitionDefinitions = new List<PartitionDefinition>();
                foreach (var pd in partDefs)
                    expr.PartitionDefinitions.Add(BuildPartitionDefinition(pd));
            }
        }
        else if (context.DROP() != null)
        {
            expr.Operation = AlterOperation.DROP_PARTITION;
            expr.PartitionNames = CollectIdentifiers(context.identifierList());
        }
        else if (context.TRUNCATE() != null)
        {
            expr.Operation = AlterOperation.TRUNCATE_PARTITION;
            expr.PartitionNames = CollectIdentifiers(context.identifierList());
        }
        else if (context.COALESCE() != null)
        {
            expr.Operation = AlterOperation.COALESCE_PARTITION;
            if (context.LONG_VALUE() != null)
                expr.CoalescePartitionNumber = int.Parse(context.LONG_VALUE().GetText());
        }
        else if (context.REORGANIZE() != null)
        {
            expr.Operation = AlterOperation.REORGANIZE_PARTITION;
            expr.PartitionNames = CollectIdentifiers(context.identifierList());
            var partDefs = context.partitionDef();
            if (partDefs != null && partDefs.Length > 0)
            {
                expr.PartitionDefinitions = new List<PartitionDefinition>();
                foreach (var pd in partDefs)
                    expr.PartitionDefinitions.Add(BuildPartitionDefinition(pd));
            }
        }
        else if (context.EXCHANGE() != null)
        {
            expr.Operation = AlterOperation.EXCHANGE_PARTITION;
            var exchangeIdent = context.identifier();
            if (exchangeIdent != null && exchangeIdent.Length > 0)
                expr.PartitionNames = new List<string> { exchangeIdent[0].GetText() };
            if (context.table() != null)
            {
                var exTable = (Table)Visit(context.table());
                expr.ExchangePartitionTable = exTable.GetFullyQualifiedName();
            }
        }
        else
        {
            expr.Operation = AlterOperation.PARTITION_BY;
        }
    }

    /// <summary>
    /// 从 partitionDef 文法节点构建 <see cref="PartitionDefinition"/>。
    /// partitionDef: name VALUES? (LESS THAN (expr) | IN (exprList))?
    /// </summary>
    private PartitionDefinition BuildPartitionDefinition(JSqlParserGrammar.PartitionDefContext ctx)
    {
        var def = new PartitionDefinition();
        if (ctx.partitionName != null) def.Name = ctx.partitionName.GetText();
        if (ctx.LESS() != null && ctx.expression() != null)
        {
            def.ValuesLessThan = new ExpressionList
            {
                Expressions = new() { (Expression.Expression)Visit(ctx.expression()) }
            };
        }
        else if (ctx.IN() != null && ctx.expressionList() != null)
        {
            def.ValuesIn = (ExpressionList)Visit(ctx.expressionList());
        }
        return def;
    }

    /// <summary>
    /// 从 identifierList 文法节点收集标识符文本列表。
    /// </summary>
    private static List<string> CollectIdentifiers(JSqlParserGrammar.IdentifierListContext? ctx)
    {
        var result = new List<string>();
        if (ctx == null) return result;
        foreach (var id in ctx.identifier()) result.Add(id.GetText());
        return result;
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

    public override object VisitResetStatement(JSqlParserGrammar.ResetStatementContext context)
    {
        // RESET name | RESET ALL
        if (context.ALL() != null)
            return new ResetStatement { Name = "ALL" };
        return new ResetStatement { Name = context.identifier().GetText() };
    }

    public override object VisitCreatePolicy(JSqlParserGrammar.CreatePolicyContext context)
    {
        var policy = new CreatePolicy
        {
            PolicyName = context.identifier(0).GetText(),
            Table = (Table)Visit(context.table())
        };

        // FOR { ALL | SELECT | INSERT | UPDATE | DELETE }
        if (context.FOR() != null)
        {
            policy.Command = context.ALL() != null ? "ALL"
                : context.SELECT() != null ? "SELECT"
                : context.INSERT() != null ? "INSERT"
                : context.UPDATE() != null ? "UPDATE"
                : context.DELETE() != null ? "DELETE" : null;
        }

        // TO role1, role2, ...（identifier 列表，跳过第 0 个即 policyName）
        if (context.TO() != null)
        {
            var allIdentifiers = context.identifier();
            // 第 0 个是 policyName，从第 1 个开始是角色
            for (int i = 1; i < allIdentifiers.Length; i++)
            {
                policy.Roles.Add(allIdentifiers[i].GetText());
            }
        }

        // USING ( expression )
        // 注意：context.expression() 返回所有层级的 expression，USING 的表达式是第 1 个
        var expressions = context.expression();
        if (context.USING() != null && expressions.Length > 0)
        {
            policy.UsingExpression = (Expression.Expression)Visit(expressions[0]);
        }

        // WITH CHECK ( expression )
        if (context.CHECK() != null)
        {
            var checkExprIdx = context.USING() != null ? 1 : 0;
            if (expressions.Length > checkExprIdx)
            {
                policy.WithCheckExpression = (Expression.Expression)Visit(expressions[checkExprIdx]);
            }
        }

        return policy;
    }

    public override object VisitCreateSequence(JSqlParserGrammar.CreateSequenceContext context)
    {
        var table = (Table)Visit(context.table());
        var sequence = new Schema.Sequence
        {
            Database = table.Database,
            SchemaName = table.SchemaName,
            Name = table.Name
        };

        if (context.sequenceParameter().Length > 0)
        {
            sequence.Parameters = new List<SequenceParameter>();
            foreach (var paramCtx in context.sequenceParameter())
            {
                sequence.Parameters.Add(BuildSequenceParameter(paramCtx));
            }
        }

        return new CreateSequence { Sequence = sequence };
    }

    private static SequenceParameter BuildSequenceParameter(JSqlParserGrammar.SequenceParameterContext ctx)
    {
        // 有值参数
        if (ctx.INCREMENT() != null)
        {
            var value = long.Parse(ctx.LONG_VALUE().GetText());
            return new SequenceParameter(
                ctx.BY() != null ? SequenceParameterType.INCREMENT_BY : SequenceParameterType.INCREMENT)
                .WithValue(value);
        }
        if (ctx.START() != null)
        {
            var value = ctx.LONG_VALUE() != null ? long.Parse(ctx.LONG_VALUE().GetText()) : (long?)null;
            return new SequenceParameter(
                ctx.WITH() != null ? SequenceParameterType.START_WITH : SequenceParameterType.START)
                .WithValue(value ?? 0);
        }
        if (ctx.RESTART() != null)
        {
            var value = ctx.LONG_VALUE() != null ? long.Parse(ctx.LONG_VALUE().GetText()) : (long?)null;
            var p = new SequenceParameter(SequenceParameterType.RESTART_WITH);
            if (value != null) p.WithValue(value.Value);
            return p;
        }
        if (ctx.MAXVALUE() != null) return new SequenceParameter(SequenceParameterType.MAXVALUE).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));
        if (ctx.MINVALUE() != null) return new SequenceParameter(SequenceParameterType.MINVALUE).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));
        if (ctx.CACHE() != null) return new SequenceParameter(SequenceParameterType.CACHE).WithValue(long.Parse(ctx.LONG_VALUE().GetText()));

        // 无值参数
        if (ctx.NOMAXVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMAXVALUE);
        if (ctx.NOMINVALUE() != null) return new SequenceParameter(SequenceParameterType.NOMINVALUE);
        if (ctx.CYCLE() != null) return new SequenceParameter(SequenceParameterType.CYCLE);
        if (ctx.NOCYCLE() != null) return new SequenceParameter(SequenceParameterType.NOCYCLE);
        if (ctx.NOCACHE() != null) return new SequenceParameter(SequenceParameterType.NOCACHE);
        if (ctx.ORDER() != null) return new SequenceParameter(SequenceParameterType.ORDER);
        if (ctx.NOORDER() != null) return new SequenceParameter(SequenceParameterType.NOORDER);
        if (ctx.KEEP() != null) return new SequenceParameter(SequenceParameterType.KEEP);
        if (ctx.NOKEEP() != null) return new SequenceParameter(SequenceParameterType.NOKEEP);

        return new SequenceParameter();
    }

    // ── CREATE SCHEMA ──────────────────────────

    public override object VisitCreateSchema(JSqlParserGrammar.CreateSchemaContext context)
    {
        var schema = new Statement.Create.Schema.CreateSchema();
        if (context.IF() != null) schema.IfNotExists = true;

        var qCtx = context.schemaQualifiedName();
        var nameParts = qCtx.identifier();
        if (nameParts.Length == 1)
        {
            schema.SchemaName = nameParts[0].GetText();
        }
        else if (nameParts.Length == 2)
        {
            // catalog.schema 形式
            schema.CatalogName = nameParts[0].GetText();
            schema.SchemaName = nameParts[1].GetText();
        }

        if (context.AUTHORIZATION() != null)
        {
            // createSchema 文法中 AUTHORIZATION 后的 identifier 是唯一直接子节点 identifier
            var authId = context.identifier();
            if (authId != null) schema.Authorization = authId.GetText();
        }
        return schema;
    }

    // ── REFRESH MATERIALIZED VIEW ──────────────

    public override object VisitRefreshStatement(JSqlParserGrammar.RefreshStatementContext context)
    {
        var refresh = new Statement.Refresh.RefreshMaterializedViewStatement();
        if (context.table() is { } viewCtx) refresh.View = (Table)Visit(viewCtx);
        if (context.CONCURRENTLY() != null) refresh.Concurrently = true;
        // WITH [NO] DATA
        if (context.WITH() != null && context.DATA() != null)
        {
            refresh.RefreshMode = context.NO() != null
                ? Statement.Refresh.RefreshMode.WithNoData
                : Statement.Refresh.RefreshMode.WithData;
        }
        return refresh;
    }

    // ── UPSERT / REPLACE ───────────────────────

    public override object VisitUpsertStatement(JSqlParserGrammar.UpsertStatementContext context)
    {
        var upsert = new Statement.Insert.UpsertStatement();
        // 类型判定
        if (context.UPSERT() != null) upsert.UpsertType = Statement.Insert.UpsertType.Upsert;
        else if (context.REPLACE() != null && context.INSERT() == null) upsert.UpsertType = Statement.Insert.UpsertType.Replace;
        else upsert.UpsertType = Statement.Insert.UpsertType.InsertOrReplace;

        if (context.INTO() != null) upsert.UseInto = true;
        if (context.table() is { } tblCtx) upsert.Table = (Table)Visit(tblCtx);

        // 列
        if (context.identifierList() != null)
        {
            upsert.Columns = new List<Column>();
            foreach (var id in context.identifierList().identifier())
                upsert.Columns.Add(new Column { ColumnName = id.GetText() });
        }

        // SET / SELECT / VALUES 三选一
        if (context.SET() != null)
        {
            upsert.SetUpdateSets = new List<UpdateSet>();
            foreach (var assignment in context.assignmentItem())
            {
                var updateSet = new UpdateSet
                {
                    Columns = new List<Column>(),
                    Values = new List<Expression.Expression>()
                };
                foreach (var target in assignment.assignmentTarget())
                    updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                updateSet.Values.Add((Expression.Expression)Visit(assignment.expression()));
                upsert.SetUpdateSets.Add(updateSet);
            }
        }
        else if (context.selectStatement() != null)
        {
            upsert.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.valuesList() != null)
        {
            upsert.UseValues = true;
            upsert.ValuesItems = new List<ExpressionList>();
            foreach (var itemCtx in context.valuesList().valuesItem())
            {
                var exprList = new ExpressionList { Expressions = new List<Expression.Expression>() };
                foreach (var exprCtx in itemCtx.expression())
                    exprList.Expressions.Add((Expression.Expression)Visit(exprCtx));
                upsert.ValuesItems.Add(exprList);
            }
        }

        // ON DUPLICATE KEY UPDATE
        if (context.onDuplicateKey() is { } dupCtx)
        {
            if (dupCtx.NOTHING() != null)
            {
                upsert.DuplicateUpdateNothing = true;
            }
            else
            {
                upsert.DuplicateUpdateSets = new List<UpdateSet>();
                foreach (var assignment in dupCtx.assignmentItem())
                {
                    var updateSet = new UpdateSet
                    {
                        Columns = new List<Column>(),
                        Values = new List<Expression.Expression>()
                    };
                    foreach (var target in assignment.assignmentTarget())
                        updateSet.Columns.Add(new Column { ColumnName = target.GetText() });
                    updateSet.Values.Add((Expression.Expression)Visit(assignment.expression()));
                    upsert.DuplicateUpdateSets.Add(updateSet);
                }
            }
        }

        return upsert;
    }

    // ── BEGIN TRANSACTION ──────────────────────

    public override object VisitBeginTransactionStatement(JSqlParserGrammar.BeginTransactionStatementContext context)
    {
        return new BeginTransactionStatement
        {
            UseTransactionKeyword = context.TRANSACTION() != null
        };
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
            var key = optCtx.identifier().GetText();
            var valueCtx = optCtx.sessionOptionValue();
            // 值侧可能是 identifier 或 TRUE/FALSE/ON/OFF/NO/LONG_VALUE/S_CHAR_LITERAL
            var value = valueCtx.GetText();
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

        // RETURNING ... INTO target1, target2（Oracle PL/SQL 变量绑定）
        if (context.INTO() != null)
        {
            clause.DataItems = new List<string>();
            foreach (var tableCtx in context.table())
            {
                clause.DataItems.Add(((Table)Visit(tableCtx)).GetFullyQualifiedName());
            }
        }

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
        // SHOW [FULL] COLUMNS FROM table [LIKE expr | WHERE expr]
        if (context.COLUMNS() != null)
        {
            return new ShowColumnsStatement
            {
                Full = context.FULL() != null,
                Table = (Table)Visit(context.table())
            };
        }

        // SHOW INDEX FROM table
        if (context.INDEX() != null || context.INDEXES() != null)
        {
            return new ShowIndexStatement { Table = (Table)Visit(context.table()) };
        }

        // SHOW TABLES [FROM db] [LIKE expr | WHERE expr]
        if (context.TABLES() != null)
        {
            var show = new ShowTablesStatement();
            var ids = context.identifier();
            if (ids != null && ids.Length > 0)
                show.DbName = ids[0].GetText();
            return show;
        }

        // 兜底：通用 SHOW identifier
        var generic = new ShowStatement();
        var identifiers = context.identifier();
        if (identifiers != null && identifiers.Length > 0)
            generic.Name = string.Join(" ", identifiers.Select(i => i.GetText()));
        return generic;
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
        // TEMPORARY/TEMP（此前 grammar 已解析但 visitor 丢弃）
        if (context.TEMPORARY() != null) createView.Temporary = "TEMPORARY";
        else if (context.TEMP() != null) createView.Temporary = "TEMP";
        // RECURSIVE
        if (context.RECURSIVE() != null) createView.Recursive = true;
        // WITH [CASCADED|LOCAL] CHECK OPTION（此前丢弃）
        if (context.CHECK() != null)
        {
            createView.WithCheckOption = context.CASCADED() != null ? "CASCADED"
                : context.LOCAL() != null ? "LOCAL" : "";
        }
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

            // = ANY/ALL/SOME (subquery) 形式
            if (suffix.ANY() != null || suffix.SOME() != null || suffix.ALL() != null)
            {
                var anyType = suffix.ALL() != null ? AnyType.All
                    : suffix.SOME() != null ? AnyType.Some : AnyType.Any;
                var select = (Select)Visit(suffix.selectStatement());
                // 包装成比较运算符 + ANY/ALL/SOME
                var anyCompare = new AnyComparisonExpression(anyType, select);
                Expression.Expression result = anyCompare;
                if (op.EQUALS() != null) return CreateBinary<EqualsTo>(concat, result);
                if (op.NOT_EQUALS() != null || op.NOT_EQUALS2() != null || op.NOT_EQUALS3() != null)
                    return CreateBinary<NotEqualsTo>(concat, result);
                if (op.GREATER_THAN() != null) return CreateBinary<GreaterThan>(concat, result);
                if (op.GREATER_THAN_EQUALS() != null) return CreateBinary<GreaterThanEquals>(concat, result);
                if (op.MINOR_THAN() != null) return CreateBinary<MinorThan>(concat, result);
                if (op.MINOR_THAN_EQUALS() != null) return CreateBinary<MinorThanEquals>(concat, result);
                return CreateBinary<EqualsTo>(concat, result);
            }

            Expression.Expression right = (Expression.Expression)Visit(suffix.concatenationExpr(0));

            if (op.GEOMETRY_DISTANCE() != null)
                return new GeometryDistance("<->") { LeftExpression = concat, RightExpression = right };
            if (op.GEOMETRY_DISTANCE_HASH() != null)
                return new GeometryDistance("<#>") { LeftExpression = concat, RightExpression = right };
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
            if (suffix.SYMMETRIC() != null) between.UsingSymmetric = true;
            else if (suffix.ASYMMETRIC() != null) between.UsingAsymmetric = true;
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

        // SIMILAR TO / NOT SIMILAR TO
        if (suffix.SIMILAR() != null)
        {
            var similar = new SimilarToExpression();
            similar.LeftExpression = concat;
            similar.RightExpression = (Expression.Expression)Visit(suffix.concatenationExpr(0));
            if (suffix.NOT() != null) similar.Not = true;
            return similar;
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

        if (suffix.MEMBER() != null)
        {
            var memberOf = new MemberOfExpression();
            memberOf.LeftExpression = concat;
            memberOf.RightExpression = (Expression.Expression)Visit(suffix.concatenationExpr(0));
            if (suffix.NOT() != null) memberOf.Not = true;
            return memberOf;
        }

        if (suffix.OVERLAPS() != null)
        {
            // 左侧：当前 Azrng grammar 不支持括号列表，按单元素 ExpressionList 包装
            var left = new ExpressionList { Expressions = new() { concat } };
            var right = (Expression.Expression)Visit(suffix.concatenationExpr(0));
            var rightList = new ExpressionList { Expressions = new() { right } };
            return new OverlapsCondition { LeftExpression = left, RightExpression = rightList };
        }

        return concat;
    }

    public override object VisitConcatenationExpr(JSqlParserGrammar.ConcatenationExprContext context)
    {
        var additiveExprs = context.additiveExpr();
        Expression.Expression result;
        if (additiveExprs.Length == 1)
        {
            result = (Expression.Expression)Visit(additiveExprs[0]);
        }
        else
        {
            result = (Expression.Expression)Visit(additiveExprs[0]);
            for (int i = 1; i < additiveExprs.Length; i++)
            {
                var concat = new Concat();
                concat.LeftExpression = result;
                concat.RightExpression = (Expression.Expression)Visit(additiveExprs[i]);
                result = concat;
            }
        }

        // COLLATE 后缀（仅当存在且非 ORDER BY 上下文已消化时）
        if (context.COLLATE() != null)
        {
            var collateName = context.S_CHAR_LITERAL()?.GetText() ?? context.identifier().GetText();
            return new CollateExpression(result, collateName);
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

        // 按出现顺序处理后缀操作符：::colDataType（cast）、.identifier（字段访问）、
        // COLLATE collation、AT TIME ZONE expr。
        // ANTLR 的 postfixExpr 文法将这些操作符以 * 循环混合，需顺序遍历子节点。
        // 注：grammar 用 colDataType（含数组维度 [] 与 TIME ZONE 后缀），支持 ::text[] / ::character varying
        var colDataTypes = context.colDataType();
        int colDataTypeIdx = 0;
        var identifiers = context.identifier();
        int identifierIdx = 0;
        // postfixExpr 内部的 expression 子节点（用于 AT TIME ZONE）
        var subExpressions = context.expression();
        int subExprIdx = 0;
        // 跳过 OPENING_PAREN ( ... ) 的子表达式（与 AT TIME ZONE 共用 expression 规则）
        // 通过遍历过程动态判断当前 expression 是属于 AT TIME ZONE 还是函数调用
        bool expectingTimeZoneExpr = false;
        bool inBracket = false;
        // 用于范围表达式的临时存储
        Expression.Expression? pendingStartIndex = null;

        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is ITerminalNode terminal)
            {
                if (terminal.Symbol.Type == JSqlParserGrammarLexer.DOUBLE_COLON && colDataTypeIdx < colDataTypes.Length)
                {
                    var cast = new CastExpression();
                    cast.Expression = expr;
                    cast.DataType = colDataTypes[colDataTypeIdx].GetText();
                    cast.UseCastKeyword = false;
                    colDataTypeIdx++;
                    expr = cast;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.DOT && identifierIdx < identifiers.Length)
                {
                    // PostgreSQL 复合类型字段访问：(expr).field 或多层 (expr).field1.field2
                    expr = new RowGetExpression(expr, identifiers[identifierIdx].GetText());
                    identifierIdx++;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.AT)
                {
                    expectingTimeZoneExpr = true;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.LBRACKET)
                {
                    inBracket = true;
                    pendingStartIndex = null;
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.COLON && inBracket)
                {
                    // 范围表达式：[start:end]，start 已存于 pendingStartIndex
                    // 不做处理，等待结束 expression
                }
                else if (terminal.Symbol.Type == JSqlParserGrammarLexer.RBRACKET && inBracket)
                {
                    inBracket = false;
                    pendingStartIndex = null;
                }
            }
            else if (child is JSqlParserGrammar.ExpressionContext)
            {
                if (expectingTimeZoneExpr && subExprIdx < subExpressions.Length)
                {
                    expr = new TimezoneExpression
                    {
                        LeftExpression = expr,
                        TimeZoneExpression = (Expression.Expression)Visit(child)
                    };
                    subExprIdx++;
                    expectingTimeZoneExpr = false;
                }
                else if (inBracket && subExprIdx < subExpressions.Length)
                {
                    var idxExpr = (Expression.Expression)Visit(child);
                    subExprIdx++;

                    // 检查下一个非终结符是否为 COLON（范围表达式）
                    int nextIdx = i + 1;
                    while (nextIdx < context.ChildCount && context.GetChild(nextIdx) is not ITerminalNode)
                        nextIdx++;
                    bool nextIsColon = nextIdx < context.ChildCount
                        && context.GetChild(nextIdx) is ITerminalNode nextTerminal
                        && nextTerminal.Symbol.Type == JSqlParserGrammarLexer.COLON;

                    if (nextIsColon && pendingStartIndex == null)
                    {
                        // 当前是 range 的 start
                        pendingStartIndex = idxExpr;
                    }
                    else if (pendingStartIndex != null)
                    {
                        // 当前是 range 的 end
                        expr = new ArrayExpression
                        {
                            ObjExpression = expr,
                            StartIndexExpression = pendingStartIndex,
                            StopIndexExpression = idxExpr
                        };
                        pendingStartIndex = null;
                    }
                    else
                    {
                        // 单索引
                        expr = new ArrayExpression
                        {
                            ObjExpression = expr,
                            IndexExpression = idxExpr
                        };
                    }
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
        if (context.connectByPriorOperator() != null) return Visit(context.connectByPriorOperator());
        if (context.connectByRootOperator() != null) return Visit(context.connectByRootOperator());
        if (context.keyExpression() != null) return Visit(context.keyExpression());
        if (context.fullTextSearch() != null) return Visit(context.fullTextSearch());
        if (context.namedFunctionParameter() != null) return Visit(context.namedFunctionParameter());
        if (context.trimFunction() != null) return Visit(context.trimFunction());
        if (context.arrayConstructor() != null) return Visit(context.arrayConstructor());
        if (context.rowConstructor() != null) return Visit(context.rowConstructor());
        if (context.timeKeyExpression() != null) return Visit(context.timeKeyExpression());
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

    // Oracle/PostgreSQL 命名函数参数：name => expr 或 name := expr
    public override object VisitNamedFunctionParameter(JSqlParserGrammar.NamedFunctionParameterContext context)
    {
        var name = context.identifier().GetText();
        var expr = (Expression.Expression)Visit(context.expression());
        // ARROW (=>) 为 Oracle 形式，ASSIGN (:=) 为 PostgreSQL 形式
        if (context.ARROW() != null)
        {
            return new OracleNamedFunctionParameter(name, expr);
        }
        return new PostgresNamedFunctionParameter(name, expr);
    }

    // 数组构造器：ARRAY[1, 2, 3] 或 [1, 2, 3]
    public override object VisitArrayConstructor(JSqlParserGrammar.ArrayConstructorContext context)
    {
        ExpressionList? exprList = null;
        if (context.arrayElementList() != null)
        {
            exprList = new ExpressionList { Expressions = new List<Expression.Expression>() };
            foreach (var elem in context.arrayElementList().arrayElement())
            {
                exprList.Expressions.Add((Expression.Expression)Visit(elem));
            }
        }
        return new ArrayConstructor(exprList, arrayKeyword: context.ARRAY() != null);
    }

    public override object VisitArrayElement(JSqlParserGrammar.ArrayElementContext context)
    {
        var start = (Expression.Expression)Visit(context.expression(0));
        if (context.COLON() != null)
        {
            var end = (Expression.Expression)Visit(context.expression(1));
            return new RangeExpression(start, end);
        }
        return start;
    }

    // 时间关键字表达式：CURRENT_DATE / CURRENT_TIMESTAMP 等
    public override object VisitTimeKeyExpression(JSqlParserGrammar.TimeKeyExpressionContext context)
    {
        // 取原始文本（保留大小写），与上游 TimeKeyExpression 行为一致
        return new TimeKeyExpression(context.GetText());
    }

    // 行构造器：ROW(1, 2, 3)
    public override object VisitRowConstructor(JSqlParserGrammar.RowConstructorContext context)
    {
        var exprList = new ExpressionList { Expressions = new List<Expression.Expression>() };
        foreach (var expr in context.expressionList().expression())
        {
            exprList.Expressions.Add((Expression.Expression)Visit(expr));
        }
        return new RowConstructor("ROW", exprList);
    }

    // TRIM([LEADING|TRAILING|BOTH] [chars] [FROM] str) 或 TRIM(str)
    public override object VisitTrimFunction(JSqlParserGrammar.TrimFunctionContext context)
    {
        var trim = new TrimFunction();

        // 规范：LEADING/TRAILING/BOTH
        if (context.LEADING() != null) trim.TrimSpecification = TrimSpecification.Leading;
        else if (context.TRAILING() != null) trim.TrimSpecification = TrimSpecification.Trailing;
        else if (context.BOTH() != null) trim.TrimSpecification = TrimSpecification.Both;

        // 解析所有 expression，按位置赋值
        var exprs = context.expression();
        if (context.FROM() != null || context.COMMA() != null)
        {
            // [chars] [FROM|,] str 形式（2 个 expression）
            if (exprs.Length >= 1) trim.Expression = (Expression.Expression)Visit(exprs[0]);
            if (exprs.Length >= 2) trim.FromExpression = (Expression.Expression)Visit(exprs[1]);
            trim.UsingFromKeyword = context.FROM() != null;
        }
        else if (exprs.Length >= 1)
        {
            // TRIM(str) 简单形式（1 个 expression）
            trim.FromExpression = (Expression.Expression)Visit(exprs[0]);
        }

        return trim;
    }

    // SQL Server 表提示：WITH (INDEX(name) | NOLOCK | ...)
    public override object VisitSqlServerHints(JSqlParserGrammar.SqlServerHintsContext context)
    {
        var hints = new SQLServerHints();
        foreach (var hintCtx in context.sqlServerHint())
        {
            if (hintCtx.NOLOCK() != null)
            {
                hints.NoLock = true;
            }
            else if (hintCtx.INDEX() != null && hintCtx.identifier() != null)
            {
                hints.IndexName = hintCtx.identifier().GetText();
            }
        }
        return hints;
    }

    // MySQL 索引提示：USE|IGNORE|FORCE INDEX|KEY (idx1, ...)
    public override object VisitMySqlIndexHint(JSqlParserGrammar.MySqlIndexHintContext context)
    {
        var action = context.USE() != null ? "USE"
            : context.IGNORE() != null ? "IGNORE"
            : context.FORCE() != null ? "FORCE" : "";
        var qualifier = context.INDEX() != null ? "INDEX"
            : context.KEY() != null ? "KEY" : "INDEX";
        var names = context.identifier().Select(id => id.GetText()).ToList();
        return new MySQLIndexHint(action.ToUpperInvariant(), qualifier.ToUpperInvariant(), names);
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
            // S_CHAR_LITERAL 可能含可选前缀（N/E/U/R/B/RB/_utf8），交给 StringValue 构造函数识别
            var text = context.S_CHAR_LITERAL().GetText();
            return new StringValue(text);
        }
        if (context.S_ORACLE_Q_STRING() != null)
        {
            // Oracle q'...{...}...' 自定义分隔引号
            var text = context.S_ORACLE_Q_STRING().GetText();
            return new StringValue(text);
        }
        if (context.S_DOLLAR_QUOTED_STRING() != null)
        {
            // PostgreSQL dollar-quoted string: $$...$$ 或 $tag$...$tag$
            // 对应上游 commit 95ebda5a
            var text = context.S_DOLLAR_QUOTED_STRING().GetText();
            return new StringValue(text);
        }
        if (context.S_HEX() != null)
            return new HexValue { Value = context.S_HEX().GetText() };
        if (context.NULL() != null)
            return new NullValue();
        if (context.TRUE() != null)
            return new BooleanValue(true);
        if (context.FALSE() != null)
            return new BooleanValue(false);
        if (context.dateTimeLiteral() != null)
            return Visit(context.dateTimeLiteral());

        return new NullValue();
    }

    public override object VisitDateTimeLiteral(JSqlParserGrammar.DateTimeLiteralContext context)
    {
        // 取类型 token：DATE / DATETIME / TIME / TIMESTAMP / TIMESTAMPTZ
        var typeText = context.DATE()?.GetText()
            ?? context.DATETIME()?.GetText()
            ?? context.TIME()?.GetText()
            ?? context.TIMESTAMP()?.GetText()
            ?? context.TIMESTAMPTZ()?.GetText() ?? "";
        // 取值：保留原始 token 文本（含引号），对齐上游 expr.setValue(t.image) 的存储行为
        var value = (context.S_CHAR_LITERAL() ?? context.QUOTED_IDENTIFIER()).GetText();

        return new DateTimeLiteralExpression
        {
            Type = Enum.Parse<DateTimeType>(typeText.ToUpperInvariant(), ignoreCase: false),
            Value = value
        };
    }

    public override object VisitParameter(JSqlParserGrammar.ParameterContext context)
    {
        if (context.S_JDBC_NAMED_PARAM() != null)
        {
            return new JdbcNamedParameter { Name = context.S_JDBC_NAMED_PARAM().GetText()[1..] };
        }

        // :1、:2 数值绑定（Oracle/MySQL），与命名参数共用 JdbcNamedParameter，Name 存数字串
        // grammar 用 COLON LONG_VALUE 组合避免与数组范围 [1:3] 的冒号冲突
        if (context.LONG_VALUE() != null && context.COLON() != null)
        {
            return new JdbcNamedParameter { Name = context.LONG_VALUE().GetText() };
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

        // 仅当 CASE 后直接跟 switch 表达式时（switch 形式）才赋值 SwitchExpression。
        // 不能用 context.expression().Length 或 GetChild<ExpressionContext>(0) 判断：
        //   - context.expression() 会递归收集 whenExpr 内嵌的 cond/then 及 ELSE 表达式
        //   - GetChild<ExpressionContext>(0) 返回首个 ExpressionContext 类型的直接子节点，
        //     在 searched 形式下那其实是 ELSE 表达式，会被错误地当作 switch 表达式，
        //     round-trip 输出形如 "CASE 'small' WHEN a > 1 THEN 'big' ... END"（语义错误）。
        // 正确判断：CASE 关键字（child[0]）之后的 child[1] 是否为 ExpressionContext。
        //   - switch 形式：child[1] = ExpressionContext（switch 操作数）
        //   - searched 形式：child[1] = WhenExprContext（首个 WHEN）
        var switchExprChild = context.GetChild(1) as JSqlParserGrammar.ExpressionContext;
        if (switchExprChild != null)
        {
            caseExpr.SwitchExpression = (Expression.Expression)Visit(switchExprChild);
        }

        caseExpr.WhenClauses = new List<WhenClause>();

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
        // 设置 CAST 关键字：CAST / TRY_CAST / SAFE_CAST
        cast.Keyword = context.SAFE_CAST() != null ? "SAFE_CAST"
            : context.TRY_CAST() != null ? "TRY_CAST" : "CAST";
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
        // MySQL GROUP_CONCAT 特殊语法分支（独立规则，便于无歧义地访问内部子句）
        // 对应上游 commit ff28f826。
        if (context.groupConcatFunction() != null)
        {
            return Visit(context.groupConcatFunction());
        }

        // CONVERT / TRY_CONVERT / SAFE_CONVERT 双风格转码函数
        if (context.transcodingFunction() != null)
        {
            return Visit(context.transcodingFunction());
        }

        // JSON 标量函数 JSON_OBJECT / JSON_ARRAY / JSON_VALUE / JSON_EXISTS
        if (context.jsonObjectFunction() != null)
        {
            return Visit(context.jsonObjectFunction());
        }
        if (context.jsonArrayFunction() != null)
        {
            return Visit(context.jsonArrayFunction());
        }
        if (context.jsonValueFunction() != null)
        {
            return Visit(context.jsonValueFunction());
        }
        if (context.jsonExistsFunction() != null)
        {
            return Visit(context.jsonExistsFunction());
        }
        if (context.jsonQueryFunction() != null)
        {
            return Visit(context.jsonQueryFunction());
        }
        if (context.jsonObjectAggFunction() != null)
        {
            return Visit(context.jsonObjectAggFunction());
        }
        if (context.jsonArrayAggFunction() != null)
        {
            return Visit(context.jsonArrayAggFunction());
        }

        // SQL 标准命名参数字符串函数 SUBSTRING(x FROM 1 FOR 3) / POSITION(a IN b) / OVERLAY(x PLACING y FROM 1)
        if (context.specialStringFunction() != null)
        {
            return Visit(context.specialStringFunction());
        }

        // 序列取值表达式：NEXTVAL FOR seq 或 NEXT VALUE FOR seq
        // （NEXTVAL(seq) PostgreSQL 风格继续按 Function 处理）
        if ((context.NEXTVAL() != null || context.NEXT() != null) && context.FOR() != null)
        {
            var col = (Column)Visit(context.columnRef());
            // 序列名可能是多段限定（schema.seq），按 . 拆分
            var fullName = col.GetFullyQualifiedName();
            var nameList = fullName.Split('.').ToList();
            return new NextValExpression(nameList, usingNextValueFor: context.NEXT() != null);
        }

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

        // 构造 AnalyticExpression 的条件：有 OVER，或有 WITHIN GROUP，或有 FILTER（对齐上游 AnalyticType 四态）
        bool hasOver = context.overClause() != null;
        bool hasWithinGroup = context.withinGroupClause() != null;
        bool hasFilter = context.filterClause() != null;
        if (hasOver || hasWithinGroup || hasFilter)
        {
            var analytic = new AnalyticExpression();
            analytic.Name = funcName;
            ApplyFunctionClauses(context, analytic);

            // 按 OVER/WITHIN GROUP/FILTER 组合设置 AnalyticType，对齐上游
            if (hasWithinGroup && hasOver) analytic.Type = AnalyticType.WithinGroupOver;
            else if (hasWithinGroup) analytic.Type = AnalyticType.WithinGroup;
            else if (hasFilter && !hasOver) analytic.Type = AnalyticType.FilterOnly;
            else analytic.Type = AnalyticType.Over;

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

            if (hasOver)
            {
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
                    if (winSpec.windowFrame() != null)
                    {
                        analytic.WindowFrame = (WindowFrame)Visit(winSpec.windowFrame());
                    }
                }
            }

            return analytic;
        }

        var func = new Function();
        func.Name = funcName;
        func.Parameters = parameters;
        func.AllColumns = context.MULTIPLY() != null ||
            (parameters != null && parameters.Expressions.Count == 1 && parameters.Expressions[0] is AllColumns);

        // 通用函数关键字参数（在 ) 之后）
        var keywordArgs = context.functionKeywordArgument();
        if (keywordArgs != null && keywordArgs.Length > 0)
        {
            func.KeywordArguments = new List<KeywordArgument>();
            foreach (var kaCtx in keywordArgs)
            {
                func.KeywordArguments.Add((KeywordArgument)Visit(kaCtx));
            }
        }

        // Oracle KEEP (DENSE_RANK FIRST|LAST ORDER BY ...)
        if (context.keepExpression() != null)
        {
            func.Keep = (KeepExpression)Visit(context.keepExpression());
        }

        ApplyFunctionClauses(context, func);
        return func;
    }

    // SQL 标准命名参数字符串函数：SUBSTRING(x FROM 1 FOR 3) / POSITION(a IN b) / OVERLAY(x PLACING y FROM 1)
    public override object VisitSpecialStringFunction(JSqlParserGrammar.SpecialStringFunctionContext context)
    {
        var func = new Function { Name = context.identifier().GetText() };
        var tail = context.namedFunctionParamTail();
        var named = new NamedExpressionList();

        // 首个表达式前缀为空；第二个表达式前缀为 FROM/IN/PLACING（specialStringFunction 中的命名关键字）
        var firstExpr = (Expression.Expression)Visit(context.expression());
        var secondExpr = (Expression.Expression)Visit(tail.expression(0));
        var firstKw = context.FROM() != null ? "FROM" : context.IN() != null ? "IN" : "PLACING";

        named.Expressions.Add(firstExpr);
        named.Names.Add("");
        named.Expressions.Add(secondExpr);
        named.Names.Add(firstKw);

        // 可选第三段（FROM/FOR）与第四段（FOR）：OVERLAY(x PLACING y FROM z FOR w)
        if (tail.expression().Length > 1)
        {
            named.Expressions.Add((Expression.Expression)Visit(tail.expression(1)));
            named.Names.Add(tail.FROM() != null ? "FROM" : "FOR");
        }
        if (tail.expression().Length > 2)
        {
            named.Expressions.Add((Expression.Expression)Visit(tail.expression(2)));
            named.Names.Add("FOR");
        }

        func.NamedParameters = named;
        return func;
    }

    // Oracle KEEP (DENSE_RANK FIRST|LAST ORDER BY ...)
    public override object VisitKeepExpression(JSqlParserGrammar.KeepExpressionContext context)
    {
        var keep = new KeepExpression
        {
            Name = context.identifier().GetText(),
            First = context.FIRST() != null,
            OrderByElements = (List<OrderByElement>)Visit(context.orderByClause())
        };
        return keep;
    }

    // 通用函数关键字参数：nonReservedKeyword expression
    public override object VisitFunctionKeywordArgument(JSqlParserGrammar.FunctionKeywordArgumentContext context)
    {
        var keyword = context.nonReservedKeyword().GetText();
        var arg = new KeywordArgument { Keyword = keyword };
        if (context.expression() != null)
        {
            arg.Expression = (Expression.Expression)Visit(context.expression());
        }
        return arg;
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

    /// <summary>
    /// 构建 MySQL GROUP_CONCAT 函数。对应上游 commit ff28f826。
    /// 语法：GROUP_CONCAT(DISTINCT? expressionList? orderByClause? (SEPARATOR expression)?)
    /// </summary>
    // CONVERT / TRY_CONVERT / SAFE_CONVERT 双风格转码函数
    public override object VisitTranscodingFunction(JSqlParserGrammar.TranscodingFunctionContext context)
    {
        var keyword = context.TRY_CONVERT() != null ? "TRY_CONVERT"
            : context.SAFE_CONVERT() != null ? "SAFE_CONVERT" : "CONVERT";

        var body = (TranscodingFunction)Visit(context.transcodingBody());
        body.Keyword = keyword;
        return body;
    }

    public override object VisitTranscodingTypeStyle(JSqlParserGrammar.TranscodingTypeStyleContext context)
    {
        return new TranscodingFunction
        {
            IsTranscodeStyle = false,
            ColDataType = context.dataType().GetText(),
            Expression = (Expression.Expression)Visit(context.expression()),
            TranscodingName = context.LONG_VALUE()?.GetText(),
        };
    }

    public override object VisitTranscodingTranscodeStyle(JSqlParserGrammar.TranscodingTranscodeStyleContext context)
    {
        return new TranscodingFunction
        {
            IsTranscodeStyle = true,
            Expression = (Expression.Expression)Visit(context.expression()),
            TranscodingName = context.transcodingName().GetText(),
        };
    }

    // JSON_OBJECT( [KEY] k (VALUE|:|,) v [FORMAT JSON] ... )
    public override object VisitJsonObjectFunction(JSqlParserGrammar.JsonObjectFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.OBJECT);

        foreach (var kvpCtx in context.jsonKeyValuePair())
        {
            func.KeyValuePairs.Add((JsonKeyValuePair)Visit(kvpCtx));
        }

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        if (context.STRICT() != null)
        {
            func.Strict = true;
        }

        if (context.uniqueKeysClause() != null)
        {
            func.UniqueKeys = context.uniqueKeysClause().WITH() != null
                ? JsonFunction.UniqueKeysType.WITH
                : JsonFunction.UniqueKeysType.WITHOUT;
        }

        FillJsonReturning(func, context.jsonReturningClause());

        return func;
    }

    public override object VisitJsonKeyValuePair(JSqlParserGrammar.JsonKeyValuePairContext context)
    {
        var kvp = new JsonKeyValuePair { UsingKeyKeyword = context.KEY() != null };

        // key：S_CHAR_LITERAL 或 columnRef
        kvp.Key = context.S_CHAR_LITERAL(0) != null
            ? (object)new StringValue(context.S_CHAR_LITERAL(0).GetText())
            : Visit(context.columnRef(0));

        // 分隔符
        if (context.VALUE() != null)
        {
            kvp.Separator = JsonKeyValuePair.SeparatorKind.VALUE;
        }
        else if (context.DOUBLE_COLON() != null || context.COLON() != null || context.S_JDBC_NAMED_PARAM() != null)
        {
            kvp.Separator = JsonKeyValuePair.SeparatorKind.COLON;
        }
        else
        {
            kvp.Separator = JsonKeyValuePair.SeparatorKind.COMMA;
        }

        // 无空格冒号形式 key:bar：:bar 被词法分析为 S_JDBC_NAMED_PARAM，去前导冒号得到值
        if (context.S_JDBC_NAMED_PARAM() != null)
        {
            var raw = context.S_JDBC_NAMED_PARAM().GetText();
            kvp.Value = new Column { ColumnName = raw.Length > 1 ? raw[1..] : raw };
        }

        // value：分隔符存在时，取其后的 S_CHAR_LITERAL / columnRef / expression
        // 注意 key 可能是 S_CHAR_LITERAL 或 columnRef，value 的索引需根据 key 实际用法调整
        var charLits = context.S_CHAR_LITERAL();
        var colRefs = context.columnRef();
        if (charLits.Length > 1)
        {
            // key 是 S_CHAR_LITERAL(0)，value 是 S_CHAR_LITERAL(1)
            kvp.Value = new StringValue(charLits[1].GetText());
        }
        else if (colRefs.Length > 1)
        {
            // key 是 columnRef(0)，value 是 columnRef(1)
            kvp.Value = Visit(colRefs[1]);
        }
        else if (colRefs.Length == 1 && charLits.Length == 1)
        {
            // key 是 S_CHAR_LITERAL(0)，value 是 columnRef(0)
            kvp.Value = Visit(colRefs[0]);
        }
        else if (context.expression() != null)
        {
            kvp.Value = Visit(context.expression());
        }

        if (context.FORMAT() != null)
        {
            kvp.UsingFormatJson = true;
            if (context.ENCODING() != null)
            {
                kvp.Encoding = context.identifier().GetText();
            }
        }

        return kvp;
    }

    // JSON_ARRAY( expr [FORMAT JSON] ... )
    public override object VisitJsonArrayFunction(JSqlParserGrammar.JsonArrayFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.ARRAY);

        foreach (var elemCtx in context.jsonArrayElement())
        {
            var elem = new JsonFunctionExpression
            {
                Expression = (Expression.Expression)Visit(elemCtx.expression())
            };
            if (elemCtx.FORMAT() != null)
            {
                elem.UsingFormatJson = true;
                if (elemCtx.ENCODING() != null)
                {
                    elem.Encoding = elemCtx.identifier().GetText();
                }
            }
            func.Expressions.Add(elem);
        }

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        FillJsonReturning(func, context.jsonReturningClause());

        return func;
    }

    // JSON 函数输入表达式：expression [FORMAT JSON [ENCODING x]]
    public override object VisitJsonFunctionInput(JSqlParserGrammar.JsonFunctionInputContext context)
    {
        var input = new JsonFunctionExpression
        {
            Expression = (Expression.Expression)Visit(context.expression())
        };
        if (context.FORMAT() != null)
        {
            input.UsingFormatJson = true;
            if (context.ENCODING() != null)
            {
                input.Encoding = context.identifier().GetText();
            }
        }
        return input;
    }

    // JSON_VALUE(input, path, ...)
    public override object VisitJsonValueFunction(JSqlParserGrammar.JsonValueFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.VALUE);
        func.InputExpression = (JsonFunctionExpression)Visit(context.jsonFunctionInput());
        // jsonFunctionInput 之后的第一个 expression 即 path
        func.JsonPathExpression = (Expression.Expression)Visit(context.expression(0));
        FillJsonReturning(func, context.jsonReturningClause());

        // ON EMPTY / ON ERROR：根据 EMPTY_KW clause 是否存在判断 behavior 归属
        // grammar: (jsonValueBehavior ON EMPTY_KW)? (jsonValueBehavior ON ERROR)?
        var behaviors = context.jsonValueBehavior();
        bool hasEmpty = context.EMPTY_KW() != null;
        if (behaviors.Length == 1)
        {
            // 只有 1 个 behavior：归属 ON EMPTY 或 ON ERROR 之一
            if (hasEmpty)
                func.OnEmptyBehavior = ParseJsonValueBehavior(behaviors[0]);
            else
                func.OnErrorBehavior = ParseJsonValueBehavior(behaviors[0]);
        }
        else if (behaviors.Length >= 2)
        {
            func.OnEmptyBehavior = ParseJsonValueBehavior(behaviors[0]);
            func.OnErrorBehavior = ParseJsonValueBehavior(behaviors[1]);
        }
        return func;
    }

    // JSON_EXISTS(input, path, ...)
    public override object VisitJsonExistsFunction(JSqlParserGrammar.JsonExistsFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.EXISTS);
        func.InputExpression = (JsonFunctionExpression)Visit(context.jsonFunctionInput());
        func.JsonPathExpression = (Expression.Expression)Visit(context.expression(0));

        if (context.jsonExistsBehavior() != null)
        {
            var b = context.jsonExistsBehavior();
            var type = b.TRUE() != null ? JsonFunction.OnResponseBehaviorType.TRUE
                : b.FALSE() != null ? JsonFunction.OnResponseBehaviorType.FALSE
                : b.UNKNOWN() != null ? JsonFunction.OnResponseBehaviorType.UNKNOWN
                : JsonFunction.OnResponseBehaviorType.ERROR;
            func.OnErrorBehavior = new JsonFunction.JsonOnResponseBehavior(type);
        }
        return func;
    }

    // JSON_QUERY(input, path, ...)
    public override object VisitJsonQueryFunction(JSqlParserGrammar.JsonQueryFunctionContext context)
    {
        var func = new JsonFunction(JsonFunction.FunctionType.QUERY);
        func.InputExpression = (JsonFunctionExpression)Visit(context.jsonFunctionInput());
        func.JsonPathExpression = (Expression.Expression)Visit(context.expression(0));
        FillJsonReturning(func, context.jsonReturningClause());

        if (context.jsonWrapperClause() != null)
        {
            var w = context.jsonWrapperClause();
            func.Wrapper = w.WITHOUT() != null
                ? JsonFunction.WrapperType.WITHOUT
                : JsonFunction.WrapperType.WITH;
            if (w.CONDITIONAL() != null) func.WrapperModeValue = JsonFunction.WrapperMode.CONDITIONAL;
            if (w.UNCONDITIONAL() != null) func.WrapperModeValue = JsonFunction.WrapperMode.UNCONDITIONAL;
            if (w.ARRAY() != null) func.WrapperArray = true;
        }

        if (context.jsonQuotesClause() != null)
        {
            var q = context.jsonQuotesClause();
            func.Quotes = q.KEEP() != null ? JsonFunction.QuotesType.KEEP : JsonFunction.QuotesType.OMIT;
            if (q.SCALAR() != null) func.QuotesOnScalarString = true;
        }

        // ON EMPTY / ON ERROR
        var behaviors = context.jsonQueryBehavior();
        bool hasEmpty = context.EMPTY_KW() != null;
        if (behaviors.Length == 1)
        {
            if (hasEmpty) func.OnEmptyBehavior = ParseJsonQueryBehavior(behaviors[0]);
            else func.OnErrorBehavior = ParseJsonQueryBehavior(behaviors[0]);
        }
        else if (behaviors.Length >= 2)
        {
            func.OnEmptyBehavior = ParseJsonQueryBehavior(behaviors[0]);
            func.OnErrorBehavior = ParseJsonQueryBehavior(behaviors[1]);
        }

        // Legacy 额外 path 参数（JSON_QUERY(input, path1, path2...)），仅在无 PASSING 时收集
        // context.expression() 含首个 path（index 0）+ 额外 path（index 1+）
        if (func.PassingExpressions.Count == 0)
        {
            var allExprs = context.expression();
            for (int i = 1; i < allExprs.Length; i++)
            {
                func.AdditionalQueryPathArguments.Add(((Expression.Expression)Visit(allExprs[i])).ToString());
            }
        }
        return func;
    }

    private JsonFunction.JsonOnResponseBehavior ParseJsonQueryBehavior(JSqlParserGrammar.JsonQueryBehaviorContext b)
    {
        if (b.DEFAULT() != null)
        {
            return new JsonFunction.JsonOnResponseBehavior(
                JsonFunction.OnResponseBehaviorType.DEFAULT,
                (Expression.Expression)Visit(b.expression()));
        }
        if (b.TRUE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.TRUE);
        if (b.FALSE() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.FALSE);
        if (b.ARRAY() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_ARRAY);
        if (b.OBJECT() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY_OBJECT);
        if (b.EMPTY_KW() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY);
        if (b.ERROR() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.ERROR);
        return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
    }

    // JSON_OBJECTAGG([KEY] key (VALUE|:|,) value [FORMAT JSON] [ON NULL] [UNIQUE KEYS])
    public override object VisitJsonObjectAggFunction(JSqlParserGrammar.JsonObjectAggFunctionContext context)
    {
        var func = new JsonAggregateFunction
        {
            AggregateFunctionType = JsonAggregateFunction.AggregateType.OBJECT,
            Name = "JSON_OBJECTAGG",
            UsingKeyKeyword = context.KEY() != null
        };

        // key
        func.Key = context.S_CHAR_LITERAL() != null
            ? (object)new StringValue(context.S_CHAR_LITERAL().GetText())
            : Visit(context.columnRef());

        // 分隔符
        if (context.VALUE() != null)
        {
            func.UsingValueSeparator = true;
            func.UsingValueKeyword = true;
        }
        else if (context.DOUBLE_COLON() != null || context.COLON() != null)
        {
            func.UsingValueSeparator = false;
        }
        else if (context.COMMA() != null)
        {
            // COMMA 分隔（MySQL 风格 key,value），对齐上游 MYSQL_OBJECT
            func.UsingValueSeparator = false;
            func.UseCommaSeparator = true;
        }

        // value
        func.Value = (Expression.Expression)Visit(context.expression());

        if (context.FORMAT() != null) func.UsingFormatJson = true;

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        if (context.uniqueKeysClause() != null)
        {
            func.UniqueKeys = context.uniqueKeysClause().WITH() != null
                ? JsonFunction.UniqueKeysType.WITH
                : JsonFunction.UniqueKeysType.WITHOUT;
        }

        return func;
    }

    // JSON_ARRAYAGG(expr [FORMAT JSON] [ORDER BY ...] [ON NULL])
    public override object VisitJsonArrayAggFunction(JSqlParserGrammar.JsonArrayAggFunctionContext context)
    {
        var func = new JsonAggregateFunction
        {
            AggregateFunctionType = JsonAggregateFunction.AggregateType.ARRAY,
            Name = "JSON_ARRAYAGG",
            AggregateExpression = (Expression.Expression)Visit(context.expression())
        };

        if (context.FORMAT() != null) func.UsingFormatJson = true;

        if (context.orderByClause() != null)
        {
            func.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        }

        if (context.onNullClause() != null)
        {
            func.OnNull = context.onNullClause().ABSENT() != null
                ? JsonFunction.OnNullType.ABSENT
                : JsonFunction.OnNullType.NULL;
        }

        return func;
    }

    private JsonFunction.JsonOnResponseBehavior ParseJsonValueBehavior(JSqlParserGrammar.JsonValueBehaviorContext b)
    {
        if (b.DEFAULT() != null)
        {
            return new JsonFunction.JsonOnResponseBehavior(
                JsonFunction.OnResponseBehaviorType.DEFAULT,
                (Expression.Expression)Visit(b.expression()));
        }
        if (b.EMPTY_KW() != null)
        {
            // EMPTY 行为输出 "EMPTY "（带尾空格，上游 JsonOnResponseBehavior 特性）
            return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.EMPTY);
        }
        if (b.ERROR() != null) return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.ERROR);
        return new JsonFunction.JsonOnResponseBehavior(JsonFunction.OnResponseBehaviorType.NULL);
    }

    private void FillJsonReturning(JsonFunction func, JSqlParserGrammar.JsonReturningClauseContext? ret)
    {
        if (ret == null) return;
        func.ReturningType = ret.dataType().GetText();
        if (ret.FORMAT() != null)
        {
            func.ReturningFormatJson = true;
            if (ret.ENCODING() != null)
            {
                func.ReturningEncoding = ret.identifier().GetText();
            }
        }
    }

    public override object VisitGroupConcatFunction(JSqlParserGrammar.GroupConcatFunctionContext context)
    {
        var func = new Function { Name = "GROUP_CONCAT" };

        if (context.DISTINCT() != null)
        {
            func.Distinct = true;
        }

        if (context.expressionList() != null)
        {
            var parameters = new ExpressionList { Expressions = new List<Expression.Expression>() };
            foreach (var expr in context.expressionList().expression())
            {
                parameters.Expressions.Add((Expression.Expression)Visit(expr));
            }
            func.Parameters = parameters;
        }

        if (context.orderByClause() != null)
        {
            func.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        }

        if (context.SEPARATOR() != null)
        {
            func.Separator = (Expression.Expression)Visit(context.expression());
        }

        if (context.filterClause() != null)
        {
            func.FilterExpression =
                (Expression.Expression)Visit(context.filterClause().whereClause().expression());
        }

        return func;
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
        Column column;
        if (identifiers.Length == 1)
        {
            column = new Column { ColumnName = identifiers[0].GetText() };
        }
        else if (identifiers.Length == 2)
        {
            var table = new Table { Name = identifiers[0].GetText() };
            column = new Column { Table = table, ColumnName = identifiers[1].GetText() };
        }
        else
        {
            column = new Column { ColumnName = context.GetText() };
        }

        // Oracle 老式外连接语法 column(+) — commit 834afe18
        if (context.oracleOuterJoinSuffix() != null)
        {
            column.OldOracleJoinSyntax = OracleJoinSyntax.OracleJoinRight;
        }
        return column;
    }

    public override object VisitTable(JSqlParserGrammar.TableContext context)
    {
        var identifiers = context.identifier();
        // 支持 1-4 段命名：name / schema.name / db.schema.name / server.db.schema.name
        return identifiers.Length switch
        {
            1 => new Table { Name = identifiers[0].GetText() },
            2 => new Table { SchemaName = identifiers[0].GetText(), Name = identifiers[1].GetText() },
            3 => new Table
            {
                Database = identifiers[0].GetText(),
                SchemaName = identifiers[1].GetText(),
                Name = identifiers[2].GetText()
            },
            _ => new Table
            {
                ServerName = identifiers[0].GetText(),
                Database = identifiers[1].GetText(),
                SchemaName = identifiers[2].GetText(),
                Name = identifiers[3].GetText()
            }
        };
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

    public override object VisitConnectByRootOperator(JSqlParserGrammar.ConnectByRootOperatorContext context)
    {
        return new ConnectByRootOperator((Expression.Expression)Visit(context.expression()));
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
                join.OnExpressions.Add((Expression.Expression)Visit(condCtx.expression()));
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
        bool corresponding = context.CORRESPONDING() != null;

        if (context.UNION() != null)
            return new SetOperation(SetOperation.OperationType.UNION, all, distinct, corresponding);
        if (context.INTERSECT() != null)
            return new SetOperation(SetOperation.OperationType.INTERSECT, all, distinct, corresponding);
        if (context.EXCEPT() != null)
            return new SetOperation(SetOperation.OperationType.EXCEPT, all, distinct, corresponding);
        return new SetOperation(SetOperation.OperationType.MINUS, all, distinct, corresponding);
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

    // ── 窗口框架 ROWS/RANGE/GROUPS ──────────────

    public override object VisitWindowFrame(JSqlParserGrammar.WindowFrameContext context)
    {
        var frame = new WindowFrame();

        // 框架类型：ROWS / RANGE / GROUPS
        if (context.ROWS() != null) frame.Type = FrameType.Rows;
        else if (context.RANGE() != null) frame.Type = FrameType.Range;
        else if (context.GROUPS() != null) frame.Type = FrameType.Groups;

        // 边界：单边界或 BETWEEN ... AND ...
        var bounds = context.windowFrameBound();
        if (context.BETWEEN() != null && bounds.Length >= 2)
        {
            frame.Start = BuildFrameBound(bounds[0]);
            frame.End = BuildFrameBound(bounds[1]);
        }
        else if (bounds.Length >= 1)
        {
            frame.Start = BuildFrameBound(bounds[0]);
        }

        // EXCLUDE 子句
        if (context.EXCLUDE() != null)
        {
            if (context.GROUP() != null)
                frame.Exclude = ExcludeType.Group;
            else if (context.TIES() != null)
                frame.Exclude = ExcludeType.Ties;
            else if (context.NO() != null && context.OTHERS() != null)
                frame.Exclude = ExcludeType.NoOthers;
            else
                frame.Exclude = ExcludeType.CurrentRow;
        }

        return frame;
    }

    private FrameBound BuildFrameBound(JSqlParserGrammar.WindowFrameBoundContext context)
    {
        if (context.UNBOUNDED() != null && context.PRECEDING() != null)
            return new FrameBound(BoundType.UnboundedPreceding);
        if (context.UNBOUNDED() != null && context.FOLLOWING() != null)
            return new FrameBound(BoundType.UnboundedFollowing);
        if (context.CURRENT() != null)
            return new FrameBound(BoundType.CurrentRow);
        if (context.PRECEDING() != null)
            return new FrameBound(BoundType.Preceding)
            {
                Offset = (Expression.Expression)Visit(context.expression())
            };
        if (context.FOLLOWING() != null)
            return new FrameBound(BoundType.Following)
            {
                Offset = (Expression.Expression)Visit(context.expression())
            };
        return new FrameBound();
    }
}
