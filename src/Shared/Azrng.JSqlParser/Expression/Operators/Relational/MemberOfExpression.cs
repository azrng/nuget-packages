using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class MemberOfExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression LeftExpression { get; set; } = null!;
    public IExpression RightExpression { get; set; } = null!;

    /// <summary>是否为 NOT MEMBER OF（对齐上游 isNot 字段）。</summary>
    public bool Not { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression}{(Not ? " NOT" : "")} MEMBER OF {RightExpression}";
}
