using System.Text.Json;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// Tunnel 返回的列定义
/// <para>对应 PyODPS <c>odps/models/table.py::TableSchema.TableColumn</c>。</para>
/// </summary>
public sealed class TunnelColumn
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// MaxCompute 类型字符串（如 <c>bigint</c> / <c>array&lt;string&gt;</c>）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public bool IsNullable { get; set; } = true;
}

/// <summary>
/// 简化版 Tunnel schema：只关心列名与类型字符串。
/// </summary>
public sealed class TableSchema
{
    public List<TunnelColumn> Columns { get; set; } = new();

    /// <summary>
    /// 从 Tunnel 返回的 <c>{"columns": [...]}</c> JSON 解析。
    /// </summary>
    public static TableSchema Parse(string json)
    {
        var schema = new TableSchema();
        if (string.IsNullOrWhiteSpace(json))
            return schema;

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("columns", out var cols))
            return schema;

        foreach (var col in cols.EnumerateArray())
        {
            var column = new TunnelColumn();
            if (col.TryGetProperty("name", out var nameEl))
                column.Name = nameEl.GetString() ?? string.Empty;
            if (col.TryGetProperty("type", out var typeEl))
                column.Type = typeEl.GetString() ?? string.Empty;
            if (col.TryGetProperty("comment", out var commentEl))
                column.Comment = commentEl.GetString();
            if (col.TryGetProperty("isNullable", out var nullableEl) && nullableEl.ValueKind == JsonValueKind.False)
                column.IsNullable = false;
            schema.Columns.Add(column);
        }

        return schema;
    }
}
