using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Azrng.NMaxCompute.Core;

/// <summary>
/// 构造 ODPS Job XML（提交 SQL 任务用）
/// <para>对应 PyODPS <c>Instance.AnonymousSubmitInstance.serialize()</c>。</para>
/// </summary>
public static class JobXmlBuilder
{
    public const string DefaultTaskName = "AnonymousSQLTask";

    /// <summary>
    /// 构造完整 Job XML
    /// </summary>
    public static string Build(string sql, SqlHints? hints = null, int priority = 9, string? taskName = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL is required", nameof(sql));

        var name = taskName ?? DefaultTaskName;
        var settingsJson = BuildSettingsJson(hints);
        var cdataQuery = FormatCData(sql);

        var sb = new StringBuilder(512);
        sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.Append("<Instance>");
        sb.Append("<Job>");
        sb.Append("<Priority>").Append(priority.ToString(CultureInfo.InvariantCulture)).Append("</Priority>");
        sb.Append("<Tasks>");
        sb.Append("<SQL>");
        sb.Append("<Name>").Append(EscapeXml(name)).Append("</Name>");
        sb.Append("<Query>").Append(cdataQuery).Append("</Query>");
        sb.Append("<Config>");
        sb.Append("<Property>");
        sb.Append("<Name>settings</Name>");
        sb.Append("<Value>").Append(EscapeXml(settingsJson)).Append("</Value>");
        sb.Append("</Property>");
        sb.Append("</Config>");
        sb.Append("</SQL>");
        sb.Append("</Tasks>");
        sb.Append("</Job>");
        sb.Append("</Instance>");

        return sb.ToString();
    }

    /// <summary>
    /// 合并默认 settings（odps.sql.udf.strict.mode）+ 用户 hints，序列化为 JSON 字符串
    /// </summary>
    private static string BuildSettingsJson(SqlHints? hints)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
        writer.WriteStartObject();
        writer.WriteString("odps.sql.udf.strict.mode", "true");

        if (hints != null)
        {
            foreach (var (key, value) in hints)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                writer.WriteString(key, value ?? string.Empty);
            }
        }

        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// 与 PyODPS <c>format_cdata</c> 一致：strip + 末尾补 ; + 包 CDATA
    /// </summary>
    public static string FormatCData(string sql)
    {
        var stripped = sql.Trim();
        if (!stripped.EndsWith(';'))
            stripped += ";";
        return $"<![CDATA[{stripped}]]>";
    }

    private static string EscapeXml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            switch (c)
            {
                case '<': sb.Append("&lt;"); break;
                case '>': sb.Append("&gt;"); break;
                case '&': sb.Append("&amp;"); break;
                case '"': sb.Append("&quot;"); break;
                case '\'': sb.Append("&apos;"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
}
