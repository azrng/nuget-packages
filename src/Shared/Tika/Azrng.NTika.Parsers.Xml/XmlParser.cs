using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Parsers.Xml
{
    public class XmlParser : IParser
    {
        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType>
            {
                MediaType.ApplicationXml,
                MediaType.Parse("image/svg+xml")!
            };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.CONTENT_TYPE)))
            {
                metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/xml");
            }

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();
            xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());

            try
            {
                ExtractText(stream, handler);
            }
            catch (XmlException)
            {
                // If XML parsing fails, fall back to reading raw text
                stream.Rewind();
                var encoding = DetectEncoding(stream, context);
                ExtractRawText(stream, handler, encoding);
            }

            xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
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

        private static void ExtractText(Stream stream, IContentHandler handler)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            using var reader = XmlReader.Create(stream, settings);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
                {
                    var text = reader.Value;
                    if (!string.IsNullOrEmpty(text))
                    {
                        handler.Characters(text.ToCharArray(), 0, text.Length);
                    }
                }
            }
        }

        private static void ExtractRawText(Stream stream, IContentHandler handler, Encoding encoding)
        {
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: false);
            var buffer = new char[4096];
            int charsRead;
            while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                handler.Characters(buffer, 0, charsRead);
            }
        }
    }
}
