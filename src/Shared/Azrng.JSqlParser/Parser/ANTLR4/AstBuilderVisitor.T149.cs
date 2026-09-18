using Antlr4.Runtime.Tree;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Create.Domain;
using Azrng.JSqlParser.Statement.Create.Event;
using Azrng.JSqlParser.Statement.Create.Extension;
using Azrng.JSqlParser.Statement.Create.Macro;
using Azrng.JSqlParser.Statement.Create.Publication;
using Azrng.JSqlParser.Statement.Create.Role;
using Azrng.JSqlParser.Statement.Create.Subscription;
using Azrng.JSqlParser.Statement.Create.Trigger;
using Azrng.JSqlParser.Statement.DuckDb;
using Azrng.JSqlParser.Statement.Export;
using Azrng.JSqlParser.Statement.Select;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Parser.ANTLR4;

/// <summary>
/// T149 批次 visitor 接线：上游 5.4 第二/三档（PG/MySQL DDL 族、ClickHouse、BigQuery、DuckDB、三元、MATCH_RECOGNIZE）。
/// </summary>
public partial class AstBuilderVisitor
{
    // ─── 批次A：PG/MySQL DDL 族 ───

    public override object VisitCreateRoleStatement(JSqlParserGrammar.CreateRoleStatementContext context)
    {
        var cmd = context.USER() != null ? "USER" : context.ROLE() != null ? "ROLE" : "GROUP";
        var role = new CreateRole
        {
            Command = cmd,
            IfNotExists = context.IF() != null,
            Name = context.userAccount().accountPart(0).GetText(),
            Host = context.userAccount().accountPart().Length > 1 ? context.userAccount().accountPart(1).GetText() : null
        };
        // 属性尾全透传（MySQL IDENTIFIED BY 无 WITH 前缀；PG WITH LOGIN 含 WITH）
        if (context.statementTail() != null)
            role.OptionsText = GetOriginalText(context.statementTail());
        return role;
    }

    public override object VisitCreateDomainStatement(JSqlParserGrammar.CreateDomainStatementContext context)
    {
        var domain = new CreateDomain
        {
            IfNotExists = context.IF() != null,
            Name = context.domainName().GetText()
        };
        if (context.dataType() != null)
        {
            domain.UseAs = context.AS() != null;
            domain.DataType = GetOriginalText(context.dataType());
        }
        if (context.domainTail() != null)
            domain.Tail = GetOriginalText(context.domainTail());
        return domain;
    }

    public override object VisitCreateExtensionStatement(JSqlParserGrammar.CreateExtensionStatementContext context)
    {
        var ext = new CreateExtension
        {
            IfNotExists = context.IF() != null,
            Name = context.identifier().GetText()
        };
        if (context.extensionOption() is { Length: > 0 } opts)
        {
            ext.Options = opts.Select(o => new ExtensionOption
            {
                Kind = o.SCHEMA() != null ? ExtensionOptionKind.Schema
                     : o.VERSION() != null ? ExtensionOptionKind.Version
                     : ExtensionOptionKind.Cascade,
                Value = o.SCHEMA() != null ? o.identifier().GetText()
                      : o.VERSION() != null ? (o.S_CHAR_LITERAL() != null ? o.S_CHAR_LITERAL().GetText() : o.identifier().GetText())
                      : null
            }).ToList();
        }
        return ext;
    }

    public override object VisitCreatePublicationStatement(JSqlParserGrammar.CreatePublicationStatementContext context)
    {
        var pub = new CreatePublication { Name = context.identifier().GetText() };
        if (context.ALL() != null)
        {
            pub.ForAllTables = true;
        }
        else if (context.tableList() != null)
        {
            pub.Tables = context.tableList().table().Select(t => (Table)Visit(t)).ToList();
        }
        if (context.withTail() != null)
        {
            var tail = GetOriginalText(context.withTail());
            pub.OptionsText = tail.StartsWith("WITH ") ? tail[5..] : tail;
        }
        return pub;
    }

