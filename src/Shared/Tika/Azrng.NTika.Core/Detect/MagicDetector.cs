using System;
using System.Collections.Generic;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Detect
{
    public class MagicDetector : IDetector
    {
        private static readonly List<MagicPattern> Patterns = new()
        {
            // PDF: %PDF
            new MagicPattern("application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 }, 0),

            // ZIP / Office Open XML / ODF
            new MagicPattern("application/zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0),

            // GZIP
            new MagicPattern("application/gzip", new byte[] { 0x1F, 0x8B }, 0),

            // 7Z
            new MagicPattern("application/x-7z-compressed", new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, 0),

            // RAR
            new MagicPattern("application/x-rar-compressed", new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 }, 0),

            // PNG
            new MagicPattern("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0),

            // JPEG
            new MagicPattern("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF }, 0),

            // GIF87a
            new MagicPattern("image/gif", Encoding.ASCII.GetBytes("GIF87a"), 0),
            // GIF89a
            new MagicPattern("image/gif", Encoding.ASCII.GetBytes("GIF89a"), 0),

            // BMP
            new MagicPattern("image/bmp", new byte[] { 0x42, 0x4D }, 0),

            // TIFF (little-endian)
            new MagicPattern("image/tiff", new byte[] { 0x49, 0x49, 0x2A, 0x00 }, 0),
            // TIFF (big-endian)
            new MagicPattern("image/tiff", new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, 0),

            // MP3: ID3 tag
            new MagicPattern("audio/mpeg", new byte[] { 0x49, 0x44, 0x33 }, 0),

            // WAV: RIFF....WAVE
            new MagicPattern("audio/vnd.wave", Encoding.ASCII.GetBytes("RIFF"), 0),

            // MP4/M4A: ftyp at offset 4
            new MagicPattern("video/mp4", Encoding.ASCII.GetBytes("ftyp"), 4),

            // OGG
            new MagicPattern("audio/ogg", Encoding.ASCII.GetBytes("OggS"), 0),

            // FLV
            new MagicPattern("video/x-flv", new byte[] { 0x46, 0x4C, 0x56 }, 0),

            // SQLite
            new MagicPattern("application/x-sqlite3", Encoding.ASCII.GetBytes("SQLite format 3\0"), 0),

            // Java class
            new MagicPattern("application/java-vm", new byte[] { 0xCA, 0xFE, 0xBA, 0xBE }, 0),

            // ELF
            new MagicPattern("application/x-executable", new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, 0),

            // Mach-O (32-bit)
            new MagicPattern("application/x-mach-binary", new byte[] { 0xFE, 0xED, 0xFA, 0xCE }, 0),
            // Mach-O (64-bit)
            new MagicPattern("application/x-mach-binary", new byte[] { 0xFE, 0xED, 0xFA, 0xCF }, 0),

            // RTF
            new MagicPattern("application/rtf", Encoding.ASCII.GetBytes("{\\rtf"), 0),

            // XML (text/xml or application/xml)
            new MagicPattern("application/xml", Encoding.ASCII.GetBytes("<?xml"), 0),
        };

        public MediaType Detect(TikaInputStream? stream, Metadata metadata, ParseContext parseContext)
        {
            if (stream == null || !stream.CanSeek)
                return MediaType.OctetStream;

            var savedPosition = stream.Position;
            try
            {
                stream.Position = 0;
                var header = new byte[16];
                var bytesRead = stream.Read(header, 0, header.Length);

                if (bytesRead < 2)
                    return MediaType.OctetStream;

                foreach (var pattern in Patterns)
                {
                    if (pattern.Matches(header, bytesRead))
                    {
                        var result = MediaType.Parse(pattern.MediaType);
                        if (result != null)
                            return result;
                    }
                }

                // TAR: check for "ustar" at offset 257
                if (bytesRead >= 8)
                {
                    stream.Position = 257;
                    var tarMagic = new byte[5];
                    var read = stream.Read(tarMagic, 0, tarMagic.Length);
                    if (read >= 5 && tarMagic[0] == 0x75 && tarMagic[1] == 0x73 &&
                        tarMagic[2] == 0x74 && tarMagic[3] == 0x61 && tarMagic[4] == 0x72)
                    {
                        return MediaType.Parse("application/x-tar") ?? MediaType.OctetStream;
                    }
                }

                return MediaType.OctetStream;
            }
            finally
            {
                stream.Position = savedPosition;
            }
        }

        private class MagicPattern
        {
            public string MediaType { get; }
            private readonly byte[] _bytes;
            private readonly int _offset;

            public MagicPattern(string mediaType, byte[] bytes, int offset)
            {
                MediaType = mediaType;
                _bytes = bytes;
                _offset = offset;
            }

            public bool Matches(byte[] header, int bytesRead)
            {
                if (_offset + _bytes.Length > bytesRead)
                    return false;

                for (int i = 0; i < _bytes.Length; i++)
                {
                    if (header[_offset + i] != _bytes[i])
                        return false;
                }

                return true;
            }
        }
    }
}
