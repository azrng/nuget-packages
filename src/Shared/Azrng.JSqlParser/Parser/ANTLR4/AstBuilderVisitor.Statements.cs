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
/// AstBuilderVisitor 的 Statements 分部：事务 / 会话 / 管理 / 杂项语句（COMMIT/SET/SHOW/EXPLAIN/GRANT 等）。
/// </summary>
public partial class AstBuilderVisitor
{
    public override object VisitExportStatement(JSqlParserGrammar.ExportStatementContext context)
    {
        var export = new Statement.Export.ExportStatement();
        if (context.selectStatement() != null)
        {
            export.Select = (Select)Visit(context.selectStatement());
        }
        else if (context.table() != null)
        {
            export.Table = (Table)Visit(context.table());
            if (context.identifierList() != null)
            {
                export.Columns = context.identifierList().identifier()
                    .Select(id => new Column { ColumnName = id.GetText() }).ToList();
            }
        }
        // destination 透传文本
        if (context.exportDestination() != null)
        {
            var destCtx = context.exportDestination();
            var start = destCtx.Start;
            var stop = destCtx.Stop;
            var interval = new Antlr4.Runtime.Misc.Interval(start.StartIndex, stop.StopIndex);
            export.IntoItem = start.InputStream?.GetText(interval) ?? "";
        }
        return export;
    }

    public override object VisitImportStatement(JSqlParserGrammar.ImportStatementContext context)
    {
        var import = new Statement.Import.ImportStatement();
        if (context.table() != null)
        {
            import.Table = (Table)Visit(context.table());
            if (context.identifierList() != null)
            {
                import.Columns = context.identifierList().identifier()
                    .Select(id => new Column { ColumnName = id.GetText() }).ToList();
            }
        }
        // source 透传文本
        if (context.importSource() != null)
        {
            var srcCtx = context.importSource();
            var start = srcCtx.Start;
            var stop = srcCtx.Stop;
            var interval = new Antlr4.Runtime.Misc.Interval(start.StartIndex, stop.StopIndex);
            import.IFromItem = start.InputStream?.GetText(interval) ?? "";
        }
        return import;
    }

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
        stmt.Value = (Expression.IExpression)Visit(context.expression());
        return stmt;
    }

    public override object VisitResetStatement(JSqlParserGrammar.ResetStatementContext context)
    {
        // RESET name | RESET ALL
        if (context.ALL() != null)
            return new ResetStatement { Name = "ALL" };
        return new ResetStatement { Name = context.identifier().GetText() };
    }

    public override object VisitBeginTransactionStatement(JSqlParserGrammar.BeginTransactionStatementContext context)
    {
        return new BeginTransactionStatement
        {
            UseTransactionKeyword = context.TRANSACTION() != null
        };
    }

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
            stmt.WaitSeconds = ParseLong(context.LONG_VALUE().GetText());
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
        explain.Statement = (Statement.IStatement)Visit(context.statement());

        // PostgreSQL 语句级前缀：首位关键字为 (EXPLAIN | ANALYZE)，其后 (ANALYZE | VERBOSE)* 为修饰符。
        // context.ANALYZE() 含首位（当首位是 ANALYZE 时）+ 修饰符里的 ANALYZE，需扣掉首位。
        var analyzeCount = context.ANALYZE().Length;
        var leadingIsAnalyze = context.EXPLAIN() == null; // 首位是 ANALYZE（无 EXPLAIN）
        explain.Analyze = analyzeCount - (leadingIsAnalyze ? 1 : 0) > 0;
        explain.Verbose = context.VERBOSE().Length > 0;

        if (context.explainOptionList() is { } optList)
        {
            explain.Options = GetOriginalText(optList);
        }
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
}
