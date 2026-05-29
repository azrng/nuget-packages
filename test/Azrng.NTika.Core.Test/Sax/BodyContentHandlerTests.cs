using Xunit;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Sax
{
    public class BodyContentHandlerTests
    {
        [Fact]
        public void BodyText_ShouldBeCaptured()
        {
            var inner = new WriteOutContentHandler();
            var handler = new BodyContentHandler(inner);
            var atts = new AttributesImpl();

            handler.StartDocument();
            handler.StartElement("", "html", "html", atts);
            handler.StartElement("", "head", "head", atts);
            handler.EndElement("", "head", "head");
            handler.StartElement("", "body", "body", atts);
            handler.Characters("body text".ToCharArray(), 0, 9);
            handler.EndElement("", "body", "body");
            handler.EndElement("", "html", "html");
            handler.EndDocument();

            handler.ToString().Should().Contain("body text");
        }

        [Fact]
        public void HeadText_ShouldBeIgnored()
        {
            var inner = new WriteOutContentHandler();
            var handler = new BodyContentHandler(inner);
            var atts = new AttributesImpl();

            handler.StartDocument();
            handler.StartElement("", "html", "html", atts);
            handler.StartElement("", "head", "head", atts);
            handler.Characters("head text".ToCharArray(), 0, 9);
            handler.EndElement("", "head", "head");
            handler.StartElement("", "body", "body", atts);
            handler.EndElement("", "body", "body");
            handler.EndElement("", "html", "html");
            handler.EndDocument();

            handler.ToString().Should().NotContain("head text");
        }
    }
}
