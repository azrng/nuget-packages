using Common.Email;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Common.Email.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEmail_ShouldRegisterHelperAndConfigureOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddEmail(options =>
        {
            options.Host = "smtp.test.local";
            options.Post = 2525;
            options.Ssl = false;
            options.FromName = "Mailer";
            options.FromAddress = "mailer@test.local";
            options.FromPassword = "secret";
        });

        using var provider = services.BuildServiceProvider();
        var helper = provider.GetRequiredService<IEmailHelper>();
        var options = provider.GetRequiredService<IOptions<EmailConfig>>().Value;

        helper.Should().BeOfType<EmailHelper>();
        options.Host.Should().Be("smtp.test.local");
        options.Post.Should().Be(2525);
        options.Ssl.Should().BeFalse();
        options.FromName.Should().Be("Mailer");
        options.FromAddress.Should().Be("mailer@test.local");
        options.FromPassword.Should().Be("secret");
    }

    [Fact]
    public void EmailConfig_ShouldExposeExpectedDefaults()
    {
        var config = new EmailConfig();

        config.Host.Should().Be("smtp.163.com");
        config.Post.Should().Be(587);
        config.Ssl.Should().BeTrue();
    }
}
