using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Drawing;
using DrawingShape = DocumentFormat.OpenXml.Drawing.Shape;
using PresShape = DocumentFormat.OpenXml.Presentation.Shape;

namespace Azrng.NTika.Parsers.PowerPoint
{
    public class PowerPointParser : IParser
    {
        private static readonly MediaType Pptx = MediaType.Parse("application/vnd.openxmlformats-officedocument.presentationml.presentation")!;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { Pptx };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, Pptx.ToString());

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            using var document = PresentationDocument.Open(ms, false);
            var presentationPart = document.PresentationPart;
            if (presentationPart?.Presentation?.SlideIdList == null)
            {
                xhtml.EndDocument();
                return;
            }

            var slideIndex = 0;
            foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<SlideId>())
            {
                slideIndex++;
                var slidePart = presentationPart.GetPartById(slideId.RelationshipId!);
                if (slidePart is not SlidePart sp) continue;

                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                handler.Characters($"[Slide {slideIndex}]".ToCharArray(), 0, $"[Slide {slideIndex}]".Length);
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);

                var texts = ExtractTextFromSlide(sp);
                foreach (var text in texts)
                {
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                    handler.Characters(text.ToCharArray(), 0, text.Length);
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                }

                // Extract notes
                var notesText = ExtractNotes(sp);
                foreach (var text in notesText)
                {
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                    handler.Characters(text.ToCharArray(), 0, text.Length);
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                }
            }

            xhtml.EndDocument();
        }

        private static List<string> ExtractTextFromSlide(SlidePart slidePart)
        {
            var texts = new List<string>();
            if (slidePart.Slide == null) return texts;

            foreach (var shape in slidePart.Slide.Descendants<PresShape>())
            {
                var textBody = shape.TextBody;
                if (textBody == null) continue;

                foreach (var paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    var paraText = string.Concat(
                        paragraph.Elements<Run>()
                            .Select(r => r.Text?.Text ?? string.Empty));

                    if (!string.IsNullOrEmpty(paraText))
                        texts.Add(paraText);
                }
            }

            return texts;
        }

        private static List<string> ExtractNotes(SlidePart slidePart)
        {
            var texts = new List<string>();
            var notesSlidePart = slidePart.NotesSlidePart;
            if (notesSlidePart?.NotesSlide == null) return texts;

            foreach (var shape in notesSlidePart.NotesSlide.Descendants<PresShape>())
            {
                var textBody = shape.TextBody;
                if (textBody == null) continue;

                foreach (var paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    var paraText = string.Concat(
                        paragraph.Elements<Run>()
                            .Select(r => r.Text?.Text ?? string.Empty));

                    if (!string.IsNullOrEmpty(paraText))
                        texts.Add(paraText);
                }
            }

            return texts;
        }
    }
}
