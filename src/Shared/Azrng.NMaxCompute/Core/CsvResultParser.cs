using System.Text;
using Azrng.NMaxCompute.Models;

namespace Azrng.NMaxCompute.Core;

/// <summary>
/// 解析 ODPS Result API 返回的 CSV 文本为 <see cref="QueryResult"/>
/// <para>
/// S0 MVP 路径专用：所有列均按字符串解析，类型识别延后到 S1 切换到 Instance Tunnel。
/// </para>
/// </summary>
public static class CsvResultParser
{
    /// <summary>
    /// 将 CSV 文本解析为 QueryResult
    /// </summary>
    public static QueryResult Parse(string csvContent, int maxRows = int.MaxValue)
    {
        if (string.IsNullOrEmpty(csvContent))
            return new QueryResult();

        var lines = SplitLines(csvContent);
        if (lines.Count == 0)
            return new QueryResult();

        var header = ParseCsvLine(lines[0]);
        var columns = header.ToArray();

        var rows = new List<object[]>(Math.Max(0, lines.Count - 1));
        for (var i = 1; i < lines.Count && rows.Count < maxRows; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;
            var fields = ParseCsvLine(lines[i]);
            var row = new object[columns.Length];
            for (var j = 0; j < columns.Length; j++)
            {
                row[j] = j < fields.Count ? fields[j] : null;
            }
            rows.Add(row);
        }

        return new QueryResult
        {
            Columns = columns,
            Rows = rows.ToArray(),
            RowCount = rows.Count
        };
    }

    private static List<string> SplitLines(string content)
    {
        var result = new List<string>();
        var sb = new StringBuilder();

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];
            if (c == '\r')
            {
                if (i + 1 < content.Length && content[i + 1] == '\n')
                    i++;
                result.Add(sb.ToString());
                sb.Clear();
            }
            else if (c == '\n')
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
            result.Add(sb.ToString());

        return result;
    }

    /// <summary>
    /// 解析单行 CSV（RFC 4180 风格：双引号包裹，引号用两个引号转义）
    /// </summary>
    public static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuote = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuote)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuote = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuote = true;
                }
                else if (c == ',')
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}
