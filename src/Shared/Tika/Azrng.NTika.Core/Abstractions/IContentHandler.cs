namespace Azrng.NTika.Core.Abstractions
{
    public interface IContentHandler
    {
        void SetDocumentLocator(ILocator locator);
        void StartDocument();
        void EndDocument();
        void StartPrefixMapping(string prefix, string uri);
        void EndPrefixMapping(string prefix);
        void StartElement(string uri, string localName, string qName, IAttributes atts);
        void EndElement(string uri, string localName, string qName);
        void Characters(char[] ch, int start, int length);
        void IgnorableWhitespace(char[] ch, int start, int length);
        void ProcessingInstruction(string target, string data);
        void SkippedEntity(string name);
    }
}
