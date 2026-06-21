using Apache.Arrow;
using Apache.Arrow.Arrays;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using Azrng.NMaxCompute.Tunnel;
using Xunit;

namespace Azrng.NMaxCompute.Arrow.Test;

/// <summary>
/// ODPS→Arrow schema 转换 + timestamp(ns) 的 struct(sec,nano) wire 解码与转回 TimestampArray 的单测。
/// 对应 PyODPS <c>_convert_struct_timestamps</c>：MaxCompute 服务端把纳秒 timestamp 按 struct 发送。
/// </summary>
public class OdpsArrowSchemaConverterTest
{
    private static TableSchema Schema(params (string name, string type)[] cols)
    {
        var s = new TableSchema();
        foreach (var (name, type) in cols)
            s.Columns.Add(new TunnelColumn { Name = name, Type = type });
        return s;
    }

    [Fact]
    public void PublicSchema_Timestamp_IsTimestampNanosecond_Datetime_IsMillisecond()
    {
        var pub = OdpsArrowSchemaConverter.ToArrowSchema(Schema(
            ("t", "timestamp"), ("d", "datetime"), ("i", "bigint")));

        Assert.True(pub.FieldsList[0].DataType is TimestampType t0 && t0.Unit == TimeUnit.Nanosecond);
        Assert.True(pub.FieldsList[1].DataType is TimestampType t1 && t1.Unit == TimeUnit.Millisecond);
        Assert.IsType<Int64Type>(pub.FieldsList[2].DataType);
    }

    [Fact]
    public void WireSchema_Timestamp_IsStructSecNano_Datetime_Unchanged()
    {
        var wire = OdpsArrowSchemaConverter.ToWireArrowSchema(Schema(
            ("t", "timestamp"), ("d", "datetime")));

        // timestamp(ns) → struct(sec:int64, nano:int32)：对齐服务端 batch 布局
        var st = Assert.IsType<StructType>(wire.FieldsList[0].DataType);
        Assert.Equal(2, st.Fields.Count);
        Assert.Equal("sec", st.Fields[0].Name);
        Assert.IsType<Int64Type>(st.Fields[0].DataType);
        Assert.Equal("nano", st.Fields[1].Name);
        Assert.IsType<Int32Type>(st.Fields[1].DataType);

        // datetime(ms) wire 端仍是原生 timestamp(ms)（直连可用）
        Assert.True(wire.FieldsList[1].DataType is TimestampType dt && dt.Unit == TimeUnit.Millisecond);
    }

    [Theory]
    [InlineData("timestamp", true)]
    [InlineData("TIMESTAMP_NTZ", true)]
    [InlineData("timestamp_ns", true)]
    [InlineData("datetime", false)]
    [InlineData("bigint", false)]
    [InlineData("decimal(10,2)", false)]
    public void IsStructTimestamp_RecognizesVariants(string type, bool expected)
        => Assert.Equal(expected, OdpsArrowSchemaConverter.IsStructTimestamp(type));

    [Fact]
    public void StructToTimestamp_ConvertsSecNanoToNanos()
    {
        // sec=[0,1,2], nano=[0, 500_000_000, 999_999_999] → total nanos = sec*1e9 + nano
        var sec = new Int64Array.Builder().AppendRange(new long[] { 0, 1, 2 }).Build();
        var nano = new Int32Array.Builder().AppendRange(new int[] { 0, 500_000_000, 999_999_999 }).Build();
        var st = new StructType(new Field[]
        {
            new("sec", Int64Type.Default, true),
            new("nano", Int32Type.Default, true),
        });
        var structArr = new StructArray(st, 3, new IArrowArray[] { sec, nano }, default, 0, 0);

        var ts = OdpsArrowSchemaConverter.StructToTimestamp(structArr, new TimestampType(TimeUnit.Nanosecond, (string?)null));

        Assert.Equal(
            new long[] { 0L, 1_500_000_000L, 2_999_999_999L },
            ts.Values.ToArray());
    }

    [Fact]
    public void StructToTimestamp_PreservesNullBitmap()
    {
        // 3 行，第 1 行为 null（struct 级 null bitmap：bit0=1,bit1=0,bit2=1 → 0b101 = 5）
        var sec = new Int64Array.Builder().AppendRange(new long[] { 10, 20, 30 }).Build();
        var nano = new Int32Array.Builder().AppendRange(new int[] { 0, 0, 0 }).Build();
        var st = new StructType(new Field[]
        {
            new("sec", Int64Type.Default, true),
            new("nano", Int32Type.Default, true),
        });
        var structArr = new StructArray(st, 3, new IArrowArray[] { sec, nano }, new ArrowBuffer(new byte[] { 5 }), 1, 0);

        var ts = OdpsArrowSchemaConverter.StructToTimestamp(structArr, new TimestampType(TimeUnit.Nanosecond, (string?)null));

        Assert.Equal(1, ts.NullCount);
        Assert.False(ts.IsNull(0));
        Assert.True(ts.IsNull(1));   // struct null bitmap 透传
        Assert.False(ts.IsNull(2));
        Assert.Equal(10_000_000_000L, ts.Values[0]);
        Assert.Equal(30_000_000_000L, ts.Values[2]);
    }

    /// <summary>
    /// 端到端（离线等价验证）：构造真实 Arrow IPC struct(sec,nano) batch → ArrowStreamReader 解码 →
    /// StructToTimestamp 转回 TimestampArray。验证 struct wire 布局可被 Apache.Arrow 正确解码且转换无误。
    /// </summary>
    [Fact]
    public void EndToEnd_StructIpcBatch_ConvertsToTimestamp()
    {
        var wireSchema = new Schema(new[]
        {
            new Field("t", new StructType(new Field[]
            {
                new("sec", Int64Type.Default, true),
                new("nano", Int32Type.Default, true),
            }), true, null)
        }, null);

        var sec = new Int64Array.Builder().AppendRange(new long[] { 1, 2 }).Build();
        var nano = new Int32Array.Builder().AppendRange(new int[] { 500_000_000, 0 }).Build();
        var structType = (StructType)wireSchema.FieldsList[0].DataType;
        var structArr = new StructArray(structType, 2, new IArrowArray[] { sec, nano }, default, 0, 0);
        var batch = new RecordBatch(wireSchema, new IArrowArray[] { structArr }, 2);

        var ms = new MemoryStream();
        using (var writer = new ArrowStreamWriter(ms, wireSchema, leaveOpen: true))
            writer.WriteRecordBatch(batch);

        ms.Position = 0;
        var ipcReader = new ArrowStreamReader(ms);
        var readBatch = ipcReader.ReadNextRecordBatch();
        Assert.NotNull(readBatch);

        var decodedStruct = Assert.IsType<StructArray>(readBatch!.Column(0));
        var ts = OdpsArrowSchemaConverter.StructToTimestamp(decodedStruct, new TimestampType(TimeUnit.Nanosecond, (string?)null));

        Assert.Equal(new long[] { 1_500_000_000L, 2_000_000_000L }, ts.Values.ToArray());
    }
}
