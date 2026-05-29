using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class LimitedContentHandler : ContentHandlerDecorator
    {
        private readonly WriteLimiter _limiter;

        public LimitedContentHandler(IContentHandler handler, int writeLimit)
            : base(handler)
        {
            _limiter = new WriteLimiter(writeLimit);
        }

        public override void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            _limiter.AddWritten(localName.Length + 2);
            base.StartElement(uri, localName, qName, atts);
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _limiter.AddWritten(localName.Length + 3);
            base.EndElement(uri, localName, qName);
        }

        public override void Characters(char[] ch, int start, int length)
        {
            _limiter.AddWritten(length);
            base.Characters(ch, start, length);
        }

        public override void IgnorableWhitespace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }
    }
}
