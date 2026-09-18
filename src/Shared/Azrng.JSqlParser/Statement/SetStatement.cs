using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class SetStatement : ASTNodeAccessImpl, IStatement
{
    public string Name { get; set; } = "";
    public Expression.IExpression? Value { get; set; }

    /// <summary>
    /// SQL Server <c>SET IDENTITY_INSERT t ON|OFF</c> 的结构化表目标（#2605）。
    /// 非 null 时本语句为 IDENTITY_INSERT 形式，<see cref="Name"/> 保留首标识符原文（通常为 IDENTITY_INSERT）。
    /// </summary>
    public Schema.Table? IdentityInsertTable { get; set; }

    /// <summary>
    /// 布尔开关值（#2604 <c>SET NOCOUNT ON</c> / #2605 <c>... OFF</c>）：true=ON，false=OFF；
    /// 赋值形式（SET x = y）为 null。
    /// </summary>
    public bool? SwitchValue { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (IdentityInsertTable != null)
            return $"SET {Name} {IdentityInsertTable} {(SwitchValue == true ? "ON" : "OFF")}";
        if (Value == null && SwitchValue != null)
            return $"SET {Name} {(SwitchValue == true ? "ON" : "OFF")}";
        return Value != null ? $"SET {Name} = {Value}" : $"SET {Name}";
    }
}
