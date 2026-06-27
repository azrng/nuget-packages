using Azrng.DevLogDashboard.Options;

namespace Azrng.DevLogDashboard.Test.Options;

public class DevLogDashboardBasicAuthenticationOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var options = new DevLogDashboardBasicAuthenticationOptions();

        options.UserName.Should().BeEmpty();
        options.Password.Should().BeEmpty();
        options.Realm.Should().Be("DevLogDashboard");
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var options = new DevLogDashboardBasicAuthenticationOptions
        {
            UserName = "admin",
            Password = "secret123",
            Realm = "CustomRealm"
        };

        options.UserName.Should().Be("admin");
        options.Password.Should().Be("secret123");
        options.Realm.Should().Be("CustomRealm");
    }
}
