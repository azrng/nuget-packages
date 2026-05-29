using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Sax
{
    public class XHTMLContentHandler : ContentHandlerDecorator
    {
        public const string Namespace = "http://www.w3.org/1999/xhtml";
        public const string Html = "html";
        public const string Head = "head";
        public const string Body = "body";
        public const string Title = "title";
        public const string P = "p";
        public const string Br = "br";
        public const string Hr = "hr";
        public const string Div = "div";
        public const string Table = "table";
        public const string Tr = "tr";
        public const string Td = "td";
        public const string Th = "th";
        public const string Ul = "ul";
        public const string Ol = "ol";
        public const string Li = "li";
        public const string A = "a";
        public const string Href = "href";

        private static readonly IAttributes EmptyAttributes = new AttributesImpl();

        public XHTMLContentHandler(IContentHandler handler)
            : base(handler)
        {
        }

        public new void StartDocument()
        {
            Handler.StartDocument();
            Handler.StartPrefixMapping("", Namespace);
            Handler.StartElement(Namespace, Html, Html, EmptyAttributes);
            Handler.StartElement(Namespace, Head, Head, EmptyAttributes);
            Handler.EndElement(Namespace, Head, Head);
            Handler.StartElement(Namespace, Body, Body, EmptyAttributes);
        }

        public new void EndDocument()
        {
            Handler.EndElement(Namespace, Body, Body);
            Handler.EndElement(Namespace, Html, Html);
            Handler.EndPrefixMapping("");
            Handler.EndDocument();
        }

        public void ElementWithCharacters(string localName, string text)
        {
            Handler.StartElement(Namespace, localName, localName, EmptyAttributes);
            Handler.Characters(text.ToCharArray(), 0, text.Length);
            Handler.EndElement(Namespace, localName, localName);
        }
    }
}
