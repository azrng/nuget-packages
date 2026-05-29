using System.Collections.Generic;
using System;
using System.IO;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;

namespace Azrng.NTika.Parsers.Office
{
    public class OfficeParser : IParser
    {
        private static readonly MediaType WordDocx = MediaType.Parse("application/vnd.openxmlformats-officedocument.wordprocessingml.document")!;
        private static readonly MediaType ExcelXlsx = MediaType.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")!;
        private static readonly MediaType ExcelXls = MediaType.Parse("application/vnd.ms-excel")!;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType>
            {
                WordDocx,
                ExcelXlsx,
                ExcelXls
            };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            if (contentType != null && contentType.Contains("wordprocessingml"))
            {
                ParseDocx(stream, handler, xhtml);
            }
            else if (contentType != null && contentType.Contains("spreadsheetml"))
            {
                ParseXlsx(stream, handler, xhtml);
            }
            else if (contentType != null && contentType.Contains("vnd.ms-excel"))
            {
                ParseXls(stream, handler, xhtml);
            }
            else
            {
                TryAutoDetect(stream, handler, xhtml);
            }

            xhtml.EndDocument();
        }

        private static void ParseDocx(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            XHTMLContentHandler xhtml)
        {
            using var doc = new XWPFDocument(stream);

            foreach (var paragraph in doc.Paragraphs)
            {
                var text = paragraph.ParagraphText;
                if (!string.IsNullOrEmpty(text))
                {
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                    handler.Characters(text.ToCharArray(), 0, text.Length);
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                }
            }

            foreach (var table in doc.Tables)
            {
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Table, XHTMLContentHandler.Table, new AttributesImpl());
                foreach (var row in table.Rows)
                {
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr, new AttributesImpl());
                    foreach (var cell in row.GetTableCells())
                    {
                        xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Td, XHTMLContentHandler.Td, new AttributesImpl());
                        var cellText = cell.GetText();
                        if (!string.IsNullOrEmpty(cellText))
                        {
                            handler.Characters(cellText.ToCharArray(), 0, cellText.Length);
                        }
                        xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Td, XHTMLContentHandler.Td);
                    }
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr);
                }
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Table, XHTMLContentHandler.Table);
            }
        }

        private static void ParseXlsx(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            XHTMLContentHandler xhtml)
        {
            using var workbook = new XSSFWorkbook(stream);
            ParseWorkbook(workbook, handler, xhtml);
        }

        private static void ParseXls(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            XHTMLContentHandler xhtml)
        {
            using var workbook = new HSSFWorkbook(stream);
            ParseWorkbook(workbook, handler, xhtml);
        }

        private static void ParseWorkbook(NPOI.SS.UserModel.IWorkbook workbook, IContentHandler handler,
            XHTMLContentHandler xhtml)
        {
            for (var i = 0; i < workbook.NumberOfSheets; i++)
            {
                var sheet = workbook.GetSheetAt(i);
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Table, XHTMLContentHandler.Table, new AttributesImpl());

                for (var rowIdx = sheet.FirstRowNum; rowIdx <= sheet.LastRowNum; rowIdx++)
                {
                    var row = sheet.GetRow(rowIdx);
                    if (row == null) continue;

                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr, new AttributesImpl());
                    for (var cellIdx = row.FirstCellNum; cellIdx < row.LastCellNum; cellIdx++)
                    {
                        var cell = row.GetCell(cellIdx);
                        xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Td, XHTMLContentHandler.Td, new AttributesImpl());
                        if (cell != null)
                        {
                            var cellValue = GetCellStringValue(cell);
                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                handler.Characters(cellValue.ToCharArray(), 0, cellValue.Length);
                            }
                        }
                        xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Td, XHTMLContentHandler.Td);
                    }
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Tr, XHTMLContentHandler.Tr);
                }

                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.Table, XHTMLContentHandler.Table);
            }
        }

        private static void TryAutoDetect(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            XHTMLContentHandler xhtml)
        {
            if (TryParseBuffered(stream, handler, ParseXlsx))
                return;

            if (TryParseBuffered(stream, handler, ParseXls))
                return;

            TryParseBuffered(stream, handler, ParseDocx);
        }

        private static bool TryParseBuffered(
            Azrng.NTika.Core.IO.TikaInputStream stream,
            IContentHandler handler,
            Action<Azrng.NTika.Core.IO.TikaInputStream, IContentHandler, XHTMLContentHandler> parse)
        {
            var bufferHandler = new ContentEventBuffer();
            var bufferXhtml = new XHTMLContentHandler(bufferHandler);

            try
            {
                stream.Rewind();
                parse(stream, bufferHandler, bufferXhtml);
            }
            catch (IOException)
            {
                return false;
            }
            catch (InvalidDataException)
            {
                return false;
            }
            catch (TikaException)
            {
                throw;
            }

            bufferHandler.Replay(handler);
            return true;
        }

        private static string GetCellStringValue(NPOI.SS.UserModel.ICell cell)
        {
            return cell.CellType switch
            {
                NPOI.SS.UserModel.CellType.String => cell.StringCellValue,
                NPOI.SS.UserModel.CellType.Numeric => cell.NumericCellValue.ToString(),
                NPOI.SS.UserModel.CellType.Boolean => cell.BooleanCellValue.ToString(),
                NPOI.SS.UserModel.CellType.Formula => cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.String
                    ? cell.StringCellValue
                    : cell.NumericCellValue.ToString(),
                NPOI.SS.UserModel.CellType.Error => string.Empty,
                _ => string.Empty
            };
        }
    }
}
