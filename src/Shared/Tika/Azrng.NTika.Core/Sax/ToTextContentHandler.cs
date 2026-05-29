using System;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class ToTextContentHandler : IContentHandler
    {
        private readonly TextWriter _writer;
        private readonly StringBuilder? _buffer;

        public ToTextContentHandler(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public ToTextContentHandler()
        {
            _buffer = new StringBuilder();
            _writer = new StringWriter(_buffer);
        }

        public void SetDocumentLocator(ILocator locator) { }
        public void StartDocument() { }
        public void EndDocument() { }
        public void StartPrefixMapping(string prefix, string uri) { }
        public void EndPrefixMapping(string prefix) { }
        public void StartElement(string uri, string localName, string qName, IAttributes atts) { }

        public void EndElement(string uri, string localName, string qName)
        {
            _writer.Write(' ');
        }

        public void Characters(char[] ch, int start, int length)
        {
            _writer.Write(ch, start, length);
        }

        public void IgnorableWhitespace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }

        public void ProcessingInstruction(string target, string data) { }
        public void SkippedEntity(string name) { }

        public override string ToString()
        {
            if (_buffer != null)
            {
                _writer.Flush();
                return _buffer.ToString();
            }
            return string.Empty;
        }
    }
}
