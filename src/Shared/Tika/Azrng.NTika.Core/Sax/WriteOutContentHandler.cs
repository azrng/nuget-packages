using System;
using System.Text;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class WriteOutContentHandler : IContentHandler
    {
        private readonly StringBuilder _builder;
        private readonly WriteLimiter? _limiter;

        public WriteOutContentHandler(int writeLimit)
        {
            _builder = new StringBuilder();
            _limiter = new WriteLimiter(writeLimit);
        }

        public WriteOutContentHandler()
        {
            _builder = new StringBuilder();
            _limiter = null;
        }

        public void SetDocumentLocator(ILocator locator) { }
        public void StartDocument() { }
        public void EndDocument() { }
        public void StartPrefixMapping(string prefix, string uri) { }
        public void EndPrefixMapping(string prefix) { }
        public void StartElement(string uri, string localName, string qName, IAttributes atts) { }

        public void EndElement(string uri, string localName, string qName)
        {
            _builder.Append(' ');
            _limiter?.AddWritten(1);
        }

        public void Characters(char[] ch, int start, int length)
        {
            _builder.Append(ch, start, length);
            _limiter?.AddWritten(length);
        }

        public void IgnorableWhitespace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }

        public void ProcessingInstruction(string target, string data) { }
        public void SkippedEntity(string name) { }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
