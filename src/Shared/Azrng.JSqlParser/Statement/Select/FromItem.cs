using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Interface for items that can appear in a FROM clause (tables, subqueries, etc.).
/// </summary>
public interface FromItem
{
    /// <summary>FROM 项的别名（AS alias_name），未指定时为 null。</summary>
    Alias? Alias { get; set; }
}
