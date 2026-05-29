using System;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public class ContentHandlerDecorator : IContentHandler
    {
        protected IContentHandler Handler;

        public ContentHandlerDecorator(IContentHandler handler)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public virtual void SetDocumentLocator(ILocator locator) => Handler.SetDocumentLocator(locator);
        public virtual void StartDocument() => Handler.StartDocument();
        public virtual void EndDocument() => Handler.EndDocument();
        public virtual void StartPrefixMapping(string prefix, string uri) => Handler.StartPrefixMapping(prefix, uri);
        public virtual void EndPrefixMapping(string prefix) => Handler.EndPrefixMapping(prefix);
        public virtual void StartElement(string uri, string localName, string qName, IAttributes atts) => Handler.StartElement(uri, localName, qName, atts);
        public virtual void EndElement(string uri, string localName, string qName) => Handler.EndElement(uri, localName, qName);
        public virtual void Characters(char[] ch, int start, int length) => Handler.Characters(ch, start, length);
        public virtual void IgnorableWhitespace(char[] ch, int start, int length) => Handler.IgnorableWhitespace(ch, start, length);
        public virtual void ProcessingInstruction(string target, string data) => Handler.ProcessingInstruction(target, data);
        public virtual void SkippedEntity(string name) => Handler.SkippedEntity(name);
    }
}
