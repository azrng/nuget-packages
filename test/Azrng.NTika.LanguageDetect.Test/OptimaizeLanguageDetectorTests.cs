using Azrng.NTika.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.LanguageDetect.Test
{
    public class OptimaizeLanguageDetectorTests
    {
        [Fact]
        public void Detect_EnglishText_ShouldDetectEnglish()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "The quick brown fox jumps over the lazy dog. This is a simple test sentence in English.";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("en");
            result.Confidence.Should().Be(LanguageConfidence.HIGH);
        }

        [Fact]
        public void Detect_ChineseText_ShouldDetectChinese()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "这是一段中文测试文本，用于检测语言识别功能是否正常工作。";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("zh");
        }

        [Fact]
        public void Detect_JapaneseText_ShouldDetectJapanese()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "これは日本語のテストテキストです。言語検出機能をテストしています。";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("ja");
        }

        [Fact]
        public void Detect_FrenchText_ShouldDetectFrench()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "Ceci est un texte de test en francais pour verifier la detection de langue.";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("fr");
        }

        [Fact]
        public void Detect_GermanText_ShouldDetectGerman()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "Dies ist ein deutscher Testtext zur Spracherkennung. Der Text enthalt viele deutsche Woerter.";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("de");
        }

        [Fact]
        public void Detect_SpanishText_ShouldDetectSpanish()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "Hola, este es un texto en espanol. La vida es bella y el mundo es grande. Cada dia es una nueva oportunidad para hacer algo bueno.";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("es");
        }

        [Fact]
        public void Detect_EmptyText_ShouldReturnNull()
        {
            var detector = new OptimaizeLanguageDetector();
            var result = detector.Detect("");

            result.Should().BeNull();
        }

        [Fact]
        public void Detect_NullText_ShouldReturnNull()
        {
            var detector = new OptimaizeLanguageDetector();
            var result = detector.Detect(null!);

            result.Should().BeNull();
        }

        [Fact]
        public void DetectAll_MixedContent_ShouldReturnMultipleResults()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "Hello world. Bonjour le monde.";
            var results = detector.DetectAll(text);

            results.Should().NotBeEmpty();
            results.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact]
        public void Detect_RussianText_ShouldDetectRussian()
        {
            var detector = new OptimaizeLanguageDetector();
            var text = "Это тестовый текст на русском языке для проверки определения языка.";
            var result = detector.Detect(text);

            result.Should().NotBeNull();
            result!.Language.Should().Be("ru");
        }
    }
}
