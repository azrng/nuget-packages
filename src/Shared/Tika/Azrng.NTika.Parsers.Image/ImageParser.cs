using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Azrng.NTika.Parsers.Image
{
    public class ImageParser : IParser
    {
        private static readonly MediaType ImageJpeg = MediaType.Image("jpeg");
        private static readonly MediaType ImagePng = MediaType.Image("png");
        private static readonly MediaType ImageGif = MediaType.Image("gif");
        private static readonly MediaType ImageTiff = MediaType.Image("tiff");
        private static readonly MediaType ImageBmp = MediaType.Image("bmp");
        private static readonly MediaType ImageWebp = MediaType.Image("webp");

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType>
            {
                ImageJpeg, ImagePng, ImageGif, ImageTiff, ImageBmp, ImageWebp
            };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = DetectImageType(stream);
                if (!string.IsNullOrEmpty(contentType))
                    metadata.Set(TikaCoreProperties.CONTENT_TYPE, contentType);
            }

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            stream.Rewind();
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(stream);

                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        var text = $"{tag.Name}: {tag.Description}";
                        xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                        handler.Characters(text.ToCharArray(), 0, text.Length);
                        xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);

                        // Extract specific metadata
                        ExtractMetadata(tag, metadata);
                    }
                }
            }
            catch (ImageProcessingException)
            {
                // Not a recognized image format
            }

            xhtml.EndDocument();
        }

        private static void ExtractMetadata(MetadataExtractor.Tag tag, Metadata metadata)
        {
            var name = tag.Name;
            var value = tag.Description;
            if (string.IsNullOrEmpty(value)) return;

            switch (name)
            {
                case "Image Width":
                case "Image Height":
                case "Exif Image Width":
                case "Exif Image Height":
                    // Store as description
                    break;
                case "Date/Time Original":
                case "Date/Time":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.CREATED)))
                        metadata.Set(TikaCoreProperties.CREATED, value);
                    break;
                case "Image Description":
                case "Description":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.DESCRIPTION)))
                        metadata.Set(TikaCoreProperties.DESCRIPTION, value);
                    break;
                case "Artist":
                case "Author":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.CREATOR)))
                        metadata.Set(TikaCoreProperties.CREATOR, value);
                    break;
                case "Copyright":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.RIGHTS)))
                        metadata.Set(TikaCoreProperties.RIGHTS, value);
                    break;
            }
        }

        private static string? DetectImageType(Azrng.NTika.Core.IO.TikaInputStream stream)
        {
            if (!stream.CanSeek) return null;

            var savedPosition = stream.Position;
            stream.Position = 0;

            var header = new byte[12];
            var bytesRead = stream.Read(header, 0, header.Length);
            stream.Position = savedPosition;

            if (bytesRead < 4) return null;

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return "image/jpeg";

            // PNG: 89 50 4E 47
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return "image/png";

            // GIF: 47 49 46 38
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                return "image/gif";

            // BMP: 42 4D
            if (header[0] == 0x42 && header[1] == 0x4D)
                return "image/bmp";

            // TIFF: 49 49 2A 00 (little-endian) or 4D 4D 00 2A (big-endian)
            if ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00) ||
                (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A))
                return "image/tiff";

            // WebP: RIFF....WEBP
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                bytesRead >= 12 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                return "image/webp";

            return null;
        }
    }
}
