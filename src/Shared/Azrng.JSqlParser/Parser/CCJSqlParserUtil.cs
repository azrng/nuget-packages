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

        var visitor = new AstBuilderVisitor();
        var result = visitor.Visit(tree);

        if (result is Statement.Statements statements && statements.StatementList.Count > 0)
        {
            return statements.StatementList[0];
        }

        return (Statement.Statement)result;
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
    /// Try to parse SQL, returning null on failure.
    /// </summary>
    public static Statement.Statement? ParseNullable(string sql)
    {
        try
        {
            return Parse(sql);
        }
        catch
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
