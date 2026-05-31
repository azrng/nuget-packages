namespace JSqlParser.Net.Statement.Piped;

public interface FromQueryVisitor<T, S>
{
    T Visit(FromQuery fromQuery, S context);
}
