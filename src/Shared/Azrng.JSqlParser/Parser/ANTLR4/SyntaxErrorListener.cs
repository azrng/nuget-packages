using Antlr4.Runtime;

namespace Azrng.JSqlParser.Parser.ANTLR4;

public class SyntaxError
{
    public int Line { get; }
    public int Column { get; }
    public string Message { get; }
    public IToken? OffendingToken { get; }

    public SyntaxError(int line, int column, string message, IToken? offendingToken)
    {
        Line = line;
        Column = column;
        Message = message;
        OffendingToken = offendingToken;
    }

    public override string ToString()
    {
        // L4 改进：附 offending token 文本，便于调用方定位
        var token = OffendingToken != null ? $" near '{OffendingToken.Text}'" : "";
        return $"Line {Line}:{Column}{token} - {Message}";
    }
}

public class CollectingErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
{
    public List<SyntaxError> Errors { get; } = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
        int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Errors.Add(new SyntaxError(line, charPositionInLine, msg, offendingSymbol));
    }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol,
        int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Errors.Add(new SyntaxError(line, charPositionInLine, msg, null));
    }
}
