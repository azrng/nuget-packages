using System.Collections.Generic;
using System.IO;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using YamlDotNet.RepresentationModel;

namespace Azrng.NTika.Parsers.Data
{
    public class YamlParser : IParser
    {
        private static readonly MediaType ApplicationYaml = MediaType.Application("x-yaml");
        private static readonly MediaType TextYaml = MediaType.Text("yaml");

        public ISet<MediaType> GetSupportedTypes(ParseContext context)
        {
            return new HashSet<MediaType> { ApplicationYaml, TextYaml };
        }

        public void Parse(Azrng.NTika.Core.IO.TikaInputStream stream, IContentHandler handler,
            Metadata metadata, ParseContext context)
        {
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/x-yaml");

            var xhtml = new XHTMLContentHandler(handler);
            xhtml.StartDocument();

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var yaml = reader.ReadToEnd();

            if (string.IsNullOrWhiteSpace(yaml))
            {
                xhtml.EndDocument();
                return;
            }

            try
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StringReader(yaml));

                foreach (var document in yamlStream.Documents)
                {
                    WalkYamlNode(document.RootNode, handler, xhtml);
                }
            }
            catch
            {
                // If YAML is invalid, emit raw text
                xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                handler.Characters(yaml.ToCharArray(), 0, yaml.Length);
                xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
            }

            xhtml.EndDocument();
        }

        private static void WalkYamlNode(YamlNode node, IContentHandler handler, XHTMLContentHandler xhtml)
        {
            switch (node)
            {
                case YamlScalarNode scalar:
                    var value = scalar.Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                        handler.Characters(value.ToCharArray(), 0, value.Length);
                        xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                    }
                    break;

                case YamlMappingNode mapping:
                    foreach (var entry in mapping.Children)
                    {
                        // Emit key
                        if (entry.Key is YamlScalarNode keyScalar && !string.IsNullOrEmpty(keyScalar.Value))
                        {
                            xhtml.StartElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P, new AttributesImpl());
                            handler.Characters(keyScalar.Value.ToCharArray(), 0, keyScalar.Value.Length);
                            xhtml.EndElement(XHTMLContentHandler.Namespace, XHTMLContentHandler.P, XHTMLContentHandler.P);
                        }

                        // Recurse into value
                        WalkYamlNode(entry.Value, handler, xhtml);
                    }
                    break;

                case YamlSequenceNode sequence:
                    foreach (var item in sequence.Children)
                    {
                        WalkYamlNode(item, handler, xhtml);
                    }
                    break;
            }
        }
    }
}
