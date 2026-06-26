using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Requests;

public class OperatorDtoTests
{
    [Fact]
    public void OperatorDto_Int_DefaultValues()
    {
        var dto = new OperatorDto<int>();

        dto.UserId.Should().Be(default(int));
        dto.Account.Should().Be(string.Empty);
        dto.RealName.Should().Be(string.Empty);
        dto.NickName.Should().Be(string.Empty);
        dto.DataPermission.Should().Be(0);
        dto.TenantId.Should().Be(1);
        dto.Avatar.Should().Be(string.Empty);
    }

    [Fact]
    public void OperatorDto_String_DefaultValues()
    {
        var dto = new OperatorDto<string>();

        dto.UserId.Should().BeNull();
        dto.Account.Should().Be(string.Empty);
        dto.RealName.Should().Be(string.Empty);
        dto.NickName.Should().Be(string.Empty);
        dto.DataPermission.Should().Be(0);
        dto.TenantId.Should().Be(1);
        dto.Avatar.Should().Be(string.Empty);
    }

    [Fact]
    public void OperatorDto_CanSetAndGetUserId()
    {
        var dto = new OperatorDto<int>();
        dto.UserId = 42;

        dto.UserId.Should().Be(42);
    }

    [Fact]
    public void OperatorDto_CanSetAndGetAccount()
    {
        var dto = new OperatorDto<int>();
        dto.Account = "admin";

        dto.Account.Should().Be("admin");
    }

    [Fact]
    public void OperatorDto_CanSetAndGetRealName()
    {
        var dto = new OperatorDto<int>();
        dto.RealName = "张三";

        dto.RealName.Should().Be("张三");
    }

    [Fact]
    public void OperatorDto_CanSetAndGetNickName()
    {
        var dto = new OperatorDto<int>();
        dto.NickName = "小张";

        dto.NickName.Should().Be("小张");
    }

    [Fact]
    public void OperatorDto_CanSetAndGetDataPermission()
    {
        var dto = new OperatorDto<int>();
        dto.DataPermission = 1;

        dto.DataPermission.Should().Be(1);
    }

    [Fact]
    public void OperatorDto_CanSetAndGetTenantId()
    {
        var dto = new OperatorDto<int>();
        dto.TenantId = 100;

        dto.TenantId.Should().Be(100);
    }

    [Fact]
    public void OperatorDto_CanSetAndGetAvatar()
    {
        var dto = new OperatorDto<int>();
        dto.Avatar = "https://example.com/avatar.png";

        dto.Avatar.Should().Be("https://example.com/avatar.png");
    }

    [Fact]
    public void OperatorDto_StringUserId_CanSetAndGet()
    {
        var dto = new OperatorDto<string>();
        dto.UserId = "user-123";

        dto.UserId.Should().Be("user-123");
    }

    [Fact]
    public void OperatorDto_SetAllProperties()
    {
        var dto = new OperatorDto<int>
        {
            UserId = 1,
            Account = "testuser",
            RealName = "测试用户",
            NickName = "测试",
            DataPermission = 1,
            TenantId = 999,
            Avatar = "/img/avatar.jpg"
        };

        dto.UserId.Should().Be(1);
        dto.Account.Should().Be("testuser");
        dto.RealName.Should().Be("测试用户");
        dto.NickName.Should().Be("测试");
        dto.DataPermission.Should().Be(1);
        dto.TenantId.Should().Be(999);
        dto.Avatar.Should().Be("/img/avatar.jpg");
    }
}
