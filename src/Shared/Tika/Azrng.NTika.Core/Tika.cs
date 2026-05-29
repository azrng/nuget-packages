using System;
using System.IO;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Detect;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Parser;
using Azrng.NTika.Core.Sax;

namespace Azrng.NTika.Core
{
    public class Tika
    {
        private readonly IDetector _detector;
        private readonly IParser _parser;
        private readonly IEncodingDetector? _encodingDetector;
        private readonly ILanguageDetector? _languageDetector;
        private int _maxStringLength = 100_000;

        public Tika()
            : this(new DefaultDetector(), new AutoDetectParser())
        {
        }

        public Tika(IDetector detector, IParser parser)
            : this(detector, parser, null, null)
        {
        }

        public Tika(IDetector detector, IParser parser, IEncodingDetector? encodingDetector)
            : this(detector, parser, encodingDetector, null)
        {
        }

        public Tika(IDetector detector, IParser parser, IEncodingDetector? encodingDetector, ILanguageDetector? languageDetector)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _encodingDetector = encodingDetector;
            _languageDetector = languageDetector;
        }

        public Tika(params IParser[] parsers)
            : this(new DefaultDetector(), new AutoDetectParser(new MediaTypeRegistry(), parsers))
        {
        }

        public int MaxStringLength
        {
            get => _maxStringLength;
            set => _maxStringLength = value;
        }

        public string Detect(Stream stream)
        {
            using var tis = TikaInputStream.Get(stream);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var type = _detector.Detect(tis, metadata, context);
            return type.ToString();
        }

        public string Detect(FileInfo file)
        {
            using var tis = TikaInputStream.Get(file);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var type = _detector.Detect(tis, metadata, context);
            return type.ToString();
        }

        public string Detect(string filePath)
        {
            return Detect(new FileInfo(filePath));
        }

        public Metadata Parse(Stream stream)
        {
            using var tis = TikaInputStream.Get(stream);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new ToTextContentHandler();
            _parser.Parse(tis, handler, metadata, context);
            var content = handler.ToString();
            metadata.Set("X-TIKA:content", content);
            DetectLanguage(content, metadata);
            return metadata;
        }

        public Metadata Parse(FileInfo file)
        {
            using var tis = TikaInputStream.Get(file);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new ToTextContentHandler();
            _parser.Parse(tis, handler, metadata, context);
            var content = handler.ToString();
            metadata.Set("X-TIKA:content", content);
            DetectLanguage(content, metadata);
            return metadata;
        }

        public string ToText(Stream stream)
        {
            using var tis = TikaInputStream.Get(stream);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new WriteOutContentHandler(_maxStringLength);
            var bodyHandler = new BodyContentHandler(handler);
            _parser.Parse(tis, bodyHandler, metadata, context);
            return bodyHandler.ToString();
        }

        public string ToText(FileInfo file)
        {
            using var tis = TikaInputStream.Get(file);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new WriteOutContentHandler(_maxStringLength);
            var bodyHandler = new BodyContentHandler(handler);
            _parser.Parse(tis, bodyHandler, metadata, context);
            return bodyHandler.ToString();
        }

        public string ToText(string filePath)
        {
            return ToText(new FileInfo(filePath));
        }

        public string ToHtml(Stream stream)
        {
            using var tis = TikaInputStream.Get(stream);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new ToHTMLContentHandler();
            var bodyHandler = new BodyContentHandler(new LimitedContentHandler(handler, _maxStringLength));
            _parser.Parse(tis, bodyHandler, metadata, context);
            return handler.ToString();
        }

        public string ToHtml(FileInfo file)
        {
            using var tis = TikaInputStream.Get(file);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new ToHTMLContentHandler();
            var bodyHandler = new BodyContentHandler(new LimitedContentHandler(handler, _maxStringLength));
            _parser.Parse(tis, bodyHandler, metadata, context);
            return handler.ToString();
        }

        public string ToHtml(string filePath)
        {
            return ToHtml(new FileInfo(filePath));
        }

        public string ToMarkdown(Stream stream)
        {
            using var tis = TikaInputStream.Get(stream);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new ToMarkdownContentHandler();
            var bodyHandler = new BodyContentHandler(new LimitedContentHandler(handler, _maxStringLength));
            _parser.Parse(tis, bodyHandler, metadata, context);
            return handler.ToString();
        }

        public string ToMarkdown(FileInfo file)
        {
            using var tis = TikaInputStream.Get(file);
            var metadata = new Metadata();
            var context = CreateParseContext();
            var handler = new ToMarkdownContentHandler();
            var bodyHandler = new BodyContentHandler(new LimitedContentHandler(handler, _maxStringLength));
            _parser.Parse(tis, bodyHandler, metadata, context);
            return handler.ToString();
        }

        public string ToMarkdown(string filePath)
        {
            return ToMarkdown(new FileInfo(filePath));
        }

        private ParseContext CreateParseContext()
        {
            var context = new ParseContext();
            if (_encodingDetector != null)
            {
                context.Set(_encodingDetector);
            }
            return context;
        }

        private void DetectLanguage(string content, Metadata metadata)
        {
            if (_languageDetector == null || string.IsNullOrWhiteSpace(content))
                return;

            var result = _languageDetector.Detect(content);
            if (result != null)
            {
                metadata.Set(TikaCoreProperties.LANGUAGE, result.Language);
                metadata.Set(TikaCoreProperties.TIKA_DETECTED_LANGUAGE, result.Language);
            }
        }
    }
}
