using System.Collections.Generic;
using System.IO;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using IAttributes = Azrng.NTika.Core.Abstractions.IAttributes;

namespace Azrng.NTika.Parsers.Html
{
    public class HtmlParser : IParser
    {
        private static readonly MediaType TextHtml = MediaType.TextHtml;
        private static readonly MediaType ApplicationXhtml = MediaType.Parse("application/xhtml+xml")!;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { TextHtml, ApplicationXhtml };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.CONTENT_TYPE)))
            {
                metadata.Set(TikaCoreProperties.CONTENT_TYPE, "text/html");
            }

            var encoding = DetectEncoding(stream, context);
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var html = reader.ReadToEnd();

            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var document = parser.ParseDocument(html);

            ExtractMetadata(document, metadata);

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            TraverseNode(document.Body ?? document.DocumentElement, handler, xhtml);

            xhtml.EndDocument();
        }

        private static Encoding DetectEncoding(Stream stream, ParseContext context)
        {
            var detector = context.Get<IEncodingDetector>();
            if (detector != null)
            {
                var savedPosition = stream.CanSeek ? stream.Position : 0;
                var result = detector.Detect(stream, new Metadata(), context);
                if (stream.CanSeek) stream.Position = savedPosition;

                if (result != null)
                    return result.Encoding;
            }

            return Encoding.UTF8;
        }

        private static void ExtractMetadata(IDocument document, Metadata metadata)
        {
            var title = document.Title;
            if (!string.IsNullOrEmpty(title))
            {
                metadata.Set(TikaCoreProperties.TITLE, title);
            }

            var metaDesc = document.QuerySelector("meta[name='description']");
            if (metaDesc != null)
            {
                var content = metaDesc.GetAttribute("content");
                if (!string.IsNullOrEmpty(content))
                {
                    metadata.Set(TikaCoreProperties.DESCRIPTION, content);
                }
            }

            var metaKeywords = document.QuerySelector("meta[name='keywords']");
            if (metaKeywords != null)
            {
                var content = metaKeywords.GetAttribute("content");
                if (!string.IsNullOrEmpty(content))
                {
                    metadata.Set(TikaCoreProperties.SUBJECT, content);
                }
            }
        }

        private static void TraverseNode(INode node, IContentHandler handler, XHTMLContentHandler xhtml)
        {
            if (node is IElement element)
            {
                var localName = element.LocalName.ToLowerInvariant();
                var attrs = BuildAttributes(element);

                handler.StartElement(XHTMLContentHandler.Namespace, localName, localName, attrs);

                foreach (var child in element.ChildNodes)
                {
                    TraverseNode(child, handler, xhtml);
                }

                handler.EndElement(XHTMLContentHandler.Namespace, localName, localName);
            }
            else if (node is IText textNode)
            {
                var text = textNode.Data;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    handler.Characters(text.ToCharArray(), 0, text.Length);
                }
            }
        }

        private static IAttributes BuildAttributes(IElement element)
        {
            var attrs = new AttributesImpl();
            foreach (var attr in element.Attributes)
            {
                attrs.AddAttribute(string.Empty, attr.LocalName, attr.LocalName, "CDATA", attr.Value ?? string.Empty);
            }
            return attrs;
        }
    }
}
