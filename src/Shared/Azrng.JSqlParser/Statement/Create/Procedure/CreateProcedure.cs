using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Create.Function;

namespace Azrng.JSqlParser.Statement.Create.Procedure;

/// <summary>
/// CREATE PROCEDURE 语句，对齐上游 CreateProcedure。
///
/// 与 CreateFunction 一样，过程体采用原始 token 列表容器式处理。
/// </summary>
public class CreateProcedure : CreateFunctionalStatement
{
    public CreateProcedure() : base("PROCEDURE") { }
    public CreateProcedure(List<string> parts) : base("PROCEDURE", parts) { }

    public override T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
