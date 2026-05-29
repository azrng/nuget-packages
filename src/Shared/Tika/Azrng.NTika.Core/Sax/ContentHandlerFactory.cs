using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Sax
{
    public static class ContentHandlerFactory
    {
        public static IContentHandler Create(ContentHandlerType type)
        {
            return type switch
            {
                ContentHandlerType.Text => new BodyContentHandler(),
                ContentHandlerType.Html => new ToHTMLContentHandler(),
                ContentHandlerType.Markdown => new ToMarkdownContentHandler(),
                _ => new BodyContentHandler(),
            };
        }

        public static IContentHandler Create(ContentHandlerType type, int maxStringLength)
        {
            return type switch
            {
                ContentHandlerType.Text => new BodyContentHandler(new WriteOutContentHandler(maxStringLength)),
                ContentHandlerType.Html => new ToHTMLContentHandler(),
                ContentHandlerType.Markdown => new ToMarkdownContentHandler(),
                _ => new BodyContentHandler(new WriteOutContentHandler(maxStringLength)),
            };
        }
    }
}
