using System.Collections.Generic;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Parsers.Text
{
    public class TextParser : IParser
    {
        private const int BufferSize = 4096;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { MediaType.TextPlain };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var (encoding, bomLength) = DetectEncoding(stream, context);
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, $"text/plain; charset={encoding.WebName}");

            // Skip BOM
            if (stream.CanSeek && bomLength > 0)
            {
                stream.Position = bomLength;
            }

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();
            xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());

            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: false);
            var buffer = new char[BufferSize];
            int charsRead;
            while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                handler.Characters(buffer, 0, charsRead);
            }

            xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
            xhtml.EndDocument();
        }

        private static (Encoding encoding, int bomLength) DetectEncoding(Stream stream, ParseContext context)
        {
            // Try pluggable encoding detector first
            var detector = context.Get<IEncodingDetector>();
            if (detector != null)
            {
                var savedPosition = stream.CanSeek ? stream.Position : 0;
                var result = detector.Detect(stream, new Metadata(), context);
                if (stream.CanSeek) stream.Position = savedPosition;

                if (result != null)
                {
                    // Check if the detected encoding came from BOM
                    var bomLength = DetectBomLength(stream);
                    return (result.Encoding, bomLength);
                }
            }

            // Fallback to inline BOM detection
            return DetectBomEncoding(stream);
        }

        private static int DetectBomLength(Stream stream)
        {
            if (!stream.CanSeek) return 0;

            var savedPosition = stream.Position;
            stream.Position = 0;

            var bom = new byte[4];
            var bytesRead = stream.Read(bom, 0, bom.Length);
            stream.Position = savedPosition;

            if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                return 3;
            if (bytesRead >= 4 && bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
                return 4;
            if (bytesRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                return 2;
            if (bytesRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
                return 2;

            return 0;
        }

        private static (Encoding encoding, int bomLength) DetectBomEncoding(Stream stream)
        {
            if (stream.CanSeek)
            {
                var savedPosition = stream.Position;
                stream.Position = 0;

                var bom = new byte[4];
                var bytesRead = stream.Read(bom, 0, bom.Length);
                stream.Position = savedPosition;

                if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                    return (Encoding.UTF8, 3);
                if (bytesRead >= 4 && bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
                    return (Encoding.UTF32, 4);
                if (bytesRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                    return (Encoding.Unicode, 2);
                if (bytesRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
                    return (Encoding.BigEndianUnicode, 2);
            }

            return (Encoding.UTF8, 0);
        }
    }
}
