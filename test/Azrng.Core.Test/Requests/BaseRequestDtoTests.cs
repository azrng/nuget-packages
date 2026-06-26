using Azrng.Core.Requests;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Requests;

public class BaseRequestDtoTests
{
    [Fact]
    public void BaseRequestDto_T_TO_DefaultValues()
    {
        var dto = new BaseRequestDto<string, int>();

        dto.Data.Should().BeNull();
        dto.UserIdentity.Should().BeNull();
    }

    [Fact]
    public void BaseRequestDto_T_TO_CanSetAndGetData()
    {
        var dto = new BaseRequestDto<string, int>();
        dto.Data = "test-data";

        dto.Data.Should().Be("test-data");
    }

    [Fact]
    public void BaseRequestDto_T_TO_CanSetAndGetUserIdentity()
    {
        var operatorDto = new OperatorDto<int> { UserId = 42, Account = "admin" };
        var dto = new BaseRequestDto<string, int>();
        dto.UserIdentity = operatorDto;

        dto.UserIdentity.Should().BeSameAs(operatorDto);
        dto.UserIdentity.UserId.Should().Be(42);
        dto.UserIdentity.Account.Should().Be("admin");
    }

    [Fact]
    public void BaseRequestDto_T_DefaultValues()
    {
        var dto = new BaseRequestDto<string>();

        dto.Data.Should().BeNull();
        dto.UserIdentity.Should().BeNull();
    }

    [Fact]
    public void BaseRequestDto_T_CanSetAndGetData()
    {
        var dto = new BaseRequestDto<int>();
        dto.Data = 100;

        dto.Data.Should().Be(100);
    }

    [Fact]
    public void BaseRequestDto_T_CanSetAndGetUserIdentity()
    {
        var operatorDto = new OperatorDto<string> { UserId = "user-1", Account = "test" };
        var dto = new BaseRequestDto<string>();
        dto.UserIdentity = operatorDto;

        dto.UserIdentity.Should().BeSameAs(operatorDto);
        dto.UserIdentity.UserId.Should().Be("user-1");
        dto.UserIdentity.Account.Should().Be("test");
    }

    [Fact]
    public void BaseRequestDto_T_UserIdentityIsOperatorDtoString()
    {
        var dto = new BaseRequestDto<string>();
        var operatorDto = new OperatorDto<string>();
        dto.UserIdentity = operatorDto;

        dto.UserIdentity.Should().BeOfType<OperatorDto<string>>();
    }

    [Fact]
    public void BaseCustomerRequestDto_DefaultValues()
    {
        var dto = new BaseCustomerRequestDto<string, TestOperator>();

        dto.Data.Should().BeNull();
        dto.UserIdentity.Should().NotBeNull();
        dto.UserIdentity.Should().BeOfType<TestOperator>();
    }

    [Fact]
    public void BaseCustomerRequestDto_CanSetAndGetData()
    {
        var dto = new BaseCustomerRequestDto<string, TestOperator>();
        dto.Data = "customer-data";

        dto.Data.Should().Be("customer-data");
    }

    [Fact]
    public void BaseCustomerRequestDto_CanSetAndGetUserIdentity()
    {
        var operatorObj = new TestOperator { Name = "test-user" };
        var dto = new BaseCustomerRequestDto<string, TestOperator>();
        dto.UserIdentity = operatorObj;

        dto.UserIdentity.Should().BeSameAs(operatorObj);
        dto.UserIdentity.Name.Should().Be("test-user");
    }

    [Fact]
    public void BaseCustomerRequestDto_UserIdentityDefaultsToNewInstance()
    {
        var dto = new BaseCustomerRequestDto<string, TestOperator>();

        dto.UserIdentity.Should().NotBeNull();
        dto.UserIdentity.Should().BeOfType<TestOperator>();
    }

    public class TestOperator
    {
        public string Name { get; set; } = string.Empty;
    }
}
