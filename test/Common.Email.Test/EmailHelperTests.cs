using Common.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using Xunit;

namespace Common.Email.Test;

public class EmailHelperTests
{
    [Fact]
    public async Task SendTextToUserAsync_WithBlankSubject_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendTextToUserAsync("", "content", CreateRecipient());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendTextToUserAsync_WithBlankContent_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendTextToUserAsync("subject", "", CreateRecipient());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendTextToUserAsync_WithNullRecipient_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendTextToUserAsync("subject", "content", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendTextToUsersAsync_WithNullRecipients_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendTextToUsersAsync("subject", "content", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendTextToUsersAsync_WithEmptyRecipients_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendTextToUsersAsync("subject", "content", Array.Empty<ToAccessVm>());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendHtmlToUserAsync_WithNullRecipients_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendHtmlToUserAsync("subject", "content", (IEnumerable<ToAccessVm>)null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendToUserAsync_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();

        var act = () => helper.SendToUserAsync("subject", null!, CreateRecipient());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendToUsersAsync_WithNullRecipientsOrBuilder_ShouldThrowArgumentNullException()
    {
        var helper = CreateHelper();
        var builder = new BodyBuilder { TextBody = "content" };

        var nullRecipients = () => helper.SendToUsersAsync("subject", builder, null!);
        var nullBuilder = () => helper.SendToUsersAsync("subject", null!, new[] { CreateRecipient() });

        await nullRecipients.Should().ThrowAsync<ArgumentNullException>();
        await nullBuilder.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendTextToUserAsync_WithUnavailableSmtpServer_ShouldReturnFalse()
    {
        var helper = CreateHelper();

        var result = await helper.SendTextToUserAsync("subject", "content", CreateRecipient());

        result.Should().BeFalse();
    }

    private static EmailHelper CreateHelper()
    {
        var config = new EmailConfig
        {
            Host = "127.0.0.1",
            Post = 1,
            Ssl = false,
            FromAddress = "sender@example.com",
            FromName = "Sender",
            FromPassword = "password"
        };

        return new EmailHelper(NullLogger<EmailHelper>.Instance, Options.Create(config));
    }

    private static ToAccessVm CreateRecipient() => new()
    {
        ToName = "Receiver",
        Address = "receiver@example.com"
    };
}
