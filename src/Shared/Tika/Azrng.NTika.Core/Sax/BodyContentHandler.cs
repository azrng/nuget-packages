using System;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class BodyContentHandler : ContentHandlerDecorator
    {
        private int _depth;
        private bool _inBody;

        public BodyContentHandler(IContentHandler handler)
            : base(handler)
        {
        }

        public BodyContentHandler()
            : base(new WriteOutContentHandler())
        {
        }

        public override void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            _depth++;
            if (!_inBody && string.Equals(localName, "body", StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(uri) || uri == "http://www.w3.org/1999/xhtml"))
            {
                _inBody = true;
                return; // Don't pass the body start element itself
            }

            if (_inBody)
            {
                base.StartElement(uri, localName, qName, atts);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            if (_inBody)
            {
                if (string.Equals(localName, "body", StringComparison.OrdinalIgnoreCase))
                {
                    _inBody = false;
                    _depth--;
                    return; // Don't pass the body end element itself
                }
                base.EndElement(uri, localName, qName);
            }

            _depth--;
        }

        public override void Characters(char[] ch, int start, int length)
        {
            if (_inBody)
            {
                base.Characters(ch, start, length);
            }
        }

        public override void IgnorableWhitespace(char[] ch, int start, int length)
        {
            if (_inBody)
            {
                base.IgnorableWhitespace(ch, start, length);
            }
        }

        public override string ToString()
        {
            return Handler.ToString() ?? string.Empty;
        }
    }
}
