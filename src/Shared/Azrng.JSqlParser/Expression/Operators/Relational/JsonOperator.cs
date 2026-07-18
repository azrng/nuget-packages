using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// JSON 路径运算符。对齐上游 <c>JsonOperator(String op)</c>，符号通过构造注入。
/// 上游可承载 <c>-&gt;</c> / <c>-&gt;&gt;</c> / <c>#&gt;</c> / <c>#&gt;&gt;</c> /
/// <c>@&gt;</c> / <c>&lt;@</c> / <c>?</c> / <c>?|</c> / <c>?&amp;</c> / <c>||</c> / <c>-</c> / <c>-#</c> 等。
/// 无参构造默认 <c>-&gt;</c>（向后兼容 Azrng 既有行为）。
/// </summary>
public class JsonOperator : BinaryExpression
{
    /// <summary>JSON 运算符文本（如 "-&gt;"、"@&gt;"）。</summary>
    public string Operator { get; set; } = "->";

    public JsonOperator() { }

    public JsonOperator(string op) => Operator = op;

    /// <summary>运算符符号，取自 <see cref="Operator"/> 字段。</summary>
    public override string OperatorSymbol => Operator;

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