    public override object VisitCreateSubscriptionStatement(JSqlParserGrammar.CreateSubscriptionStatementContext context)
    {
        var sub = new CreateSubscription
        {
            Name = context.identifier().GetText(),
            Connection = context.S_CHAR_LITERAL().GetText(),
            Publications = context.identifierList().identifier().Select(i => i.GetText()).ToList()
        };
        if (context.withTail() != null)
        {
            var tail = GetOriginalText(context.withTail());
            sub.OptionsText = tail.StartsWith("WITH ") ? tail[5..] : tail;
        }
        return sub;
    }

    public override object VisitCreateTriggerStatement(JSqlParserGrammar.CreateTriggerStatementContext context)
    {
        var trigger = new CreateTrigger
        {
            Definer = context.userAccount() != null ? context.userAccount().GetText() : null,
            Trigger = (Table)Visit(context.table(0)),
            Timing = context.BEFORE() != null ? TriggerTiming.Before : TriggerTiming.After,
            Event = context.INSERT() != null ? TriggerEvent.Insert
                  : context.UPDATE() != null ? TriggerEvent.Update : TriggerEvent.Delete,
            Table = (Table)Visit(context.table(1))
        };
        if (context.FOLLOWS() != null || context.PRECEDES() != null)
        {
            trigger.Order = new TriggerOrder
            {
                Follows = context.FOLLOWS() != null,
                OtherTrigger = (Table)Visit(context.table(2))
            };
        }
        var bodyCtx = context.triggerBody();
        if (bodyCtx.blockStatement() != null)
            trigger.Body = (IStatement)Visit(bodyCtx.blockStatement());
        else if (bodyCtx.triggerSimpleStatement() != null)
            trigger.Body = (IStatement)Visit(bodyCtx.triggerSimpleStatement().GetChild(0));
        else if (bodyCtx.statementTail() != null)
            trigger.BodyText = GetOriginalText(bodyCtx.statementTail());
        return trigger;
    }

    public override object VisitCreateEventStatement(JSqlParserGrammar.CreateEventStatementContext context)
    {
        var ev = new CreateEvent
        {
            IfNotExists = context.IF() != null,
            Event = (Table)Visit(context.table())
        };
        if (context.eventScheduleClause() != null)
        {
            // 原文含 "ON SCHEDULE" 前缀，模型只存调度本体（渲染时补回）
            var scheduleText = GetOriginalText(context.eventScheduleClause());
            ev.ScheduleText = scheduleText.StartsWith("ON SCHEDULE ")
                ? scheduleText["ON SCHEDULE ".Length..] : scheduleText;
        }
        if (context.COMPLETION() != null)
        {
            // NOT 是数组（IF NOT EXISTS 与 ON COMPLETION [NOT] 两处），扣掉 IF NOT EXISTS 的一个
            var completionNot = context.NOT().Length - (context.IF() != null ? 1 : 0);
            ev.OnCompletionPreserve = completionNot == 0;
        }
        if (context.ENABLE() != null)
            ev.Enabled = true;
        else if (context.DISABLE() != null)
        {
            ev.Enabled = false;
            ev.DisableOnSlave = context.SLAVE() != null;
        }
        if (context.COMMENT() != null)
            ev.Comment = context.S_CHAR_LITERAL().GetText();
        ev.Body = (IStatement)Visit(context.statement());
        return ev;
    }

