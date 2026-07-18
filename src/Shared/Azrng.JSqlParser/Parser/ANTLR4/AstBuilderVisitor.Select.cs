using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Globalization;
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
/// AstBuilderVisitor 的 Select 分部：SELECT / WITH / JOIN / FROM / 窗口 / ORDER BY / LIMIT / 管道查询 / VALUES 等查询构造。
/// </summary>
public partial class AstBuilderVisitor
{
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

        // ksqlDB EMIT CHANGES（ORDER BY 之后、LIMIT 之前）
        if (context.ksqlEmitClause() != null && select is PlainSelect emitPlainSelect)
        {
            emitPlainSelect.EmitChanges = true;
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

        // SQL Server FOR CLAUSE（FOR XML PATH/RAW/AUTO/EXPLICIT / FOR JSON / FOR BROWSE，透传保 round-trip）
        if (context.forClauseSpec() != null && select is PlainSelect forClausePlainSelect)
        {
            var forCtx = context.forClauseSpec();
            if (forCtx.BROWSE() != null)
            {
                forClausePlainSelect.ForClause = "BROWSE";
            }
            else if (forCtx.forXmlJsonSpec() != null)
            {
                // 透传 FOR XML/JSON 后的完整子句文本（含 RAW/AUTO/EXPLICIT/PATH 及选项）
                var xmlJsonCtx = forCtx.forXmlJsonSpec();
                var start = xmlJsonCtx.Start;
                var stop = xmlJsonCtx.Stop;
                var interval = new Antlr4.Runtime.Misc.Interval(start.StartIndex, stop.StopIndex);
                var forClauseText = start.InputStream?.GetText(interval) ?? "";

                // 向后兼容：FOR XML PATH 走 ForXmlPath 字段（提取括号内路径名），其他走 ForClause 透传
                if (forClauseText.StartsWith("XML PATH", StringComparison.OrdinalIgnoreCase))
                {
                    // XML PATH 后可选 ('name')，提取引号内容
                    var pathContent = forClauseText["XML PATH".Length..].Trim();
                    if (pathContent.StartsWith('(') && pathContent.EndsWith(')'))
                    {
                        forClausePlainSelect.ForXmlPath = pathContent[1..^1];
                    }
                    else
                    {
                        forClausePlainSelect.ForXmlPath = "";
                    }
                }
                else
                {
                    forClausePlainSelect.ForClause = forClauseText;
                }
            }
        }

        // DB2 WITH ISOLATION 隔离级别（保留原始大小写）
        if (context.isolationClause() != null)
        {
            select.Isolation = context.isolationClause().IDENTIFIER().GetText();
        }

        return select;
    }

    public override object VisitTableStatement(JSqlParserGrammar.TableStatementContext context)
    {
        var tableStmt = new TableStatement
        {
            Table = (Table)Visit(context.table())
        };

        if (context.orderByClause() != null)
        {
            tableStmt.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        }
        if (context.limitClause() != null)
        {
            tableStmt.Limit = (Limit)Visit(context.limitClause());
        }
        if (context.offsetClause() != null)
        {
            tableStmt.Offset = (Offset)Visit(context.offsetClause());
        }
        return tableStmt;
    }

    public override object VisitWithClause(JSqlParserGrammar.WithClauseContext context)
    {
        var items = new List<WithItem>();
        // Azrng grammar 把 RECURSIVE 放在 withClause 级别（WITH RECURSIVE a, b），
        // 而上游 jjt 放在每个 WithItem 内。对齐上游存储模型：赋给每个 WithItem.Recursive，
        // 输出时只在 WITH 关键字后输出一次 RECURSIVE（见 Select.AppendTo）。
        bool recursive = context.RECURSIVE() != null;
        foreach (var withItemCtx in context.withItem())
        {
            var item = (WithItem)Visit(withItemCtx);
            if (recursive) item.Recursive = true;
            items.Add(item);
        }
        return items;
    }

