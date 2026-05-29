using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class ToHTMLContentHandler : IContentHandler
    {
        private readonly TextWriter _writer;
        private readonly StringBuilder? _buffer;
        private readonly Stack<string> _elementStack = new();
        private bool _inBody;
        private int _suppressOutputDepth;

        public ToHTMLContentHandler(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public ToHTMLContentHandler()
        {
            _buffer = new StringBuilder();
            _writer = new StringWriter(_buffer);
        }

        public void SetDocumentLocator(ILocator locator) { }
        public void StartDocument() { }

        public void EndDocument()
        {
            _writer.Flush();
        }

        public void StartPrefixMapping(string prefix, string uri) { }
        public void EndPrefixMapping(string prefix) { }

        public void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            var ln = localName.ToLowerInvariant();
            _elementStack.Push(ln);

            if (ln == "body")
            {
                _inBody = true;
                return;
            }

            switch (ln)
            {
                case "head":
                case "script":
                case "style":
                    _suppressOutputDepth++;
                    return;
            }

            if (!_inBody || _suppressOutputDepth > 0)
                return;

            _writer.Write('<');
            _writer.Write(localName);

            for (int i = 0; i < atts.Length; i++)
            {
                _writer.Write(' ');
                _writer.Write(atts.GetLocalName(i));
                _writer.Write("=\"");
                _writer.Write(EscapeAttributeValue(atts.GetValue(i)));
                _writer.Write('"');
            }

            if (IsVoidElement(ln))
                _writer.Write(" /");

            _writer.Write('>');
        }

        public void EndElement(string uri, string localName, string qName)
        {
            if (_elementStack.Count > 0)
                _elementStack.Pop();

            var ln = localName.ToLowerInvariant();

            if (ln == "body")
            {
                _inBody = false;
                return;
            }

            if (_suppressOutputDepth > 0)
            {
                if (ln == "head" || ln == "script" || ln == "style")
                    _suppressOutputDepth--;
                return;
            }

            if (!_inBody)
                return;

            if (IsVoidElement(ln))
                return;

            _writer.Write("</");
            _writer.Write(localName);
            _writer.Write('>');
        }

        public void Characters(char[] ch, int start, int length)
        {
            if (!_inBody || _suppressOutputDepth > 0)
                return;

            var text = new string(ch, start, length);
            _writer.Write(EscapeTextContent(text));
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

        private static bool IsVoidElement(string localName)
        {
            return localName switch
            {
                "area" or "base" or "br" or "col" or "embed" or "hr" or
                "img" or "input" or "link" or "meta" or "source" or "track" or "wbr" => true,
                _ => false,
            };
        }

        private static string EscapeTextContent(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private static string EscapeAttributeValue(string? text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}
