using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Parsers.Rtf
{
    public class RtfParser : IParser
    {
        private static readonly MediaType ApplicationRtf = MediaType.Application("rtf");

        static RtfParser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { ApplicationRtf };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/rtf");

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var rtf = reader.ReadToEnd();

            var text = ExtractPlainText(rtf);

            if (!string.IsNullOrWhiteSpace(text))
            {
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                handler.Characters(text.ToCharArray(), 0, text.Length);
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
            }

            xhtml.EndDocument();
        }

        private static string ExtractPlainText(string rtf)
        {
            if (string.IsNullOrEmpty(rtf) || !rtf.StartsWith(@"{\rtf"))
                return rtf;

            var sb = new StringBuilder();
            var depth = 0;
            var i = 0;
            var skipGroup = false;
            var codePageEncoding = Encoding.GetEncoding(1252);

            while (i < rtf.Length)
            {
                var c = rtf[i];

                switch (c)
                {
                    case '{':
                        depth++;
                        i++;
                        break;

                    case '}':
                        depth--;
                        if (depth <= 0)
                            return sb.ToString().Trim();
                        skipGroup = false;
                        i++;
                        break;

                    case '\\':
                        i++;
                        if (i >= rtf.Length) break;

                        // Control word
                        if (char.IsLetter(rtf[i]))
                        {
                            var wordStart = i;
                            while (i < rtf.Length && char.IsLetter(rtf[i]))
                                i++;

                            var controlWord = rtf.Substring(wordStart, i - wordStart);

                            var parameterStart = i;
                            while (i < rtf.Length && (char.IsDigit(rtf[i]) || rtf[i] == '-'))
                                i++;
                            var parameter = rtf.Substring(parameterStart, i - parameterStart);

                            // Skip space after control word
                            if (i < rtf.Length && rtf[i] == ' ')
                                i++;

                            if (controlWord == "ansicpg" && int.TryParse(parameter, out var codePage))
                            {
                                try
                                {
                                    codePageEncoding = Encoding.GetEncoding(codePage);
                                }
                                catch (ArgumentException)
                                {
                                    codePageEncoding = Encoding.GetEncoding(1252);
                                }
                            }

                            // Handle special control words
                            if (controlWord == "fonttbl" || controlWord == "colortbl" ||
                                controlWord == "stylesheet" || controlWord == "info" ||
                                controlWord == "pict" || controlWord == "object")
                            {
                                skipGroup = true;
                            }
                            else if (controlWord == "par" || controlWord == "line")
                            {
                                if (!skipGroup) sb.Append('\n');
                            }
                            else if (controlWord == "tab")
                            {
                                if (!skipGroup) sb.Append('\t');
                            }
                            else if (controlWord == "u")
                            {
                                // Unicode character
                                if (int.TryParse(parameter, out var code))
                                {
                                    if (code < 0)
                                        code += 65536;

                                    sb.Append((char)code);
                                }
                            }
                        }
                        else if (rtf[i] == '\'' && i + 2 < rtf.Length)
                        {
                            // Hex encoded character
                            var hex = rtf.Substring(i + 1, 2);
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var code))
                            {
                                if (!skipGroup)
                                {
                                    var bytes = new[] { (byte)code };
                                    sb.Append(codePageEncoding.GetString(bytes));
                                }
                            }
                            i += 3;
                        }
                        else
                        {
                            // Escaped character
                            if (!skipGroup) sb.Append(rtf[i]);
                            i++;
                        }
                        break;

                    default:
                        if (!skipGroup && depth > 0 && c >= 32)
                        {
                            sb.Append(c);
                        }
                        i++;
                        break;
                }
            }

            return sb.ToString().Trim();
        }
    }
}
