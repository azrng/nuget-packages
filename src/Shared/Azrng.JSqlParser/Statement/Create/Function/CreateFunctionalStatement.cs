using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Function;

/// <summary>
/// CREATE FUNCTION / CREATE PROCEDURE 抽象基类，对齐上游 CreateFunctionalStatement。
///
/// 上游对 body 采用"原始 token 列表"容器式处理（captureFunctionBody），
/// 不递归解析为 AST，仅原样保留 token 字符串列表。
/// </summary>
public abstract class CreateFunctionalStatement : ASTNodeAccessImpl, Statement
{
    /// <summary>FUNCTION 或 PROCEDURE。</summary>
    public string Kind { get; set; }

    /// <summary>是否带 OR REPLACE。</summary>
    public bool OrReplace { get; set; }

    /// <summary>函数/过程声明的 token 字符串列表（含 body 原文，对齐上游 functionDeclarationParts）。</summary>
    public List<string> FunctionDeclarationParts { get; } = new();

    protected CreateFunctionalStatement(string kind) => Kind = kind;

    protected CreateFunctionalStatement(string kind, List<string> parts) : this(kind) =>
        FunctionDeclarationParts.AddRange(parts);

    public abstract T Accept<T, S>(StatementVisitor<T> visitor, S context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE ");
        if (OrReplace) sb.Append("OR REPLACE ");
        sb.Append(Kind).Append(' ');
        // 把 token 列表拼回，token 间用空格（上游 formatDeclaration 同样行为）
        sb.Append(string.Join(" ", FunctionDeclarationParts));
        return sb.ToString();
    }
}
