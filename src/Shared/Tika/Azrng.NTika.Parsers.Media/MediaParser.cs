using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using MetadataExtractor;

namespace Azrng.NTika.Parsers.Media
{
    public class MediaParser : IParser
    {
        private static readonly MediaType AudioMpeg = MediaType.Parse("audio/mpeg")!;
        private static readonly MediaType AudioWav = MediaType.Parse("audio/vnd.wave")!;
        private static readonly MediaType VideoMp4 = MediaType.Parse("video/mp4")!;
        private static readonly MediaType AudioMp4 = MediaType.Parse("audio/mp4")!;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType>
            {
                AudioMpeg, AudioWav, VideoMp4, AudioMp4
            };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = DetectMediaType(stream);
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

                        ExtractMetadata(tag, metadata);
                    }
                }
            }
            catch (ImageProcessingException)
            {
                // Not a recognized format
            }

            xhtml.EndDocument();
        }

        private static void ExtractMetadata(MetadataExtractor.Tag tag, Metadata metadata)
        {
            var value = tag.Description;
            if (string.IsNullOrEmpty(value)) return;

            switch (tag.Name)
            {
                case "Title":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.TITLE)))
                        metadata.Set(TikaCoreProperties.TITLE, value);
                    break;
                case "Artist":
                case "Author":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.CREATOR)))
                        metadata.Set(TikaCoreProperties.CREATOR, value);
                    break;
                case "Album":
                case "Description":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.DESCRIPTION)))
                        metadata.Set(TikaCoreProperties.DESCRIPTION, value);
                    break;
                case "Genre":
                    if (string.IsNullOrEmpty(metadata.Get(TikaCoreProperties.SUBJECT)))
                        metadata.Set(TikaCoreProperties.SUBJECT, value);
                    break;
            }
        }

        private static string? DetectMediaType(Azrng.NTika.Core.IO.TikaInputStream stream)
        {
            if (!stream.CanSeek) return null;

            var savedPosition = stream.Position;
            stream.Position = 0;

            var header = new byte[12];
            var bytesRead = stream.Read(header, 0, header.Length);
            stream.Position = savedPosition;

            if (bytesRead < 4) return null;

            // MP3: ID3 tag or MPEG sync
            if ((header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33) ||
                (header[0] == 0xFF && (header[1] & 0xE0) == 0xE0))
                return "audio/mpeg";

            // WAV: RIFF....WAVE
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                bytesRead >= 12 && header[8] == 0x57 && header[9] == 0x41 && header[10] == 0x56 && header[11] == 0x45)
                return "audio/vnd.wave";

            // MP4/M4A: ftyp
            if (header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70)
                return "video/mp4";

            return null;
        }
    }
}