    public override object VisitDoStatement(JSqlParserGrammar.DoStatementContext context)
    {
        // 按子节点顺序扫描：LANGUAGE 与代码块字面量的相对位置决定前后置；MySQL DO expr 走 Text 兜底
        string? language = null, code = null;
        int langIndex = -1, codeIndex = -1;
        bool pendingLanguage = false;
        for (int i = 0; i < context.ChildCount; i++)
        {
            // identifier 是 parser 规则节点（包裹 terminal），需单独识别
            if (pendingLanguage && context.GetChild(i) is JSqlParserGrammar.IdentifierContext idCtx)
            {
                language = idCtx.GetText();
                pendingLanguage = false;
                continue;
            }
            if (context.GetChild(i) is Antlr4.Runtime.Tree.ITerminalNode t)
            {
                var type = t.Symbol.Type;
                if (type == JSqlParserGrammarLexer.LANGUAGE) { langIndex = i; pendingLanguage = true; }
                else if (type == JSqlParserGrammarLexer.S_CHAR_LITERAL
                         || type == JSqlParserGrammarLexer.S_DOLLAR_QUOTED_STRING)
                { code = t.GetText(); codeIndex = i; pendingLanguage = false; }
            }
        }

        if (code != null)
        {
            return new DoStatement
            {
                Language = language,
                Code = code,
                LanguageBeforeCode = langIndex >= 0 && langIndex < codeIndex
            };
        }
        return new DoStatement
        {
            Text = context.statementTail() != null ? GetOriginalText(context.statementTail()) : null
        };
    }

    // ─── 批次C：BigQuery ───

    public override object VisitExportDataStatement(JSqlParserGrammar.ExportDataStatementContext context)
    {
        var export = new ExportData { UseAs = context.AS() != null };
        // DATA 之后到查询之前的片段（'uri' / OPTIONS(...) / WITH CONNECTION ...）整段透传保 round-trip
        var dataStop = context.DATA().Symbol.StopIndex;
        var queryStart = context.selectStatement().Start.StartIndex;
        var interval = new Antlr4.Runtime.Misc.Interval(dataStop + 1, queryStart - 1);
        var specText = context.Start.InputStream?.GetText(interval)?.Trim();
        if (!string.IsNullOrEmpty(specText))
        {
            if (specText.EndsWith("AS"))
                specText = specText[..^2].Trim();
            export.Specs.Add(specText!);
        }
        export.Select = (Select)Visit(context.selectStatement());
        return export;
    }

    public override object VisitLoadDataStatement(JSqlParserGrammar.LoadDataStatementContext context)
    {
        return new LoadDataStatement
        {
            Overwrite = context.OVERWRITE() != null,
            Text = context.statementTail() != null ? GetOriginalText(context.statementTail()) : null
        };
    }

    public override object VisitAssertStatement(JSqlParserGrammar.AssertStatementContext context)
    {
        return new AssertStatement
        {
            Condition = (IExpression)Visit(context.expression()),
            Message = context.S_CHAR_LITERAL() != null ? context.S_CHAR_LITERAL().GetText() : null
        };
    }

    public override object VisitAssertFunction(JSqlParserGrammar.AssertFunctionContext context)
    {
        var fn = new Function { Name = "ASSERT" };
        if (context.expressionList() != null)
        {
            var list = new ExpressionList
            {
                Expressions = context.expressionList().expression()
                    .Select(e => (IExpression)Visit(e)).ToList()
            };
            fn.Parameters = list;
        }
        else if (context.MULTIPLY() != null)
        {
            fn.AllColumns = true;
        }
        return fn;
    }

    // ─── 批次D：DuckDB ───

    public override object VisitCopyStatement(JSqlParserGrammar.CopyStatementContext context)
    {
        var sourceText = context.table() != null
            ? context.table().GetText()
            : GetOriginalText(context.selectStatement());
        return new CopyStatement
        {
            Source = context.OPENING_PAREN() != null ? $"({sourceText})" : sourceText,
            To = context.TO() != null,
            Tail = context.statementTail() != null ? GetOriginalText(context.statementTail()) : null
        };
    }

    public override object VisitAttachStatement(JSqlParserGrammar.AttachStatementContext context)
        => new AttachStatement { Text = context.statementTail() != null ? GetOriginalText(context.statementTail()) : null };

