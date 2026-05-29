using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Parsers.Data
{
    public class JsonParser : IParser
    {
        private static readonly MediaType ApplicationJson = MediaType.Application("json");

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { ApplicationJson };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/json");

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var json = reader.ReadToEnd();

            if (string.IsNullOrWhiteSpace(json))
            {
                xhtml.EndDocument();
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                WalkJsonElement(doc.RootElement, handler, xhtml);
            }
            catch (JsonException)
            {
                // If JSON is invalid, emit raw text
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                handler.Characters(json.ToCharArray(), 0, json.Length);
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
            }

            xhtml.EndDocument();
        }

        private static void WalkJsonElement(JsonElement element, IContentHandler handler, XHTMLContentHandler xhtml)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                        var keyText = property.Name;
                        handler.Characters(keyText.ToCharArray(), 0, keyText.Length);
                        xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);

                        WalkJsonElement(property.Value, handler, xhtml);
                    }
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        WalkJsonElement(item, handler, xhtml);
                    }
                    break;

                case JsonValueKind.String:
                    var str = element.GetString();
                    if (!string.IsNullOrEmpty(str))
                    {
                        xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                        handler.Characters(str.ToCharArray(), 0, str.Length);
                        xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                    }
                    break;

                case JsonValueKind.Number:
                    var num = element.GetRawText();
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                    handler.Characters(num.ToCharArray(), 0, num.Length);
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    var boolVal = element.GetBoolean().ToString();
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                    handler.Characters(boolVal.ToCharArray(), 0, boolVal.Length);
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                    break;
            }
        }
    }
}
