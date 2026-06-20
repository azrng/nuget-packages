using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Types;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// 写侧 round-trip：TunnelRecordWriter 编码 → TunnelRecordReader 解码 → 断言相等。
/// 不依赖集群，证明 encoder 与既有 decoder 严格互逆（含 record CRC + 块尾部）。
/// </summary>
public class TunnelRecordWriterRoundTripTest
{
    private static TableSchema Schema(params (string name, string type)[] cols)
    {
        var s = new TableSchema();
        foreach (var (name, type) in cols)
            s.Columns.Add(new TunnelColumn { Name = name, Type = type });
        return s;
    }

    private static List<object?[]?> RoundTrip(TableSchema schema, params object?[][] rows)
    {
        var writer = new TunnelRecordWriter(schema);
        foreach (var row in rows)
            writer.Write(row);
        var block = writer.ToBlockBytes();

        var decoders = schema.Columns.Select(c => TypeDecoderFactory.GetDecoder(c.Type)).ToArray();
        using var reader = new TunnelRecordReader(new MemoryStream(block), decoders);

        var result = new List<object?[]?>();
        object?[]? r;
        while ((r = reader.Read()) != null)
            result.Add(r);
        return result;
    }

    [Fact]
    public void ScalarColumns_RoundTrip()
    {
        var schema = Schema(("a", "bigint"), ("b", "double"), ("c", "string"), ("d", "boolean"), ("e", "decimal(10,2)"));
        var rows = new[]
        {
            new object?[] { 1L, 1.5, "x", true, 9.99m },
            new object?[] { 2L, 2.5, "yy", false, 0.01m }
        };
        var got = RoundTrip(schema, rows);
        Assert.Equal(2, got.Count);
        Assert.Equal(1L, got[0]![0]);
        Assert.Equal(1.5, got[0]![1]);
        Assert.Equal("x", got[0]![2]);
        Assert.Equal(true, got[0]![3]);
        Assert.Equal(9.99m, got[0]![4]);
        Assert.Equal("yy", got[1]![2]);
    }

    [Fact]
    public void NullFields_Omitted_RoundTrip()
    {
        var schema = Schema(("a", "bigint"), ("b", "string"));
        var got = RoundTrip(schema, new object?[] { 1L, null });
        Assert.Equal(1L, got[0]![0]);
        Assert.Null(got[0]![1]);
    }

    [Fact]
    public void ArrayColumn_RoundTrip()
    {
        var schema = Schema(("a", "array<bigint>"));
        var got = RoundTrip(schema, new object?[] { new List<object?> { 1L, 2L, 3L } });
        var arr = Assert.IsAssignableFrom<System.Collections.IList>(got[0]![0]);
        Assert.Equal(new object?[] { 1L, 2L, 3L }, arr.Cast<object?>().ToArray());
    }

    [Fact]
    public void StructColumn_RoundTrip()
    {
        var schema = Schema(("s", "struct<a:bigint,b:string>"));
        var got = RoundTrip(schema, new object?[] { new object?[] { 1L, "x" } });
        var arr = Assert.IsType<object[]>(got[0]![0]);
        Assert.Equal(new object?[] { 1L, "x" }, arr);
    }

    [Fact]
    public void MapColumn_RoundTrip()
    {
        var schema = Schema(("m", "map<string,bigint>"));
        var dict = new Dictionary<object, object?> { ["a"] = 1L, ["b"] = 2L };
        var got = RoundTrip(schema, new object?[] { dict });
        var d = Assert.IsAssignableFrom<System.Collections.IDictionary>(got[0]![0]);
        Assert.Equal(2, d.Count);
    }

    [Fact]
    public void MultipleRowsAndTypes_RoundTrip()
    {
        var schema = Schema(("id", "bigint"), ("name", "string"), ("tags", "array<string>"));
        var rows = new[]
        {
            new object?[] { 1L, "alice", new List<object?> { "a", "b" } },
            new object?[] { 2L, null, new List<object?> { "c" } }
        };
        var got = RoundTrip(schema, rows);
        Assert.Equal(2, got.Count);
        Assert.Equal("alice", got[0]![1]);
        Assert.Null(got[1]![1]);
        var tags0 = (System.Collections.IList)got[0]![2]!;
        Assert.Equal(2, tags0.Count);
    }

    [Fact]
    public void EmptyBlock_RoundTrip()
    {
        var schema = Schema(("a", "bigint"));
        var writer = new TunnelRecordWriter(schema);
        var block = writer.ToBlockBytes();
        Assert.Equal(0, writer.Count);

        var decoders = schema.Columns.Select(c => TypeDecoderFactory.GetDecoder(c.Type)).ToArray();
        using var reader = new TunnelRecordReader(new MemoryStream(block), decoders);
        Assert.Null(reader.Read());   // 无记录
    }
}
