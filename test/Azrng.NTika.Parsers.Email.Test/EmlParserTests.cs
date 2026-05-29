using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using MimeKit;
using Xunit;

namespace Azrng.NTika.Parsers.Email.Test
{
    public class EmlParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnRfc822()
        {
            var parser = new EmlParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(1);
            types.First().ToString().Should().Contain("rfc822");
        }

        [Fact]
        public void Parse_SimpleEml_ShouldExtractSubjectAndBody()
        {
            var parser = new EmlParser();
            var emlBytes = CreateTestEml("Test Subject", "Hello, this is the email body.");
            using var stream = TikaInputStream.Get(emlBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Test Subject");
            text.Should().Contain("Hello, this is the email body.");
        }

        [Fact]
        public void Parse_SimpleEml_ShouldSetMetadata()
        {
            var parser = new EmlParser();
            var emlBytes = CreateTestEml("Important Email", "Content here");
            using var stream = TikaInputStream.Get(emlBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.TITLE).Should().Be("Important Email");
            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Be("message/rfc822");
        }

        [Fact]
        public void Parse_HtmlOnlyBody_ShouldExtractTextWithoutTags()
        {
            var parser = new EmlParser();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Test Sender", "sender@example.com"));
            message.To.Add(new MailboxAddress("Test Recipient", "recipient@example.com"));
            message.Subject = "HTML";
            message.Body = new TextPart("html")
            {
                Text = "<html><body><script>alert(1)</script><p>Hello <b>HTML</b></p></body></html>"
            };

            using var ms = new MemoryStream();
            message.WriteTo(ms);
            ms.Position = 0;
            using var stream = TikaInputStream.Get(ms);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello  HTML");
            text.Should().NotContain("<p>");
            text.Should().NotContain("alert");
        }

        private static byte[] CreateTestEml(string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Test Sender", "sender@example.com"));
            message.To.Add(new MailboxAddress("Test Recipient", "recipient@example.com"));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var ms = new MemoryStream();
            message.WriteTo(ms);
            return ms.ToArray();
        }
    }
}
