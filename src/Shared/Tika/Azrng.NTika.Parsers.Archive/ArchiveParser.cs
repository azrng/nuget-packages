using System.Collections.Generic;
using System;
using System.IO;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.Extractor;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Azrng.NTika.Parsers.Archive
{
    public class ArchiveParser : IParser
    {
        private static readonly MediaType ApplicationZip = MediaType.Application("zip");
        private static readonly MediaType ApplicationXTar = MediaType.Application("x-tar");
        private static readonly MediaType ApplicationGzip = MediaType.Application("gzip");
        private static readonly MediaType ApplicationX7z = MediaType.Application("x-7z-compressed");
        private static readonly MediaType ApplicationXRar = MediaType.Application("x-rar-compressed");

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType>
            {
                ApplicationZip, ApplicationXTar, ApplicationGzip,
                ApplicationX7z, ApplicationXRar
            };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = DetectArchiveType(stream);
                if (!string.IsNullOrEmpty(contentType))
                    metadata.Set(TikaCoreProperties.CONTENT_TYPE, contentType);
            }

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            var limits = context.Get<EmbeddedLimits>();
            var shouldExtractEmbedded = limits != null;
            IEmbeddedDocumentExtractor? extractor = null;
            if (shouldExtractEmbedded)
                extractor = EmbeddedDocumentUtil.GetEmbeddedDocumentExtractor(context);

            stream.Rewind();
            try
            {
                using var archive = ArchiveFactory.Open(stream);
                var entryCount = 0;

                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory) continue;

                    entryCount++;
                    if (limits != null && limits.MaxEmbeddedCount > 0 && entryCount > limits.MaxEmbeddedCount)
                    {
                        throw new EmbeddedLimitReachedException(
                            $"Archive entry count exceeds the configured limit ({limits.MaxEmbeddedCount}).");
                    }

                    var entryName = entry.Key ?? string.Empty;
                    var entryInfo = $"{entryName} ({entry.Size} bytes)";
                    xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                    handler.Characters(entryInfo.ToCharArray(), 0, entryInfo.Length);
                    xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);

                    // Recursively parse embedded documents
                    if (extractor != null && extractor.ShouldParseEmbedded(metadata))
                    {
                        var currentDepth = EmbeddedDocumentUtil.GetCurrentDepth(context);
                        if (limits!.MaxEmbeddedDepth <= 0 || currentDepth < limits.MaxEmbeddedDepth)
                        {
                            try
                            {
                                EmbeddedStreamLimiter.EnsureSizeAllowed(entry.Size, limits, entryName);

                                using var entryStream = entry.OpenEntryStream();
                                using var ms = new MemoryStream();
                                EmbeddedStreamLimiter.CopyTo(entryStream, ms, limits, entryName);
                                ms.Position = 0;

                                var entryMetadata = new Metadata();
                                entryMetadata.Set(TikaCoreProperties.RESOURCE_NAME_KEY, entryName);

                                EmbeddedDocumentUtil.PushDepth(context);
                                try
                                {
                                    extractor.ParseEmbedded(ms, handler, entryMetadata, context);
                                }
                                finally
                                {
                                    EmbeddedDocumentUtil.PopDepth(context);
                                }
                            }
                            catch (InvalidFormatException)
                            {
                                // Skip entries that can't be parsed
                            }
                            catch (IOException)
                            {
                                // Skip entries that can't be parsed
                            }
                        }
                    }
                }

                metadata.Set("entryCount", entryCount.ToString());
            }
            catch (InvalidFormatException)
            {
                // Not a recognized archive format
            }
            catch (IOException)
            {
                // Not a recognized archive format
            }
            catch (InvalidOperationException)
            {
                // Not a recognized archive format
            }

            xhtml.EndDocument();
        }

        private static string? DetectArchiveType(Azrng.NTika.Core.IO.TikaInputStream stream)
        {
            if (!stream.CanSeek) return null;

            var savedPosition = stream.Position;
            stream.Position = 0;

            var header = new byte[8];
            var bytesRead = stream.Read(header, 0, header.Length);
            stream.Position = savedPosition;

            if (bytesRead < 4) return null;

            // ZIP: PK\x03\x04
            if (header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04)
                return "application/zip";

            // GZIP: \x1F\x8B
            if (header[0] == 0x1F && header[1] == 0x8B)
                return "application/gzip";

            // 7Z: 7z\xBC\xAF\x27\x1C
            if (header[0] == 0x37 && header[1] == 0x7A && header[2] == 0xBC && header[3] == 0xAF)
                return "application/x-7z-compressed";

            // RAR: Rar!\x1A\x07
            if (header[0] == 0x52 && header[1] == 0x61 && header[2] == 0x72 && header[3] == 0x21)
                return "application/x-rar-compressed";

            // TAR: check for "ustar" at offset 257
            if (bytesRead >= 8)
            {
                stream.Position = 257;
                var tarMagic = new byte[5];
                var read = stream.Read(tarMagic, 0, tarMagic.Length);
                stream.Position = savedPosition;

                if (read >= 5 && tarMagic[0] == 0x75 && tarMagic[1] == 0x73 &&
                    tarMagic[2] == 0x74 && tarMagic[3] == 0x61 && tarMagic[4] == 0x72)
                    return "application/x-tar";
            }

            return null;
        }
    }
}
