namespace Azrng.JSqlParser.Statement.Piped;

public interface FromQueryVisitor<T, S>
{
    T Visit(FromQuery fromQuery, S context);
}
