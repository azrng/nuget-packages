using System.Text;
using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class TunnelResultMaterializerTest
{
    /// <summary>
    /// 构造一条 wire 流记录：2 列 (bigint, string)。
    /// </summary>
    private static byte[] BuildRecord(long intValue, string strValue)
    {
        var ms = new MemoryStream();

        // field1 (bigint): tag(fieldNumber=1, wire=0) + zigzag varint
        WriteTag(ms, 1, 0);
        WriteSInt64(ms, intValue);

        // field2 (string): tag(fieldNumber=2, wire=2) + len + bytes
        var strBytes = Encoding.UTF8.GetBytes(strValue);
        WriteTag(ms, 2, 2);
        WriteVarUInt32(ms, (uint)strBytes.Length);
        ms.Write(strBytes, 0, strBytes.Length);

        // 计算 CRC
        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(intValue);
        crc.UpdateInt(2);
        crc.Update(strBytes);
        var expected = crc.GetValue();

        WriteTag(ms, TunnelWireConstants.TunnelEndRecord, 0);
        WriteVarUInt32(ms, expected);

        return ms.ToArray();
    }

    private static void WriteVarUInt32(MemoryStream ms, uint value)
    {
        while (value > 0x7F)
        {
            ms.WriteByte((byte)(value | 0x80));
            value >>= 7;
        }
        ms.WriteByte((byte)value);
    }

    private static void WriteSInt64(MemoryStream ms, long value)
    {
        var zz = (ulong)((value << 1) ^ (value >> 63));
        while (zz > 0x7F)
        {
            ms.WriteByte((byte)(zz | 0x80));
            zz >>= 7;
        }
        ms.WriteByte((byte)zz);
    }

    private static void WriteTag(MemoryStream ms, int fieldNumber, int wireType)
    {
        var tag = ((ulong)fieldNumber << 3) | (uint)wireType;
        while (tag > 0x7F)
        {
            ms.WriteByte((byte)(tag | 0x80));
            tag >>= 7;
        }
        ms.WriteByte((byte)tag);
    }

    [Fact]
    public void Materialize_ReadsAllRows_WithSchemaTypes()
    {
        var stream = new MemoryStream();
        var r1 = BuildRecord(1L, "a");
        var r2 = BuildRecord(2L, "bb");
        stream.Write(r1, 0, r1.Length);
        stream.Write(r2, 0, r2.Length);
        stream.Position = 0;

        var schema = new TableSchema();
        schema.Columns.Add(new TunnelColumn { Name = "id", Type = "bigint" });
        schema.Columns.Add(new TunnelColumn { Name = "name", Type = "string" });

        var decoders = new ITypeDecoder[]
        {
            TypeDecoderFactory.GetDecoder("bigint"),
            TypeDecoderFactory.GetDecoder("string")
        };
        var reader = new TunnelRecordReader(stream, decoders);

        var result = TunnelResultMaterializer.Materialize(reader, schema);

        Assert.Equal(2, result.RowCount);
        Assert.Equal(new[] { "id", "name" }, result.Columns);
        Assert.Equal(new[] { "bigint", "string" }, result.ColumnTypes);
        Assert.Equal(1L, result.Rows[0][0]);
        Assert.Equal("a", result.Rows[0][1]);
        Assert.Equal(2L, result.Rows[1][0]);
        Assert.Equal("bb", result.Rows[1][1]);
    }
}
