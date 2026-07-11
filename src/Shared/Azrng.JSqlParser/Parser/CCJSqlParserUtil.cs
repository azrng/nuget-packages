using Antlr4.Runtime;
using Azrng.JSqlParser.Parser.ANTLR4;

namespace Azrng.JSqlParser.Parser;

/// <summary>
/// Native SQL parser utility using ANTLR4.
/// Replaces the IKVM-based Java parser.
/// </summary>
public static class CCJSqlParserUtil
{
    /// <summary>
    /// Parse a SQL statement string into an AST.
    /// </summary>
    public static Statement.Statement? Parse(string? sql)
    {
        if (string.IsNullOrEmpty(sql)) return null;

        var (parser, errorListener) = CreateParser(sql);
        var tree = parser.statements();

        if (errorListener.Errors.Count > 0)
        {
            throw new JSqlParserException($"Syntax error: {errorListener.Errors[0]}");
        }

        Statement.Statement result;
        try
        {
            var visitor = new AstBuilderVisitor();
            result = (Statement.Statement)visitor.Visit(tree);
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
    /// Parse a SQL string into multiple statements.
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
    /// Parse a SQL expression string.
    /// </summary>
    public static Expression.Expression? ParseExpression(string? sql)
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
        return (Expression.Expression)visitor.Visit(tree);
    }

    /// <summary>
    /// Parse a SQL conditional expression (alias for ParseExpression).
    /// </summary>
    public static Expression.Expression? ParseCondExpression(string? sql)
    {
        if (string.IsNullOrEmpty(sql)) return null;
        return ParseExpression(sql);
    }

    /// <summary>
    /// Try to parse SQL, returning null only on syntax errors (<see cref="JSqlParserException"/>).
    /// 其他异常（OOM、NRE 等程序缺陷）继续上抛，不被吞没。
    /// </summary>
    public static Statement.Statement? ParseNullable(string sql)
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
