using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using FluentAssertions;
using Xunit;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace Azrng.NTika.Parsers.PowerPoint.Test
{
    public class PowerPointParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnPptx()
        {
            var parser = new PowerPointParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(1);
            types.First().ToString().Should().Contain("presentationml.presentation");
        }

        [Fact]
        public void Parse_PptxDocument_ShouldExtractText()
        {
            var parser = new PowerPointParser();
            var pptxBytes = CreateTestPptx();
            using var stream = TikaInputStream.Get(pptxBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello PowerPoint");
            text.Should().Contain("Second slide text");
        }

        [Fact]
        public void Parse_PptxDocument_ShouldSetContentType()
        {
            var parser = new PowerPointParser();
            var pptxBytes = CreateTestPptx();
            using var stream = TikaInputStream.Get(pptxBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Contain("presentationml.presentation");
        }

        private static byte[] CreateTestPptx()
        {
            using var ms = new MemoryStream();
            using (var presentation = PresentationDocument.Create(ms, PresentationDocumentType.Presentation, true))
            {
                var presentationPart = presentation.AddPresentationPart();
                presentationPart.Presentation = new P.Presentation();

                var slideIdList = new P.SlideIdList();
                presentationPart.Presentation.SlideIdList = slideIdList;

                // Slide 1
                AddSlide(presentationPart, slideIdList, 256, "Hello PowerPoint");

                // Slide 2
                AddSlide(presentationPart, slideIdList, 257, "Second slide text");

                presentationPart.Presentation.Save();
            }

            return ms.ToArray();
        }

        private static void AddSlide(PresentationPart presentationPart, P.SlideIdList slideIdList, uint slideIdValue, string text)
        {
            var slidePart = presentationPart.AddNewPart<SlidePart>();
            var slide = new P.Slide(
                new P.CommonSlideData(
                    new P.ShapeTree(
                        new P.Shape(
                            new P.TextBody(
                                new A.BodyProperties(),
                                new A.Paragraph(
                                    new A.Run(
                                        new A.Text(text)
                                    )
                                )
                            ),
                            new P.ShapeProperties()
                        )
                    )
                )
            );
            slidePart.Slide = slide;

            var relationshipId = presentationPart.GetIdOfPart(slidePart);
            slideIdList.Append(new P.SlideId
            {
                Id = slideIdValue,
                RelationshipId = relationshipId
            });
        }
    }
}
