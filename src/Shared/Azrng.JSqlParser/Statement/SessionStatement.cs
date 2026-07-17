using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// Represents SQL session management commands: SESSION START/APPLY/DROP/SHOW/DESCRIBE.
/// </summary>
public class SessionStatement : ASTNodeAccessImpl, Statement
{
    public enum Action
    {
        START,
        APPLY,
        DROP,
        SHOW,
        DESCRIBE
    }

    public Action SessionAction { get; }
    public string? Id { get; }
    public Dictionary<string, string> Options { get; } = new(StringComparer.OrdinalIgnoreCase);

    public SessionStatement(Action action, string? id = null)
    {
        SessionAction = action;
        Id = id;
    }

    public SessionStatement(string action, string? id = null)
        : this(Enum.Parse<Action>(action, ignoreCase: true), id) { }

    public void PutOption(string key, string value)
    {
        Options[key.Replace("\"", "").Replace("'", "").ToLowerInvariant()] = value.ToLowerInvariant();
    }

    public bool HasOptions() => Options.Count > 0;
    public bool HasOption(string key) => Options.ContainsKey(key);
    public string? GetOption(string key) => Options.GetValueOrDefault(key);

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder($"SESSION {SessionAction} {Id}");
        if (Options.Count > 0)
        {
            sb.Append(" WITH ");
            int i = 0;
            foreach (var kvp in Options)
            {
                if (i++ > 0) sb.Append(", ");
                sb.Append(kvp.Key).Append('=').Append(kvp.Value);
            }
        }
        sb.Append(';');
        return sb.ToString();
    }
}
