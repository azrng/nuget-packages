using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Oracle 命名函数参数：<c>name =&gt; expression</c>。
/// 例如：<c>FUNC(arg1 =&gt; value1, arg2 =&gt; value2)</c>。
/// 对应上游 OracleNamedFunctionParameter。
/// </summary>
public class OracleNamedFunctionParameter : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public Expression? Expression { get; set; }

    public OracleNamedFunctionParameter() { }

    public OracleNamedFunctionParameter(string name, Expression? expression)
    {
        Name = name;
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{Name} => {Expression}";
}

/// <summary>
/// PostgreSQL 命名函数参数：<c>name := expression</c>。
/// 例如：<c>FUNC(arg1 := value1, arg2 := value2)</c>。
/// 对应上游 PostgresNamedFunctionParameter。
/// </summary>
public class PostgresNamedFunctionParameter : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public Expression? Expression { get; set; }

    public PostgresNamedFunctionParameter() { }

    public PostgresNamedFunctionParameter(string name, Expression? expression)
    {
        Name = name;
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{Name} := {Expression}";
}
