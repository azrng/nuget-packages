namespace Azrng.JSqlParser.Util.Validation;

/// <summary>
/// Defines feature capabilities that can be allowed or disallowed in SQL validation.
/// </summary>
public class FeaturesAllowed
{
    public static readonly FeaturesAllowed SELECT = new(nameof(SELECT));
    public static readonly FeaturesAllowed INSERT = new(nameof(INSERT));
    public static readonly FeaturesAllowed UPDATE = new(nameof(UPDATE));
    public static readonly FeaturesAllowed DELETE = new(nameof(DELETE));
    public static readonly FeaturesAllowed CREATE = new(nameof(CREATE));
    public static readonly FeaturesAllowed ALTER = new(nameof(ALTER));
    public static readonly FeaturesAllowed DROP = new(nameof(DROP));
    public static readonly FeaturesAllowed MERGE = new(nameof(MERGE));
    public static readonly FeaturesAllowed TRUNCATE = new(nameof(TRUNCATE));
    public static readonly FeaturesAllowed JOIN = new(nameof(JOIN));
    public static readonly FeaturesAllowed SUBQUERY = new(nameof(SUBQUERY));
    public static readonly FeaturesAllowed UNION = new(nameof(UNION));
    public static readonly FeaturesAllowed EXCEPT = new(nameof(EXCEPT));
    public static readonly FeaturesAllowed INTERSECT = new(nameof(INTERSECT));

    public string Name { get; }

    private FeaturesAllowed(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj) => obj is FeaturesAllowed other && Name == other.Name;

    public override int GetHashCode() => Name.GetHashCode();
}
