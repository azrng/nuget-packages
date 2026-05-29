using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Parsers.Odf
{
    public class OpenDocumentParser : IParser
    {
        private static readonly MediaType Odt = MediaType.Parse("application/vnd.oasis.opendocument.text")!;
        private static readonly MediaType Ods = MediaType.Parse("application/vnd.oasis.opendocument.spreadsheet")!;
        private static readonly MediaType Odp = MediaType.Parse("application/vnd.oasis.opendocument.presentation")!;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { Odt, Ods, Odp };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = DetectOdfType(stream);
                if (!string.IsNullOrEmpty(contentType))
                    metadata.Set(TikaCoreProperties.CONTENT_TYPE, contentType);
            }

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            stream.Rewind();
            try
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                var contentEntry = archive.GetEntry("content.xml");

                if (contentEntry != null)
                {
                    using var entryStream = contentEntry.Open();
                    var doc = LoadXmlSafely(entryStream);
                    var ns = XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:text:1.0");

                    // Extract all text:p and text:h elements
                    var textElements = doc.Descendants()
                        .Where(e => e.Name.LocalName == "p" || e.Name.LocalName == "h")
                        .Where(e => e.Name.NamespaceName.Contains("text"));

                    foreach (var element in textElements)
                    {
                        var text = string.Concat(element.Nodes()
                            .Where(n => n.NodeType == System.Xml.XmlNodeType.Text || n.NodeType == System.Xml.XmlNodeType.CDATA)
                            .Select(n => ((XText)n).Value));

                        // Also get text from text:span children
                        var spanTexts = element.Descendants()
                            .Where(e => e.Name.LocalName == "span" && e.Name.NamespaceName.Contains("text"))
                            .SelectMany(e => e.Nodes())
                            .Where(n => n.NodeType == System.Xml.XmlNodeType.Text)
                            .Select(n => ((XText)n).Value);

                        var fullText = text + string.Concat(spanTexts);

                        if (!string.IsNullOrWhiteSpace(fullText))
                        {
                            xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                            handler.Characters(fullText.ToCharArray(), 0, fullText.Length);
                            xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                        }
                    }

                    // Extract metadata from meta.xml
                    ExtractMetadata(archive, metadata);
                }
            }
            catch (InvalidDataException)
            {
                // Not a valid ODF file
            }
            catch (XmlException)
            {
                // Not a valid ODF file
            }

            xhtml.EndDocument();
        }

        private static void ExtractMetadata(ZipArchive archive, Metadata metadata)
        {
            var metaEntry = archive.GetEntry("meta.xml");
            if (metaEntry == null) return;

            using var entryStream = metaEntry.Open();
            var doc = LoadXmlSafely(entryStream);

            var title = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "title");
            if (title != null && !string.IsNullOrEmpty(title.Value))
                metadata.Set(TikaCoreProperties.TITLE, title.Value);

            var creator = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "creator");
            if (creator != null && !string.IsNullOrEmpty(creator.Value))
                metadata.Set(TikaCoreProperties.CREATOR, creator.Value);

            var description = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "description");
            if (description != null && !string.IsNullOrEmpty(description.Value))
                metadata.Set(TikaCoreProperties.DESCRIPTION, description.Value);
        }

        private static string? DetectOdfType(Azrng.NTika.Core.IO.TikaInputStream stream)
        {
            if (!stream.CanSeek) return null;

            var savedPosition = stream.Position;
            try
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
                var mimeTypeEntry = archive.GetEntry("mimetype");
                if (mimeTypeEntry != null)
                {
                    using var reader = new StreamReader(mimeTypeEntry.Open());
                    return reader.ReadToEnd().Trim();
                }
            }
            catch (InvalidDataException)
            {
                // Not a ZIP file
            }
            finally
            {
                stream.Position = savedPosition;
            }

            return null;
        }

        private static XDocument LoadXmlSafely(Stream stream)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var reader = XmlReader.Create(stream, settings);
            return XDocument.Load(reader);
        }
    }
}
