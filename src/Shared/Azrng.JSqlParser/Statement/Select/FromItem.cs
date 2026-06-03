using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Interface for items that can appear in a FROM clause (tables, subqueries, etc.).
/// </summary>
public interface FromItem
{
    Alias? GetAlias();
    void SetAlias(Alias alias);
}
