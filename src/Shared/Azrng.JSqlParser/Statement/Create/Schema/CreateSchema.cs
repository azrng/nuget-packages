using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Schema;

/// <summary>
/// CREATE SCHEMA 语句。
/// <para>语法：CREATE SCHEMA [IF NOT EXISTS] [catalog.]schemaName [AUTHORIZATION auth]</para>
/// 移植自上游 JSqlParser 的 CreateSchema（commit ac46c434 含 catalog 支持）。
/// 简化版不含 schemaPath 和内嵌 statements。
/// </summary>
public class CreateSchema : ASTNodeAccessImpl, Statement
{
    /// <summary>IF NOT EXISTS 修饰符。</summary>
    public bool IfNotExists { get; set; }

    /// <summary>目录名（catalog.schema 形式时的 catalog 部分），可空。</summary>
    public string? CatalogName { get; set; }

    /// <summary>模式名。</summary>
    public string? SchemaName { get; set; }

    /// <summary>AUTHORIZATION 所有者名，可空。</summary>
    public string? Authorization { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE SCHEMA");
        if (IfNotExists) sb.Append(" IF NOT EXISTS");
        if (SchemaName != null)
        {
            sb.Append(' ');
            if (CatalogName != null) sb.Append(CatalogName).Append('.');
            sb.Append(SchemaName);
        }
        if (Authorization != null) sb.Append(" AUTHORIZATION ").Append(Authorization);
        return sb.ToString();
    }
}
