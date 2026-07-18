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
/// Converts ANTLR4 parse tree to native C# AST nodes.
/// </summary>
public partial class AstBuilderVisitor : JSqlParserGrammarBaseVisitor<object>
{
    public override object VisitStatements(JSqlParserGrammar.StatementsContext context)
    {
        var statements = new Statements();
        foreach (var stmtCtx in context.statement())
        {
            var stmt = (Statement.IStatement)Visit(stmtCtx);
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
        if (context.tableStatement() != null) return Visit(context.tableStatement());
        if (context.exportStatement() != null) return Visit(context.exportStatement());
        if (context.importStatement() != null) return Visit(context.importStatement());

        return new UnsupportedStatement();
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

    private static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);

    private static int ParseInt(string s) => int.Parse(s, CultureInfo.InvariantCulture);

    private static double ParseDouble(string s) => double.Parse(s, CultureInfo.InvariantCulture);
}