    public override object VisitWithItem(JSqlParserGrammar.WithItemContext context)
    {
        var withItem = new WithItem();

        // WITH FUNCTION 内联函数声明分支（与 CTE alias/select 互斥）
        if (context.withFunctionDeclaration() != null)
        {
            withItem.WithFunctionDeclaration =
                (WithFunctionDeclaration)Visit(context.withFunctionDeclaration());
            return withItem;
        }

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

        // 标准递归 CTE 序列化子句：SEARCH {BREADTH|DEPTH} FIRST BY cols SET seqcol
        if (context.withSearchClause() != null)
        {
            withItem.SearchClause = (WithSearchClause)Visit(context.withSearchClause());
        }

        return withItem;
    }

    public override object VisitWithSearchClause(JSqlParserGrammar.WithSearchClauseContext context)
    {
        var searchClause = new WithSearchClause
        {
            SearchOrder = context.BREADTH() != null ? SearchOrder.BREADTH : SearchOrder.DEPTH,
            SequenceColumnName = context.identifier().GetText()
        };

        foreach (var id in context.identifierList().identifier())
        {
            searchClause.SearchColumns.Add(id.GetText());
        }

        return searchClause;
    }

    public override object VisitWithFunctionDeclaration(JSqlParserGrammar.WithFunctionDeclarationContext context)
    {
        var funcDecl = new WithFunctionDeclaration
        {
            FunctionName = context.identifier().GetText(),
            ReturnType = context.dataType().GetText(),
            ReturnExpression = (Expression.IExpression)Visit(context.expression())
        };

        if (context.withFunctionParameterList() != null)
        {
            foreach (var paramCtx in context.withFunctionParameterList().withFunctionParameter())
            {
                funcDecl.Parameters.Add(new WithFunctionParameter
                {
                    Name = paramCtx.identifier().GetText(),
                    Type = paramCtx.dataType().GetText()
                });
            }
        }

        return funcDecl;
    }

    public override object VisitSelectBody(JSqlParserGrammar.SelectBodyContext context)
    {
        // VALUES 表构造器分支：VALUES (1,2),(3,4) [setOperator VALUES ...]
        var valuesClauses = context.valuesClause();
        if (valuesClauses is { Length: > 0 })
        {
            var firstValues = (Select)Visit(valuesClauses[0]);
            if (valuesClauses.Length == 1)
            {
                return firstValues;
            }

            var setOpList = new SetOperationList();
            setOpList.Selects = new List<Select> { firstValues };
            for (int i = 1; i < valuesClauses.Length; i++)
            {
                setOpList.Selects.Add((Select)Visit(valuesClauses[i]));
                setOpList.Operations.Add(CreateSetOperation(context.setOperator(i - 1)));
            }
            return setOpList;
        }

        var plainSelects = context.plainSelect();
        if (plainSelects.Length == 1)
        {
            return Visit(plainSelects[0]);
        }

        var setOpList2 = new SetOperationList();
        setOpList2.Selects = new List<Select>();

        setOpList2.Selects.Add((Select)Visit(plainSelects[0]));

        for (int i = 1; i < plainSelects.Length; i++)
        {
            setOpList2.Selects.Add((Select)Visit(plainSelects[i]));
            var setOp = context.setOperator(i - 1);
            setOpList2.Operations.Add(CreateSetOperation(setOp));
        }

        return setOpList2;
    }

    /// <summary>
    /// VALUES 表构造器：VALUES (1,2),(3,4)。复用 INSERT 的 valuesList/valuesItem 解析。
    /// </summary>
    public override object VisitValuesClause(JSqlParserGrammar.ValuesClauseContext context)
    {
        var values = new Values();
        foreach (var itemCtx in context.valuesList().valuesItem())
        {
            var exprList = new ExpressionList
            {
                Expressions = new List<Expression.IExpression>()
            };
            foreach (var exprCtx in itemCtx.expression())
            {
                exprList.Expressions.Add((Expression.IExpression)Visit(exprCtx));
            }
            values.Rows.Add(exprList);
        }
        return values;
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
            var top = new Top
            {
                HasParenthesis = topCtx.OPENING_PAREN() != null,
                Expression = topCtx.OPENING_PAREN() != null
                    ? (Expression.IExpression)Visit(topCtx.expression())
                    : new LongValue(topCtx.LONG_VALUE().GetText())
            };
            if (topCtx.PERCENT() != null) top.IsPercentage = true;
            if (topCtx.TIES() != null) top.IsWithTies = true;
            select.Top = top;
        }

