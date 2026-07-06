using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// ANY/SOME/ALL 比较表达式：<c>expr = ANY (subquery)</c>、<c>x &gt; ALL (SELECT ...)</c>。
/// 与上游 AnyComparisonExpression 对齐。
/// <para>
/// ANY 和 SOME 等价；ALL 表示所有。
/// </para>
/// </summary>
public class AnyComparisonExpression : ASTNodeAccessImpl, Expression
{
    public AnyType AnyType { get; set; }

    /// <summary>子查询。</summary>
    public Select? Select { get; set; }

    public AnyComparisonExpression() { }

    public AnyComparisonExpression(AnyType anyType, Select? select)
    {
        AnyType = anyType;
        Select = select;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{AnyType.ToString().ToUpperInvariant()} {Select}";
}

/// <summary>ANY/SOME/ALL 类型。</summary>
public enum AnyType
{
    Any,
    Some,
    All
}
