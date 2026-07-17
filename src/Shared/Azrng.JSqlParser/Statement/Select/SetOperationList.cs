using System.Text;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a list of SELECT statements combined with UNION, INTERSECT, or EXCEPT.
/// </summary>
public class SetOperationList : Select
{
    public List<Select> Selects { get; set; } = new();
    public List<SetOperation> Operations { get; set; } = new();

    public override T Accept<T, S>(ISelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        for (int i = 0; i < Selects.Count; i++)
        {
            if (i > 0) builder.Append(' ').Append(Operations[i - 1]).Append(' ');
            builder.Append(Selects[i]);
        }
        return builder;
    }
}

/// <summary>
/// Represents a set operation (UNION, INTERSECT, EXCEPT, MINUS).
/// </summary>
public class SetOperation
{
    public enum OperationType
    {
        UNION,
        INTERSECT,
        EXCEPT,
        MINUS
    }

    public OperationType Type { get; set; }
    public bool All { get; set; }
    public bool Distinct { get; set; }

    /// <summary>SQL:2016 CORRESPONDING 修饰符（按匹配列名做集合操作）。</summary>
    public bool Corresponding { get; set; }

    public SetOperation() { }

    public SetOperation(OperationType type, bool all = false, bool distinct = false, bool corresponding = false)
    {
        Type = type;
        All = all;
        Distinct = distinct;
        Corresponding = corresponding;
    }

    public override string ToString()
    {
        var modifier = All ? " ALL" : (Distinct ? " DISTINCT" : "");
        var corresponding = Corresponding ? " CORRESPONDING" : "";
        var op = Type == OperationType.MINUS ? "MINUS" : Type.ToString();
        return op + modifier + corresponding;
    }
}
