using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Azrng.NTika.Parsers.Pdf
{
    public class PdfParser : IParser
    {
        private static readonly MediaType ApplicationPdf = MediaType.Application("pdf");

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { ApplicationPdf };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/pdf");

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            try
            {
                using var document = PdfDocument.Open(stream);
                ExtractMetadata(document, metadata);
                ExtractText(document, handler, xhtml);
            }
            catch (UglyToad.PdfPig.Exceptions.PdfDocumentEncryptedException)
            {
                metadata.Set("pdf:encrypted", "true");
                throw new EncryptedDocumentException("PDF document is encrypted");
            }

            xhtml.EndDocument();
        }

        private static void ExtractMetadata(PdfDocument document, Metadata metadata)
        {
            var info = document.Information;
            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.Title))
                    metadata.Set(TikaCoreProperties.TITLE, info.Title);
                if (!string.IsNullOrEmpty(info.Author))
                    metadata.Set(TikaCoreProperties.CREATOR, info.Author);
                if (!string.IsNullOrEmpty(info.Creator))
                    metadata.Set("pdf:creator", info.Creator);
                if (!string.IsNullOrEmpty(info.Producer))
                    metadata.Set("pdf:producer", info.Producer);
                if (!string.IsNullOrEmpty(info.CreationDate))
                    metadata.Set("pdf:creationDate", info.CreationDate);
                if (!string.IsNullOrEmpty(info.ModifiedDate))
                    metadata.Set("pdf:modDate", info.ModifiedDate);
            }

            metadata.Set("pdf:pageCount", document.NumberOfPages.ToString(CultureInfo.InvariantCulture));
        }

        private static void ExtractText(PdfDocument document, IContentHandler handler, XHTMLContentHandler xhtml)
        {
            for (var i = 1; i <= document.NumberOfPages; i++)
            {
                var page = document.GetPage(i);
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Div, XHTMLContentHandler.Div, new AttributesImpl());

                var textBuilder = new StringBuilder();
                var hasPrevious = false;
                var prevEndX = 0.0;
                var prevGlyphWidth = 0.0;

                foreach (var letter in page.Letters)
                {
                    if (hasPrevious)
                    {
                        var distance = letter.StartBaseLine.X - prevEndX;
                        var avgWidth = (prevGlyphWidth + letter.BoundingBox.Width) / 2;
                        if (distance > avgWidth * 0.3)
                        {
                            textBuilder.Append(' ');
                        }
                    }
                    textBuilder.Append(letter.Value);
                    hasPrevious = true;
                    prevEndX = letter.EndBaseLine.X;
                    prevGlyphWidth = letter.BoundingBox.Width;
                }

                if (textBuilder.Length > 0)
                {
                    var pageText = textBuilder.ToString();
                    handler.Characters(pageText.ToCharArray(), 0, pageText.Length);
                }

                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Div, XHTMLContentHandler.Div);
            }
        }
    }
}
