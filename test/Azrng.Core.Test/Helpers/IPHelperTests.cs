using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class IPHelperTests
{
    [Theory]
    [InlineData("192.168.1.1", 3232235777)]
    [InlineData("0.0.0.0", 0)]
    [InlineData("255.255.255.255", 4294967295)]
    [InlineData("127.0.0.1", 2130706433)]
    [InlineData("10.0.0.1", 167772161)]
    [InlineData("172.16.0.1", 2886729729)]
    public void ToLongFromIp_ValidIp_ReturnsExpectedLong(string ip, long expected)
    {
        var result = IpHelper.ToLongFromIp(ip);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(3232235777, "192.168.1.1")]
    [InlineData(0, "0.0.0.0")]
    [InlineData(4294967295, "255.255.255.255")]
    [InlineData(2130706433, "127.0.0.1")]
    [InlineData(167772161, "10.0.0.1")]
    [InlineData(2886729729, "172.16.0.1")]
    public void ToIpFromLong_ValidLong_ReturnsExpectedIp(long ipLong, string expected)
    {
        var result = IpHelper.ToIpFromLong(ipLong);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    [InlineData("127.0.0.1")]
    [InlineData("10.0.0.1")]
    public void ToLongFromIp_ToIpFromLong_RoundTrip_ReturnsOriginalIp(string ip)
    {
        var longValue = IpHelper.ToLongFromIp(ip);
        var result = IpHelper.ToIpFromLong(longValue);

        result.Should().Be(ip);
    }

    [Theory]
    [InlineData(3232235777)]
    [InlineData(0)]
    [InlineData(4294967295)]
    [InlineData(2130706433)]
    public void ToIpFromLong_ToLongFromIp_RoundTrip_ReturnsOriginalLong(long ipLong)
    {
        var ip = IpHelper.ToIpFromLong(ipLong);
        var result = IpHelper.ToLongFromIp(ip);

        result.Should().Be(ipLong);
    }
}
