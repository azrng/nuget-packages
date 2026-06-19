using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Tunnel.Types;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// 把 <see cref="TunnelRecordReader"/> 流物化成 <see cref="QueryResult"/>。
/// <para>
/// 适合小批量数据；大批量应直接消费 <see cref="TunnelRecordReader"/>。
/// S1 阶段为打通 DataReader 接入而提供。
/// </para>
/// </summary>
public static class TunnelResultMaterializer
{
    /// <summary>
    /// 同步读取流到内存。
    /// </summary>
    /// <param name="reader">已构造好的 TunnelRecordReader</param>
    /// <param name="schema">列 schema（决定列名/类型/解码器）</param>
    public static QueryResult Materialize(TunnelRecordReader reader, TableSchema schema)
    {
        if (reader == null) throw new ArgumentNullException(nameof(reader));
        if (schema == null) throw new ArgumentNullException(nameof(schema));

        var columns = new string[schema.Columns.Count];
        var columnTypes = new string[schema.Columns.Count];
        for (var i = 0; i < schema.Columns.Count; i++)
        {
            columns[i] = schema.Columns[i].Name;
            columnTypes[i] = schema.Columns[i].Type;
        }

        var rows = new List<object[]>();
        while (true)
        {
            var row = reader.Read();
            if (row == null)
                break;
            // object?[] → object[]（DBNull 替代 null 以兼容 ADO.NET）
            var materialized = new object[row.Length];
            for (var i = 0; i < row.Length; i++)
                materialized[i] = row[i] ?? DBNull.Value;
            rows.Add(materialized);
        }

        return new QueryResult
        {
            Columns = columns,
            ColumnTypes = columnTypes,
            Rows = rows.ToArray(),
            RowCount = rows.Count
        };
    }
}
