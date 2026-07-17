using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// PURGE 语句（Oracle），对齐上游 PurgeStatement。
/// 形式：<c>PURGE TABLE t</c> / <c>PURGE INDEX i</c> / <c>PURGE RECYCLEBIN</c> / <c>PURGE DBA_RECYCLEBIN</c> / <c>PURGE TABLESPACE ts [USER u]</c>。
/// </summary>
public class PurgeStatement : ASTNodeAccessImpl, Statement
{
    public PurgeObjectType PurgeObjectType { get; set; }

    public Table? Table { get; set; }

    public Schema.Index? Index { get; set; }

    public string? TableSpaceName { get; set; }

    public string? UserName { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("PURGE ");
        switch (PurgeObjectType)
        {
            case PurgeObjectType.RECYCLEBIN:
            case PurgeObjectType.DBA_RECYCLEBIN:
                sb.Append(PurgeObjectType);
                break;
            case PurgeObjectType.TABLE:
                sb.Append("TABLE ").Append(Table);
                break;
            case PurgeObjectType.INDEX:
                sb.Append("INDEX ").Append(Index);
                break;
            case PurgeObjectType.TABLESPACE:
                sb.Append("TABLESPACE ").Append(TableSpaceName);
                if (UserName != null) sb.Append(" USER ").Append(UserName);
                break;
        }
        return sb.ToString();
    }
}

/// <summary>PURGE 目标类型，对齐上游 PurgeObjectType。</summary>
public enum PurgeObjectType { TABLE, INDEX, RECYCLEBIN, DBA_RECYCLEBIN, TABLESPACE }
