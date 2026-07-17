using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement;

public class GrantStatement : ASTNodeAccessImpl, Statement
{
    public List<string> Privileges { get; set; } = new();
    public Table? Table { get; set; }
    public string Grantee { get; set; } = "";
    public bool WithGrantOption { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var privileges = Privileges.Count > 0 ? string.Join(", ", Privileges) : "";
        var grantOption = WithGrantOption ? " WITH GRANT OPTION" : "";
        return $"GRANT {privileges} ON {Table} TO {Grantee}{grantOption}";
    }
}
