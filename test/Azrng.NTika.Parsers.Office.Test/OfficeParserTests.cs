using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using Xunit;

namespace Azrng.NTika.Parsers.Office.Test
{
    public class OfficeParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnOfficeTypes()
        {
            var parser = new OfficeParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(3);
        }

        [Fact]
        public void Parse_DocxDocument_ShouldExtractText()
        {
            var parser = new OfficeParser();
            var docxBytes = CreateTestDocx();
            using var stream = TikaInputStream.Get(docxBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello DOCX World");
        }

        [Fact]
        public void Parse_XlsxDocument_ShouldExtractText()
        {
            var parser = new OfficeParser();
            var xlsxBytes = CreateTestXlsx();
            using var stream = TikaInputStream.Get(xlsxBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Cell1");
            text.Should().Contain("Cell2");
        }

        [Fact]
        public void Parse_XlsDocument_ShouldExtractText()
        {
            var parser = new OfficeParser();
            var xlsBytes = CreateTestXls();
            using var stream = TikaInputStream.Get(xlsBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/vnd.ms-excel");
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("XLS Cell1");
            text.Should().Contain("XLS Cell2");
        }

        private static byte[] CreateTestDocx()
        {
            using var ms = new MemoryStream();
            var doc = new XWPFDocument();
            var para = doc.CreateParagraph();
            para.CreateRun().SetText("Hello DOCX World");
            doc.Write(ms);
            return ms.ToArray();
        }

        private static byte[] CreateTestXlsx()
        {
            using var ms = new MemoryStream();
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("Cell1");
            row.CreateCell(1).SetCellValue("Cell2");
            workbook.Write(ms);
            return ms.ToArray();
        }

        private static byte[] CreateTestXls()
        {
            using var ms = new MemoryStream();
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("XLS Cell1");
            row.CreateCell(1).SetCellValue("XLS Cell2");
            workbook.Write(ms);
            return ms.ToArray();
        }
    }
}
