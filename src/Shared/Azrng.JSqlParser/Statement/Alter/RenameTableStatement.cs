using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Alter;

/// <summary>
/// RENAME TABLE 语句，对齐上游 RenameTableStatement。
/// 形式：<c>RENAME [TABLE] [IF EXISTS] old TO new [, old TO new]*</c>，
/// 可选 <c>WAIT n</c> 或 <c>NOWAIT</c> 指令（Oracle）。
/// </summary>
public class RenameTableStatement : ASTNodeAccessImpl, Statement
{
    /// <summary>old → new 的映射（保留插入顺序，支持多表重命名）。</summary>
    public List<KeyValuePair<Table, Table>> TableNames { get; } = new();

    /// <summary>是否使用 TABLE 关键字。</summary>
    public bool UsingTableKeyword { get; set; }

    /// <summary>是否使用 IF EXISTS 关键字。</summary>
    public bool UsingIfExistsKeyword { get; set; }

    /// <summary>WAIT/NOWAIT 指令（如 "WAIT 10" 或 "NOWAIT"），无则为空字符串。</summary>
    public string WaitDirective { get; set; } = "";

    public RenameTableStatement() { }

    public RenameTableStatement(Table oldName, Table newName) => AddTableNames(oldName, newName);

    public void AddTableNames(Table oldName, Table newName) =>
        TableNames.Add(new KeyValuePair<Table, Table>(oldName, newName));

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("RENAME");
        if (UsingTableKeyword) sb.Append(" TABLE");
        if (UsingIfExistsKeyword) sb.Append(" IF EXISTS");
        var pairs = string.Join(", ", TableNames.Select(p => $"{p.Key} TO {p.Value}"));
        sb.Append(' ').Append(pairs);
        if (!string.IsNullOrEmpty(WaitDirective)) sb.Append(' ').Append(WaitDirective);
        return sb.ToString();
    }
}
