namespace Azrng.JSqlParser;

/// <summary>
/// Exception thrown by JSqlParser when SQL parsing fails.
/// </summary>
public class JSqlParserException : Exception
{
    public JSqlParserException() : base()
    {
    }

    public JSqlParserException(string message) : base(message)
    {
    }

    public JSqlParserException(string message, Exception cause) : base(message, cause)
    {
    }

    public JSqlParserException(Exception cause) : base(cause?.Message, cause)
    {
    }
}
