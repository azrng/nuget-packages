namespace Azrng.JSqlParser.Parser;

/// <summary>
/// Represents a token from the SQL lexer/parser.
/// This is a placeholder for the JavaCC-generated Token.
/// </summary>
public class Token
{
    public int Kind { get; set; }
    public string Image { get; set; } = "";
    public int BeginLine { get; set; }
    public int BeginColumn { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public int AbsoluteEnd { get; set; }
    public Token? Next { get; set; }
    public Token? SpecialToken { get; set; }
}
