using System.Collections.Generic;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using Sylvan.Data.Csv;

namespace Azrng.NTika.Parsers.Csv
{
    public class CsvParser : IParser
    {
        private static readonly MediaType TextCsv = MediaType.Text("csv");
        private static readonly MediaType TextTsv = MediaType.Text("tsv");

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { TextCsv, TextTsv };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var config = context.Get<CsvConfig>()?.Clone() ?? new CsvConfig();

            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            if (!string.IsNullOrEmpty(contentType) && contentType.Contains("tsv"))
            {
                config.Delimiter = '\t';
            }

            var encoding = DetectEncoding(stream, context);

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();
            xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Table, XHTMLContentHandler.Table, new AttributesImpl());

            int rowCount = 0;
            int colCount = 0;

            var csvOptions = new CsvDataReaderOptions
            {
                Delimiter = config.Delimiter,
                HasHeaders = config.HasHeaders
            };

            using var streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            using var reader = CsvDataReader.Create(streamReader, csvOptions);
            if (config.HasHeaders)
            {
                colCount = reader.FieldCount;
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr, new AttributesImpl());
                for (int i = 0; i < colCount; i++)
                {
                    var header = reader.GetName(i);
                    xhtml.ElementWithCharacters(XHTMLContentHandler.Th, header ?? string.Empty);
                }
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr);
            }

            while (reader.Read())
            {
                if (colCount == 0)
                {
                    colCount = reader.FieldCount;
                }

                rowCount++;
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr, new AttributesImpl());
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetString(i);
                    xhtml.ElementWithCharacters(XHTMLContentHandler.Td, value ?? string.Empty);
                }
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr);
            }

            xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Table, XHTMLContentHandler.Table);
            xhtml.EndDocument();

            metadata.Set("numColumns", colCount.ToString());
            metadata.Set("numRows", rowCount.ToString());
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
    }
}
