using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.Extractor;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using MimeKit;

namespace Azrng.NTika.Parsers.Email
{
    public class EmlParser : IParser
    {
        private static readonly MediaType MessageRfc822 = MediaType.Parse("message/rfc822")!;

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { MessageRfc822 };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "message/rfc822");

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            stream.Rewind();
            var message = MimeMessage.Load(stream);

            // Extract headers as metadata
            if (!string.IsNullOrEmpty(message.Subject))
                metadata.Set(TikaCoreProperties.TITLE, message.Subject);

            if (message.From.Count > 0)
                metadata.Set(TikaCoreProperties.CREATOR, message.From.ToString());

            if (message.Date != default)
                metadata.Set(TikaCoreProperties.CREATED, message.Date.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            // Emit subject
            if (!string.IsNullOrEmpty(message.Subject))
            {
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                handler.Characters(message.Subject.ToCharArray(), 0, message.Subject.Length);
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
            }

            // Emit body
            var body = message.TextBody ?? ExtractHtmlBodyText(message.HtmlBody);
            if (!string.IsNullOrEmpty(body))
            {
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                handler.Characters(body.ToCharArray(), 0, body.Length);
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
            }

            // Parse attachments if embedded extraction is enabled
            var limits = context.Get<EmbeddedLimits>();
            if (limits != null)
            {
                var extractor = EmbeddedDocumentUtil.GetEmbeddedDocumentExtractor(context);
                var currentDepth = EmbeddedDocumentUtil.GetCurrentDepth(context);

                if (limits.MaxEmbeddedDepth <= 0 || currentDepth < limits.MaxEmbeddedDepth)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        if (!extractor.ShouldParseEmbedded(metadata))
                            break;

                        if (attachment is MimePart mimePart && mimePart.Content != null)
                        {
                            try
                            {
                                var resourceName = mimePart.FileName ?? "attachment";
                                var contentStream = mimePart.Content.Stream;
                                if (contentStream != null && contentStream.CanSeek)
                                {
                                    EmbeddedStreamLimiter.EnsureSizeAllowed(
                                        contentStream.Length,
                                        limits,
                                        resourceName);
                                }

                                using var ms = new LimitedMemoryStream(limits, resourceName);
                                mimePart.Content.DecodeTo(ms);
                                ms.Position = 0;

                                var attachmentMetadata = new Metadata();
                                attachmentMetadata.Set(TikaCoreProperties.RESOURCE_NAME_KEY, resourceName);

                                EmbeddedDocumentUtil.PushDepth(context);
                                try
                                {
                                    extractor.ParseEmbedded(ms, handler, attachmentMetadata, context);
                                }
                                finally
                                {
                                    EmbeddedDocumentUtil.PopDepth(context);
                                }
                            }
                            catch (IOException)
                            {
                                // Skip attachments that can't be parsed
                            }
                            catch (FormatException)
                            {
                                // Skip attachments that can't be parsed
                            }
                        }
                    }
                }
            }

            xhtml.EndDocument();
        }

        private static string? ExtractHtmlBodyText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            var withoutScriptAndStyle = Regex.Replace(
                html,
                @"<(script|style)\b[^>]*>.*?</\1>",
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var withoutTags = Regex.Replace(withoutScriptAndStyle, "<[^>]+>", " ");
            return System.Net.WebUtility.HtmlDecode(withoutTags);
        }
    }
}
