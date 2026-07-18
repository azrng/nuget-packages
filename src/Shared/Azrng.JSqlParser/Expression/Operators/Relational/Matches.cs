using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// PostgreSQL 全文匹配运算符 <c>@@</c>，对齐上游 JSqlParser 的 Matches。
/// </summary>
/// <remarks>
/// <b>已知遗留</b>：当前 grammar 未定义 <c>@@</c> token，AstBuilderVisitor 也不构建本类实例，
/// 故 <c>@@</c> 运算符目前不可解析。本类仅修正符号定义（曾误写为 <c>~</c>），
/// 保留类型骨架以对齐上游模型，待后续补 grammar 时激活。
/// </remarks>
public class Matches : BinaryExpression
{
    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    /// <summary>全文匹配符号 @@（曾误写为 ~，已修正）。</summary>
    public override string OperatorSymbol => "@@";
}
