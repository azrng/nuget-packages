using JSqlParser.Net.Expression;
using JSqlParser.Net.Schema;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Interface for items that can appear in a FROM clause (tables, subqueries, etc.).
/// </summary>
public interface FromItem
{
    Alias? GetAlias();
    void SetAlias(Alias alias);
}