    public override object VisitPragmaStatement(JSqlParserGrammar.PragmaStatementContext context)
        => new PragmaStatement { Text = context.statementTail() != null ? GetOriginalText(context.statementTail()) : null };

    public override object VisitCreateMacroStatement(JSqlParserGrammar.CreateMacroStatementContext context)
        => new CreateMacro { Text = context.statementTail() != null ? GetOriginalText(context.statementTail()) : null };

    // ─── 批次B：ClickHouse ───

    public override object VisitArrayJoinItem(JSqlParserGrammar.ArrayJoinItemContext context)
    {
        var item = new ArrayJoinItem
        {
            Expression = (IExpression)Visit(context.expression())
        };
        if (context.identifier() != null)
            item.Alias = new Alias(context.identifier().GetText(), context.AS() != null);
        return item;
    }

    public override object VisitWithFillClause(JSqlParserGrammar.WithFillClauseContext context)
    {
        var fill = new WithFillClause();
        var exprs = context.expression();
        if (exprs.Length > 0) fill.From = (IExpression)Visit(exprs[0]);
        if (exprs.Length > 1) fill.To = (IExpression)Visit(exprs[1]);
        if (exprs.Length > 2) fill.Step = (IExpression)Visit(exprs[2]);
        if (exprs.Length > 3) fill.Staleness = (IExpression)Visit(exprs[3]);
        return fill;
    }

    public override object VisitInterpolateClause(JSqlParserGrammar.InterpolateClauseContext context)
    {
        return context.interpolateElement().Select(e => (InterpolateElement)Visit(e)).ToList();
    }

    public override object VisitInterpolateElement(JSqlParserGrammar.InterpolateElementContext context)
    {
        var element = new InterpolateElement { ColumnName = context.identifier().GetText() };
        if (context.expression() != null)
            element.Expression = (IExpression)Visit(context.expression());
        return element;
    }

    public override object VisitColumnsTransformer(JSqlParserGrammar.ColumnsTransformerContext context)
    {
        // 单个变换器的中间结构：VisitSelectItem 中按 token 组装（此处仅满足生成访问器）
        return VisitChildren(context);
    }

    // ─── 批次E：三元表达式 ───

    public override object VisitTernaryExpr(JSqlParserGrammar.TernaryExprContext context)
    {
        var cond = (IExpression)Visit(context.orExpression());
        if (context.QUESTION_MARK() == null)
            return cond;
        var thenExpr = (IExpression)Visit(context.ternaryExpr(0));
        var elseExpr = (IExpression)Visit(context.ternaryExpr(1));
        return new TernaryExpression
        {
            Condition = cond,
            ThenExpression = thenExpr,
            ElseExpression = elseExpr
        };
    }

    // ─── 批次F：MATCH_RECOGNIZE ───

