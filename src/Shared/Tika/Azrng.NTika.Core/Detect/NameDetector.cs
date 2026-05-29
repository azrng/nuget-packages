using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Detect
{
    public class NameDetector : IDetector
    {
        private static readonly ConcurrentDictionary<string, MediaType> ExtensionMap = new(StringComparer.OrdinalIgnoreCase);

        static NameDetector()
        {
            // Add some common extensions as fallback
            ExtensionMap.TryAdd("pdf", MediaType.Application("pdf"));
            ExtensionMap.TryAdd("html", MediaType.TextHtml);
            ExtensionMap.TryAdd("htm", MediaType.TextHtml);
            ExtensionMap.TryAdd("xml", MediaType.ApplicationXml);
            ExtensionMap.TryAdd("txt", MediaType.TextPlain);
            ExtensionMap.TryAdd("zip", MediaType.ApplicationZip);

            LoadFromResource();
        }

        public MediaType Detect(TikaInputStream? stream, Metadata metadata, ParseContext parseContext)
        {
            var name = metadata.Get(TikaCoreProperties.RESOURCE_NAME_KEY);
            if (string.IsNullOrEmpty(name))
            {
                return MediaType.OctetStream;
            }

            var extension = Path.GetExtension(name);
            if (string.IsNullOrEmpty(extension))
            {
                return MediaType.OctetStream;
            }

            // Remove leading dot
            extension = extension.TrimStart('.');

            return ExtensionMap.TryGetValue(extension, out var type) ? type : MediaType.OctetStream;
        }

        private static void LoadFromResource()
        {
            try
            {
                var assembly = typeof(NameDetector).Assembly;

                // Try multiple resource name patterns
                Stream? stream = null;
                foreach (var resourceName in new[]
                {
                    "Azrng.NTika.Core.Resources.tika-mimetypes.xml",
                    "Azrng.NTika.Core.tika-mimetypes.xml",
                    "tika-mimetypes.xml"
                })
                {
                    stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null) break;
                }

                if (stream == null) return;

                using (stream)
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    // The XML has a default namespace, so we need to use local-name()
                    var nsManager = new XmlNamespaceManager(doc.NameTable);
                    nsManager.AddNamespace("tika", "https://tika.apache.org/");

                    // Use local-name() to match elements regardless of namespace
                    var mimeTypes = doc.SelectNodes("//*[local-name()='mime-type']");
                    if (mimeTypes == null) return;

                    foreach (XmlNode mimeType in mimeTypes)
                    {
                        var typeName = mimeType.Attributes?["type"]?.Value;
                        if (string.IsNullOrEmpty(typeName)) continue;

                        var mediaType = MediaType.Parse(typeName);
                        if (mediaType == null) continue;

                        // Use local-name() for glob elements too
                        var globs = mimeType.SelectNodes("*[local-name()='glob']");
                        if (globs == null) continue;

                        foreach (XmlNode glob in globs)
                        {
                            var pattern = glob.Attributes?["pattern"]?.Value;
                            if (string.IsNullOrEmpty(pattern)) continue;

                            // Simple extension patterns like "*.pdf"
                            if (pattern.StartsWith("*.") && !pattern.Contains('*'))
                            {
                                var ext = pattern[2..];
                                ExtensionMap.TryAdd(ext, mediaType);
                            }
                        }
                    }
                }
            }
            catch
            {
                // If resource loading fails, we'll just have an empty map
            }
        }
    }
}
