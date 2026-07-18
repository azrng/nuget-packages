using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// DECLARE 语句，对齐上游 DeclareStatement。
/// 形式：<c>DECLARE @var TYPE = expr</c>（多变量）。
/// </summary>
public class DeclareStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>声明类型（TYPE / AS / TABLE），对齐上游 DeclareType，默认按变量声明处理。</summary>
    public DeclareType DeclareType { get; set; } = DeclareType.Variable;

    /// <summary>变量定义列表（每项含 UserVariable + 类型 + 默认值）。</summary>
    public List<TypeDefExpr> TypeDefExprList { get; } = new();

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("DECLARE ");
        sb.Append(string.Join(", ", TypeDefExprList));
        return sb.ToString();
    }
}

/// <summary>声明类型枚举，对齐上游 DeclareType。</summary>
public enum DeclareType { Variable, TYPE, AS, TABLE }

/// <summary>
/// DECLARE 的单个变量定义，对齐上游 TypeDefExpr 内部类。
/// </summary>
public class TypeDefExpr
{
    public string UserVariable { get; set; } = "";

    public string Type { get; set; } = "";

    /// <summary>默认值表达式，可选。</summary>
    public Expression.IExpression? DefaultExpression { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder($"{UserVariable} {Type}");
        if (DefaultExpression != null) sb.Append($" = {DefaultExpression}");
        return sb.ToString();
    }
}
