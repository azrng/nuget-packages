using System.Collections.Generic;
using System.Text;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class LinkContentHandler : IContentHandler
    {
        private readonly List<Link> _links = new();
        private bool _inAnchor;
        private string? _currentHref;
        private readonly StringBuilder _text = new();

        public IReadOnlyList<Link> Links => _links;

        public void SetDocumentLocator(ILocator locator) { }
        public void StartDocument() { }
        public void EndDocument() { }
        public void StartPrefixMapping(string prefix, string uri) { }
        public void EndPrefixMapping(string prefix) { }

        public void StartElement(string uri, string localName, string qName, IAttributes atts)
        {
            if (string.Equals(localName, "a", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(localName, "area", System.StringComparison.OrdinalIgnoreCase))
            {
                var href = atts.GetValue("href");
                if (!string.IsNullOrEmpty(href))
                {
                    _inAnchor = true;
                    _currentHref = href;
                    _text.Clear();
                }
            }
        }

        public void EndElement(string uri, string localName, string qName)
        {
            if (_inAnchor &&
                (string.Equals(localName, "a", System.StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(localName, "area", System.StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrEmpty(_currentHref))
                {
                    _links.Add(new Link(_currentHref, _text.ToString().Trim()));
                }
                _inAnchor = false;
                _currentHref = null;
            }
        }

        public void Characters(char[] ch, int start, int length)
        {
            if (_inAnchor)
            {
                _text.Append(ch, start, length);
            }
        }

        public void IgnorableWhitespace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }

        public void ProcessingInstruction(string target, string data) { }
        public void SkippedEntity(string name) { }
    }
}