    public override object VisitMatchRecognize(JSqlParserGrammar.MatchRecognizeContext context)
    {
        var mr = new MatchRecognize();
        if (context.PARTITION() != null)
        {
            // expression() 数组混有 DEFINE 的条件，仅取 PARTITION BY 与下一个子句关键字之间的表达式
            var partitionExprs = new List<IExpression>();
            var children = context.children;
            var inPartition = false;
            foreach (var node in children)
            {
                if (node is Antlr4.Runtime.Tree.ITerminalNode tn)
                {
                    var type = tn.Symbol.Type;
                    if (type == JSqlParserGrammarLexer.PARTITION) inPartition = true;
                    else if (inPartition && type != JSqlParserGrammarLexer.BY
                             && type != JSqlParserGrammarLexer.COMMA) inPartition = false;
                }
                else if (inPartition && node is JSqlParserGrammar.ExpressionContext exprCtx)
                {
                    partitionExprs.Add((IExpression)Visit(exprCtx));
                }
            }
            mr.PartitionKeys = partitionExprs;
        }
        if (context.orderByClause() != null)
            mr.OrderByElements = (List<OrderByElement>)Visit(context.orderByClause());
        if (context.MEASURES() != null)
            mr.Measures = context.selectColumnList().selectItem()
                .Select(i => (SelectItem)Visit(i)).ToList();
        if (context.ONE() != null)
        {
            mr.RowsPerMatch = true;
            mr.PerMatch = context.PER() != null;
        }
        else if (context.ALL() != null)
        {
            mr.RowsPerMatch = false;
            mr.PerMatch = context.PER() != null;
        }
        if (context.matchRecognizeSkip() != null)
        {
            mr.Skip = BuildMatchRecognizeSkip(context.matchRecognizeSkip());
            mr.Skip.SkipKeyword = context.SKIP_KW() != null;
        }
        if (context.PATTERN_KW() != null)
            mr.PatternText = GetOriginalText(context.patternExpression());
        if (context.subsetDefinition() is { Length: > 0 } subsets)
            mr.Subsets = subsets.Select(BuildSubsetDefinition).ToList();
        if (context.DEFINE() != null)
            mr.Defines = BuildDefines(context);
        // identifier/AS 因 DEFINE 多次出现：尾部的 identifier 是别名，AS 计数多一即别名带 AS
        if (context.identifier().Length > (mr.Defines?.Count ?? 0))
        {
            var aliasHasAs = context.AS().Length > (mr.Defines?.Count ?? 0);
            mr.Alias = new Alias(context.identifier()[^1].GetText(), aliasHasAs);
        }
        return mr;
    }

    /// <summary>DEFINE 变量 AS 条件 的配对构建（变量与 expression 子节点按文法顺序配对）。</summary>
    private List<MatchDefine> BuildDefines(JSqlParserGrammar.MatchRecognizeContext context)
    {
        var defines = new List<MatchDefine>();
        var children = context.children.OfType<Antlr4.Runtime.Tree.IParseTree>().ToList();
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is ITerminalNode tn && tn.Symbol.Type == JSqlParserGrammarLexer.DEFINE)
            {
                // DEFINE var AS expr (, var AS expr)* —— 按子节点顺序配对
                var j = i + 1;
                while (j + 2 < children.Count
                       && children[j] is JSqlParserGrammar.IdentifierContext idCtx
                       && children[j + 1] is ITerminalNode asNode && asNode.Symbol.Type == JSqlParserGrammarLexer.AS
                       && children[j + 2] is JSqlParserGrammar.ExpressionContext exprCtx)
                {
                    defines.Add(new MatchDefine
                    {
                        Variable = idCtx.GetText(),
                        Expression = (IExpression)Visit(exprCtx)
                    });
                    j += 3;
                    // 逗号分隔符跳过
                    if (j < children.Count && children[j] is ITerminalNode comma
                        && comma.Symbol.Type == JSqlParserGrammarLexer.COMMA)
                        j += 1;
                }
                break;
            }
        }
        return defines;
    }

    private AfterMatchSkip BuildMatchRecognizeSkip(JSqlParserGrammar.MatchRecognizeSkipContext context)
    {
        var skip = new AfterMatchSkip();
        if (context.PAST() != null) skip.Mode = SkipMode.PastLastRow;
        else if (context.NEXT() != null && context.ROW() != null && context.TO() == null) skip.Mode = SkipMode.NextRow;
        else if (context.TO() != null) skip.Mode = context.FIRST() != null ? SkipMode.ToFirst : SkipMode.ToLast;
        else if (context.FIRST() != null) skip.Mode = SkipMode.First;
        else skip.Mode = SkipMode.Last;
        if (context.identifier() != null) skip.Variable = context.identifier().GetText();
        return skip;
    }

    private SubsetDefinition BuildSubsetDefinition(JSqlParserGrammar.SubsetDefinitionContext context)
        => new()
        {
            Name = context.identifier().GetText(),
            Variables = context.identifierList().identifier().Select(i => i.GetText()).ToList()
        };
}
