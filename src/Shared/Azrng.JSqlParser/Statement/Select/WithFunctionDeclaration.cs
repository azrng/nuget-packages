using System.Text;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// WITH FUNCTION 声明，对齐上游 WithFunctionDeclaration。
/// 语法：<c>FUNCTION name(param1 type1, param2 type2) RETURNS returnType RETURN expression</c>
/// 用于 SQL 标准的 WITH 子句中内联函数声明。
/// </summary>
public class WithFunctionDeclaration
{
    /// <summary>函数名。</summary>
    public string? FunctionName { get; set; }

    /// <summary>参数列表（名称+类型）。</summary>
    public List<WithFunctionParameter> Parameters { get; set; } = new();

    /// <summary>返回类型。</summary>
    public string? ReturnType { get; set; }

    /// <summary>返回值表达式。</summary>
    public Expression.IExpression? ReturnExpression { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("FUNCTION ");
        sb.Append(FunctionName).Append('(');
        sb.Append(string.Join(", ", Parameters));
        sb.Append(") RETURNS ").Append(ReturnType);
        sb.Append(" RETURN ").Append(ReturnExpression);
        return sb.ToString();
    }
}

/// <summary>
/// WITH FUNCTION 的参数，对齐上游 WithFunctionParameter。
/// </summary>
public class WithFunctionParameter
{
    /// <summary>参数名。</summary>
    public string? Name { get; set; }

    /// <summary>参数类型（如 INT、VARCHAR、ARRAY&lt;INT&gt;）。</summary>
    public string? Type { get; set; }

    public override string ToString() => $"{Name} {Type}";
}
