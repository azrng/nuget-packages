using Apache.Arrow;
using Apache.Arrow.Arrays;
using Apache.Arrow.Types;
using Azrng.NMaxCompute.Tunnel;

namespace Azrng.NMaxCompute.Arrow;

/// <summary>
/// ODPS schema → Apache.Arrow schema。对应 PyODPS <c>odps_schema_to_arrow_schema</c> / <c>odps_type_to_arrow_type</c>。
/// MaxCompute arrow 流不含 schema 消息，客户端须据此转换后前置。
/// </summary>
internal static class OdpsArrowSchemaConverter
{
    public static Schema ToArrowSchema(TableSchema odps)
    {
        var fields = new List<Field>(odps.Columns.Count);
        foreach (var col in odps.Columns)
            // 服务端 batch 普遍带 validity buffer（列可空），强制 nullable 以对齐 buffer 布局
            fields.Add(new Field(col.Name, ToArrowType(col.Type), true, null));
        return new Schema(fields, null);
    }

    private static IArrowType ToArrowType(string odpsType)
    {
        var key = odpsType.Trim().ToLowerInvariant();
        var paren = key.IndexOf('(');
        if (paren > 0) key = key[..paren];

        return key switch
        {
            "tinyint" => Int8Type.Default,
            "smallint" => Int16Type.Default,
            "int" or "int_" or "integer" => Int32Type.Default,
            "bigint" or "long" => Int64Type.Default,
            "boolean" or "bool" => BooleanType.Default,
            "double" => DoubleType.Default,
            "string" or "varchar" or "char" => StringType.Default,
            "binary" => BinaryType.Default,
            "date" => Date32Type.Default,
            "datetime" => new TimestampType(TimeUnit.Millisecond, (string?)null),
            "timestamp" or "timestamp_ntz" => new TimestampType(TimeUnit.Nanosecond, (string?)null),
            "float" or "float_" => FloatType.Default,
            // 复合 / decimal / vector / interval：Arrow 端服务端用 struct/list 表示，
            // 此处保守回落到 string；如需精确映射按 PyODPS odps_type_to_arrow_type 扩展。
            _ => StringType.Default
        };
    }

    /// <summary>
    /// 构造一个 0 行的空 RecordBatch，列与 schema 一致。
    /// 仅用于触发 ArrowStreamWriter 写出 schema IPC 消息（writer 不提供 schema-only 写法）。
    /// </summary>
    public static RecordBatch BuildEmptyRecordBatch(Schema schema)
    {
        var arrays = schema.FieldsList.Select(f => BuildEmptyArray(f.DataType)).Cast<IArrowArray>().ToArray();
        return new RecordBatch(schema, arrays, 0);
    }

    private static IArrowArray BuildEmptyArray(IArrowType type)
    {
        return type switch
        {
            Int64Type => new Int64Array.Builder().Build(),
            Int32Type => new Int32Array.Builder().Build(),
            Int16Type => new Int16Array.Builder().Build(),
            Int8Type => new Int8Array.Builder().Build(),
            BooleanType => new BooleanArray.Builder().Build(),
            DoubleType => new DoubleArray.Builder().Build(),
            StringType => new StringArray.Builder().Build(),
            _ => new StringArray.Builder().Build()
        };
    }
}