        // Informix SKIP n / FIRST n 量词
        if (context.informixSkipFirstClause() != null)
        {
            var sfc = context.informixSkipFirstClause();
            if (sfc.SKIP_KW() != null && sfc.expression().Length > 0)
                select.Skip = (Expression.IExpression)Visit(sfc.expression(0));
            // FIRST 可能在 SKIP 后（expression(1)）或单独（expression(0)）
            if (sfc.FIRST() != null)
            {
                var firstExpr = sfc.SKIP_KW() != null && sfc.expression().Length > 1
                    ? sfc.expression(1) : sfc.expression(0);
                select.First = (Expression.IExpression)Visit(firstExpr);
            }
        }

        if (context.DISTINCT() != null)
        {
            var distinct = new Distinct();
            // PostgreSQL DISTINCT ON (cols)：对齐上游 PlainSelectCC.jjt:4994-4995
            if (context.distinctOnClause() != null)
            {
                distinct.OnSelectItems = context.distinctOnClause().selectColumnList().selectItem()
                    .Select(item => (SelectItem)Visit(item)).ToList();
            }
            select.Distinct = distinct;
        }
        else if (context.DISTINCTROW() != null)
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
                select.FromItem = (IFromItem)Visit(firstFrom.tableOrSubquery());
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
                        RightItem = (IFromItem)Visit(fromItems[i].tableOrSubquery())
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
            select.Where = (Expression.IExpression)Visit(context.whereClause().expression());
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
                hier.StartExpression = (Expression.IExpression)Visit(expressions[0]);
                hier.ConnectExpression = (Expression.IExpression)Visit(expressions[1]);
            }
            else
            {
                // CONNECT BY expr [START WITH expr]
                hier.ConnectFirst = true;
                hier.ConnectExpression = (Expression.IExpression)Visit(expressions[0]);
                if (expressions.Length > 1)
                    hier.StartExpression = (Expression.IExpression)Visit(expressions[1]);
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
            select.Having = (Expression.IExpression)Visit(context.havingClause().expression());
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
        else if (intoClause != null && intoClause.table() != null)
        {
            // PostgreSQL/Informix SELECT ... INTO target_table / INTO TEMP tmp / INTO UNLOGGED tmp
            // 对齐上游 PlainSelect.intoTables / intoTempTable
            var target = (Table)Visit(intoClause.table());
            if (intoClause.TEMPORARY() != null || intoClause.TEMP() != null || intoClause.UNLOGGED() != null)
            {
                select.IntoTempTable = target;
            }
            else
            {
                select.IntoTables = new List<Table> { target };
            }
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
            select.OptimizeFor = ParseLong(context.optimizeForClause().LONG_VALUE().GetText());
        }

        // WINDOW 命名窗口定义：透传 windowItem 原始文本保 round-trip（对齐上游 windowDefinitions）
        if (context.windowClause() is { } windowCtx)
        {
            select.WindowDefinitions = windowCtx.windowItem().Select(GetOriginalText).ToList();
        }

        // ksqlDB 流式窗口（FROM/JOIN 之后、WHERE 之前）：WINDOW HOPPING/TUMBLING/SESSION (...)
        if (context.ksqlWindowClause() != null)
        {
            select.KsqlWindow = (KSQLWindow)Visit(context.ksqlWindowClause().ksqlWindowSpec());
        }

        // QUALIFY 过滤表达式（Snowflake/Teradata）
        if (context.qualifyClause() is { } qualifyCtx)
        {
            select.Qualify = (Expression.IExpression)Visit(qualifyCtx.expression());
        }

        return select;
    }

    /// <summary>构建 ksqlDB 流式窗口 KSQLWindow（HOPPING/TUMBLING/SESSION）。</summary>
    public override object VisitKsqlWindowSpec(JSqlParserGrammar.KsqlWindowSpecContext context)
    {
        var window = new KSQLWindow();
        var timeUnits = context.ksqlTimeUnit();

        if (context.HOPPING() != null)
        {
            window.Hopping = true;
            window.SizeDuration = ParseLong(context.LONG_VALUE(0).GetText());
            window.SizeTimeUnit = ParseKsqlTimeUnit(timeUnits[0]);
            window.AdvanceDuration = ParseLong(context.LONG_VALUE(1).GetText());
            window.AdvanceTimeUnit = ParseKsqlTimeUnit(timeUnits[1]);
        }
        else if (context.TUMBLING() != null)
        {
            window.Tumbling = true;
            window.SizeDuration = ParseLong(context.LONG_VALUE(0).GetText());
            window.SizeTimeUnit = ParseKsqlTimeUnit(timeUnits[0]);
        }
        else if (context.SESSION() != null)
        {
            window.Session = true;
            window.SizeDuration = ParseLong(context.LONG_VALUE(0).GetText());
            window.SizeTimeUnit = ParseKsqlTimeUnit(timeUnits[0]);
        }
        return window;
    }

    /// <summary>构建 ksqlDB JOIN WITHIN 窗口 KSQLJoinWindow。</summary>
    private KSQLJoinWindow BuildKsqlJoinWindow(JSqlParserGrammar.KsqlJoinWindowClauseContext context)
    {
        var durations = context.LONG_VALUE();
        var timeUnits = context.ksqlTimeUnit();
        var joinWindow = new KSQLJoinWindow();

        if (durations.Length == 1)
        {
            // 单值窗口：WITHIN (n unit)
            joinWindow.Duration = ParseLong(durations[0].GetText());
            joinWindow.TimeUnit = ParseKsqlTimeUnit(timeUnits[0]);
        }
        else if (durations.Length >= 2)
        {
            // before/after 双值窗口：WITHIN (n unit, n unit)
            joinWindow.BeforeAfter = true;
            joinWindow.BeforeDuration = ParseLong(durations[0].GetText());
            joinWindow.BeforeTimeUnit = ParseKsqlTimeUnit(timeUnits[0]);
            joinWindow.AfterDuration = ParseLong(durations[1].GetText());
            joinWindow.AfterTimeUnit = ParseKsqlTimeUnit(timeUnits[1]);
        }
        // L6 修复：durations.Length == 0 的异常情况兜底为 null（不抛 IOORE，与其它兜底风格一致）
        return joinWindow;
    }

    /// <summary>解析 ksqlDB 时间单位文本为枚举（大写化匹配）。</summary>
    private static KSQLTimeUnit ParseKsqlTimeUnit(JSqlParserGrammar.KsqlTimeUnitContext context)
    {
        // L6 修复（Parser #14）：未知时间单位用 TryParse 兜底，避免裸 ArgumentException
        var text = context.GetText().ToUpperInvariant();
        return Enum.TryParse<KSQLTimeUnit>(text, out var unit)
            ? unit
            : throw new JSqlParserException($"Unknown ksqlDB time unit: {text}");
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
                    gb.GroupByExpressions.Add((Expression.IExpression)Visit(rollupExprs[0]));
                }
            }
            return gb;
        }

        // 普通表达式列表
        gb.GroupByExpressions = ctx.expression().Select(e => (Expression.IExpression)Visit(e)).ToList();
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
            select.Wait = new Wait { Timeout = ParseLong(context.LONG_VALUE().GetText()) };
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
            fetch.FetchExpression = (Expression.IExpression)Visit(context.expression());
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
        if (context.NO() != null) return ForMode.NoKeyUpdate;
        if (context.KEY() != null) return ForMode.KeyShare;
        if (context.READ() != null) return ForMode.ReadOnly;
        if (context.FETCH() != null) return ForMode.FetchOnly;
        if (context.UPDATE() != null) return ForMode.Update;
        return ForMode.Share;
    }

    public override object VisitSelectItem(JSqlParserGrammar.SelectItemContext context)
    {
        // PostgreSQL 行展开 (expr).* —— 用 RowGetExpression 保留外层括号保 round-trip
        if (context.OPENING_PAREN() != null && context.DOT() != null)
        {
            var inner = (Expression.IExpression)Visit(context.expression());
            return new SelectItem(new RowGetExpression(new Parenthesis { Expression = inner }, "*"));
        }

        if (context.identifier() != null && context.DOT() != null)
        {
            var allTableCols = new AllTableColumns
            {
                Table = new Table { Name = context.identifier().GetText() }
            };
            return new SelectItem(allTableCols);
        }

        if (context.MULTIPLY() != null)
        {
            return new SelectItem(new AllColumns());
        }

        var expr = (Expression.IExpression)Visit(context.expression());
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
        // Hive/Spark LATERAL VIEW [OUTER] function() AS col（接在表后，语义类似 join）
        if (context.lateralViewClause() is { } lateralViewCtx)
        {
            // GeneratorFunction 取 functionExpr 起到 lateralViewClause 结尾的整体原始文本（含 tblAlias AS col，保 round-trip）
            var fnStart = lateralViewCtx.functionExpr().Start;
            var fnStop = lateralViewCtx.Stop;
            var interval = new Antlr4.Runtime.Misc.Interval(fnStart.StartIndex, fnStop.StopIndex);
            var generatorText = fnStart.InputStream?.GetText(interval) ?? "";

            var lv = new LateralView
            {
                UsingOuter = lateralViewCtx.OUTER() != null,
                GeneratorFunction = generatorText,
            };
            // 包装为 Simple Join（RightItem = LateralView），放入 joinClause* 列表
            return new Join { Simple = true, RightItem = lv };
        }

        // RightItem 是 required，需先计算以便在初始化器中赋值
        var rightItem = (IFromItem)Visit(context.tableOrSubquery());
        var join = new Join { RightItem = rightItem };

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

        // ksqlDB WITHIN 窗口（RightItem 之后、ON/USING 之前）
        if (context.ksqlJoinWindowClause() != null)
        {
            join.JoinWindow = BuildKsqlJoinWindow(context.ksqlJoinWindowClause());
        }

        if (context.joinCondition() != null)
        {
            var cond = context.joinCondition();
            if (cond.ON().Length > 0)
            {
                // 支持 JOIN 多 ON（JOIN t ON a ON b），对齐上游 jjt:5995 ( <K_ON> expr )*。
                // 此前 grammar 只允许单个 ON，导致 OnExpressions 列表形同虚设。
                foreach (var onExpr in cond.expression())
                {
                    join.OnExpressions.Add((Expression.IExpression)Visit(onExpr));
                }
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
        // 表函数（FROM func(...) [WITH ORDINALITY] alias[(cols)]）—— 必须在 ROWS FROM 之后判定
        if (context.tableFunction().Length > 0 && context.ROWS() == null)
        {
            var fn = BuildFunctionFromTableFunction(context.tableFunction(0));
            var tableFn = new TableFunction { Function = fn };
            if (context.ORDINALITY() != null) tableFn.WithOrdinality = true;
            if (context.alias() != null)
                tableFn.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            if (context.columnList() != null)
                tableFn.ColumnAliases = context.columnList().identifier().Select(i => i.GetText()).ToList();
            return tableFn;
        }

        // PostgreSQL ROWS FROM (func(), func(), ...) AS t(a, b)
        if (context.ROWS() != null)
        {
            var rowsFrom = new RowsFrom
            {
                TableFunctions = context.tableFunction()
                    .Select(t => new TableFunction { Function = BuildFunctionFromTableFunction(t) })
                    .ToList()
            };
            if (context.alias() != null)
                rowsFrom.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            if (context.columnList() != null)
                rowsFrom.ColumnAliases = context.columnList().identifier().Select(i => i.GetText()).ToList();
            return rowsFrom;
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

        if (context.xmlTable() != null)
        {
            var xmlTable = (XmlTable)Visit(context.xmlTable());
            if (context.alias() != null)
            {
                xmlTable.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            }
            return xmlTable;
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

        // 括号 FROM 项：OPENING_PAREN fromItem CLOSING_PAREN alias?
        // 递归访问内部 fromItem，并保留可选 alias（此前兜底 GetChild(0) 丢弃 alias）
        if (context.fromItem() != null)
        {
            var inner = Visit(context.fromItem());
            if (context.alias() != null && inner is IFromItem fromItem)
            {
                fromItem.Alias = new Alias(context.alias().identifier().GetText(),
                    context.alias().AS() != null);
            }
            return inner;
        }

        return Visit(context.GetChild(0));
    }

    /// <summary>构建 TableSample（FROM 子句采样）。</summary>
    private TableSample BuildTableSample(JSqlParserGrammar.TableSampleClauseContext context)
    {
        var sample = new TableSample
        {
            SampleSize = (Expression.IExpression)Visit(context.expression())
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
            IsBefore = context.BEFORE() != null,
            Expression = (Expression.IExpression)Visit(context.expression())
        };
        if (context.TIMESTAMP() != null) clause.TravelType = "TIMESTAMP";
        else if (context.OFFSET() != null) clause.TravelType = "OFFSET";
        else if (context.STATEMENT() != null) clause.TravelType = "STATEMENT";
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
        // PIVOT [XML] (func1, func2, ... FOR cols IN (vals)) [AS alias]
        if (context.PIVOT() != null)
        {
            // 多聚合函数：对齐上游 functionItems，多函数 PIVOT (SUM(a), COUNT(b)) 全部收集
            var functions = context.functionExpr()
                .Select(f => (Function)Visit(f))
                .ToList();
            var pivot = new Pivot
            {
                Functions = functions,
                IsXml = context.XML() != null,
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

    public override object VisitSubSelect(JSqlParserGrammar.SubSelectContext context)
    {
        var select = (Select)Visit(context.selectStatement());
        var parenSelect = new ParenthesedSelect { Select = select };

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
        var expr = (Expression.IExpression)Visit(context.expression());
        string? collateName = null;

        // 兼容：当 COLLATE 由 concatenationExpr 解析后，从表达式层提取 Collate 信息
        if (expr is CollateExpression collateExpr)
        {
            collateName = collateExpr.Collate;
            expr = collateExpr.LeftExpression!;
        }

        // 兼容：当 ORDER BY 文法直接消费 COLLATE 时（向后兼容）
        if (context.COLLATE() != null)
        {
            collateName = context.S_CHAR_LITERAL()?.GetText() ?? context.QUOTED_IDENTIFIER()?.GetText();
        }

        var item = new OrderByElement
        {
            Expression = expr,
            Asc = context.DESC() == null,
            AscDescPresent = context.ASC() != null || context.DESC() != null
        };
        item.CollateName = collateName;

        // NULLS FIRST/LAST：grammar 已解析（orderByItem: ... (NULLS (FIRST|LAST))?），赋给 NullOrder。
        // 此前未读 context.NULLS()，导致 round-trip 丢失 NULLS 子句。
        if (context.NULLS() != null)
        {
            item.NullOrder = context.FIRST() != null
                ? OrderByElement.NullOrdering.NULLS_FIRST
                : OrderByElement.NullOrdering.NULLS_LAST;
        }

        // MySQL ORDER BY ... WITH ROLLUP：对齐上游 mysqlWithRollup（jjt:6382）
        if (context.WITH() != null && context.ROLLUP() != null)
        {
            item.MysqlWithRollup = true;
        }

        return item;
    }

    public override object VisitLimitClause(JSqlParserGrammar.LimitClauseContext context)
    {
        var limit = new Limit();
        var exprs = context.expression();
        if (exprs.Length == 1)
        {
            limit.RowCount = (Expression.IExpression)Visit(exprs[0]);
        }
        else if (exprs.Length == 2)
        {
            // LIMIT offset,rowCount (MySQL syntax) — swap: first is offset, second is rowCount
            limit.Offset = (Expression.IExpression)Visit(exprs[0]);
            limit.RowCount = (Expression.IExpression)Visit(exprs[1]);
        }

        // ClickHouse LIMIT n BY expr_list：对齐上游 Limit.byExpressions。
        // grammar limitClause 末尾可选 (BY expressionList)?
        if (context.BY() != null && context.expressionList() != null)
        {
            limit.ByExpressions = ((ExpressionList)Visit(context.expressionList())).Expressions;
        }

        return limit;
    }

    public override object VisitOffsetClause(JSqlParserGrammar.OffsetClauseContext context)
    {
        var offset = new Offset();
        offset.OffsetExpression = (Expression.IExpression)Visit(context.expression());
        // L2 修复：保留 ROW/ROWS 修饰符，避免 round-trip 丢失
        if (context.ROW() != null) offset.OffsetParam = "ROW";
        else if (context.ROWS() != null) offset.OffsetParam = "ROWS";
        return offset;
    }

    public override object VisitFromQuery(JSqlParserGrammar.FromQueryContext context)
    {
        var fromQuery = new FromQuery((IFromItem)Visit(context.fromItem()))
        {
            UsingFromKeyword = context.FROM() != null
        };

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
        return new WherePipeOperator
        {
            Expression = (Expression.IExpression)Visit(context.expression())
        };
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
            op.GroupBy = new List<Expression.IExpression>();
            // GROUP BY expressions are after the GROUP BY keywords
            var expressions = context.expression();
            // L7 修复：删除从未被读取的死变量 groupByCount；
            // HAVING（若存在）是最后一个 expression，循环跳过它
            for (int i = 0; i < expressions.Length; i++)
            {
                if (context.HAVING() != null && i == expressions.Length - 1)
                    break;
                op.GroupBy.Add((Expression.IExpression)Visit(expressions[i]));
            }

            if (context.HAVING() != null)
            {
                op.Having = (Expression.IExpression)Visit(expressions[expressions.Length - 1]);
            }
        }
        else if (context.HAVING() != null)
        {
            var expressions = context.expression();
            op.Having = (Expression.IExpression)Visit(expressions[expressions.Length - 1]);
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
        op.Expression = (Expression.IExpression)Visit(expressions[0]);
        if (expressions.Length > 1)
            op.Offset = (Expression.IExpression)Visit(expressions[1]);
        return op;
    }

    public override object VisitJoinPipeOp(JSqlParserGrammar.JoinPipeOpContext context)
    {
        var join = new Join { RightItem = (IFromItem)Visit(context.tableOrSubquery()) };
        if (context.joinType() != null)
            SetJoinType(join, context.joinType());
        if (context.joinCondition() != null)
        {
            var condCtx = context.joinCondition();
            if (condCtx.ON().Length > 0)
            {
                // 与 VisitJoinClause 同步：JOIN 多 ON 收集到 OnExpressions 列表
                foreach (var onExpr in condCtx.expression())
                {
                    join.OnExpressions.Add((Expression.IExpression)Visit(onExpr));
                }
            }
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
        var op = new ExtendPipeOperator
        {
            Expression = (Expression.IExpression)Visit(context.expression())
        };
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
            var expr = (Expression.IExpression)Visit(itemCtx.expression());
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

        op.InExpressions = new List<Expression.IExpression>();
        if (context.expressionList() != null)
        {
            foreach (var exprCtx in context.expressionList().expression())
                op.InExpressions.Add((Expression.IExpression)Visit(exprCtx));
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

        op.InExpressions = new List<Expression.IExpression>();
        if (context.expressionList() != null)
        {
            foreach (var exprCtx in context.expressionList().expression())
                op.InExpressions.Add((Expression.IExpression)Visit(exprCtx));
        }
        return op;
    }

    public override object VisitTableSamplePipeOp(JSqlParserGrammar.TableSamplePipeOpContext context)
    {
        return new TableSamplePipeOperator
        {
            SampleSize = (Expression.IExpression)Visit(context.expression())
        };
    }

    public override object VisitWindowPipeOp(JSqlParserGrammar.WindowPipeOpContext context)
    {
        return new WindowPipeOperator
        {
            WindowName = context.identifier().GetText(),
            // Window expression is the full specification
            WindowExpression = new Column { ColumnName = context.windowSpecification().GetText() }
        };
    }

    public override object VisitSetOperationPipeOp(JSqlParserGrammar.SetOperationPipeOpContext context)
    {
        var op = new SetOperationPipeOperator();
        var setOp = CreateSetOperation(context.setOperator());
        op.OperationType = setOp.Type;
        op.All = setOp.All;
        return op;
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
                Offset = (Expression.IExpression)Visit(context.expression())
            };
        if (context.FOLLOWING() != null)
            return new FrameBound(BoundType.Following)
            {
                Offset = (Expression.IExpression)Visit(context.expression())
            };
        return new FrameBound();
    }

    // SQL 数值字面量按规范永远用 . 小数点、无千分位，必须用 InvariantCulture，
    // 否则在 de-DE 等区域下 SELECT 1.5 会被静默解析成 15（数据损坏）。
}
