using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// ANY/SOME/ALL 比较表达式：<c>expr = ANY (subquery)</c>、<c>expr = ANY (@ids)</c>、<c>x &gt; ALL (SELECT ...)</c>。
/// 与上游 AnyComparisonExpression 对齐。
/// <para>
/// ANY 和 SOME 等价；ALL 表示所有。
/// </para>
/// </summary>
public class AnyComparisonExpression : ASTNodeAccessImpl, IExpression
{
    public AnyType AnyType { get; set; }

    /// <summary>右侧子查询；当右侧不是子查询时为 null。</summary>
    public Select? Select { get; set; }

    /// <summary>右侧数组、参数、函数或子查询表达式。</summary>
    public IExpression? RightExpression { get; set; }

    public AnyComparisonExpression() { }

    public AnyComparisonExpression(AnyType anyType, Select? select)
    {
        AnyType = anyType;
        Select = select;
        RightExpression = select;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{AnyType.ToString().ToUpperInvariant()} ({RightExpression ?? Select})";
}

/// <summary>ANY/SOME/ALL 类型。</summary>
public enum AnyType
{
    Any,
    Some,
    All
}
