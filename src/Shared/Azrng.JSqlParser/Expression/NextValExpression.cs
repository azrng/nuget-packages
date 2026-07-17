using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 序列取值表达式。与上游 NextValExpression 对齐。
/// <para>
/// 支持两种形式：
/// <list type="bullet">
/// <item><c>NEXTVAL FOR seq</c>（IBM DB2 / Apache Derby / PostgreSQL）</item>
/// <item><c>NEXT VALUE FOR seq</c>（SQL Server / SQL:2008）</item>
/// </list>
/// </para>
/// </summary>
public class NextValExpression : ASTNodeAccessImpl, Expression
{
    /// <summary>序列名分段列表（如 schema.seq -> ["schema", "seq"]）。</summary>
    public List<string> NameList { get; set; } = new();

    /// <summary>true 表示输出 NEXT VALUE FOR，false 表示 NEXTVAL FOR。</summary>
    public bool UsingNextValueFor { get; set; }

    public NextValExpression() { }

    public NextValExpression(List<string> nameList, bool usingNextValueFor)
    {
        NameList = nameList;
        UsingNextValueFor = usingNextValueFor;
    }

    /// <summary>序列全名（如 schema.seq）。</summary>
    public string Name => string.Join(".", NameList);

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
        => $"{(UsingNextValueFor ? "NEXT VALUE FOR " : "NEXTVAL FOR ")}{Name}";
}
