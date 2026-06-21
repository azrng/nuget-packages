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
    /// <summary>
    /// 客户端对外暴露的 Arrow schema：timestamp(ns) → <see cref="TimestampType"/>（Nanosecond），
    /// 与 datetime(ms) 直连语义一致。reader 在读出 wire 的 struct 后转回 TimestampArray（见 <see cref="StructToTimestamp"/>）。
    /// </summary>
    public static Schema ToArrowSchema(TableSchema odps)
    {
        var fields = new List<Field>(odps.Columns.Count);
        foreach (var col in odps.Columns)
            // 服务端 batch 普遍带 validity buffer（列可空），强制 nullable 以对齐 buffer 布局
            fields.Add(new Field(col.Name, ToArrowType(col.Type, wire: false), true, null));
        return new Schema(fields, null);
    }

    /// <summary>
    /// wire schema：用于构造 <see cref="ArrowStreamReader"/> 的前置 schema。
    /// timestamp(ns) 映射为 struct(sec:int64, nano:int32)，对齐服务端 batch 的 buffer 布局——
    /// 否则按 TimestampType（2 buffer）解码 struct 布局（5 buffer）会 index out of range。
    /// <para>仅当存在 timestamp(ns) 列时才与对外 schema 不同；无则两者等价。</para>
    /// </summary>
    public static Schema ToWireArrowSchema(TableSchema odps)
    {
        var fields = new List<Field>(odps.Columns.Count);
        foreach (var col in odps.Columns)
            fields.Add(new Field(col.Name, ToArrowType(col.Type, wire: true), true, null));
        return new Schema(fields, null);
    }

    /// <summary>该 ODPS 列是否被服务端按 struct(sec,nano) 发送（即纳秒精度 timestamp）。</summary>
    public static bool IsStructTimestamp(string odpsType)
        => NormalizeTypeName(odpsType) is "timestamp" or "timestamp_ntz" or "timestamp_ns";

    /// <summary>
    /// 把服务端 wire 的 struct(sec:int64, nano:int32) 列转回 Arrow <see cref="TimestampArray"/>（Nanosecond）。
    /// <para>对应 PyODPS <c>_convert_struct_timestamps</c>：total_nanos = sec * 1e9 + nano（Unix epoch 起的纳秒）。</para>
    /// <para>struct 的 null bitmap 直接复用为 timestamp 的 null bitmap（null struct = null timestamp）。</para>
    /// </summary>
    public static TimestampArray StructToTimestamp(StructArray structArr, TimestampType timestampType)
    {
        var sec = (Int64Array)structArr.Fields[0];
        var nano = (Int32Array)structArr.Fields[1];
        var length = structArr.Length;
        var seconds = sec.Values;
        var nanos = nano.Values;

        var builder = new ArrowBuffer.Builder<long>(length);
        for (var i = 0; i < length; i++)
            builder.Append(seconds[i] * 1_000_000_000L + nanos[i]);

        // default(ArrowBuffer) 是 0 字节空 buffer；复用 struct 自身的 null bitmap
        var valueBuffer = length == 0 ? default : builder.Build(default);
        return new TimestampArray(timestampType, valueBuffer, structArr.NullBitmapBuffer, length, structArr.NullCount, 0);
    }

    private static IArrowType ToArrowType(string odpsType, bool wire)
    {
        var raw = odpsType.Trim();

        // decimal(p,s) → Decimal128Type（服务端以 16 字节定点发送）
        if (raw.ToLowerInvariant().StartsWith("decimal"))
        {
            var (precision, scale) = ParseDecimalPrecision(raw);
            return new Decimal128Type(precision, scale);
        }

        return NormalizeTypeName(odpsType) switch
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
            // datetime(ms) 服务端发原生 Arrow timestamp(ms)，直连可用
            "datetime" => new TimestampType(TimeUnit.Millisecond, (string?)null),
            // timestamp(ns) 服务端发 struct(sec,nano)：wire 端必须按 struct 声明才能正确解码 batch，
            // 读出后由 reader 转回 TimestampArray（对齐 PyODPS _convert_struct_timestamps）
            "timestamp" or "timestamp_ntz" or "timestamp_ns" => wire
                ? CreateStructTimestampType()
                : new TimestampType(TimeUnit.Nanosecond, (string?)null),
            "float" or "float_" => FloatType.Default,
            // 复合 / vector / interval：Arrow 端服务端用 struct/list 表示，保守回落 string
            _ => StringType.Default
        };
    }

    /// <summary>构造 wire 端 timestamp(ns) 的 struct(sec:int64, nano:int32) 类型。每次新建 Field 实例，避免跨 schema 共享。</summary>
    private static StructType CreateStructTimestampType() => new(new Field[]
    {
        new("sec", Int64Type.Default, true),
        new("nano", Int32Type.Default, true),
    });

    /// <summary>归一化 ODPS 类型名：trim + 小写 + 去掉括号参数（如 decimal(10,2) → decimal）。</summary>
    private static string NormalizeTypeName(string odpsType)
    {
        var key = odpsType.Trim().ToLowerInvariant();
        var paren = key.IndexOf('(');
        return paren > 0 ? key[..paren] : key;
    }

    private static (int precision, int scale) ParseDecimalPrecision(string odpsType)
    {
        var start = odpsType.IndexOf('(');
        var end = odpsType.IndexOf(')');
        if (start < 0 || end < 0 || end <= start)
            return (38, 18);
        var inner = odpsType[(start + 1)..end];
        var parts = inner.Split(',');
        if (parts.Length >= 2 && int.TryParse(parts[0], out var p) && int.TryParse(parts[1], out var s))
            return (p, s);
        if (parts.Length >= 1 && int.TryParse(parts[0], out p))
            return (p, 0);
        return (38, 18);
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
            Decimal128Type d => new Decimal128Array.Builder(d).Build(),
            // wire 端 timestamp(ns) 的 struct(sec,nano)：0 行空 struct，null bitmap 用空 buffer
            StructType st => new StructArray(st, 0, st.Fields.Select(f => BuildEmptyArray(f.DataType)), default, 0, 0),
            _ => new StringArray.Builder().Build()
        };
    }
}
