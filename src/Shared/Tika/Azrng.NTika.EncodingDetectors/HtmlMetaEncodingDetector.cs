using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.EncodingDetectors
{
    public partial class HtmlMetaEncodingDetector : IEncodingDetector
    {
        private static readonly HashSet<string> AllowedCharsets = new(StringComparer.OrdinalIgnoreCase)
        {
            "utf-8",
            "utf8",
            "utf-16",
            "utf-16le",
            "utf-16be",
            "us-ascii",
            "ascii",
            "iso-8859-1",
            "windows-1252",
            "gb2312",
            "gbk",
            "big5",
            "shift_jis",
            "euc-jp"
        };

        [GeneratedRegex(@"<meta[^>]+charset\s*=\s*[""']?\s*([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
        private static partial Regex CharsetPattern();

        [GeneratedRegex(@"<meta[^>]+http-equiv\s*=\s*[""']?Content-Type[""']?[^>]+content\s*=\s*[""']?[^;]+;\s*charset=\s*([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
        private static partial Regex HttpEquivPattern();

        public EncodingResult? Detect(Stream stream, Metadata metadata, ParseContext context)
        {
            if (!stream.CanSeek)
                return null;

            var savedPosition = stream.Position;
            stream.Position = 0;

            try
            {
                var buffer = new byte[4096];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    return null;

                var html = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                var match = CharsetPattern().Match(html);
                if (!match.Success)
                    match = HttpEquivPattern().Match(html);

                if (!match.Success)
                    return null;

                var charset = match.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(charset) || !AllowedCharsets.Contains(charset))
                    return null;

                try
                {
                    var encoding = Encoding.GetEncoding(charset);
                    return new EncodingResult(encoding, EncodingConfidence.MEDIUM, nameof(HtmlMetaEncodingDetector));
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            finally
            {
                stream.Position = savedPosition;
            }
        }
    }
}
