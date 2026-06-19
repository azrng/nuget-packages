using Azrng.NMaxCompute.Rest;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class OdpsErrorTest
{
    [Fact]
    public void Parse_XmlError()
    {
        var body = @"<?xml version=""1.0""?>
<Error>
  <Code>ODPS-0130131</Code>
  <Message>Table not found</Message>
  <RequestId>abc-123</RequestId>
  <HostId>odps.aliyun.com</HostId>
</Error>";

        var err = OdpsError.TryParse(400, body);

        Assert.NotNull(err);
        Assert.Equal("ODPS-0130131", err!.Code);
        Assert.Contains("Table not found", err.Message);
        Assert.Equal("abc-123", err.RequestId);
    }

    [Fact]
    public void Parse_PlainText_OdpsPrefixed()
    {
        var err = OdpsError.TryParse(403, "ODPS-0410051:Invalid signature");

        Assert.NotNull(err);
        Assert.Equal("ODPS-0410051", err!.Code);
    }

    [Fact]
    public void Parse_Unrecognized_ReturnsNull()
    {
        var err = OdpsError.TryParse(500, "random garbage");
        Assert.Null(err);
    }

    [Fact]
    public void Parse_EmptyBody_ReturnsNull()
    {
        var err = OdpsError.TryParse(500, "");
        Assert.Null(err);
    }
}
