using Antlr4.Runtime;
using Azrng.JSqlParser.Parser.ANTLR4;

namespace Azrng.JSqlParser.Parser;

/// <summary>
/// SQL 解析入口：把 SQL 文本解析为强类型 AST。
/// </summary>
/// <remarks>
/// 历史命名 <c>CCJSqlParserUtil</c> 已删除，统一使用 <see cref="SqlParser"/>。
/// </remarks>
public static class SqlParser
{
    /// <summary>
    /// 解析单条 SQL 语句为 AST。
    /// </summary>
    public static Statement.IStatement? Parse(string? sql)
    {
        if (string.IsNullOrEmpty(sql)) return null;

        var (parser, errorListener) = CreateParser(sql);
        var tree = parser.statements();

        if (errorListener.Errors.Count > 0)
        {
            throw new JSqlParserException($"Syntax error: {errorListener.Errors[0]}");
        }

        Statement.IStatement result;
        try
        {
            var visitor = new AstBuilderVisitor();
            result = (Statement.IStatement)visitor.Visit(tree);
        }
        catch (InvalidCastException ex)
        {
            // visitor 未覆盖分支时强转失败，转为统一的解析异常
            throw new JSqlParserException("Failed to build AST: unexpected node type.", ex);
        }

        if (result is Statement.Statements statements)
        {
            if (statements.StatementList.Count == 0) return null;
            if (statements.StatementList.Count > 1)
            {
                // 多语句静默丢弃会掩盖 SQL 注入，显式报错并提示用 ParseStatements
                throw new JSqlParserException(
                    "Input contains multiple statements separated by ';'. " +
                    "Use ParseStatements() to parse all of them.");
            }
            return statements.StatementList[0];
        }

        return result;
    }

    /// <summary>
    /// 解析多条 SQL 语句（分号分隔）。
    /// </summary>
    public static Statement.Statements? ParseStatements(string? sql)
    {
        if (string.IsNullOrEmpty(sql)) return null;

        var (parser, errorListener) = CreateParser(sql);
        var tree = parser.statements();

        if (errorListener.Errors.Count > 0)
        {
            throw new JSqlParserException($"Syntax error: {errorListener.Errors[0]}");
        }

        var visitor = new AstBuilderVisitor();
        return (Statement.Statements)visitor.Visit(tree);
    }

    /// <summary>
    /// 解析独立 SQL 表达式。
    /// </summary>
    public static Expression.IExpression? ParseExpression(string? sql)
    {
        if (string.IsNullOrEmpty(sql)) return null;

        var inputStream = new AntlrInputStream(sql);
        var lexer = new JSqlParserGrammarLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new JSqlParserGrammar(tokenStream);

        var errorListener = new CollectingErrorListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        var tree = parser.expressionEntry();

        if (errorListener.Errors.Count > 0)
        {
            throw new JSqlParserException($"Syntax error: {errorListener.Errors[0]}");
        }

        var visitor = new AstBuilderVisitor();
        return (Expression.IExpression)visitor.Visit(tree);
    }

    /// <summary>
    /// 解析条件表达式（<see cref="ParseExpression"/> 的别名）。
    /// </summary>
    public static Expression.IExpression? ParseCondExpression(string? sql)
    {
        if (string.IsNullOrEmpty(sql)) return null;
        return ParseExpression(sql);
    }

    /// <summary>
    /// 尝试解析 SQL，仅在语法错误（<see cref="JSqlParserException"/>）时返回 null。
    /// 其他异常（OOM、NRE 等程序缺陷）继续上抛，不被吞没。
    /// </summary>
    public static Statement.IStatement? ParseNullable(string sql)
    {
        try
        {
            return Parse(sql);
        }
        catch (JSqlParserException)
        {
            return null;
        }
    }

    private static (JSqlParserGrammar Parser, CollectingErrorListener ErrorListener) CreateParser(string sql)
    {
        var inputStream = new AntlrInputStream(sql);
        var lexer = new JSqlParserGrammarLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new JSqlParserGrammar(tokenStream);

        var errorListener = new CollectingErrorListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        return (parser, errorListener);
    }
}
