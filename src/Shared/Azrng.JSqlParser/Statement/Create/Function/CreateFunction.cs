using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Function;

/// <summary>
/// CREATE FUNCTION 语句，对齐上游 CreateFunction。
///
/// 注意：上游对函数体采用"原始 token 列表"容器式处理（captureFunctionBody），
/// 不递归解析 body 为 AST。本类对齐该行为——functionDeclarationParts 存 token 字符串列表。
/// </summary>
public class CreateFunction : CreateFunctionalStatement
{
    public CreateFunction() : base("FUNCTION") { }
    public CreateFunction(List<string> parts) : base("FUNCTION", parts) { }

    public override T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
