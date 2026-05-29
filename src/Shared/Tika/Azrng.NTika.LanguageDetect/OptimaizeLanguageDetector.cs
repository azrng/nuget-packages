using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.LanguageDetect
{
    public class OptimaizeLanguageDetector : ILanguageDetector
    {
        private static readonly Dictionary<string, HashSet<string>> CommonWords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
                "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
                "this", "but", "his", "by", "from", "they", "we", "say", "her", "she",
                "or", "an", "will", "my", "one", "all", "would", "there", "their", "what",
                "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                "when", "make", "can", "like", "time", "no", "just", "him", "know", "take",
                "people", "into", "year", "your", "good", "some", "could", "them", "see",
                "other", "than", "then", "now", "look", "only", "come", "its", "over",
                "think", "also", "back", "after", "use", "two", "how", "our", "work",
                "first", "well", "way", "even", "new", "want", "because", "any", "these",
                "give", "day", "most", "us"
            },
            ["fr"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "le", "la", "les", "de", "des", "un", "une", "et", "est", "en",
                "que", "qui", "dans", "du", "ce", "il", "pas", "pour", "sur", "se",
                "avec", "ne", "son", "par", "je", "au", "plus", "mais", "comme", "ou",
                "sa", "nous", "vous", "ils", "leur", "bien", "tout", "fait", "etre", "avoir",
                "aussi", "cette", "ses", "peut", "meme", "faire", "si", "tres", "quelque",
                "notre", "sans", "avant", "deux", "mon", "elle", "entre", "encore", "tous",
                "autre", "apres", "chez", "toute", "alors", "donc", "sont", "ces", "mes",
                "deja", "moins", "autres", "quand", "rien", "dit", "sous", "toujours"
            },
            ["de"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "der", "die", "und", "in", "den", "von", "zu", "das", "mit", "sich",
                "des", "auf", "ist", "ein", "eine", "dem", "nicht", "auch", "als", "an",
                "werden", "aus", "er", "hat", "dass", "sie", "nach", "wird", "bei", "einer",
                "um", "am", "sind", "noch", "wie", "einem", "einen", "zum", "war", "haben",
                "ich", "ihr", "aber", "wir", "so", "kann", "nur", "sein", "durch", "wenn",
                "seinem", "seine", "hatte", "oder", "vor", "zur", "bis", "mehr", "anderen",
                "seiner", "dann", "schon", "diese", "dieser", "was", "man", "wird", "sehr",
                "etwa", "alle", "Jahre", "Jahr", "zwischen", "immer", "wieder", "beim"
            },
            ["es"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "el", "la", "de", "en", "los", "del", "se", "las", "por", "un",
                "para", "con", "una", "su", "al", "es", "lo", "como", "mas", "pero",
                "sus", "le", "ya", "o", "este", "si", "porque", "esta", "entre", "cuando",
                "muy", "sin", "sobre", "tambien", "me", "hasta", "hay", "donde", "quien",
                "desde", "todo", "nos", "durante", "todos", "uno", "les", "ni", "contra",
                "otros", "ese", "eso", "ante", "ellos", "e", "esto", "antes", "algunos",
                "que", "ser", "son", "fue", "era", "han", "sido", "tiene", "puede", "habia",
                "hacer", "cada", "tiempo", "bien", "tambien", "aqui", "ahora", "donde",
                "parte", "tiene", "estos", "como", "vida", "mundo", "ano", "dia", "vez",
                "hombre", "mujer", "momento", "forma", "caso", "grupo", "lugar", "pais"
            },
            ["pt"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "de", "a", "o", "que", "e", "do", "da", "em", "um", "para",
                "com", "na", "os", "no", "se", "uma", "por", "mais", "dos", "as",
                "como", "mas", "ao", "ele", "das", "tem", "a", "seu", "sua", "ou",
                "ja", "quando", "muito", "nos", "est", "eu", "tambem", "so", "pelo",
                "pela", "ate", "isso", "ela", "entre", "depois", "sem", "mesmo", "aos",
                "seus", "quem", "nas", "esse", "eles", "estao", "tudo", "foi", "tem",
                "ser", "qual", "era", "ter", "desde", "sobre", "entre", "ainda", "cada"
            },
            ["ru"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "и", "в", "не", "на", "я", "что", "то", "он", "как", "она",
                "по", "это", "из", "за", "к", "он", "на", "мы", "они", "бы",
                "про", "но", "вы", "за", "от", "как", "или", "со", "для", "от",
                "был", "все", "его", "еще", "год", "когда", "их", "этой", "так", "была"
            },
            ["ja"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "の", "に", "を", "は", "た", "が", "で", "る", "し", "ま",
                "す", "れ", "か", "ら", "や", "よ", "り", "と", "も", "だ"
            },
            ["ko"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "은", "을", "이", "가", "의", "\xC5D0", "\xB97C", "\xC73C", "\xB85C", "\xB2E4",
                "\xC790", "\xC2DC", "\xC5B4", "\xC2DC", "\xB9C8", "\xC9C0", "\xB9CC", "\xC544", "\xB2C8", "\xAE4C"
            }
        };

        public LanguageResult? Detect(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var results = DetectAll(text);
            return results.FirstOrDefault();
        }

        public IList<LanguageResult> DetectAll(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<LanguageResult>();

            var scores = new Dictionary<string, double>();

            // CJK character ratio detection
            var cjkScores = DetectByCharacterRanges(text);
            foreach (var kvp in cjkScores)
            {
                scores[kvp.Key] = kvp.Value;
            }

            // Common word frequency detection
            var wordScores = DetectByCommonWords(text);
            foreach (var kvp in wordScores)
            {
                if (scores.ContainsKey(kvp.Key))
                    scores[kvp.Key] = (scores[kvp.Key] + kvp.Value) / 2;
                else
                    scores[kvp.Key] = kvp.Value;
            }

            return scores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => new LanguageResult(
                    kvp.Key,
                    kvp.Value > 0.7 ? LanguageConfidence.HIGH :
                    kvp.Value > 0.4 ? LanguageConfidence.MEDIUM :
                    kvp.Value > 0.1 ? LanguageConfidence.LOW :
                    LanguageConfidence.NONE,
                    kvp.Value))
                .ToList();
        }

        private static Dictionary<string, double> DetectByCharacterRanges(string text)
        {
            var scores = new Dictionary<string, double>();
            int totalChars = 0;
            int cjkChars = 0;
            int hiraganaKatakana = 0;
            int hangul = 0;
            int cyrillic = 0;

            foreach (var c in text)
            {
                if (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsDigit(c))
                    continue;

                totalChars++;

                if (c >= 0x4E00 && c <= 0x9FFF) // CJK Unified Ideographs
                    cjkChars++;
                else if (c >= 0x3040 && c <= 0x30FF) // Hiragana + Katakana
                    hiraganaKatakana++;
                else if (c >= 0xAC00 && c <= 0xD7AF) // Hangul
                    hangul++;
                else if (c >= 0x0400 && c <= 0x04FF) // Cyrillic
                    cyrillic++;
            }

            if (totalChars == 0)
                return scores;

            double cjkRatio = (double)cjkChars / totalChars;
            double jpRatio = (double)hiraganaKatakana / totalChars;
            double koRatio = (double)hangul / totalChars;
            double ruRatio = (double)cyrillic / totalChars;

            if (koRatio > 0.1)
                scores["ko"] = Math.Min(1.0, koRatio * 2);

            if (jpRatio > 0.05)
                scores["ja"] = Math.Min(1.0, (cjkRatio + jpRatio) * 0.8);

            if (cjkRatio > 0.1 && jpRatio < 0.05 && koRatio < 0.1)
                scores["zh"] = Math.Min(1.0, cjkRatio * 1.5);

            if (ruRatio > 0.3)
                scores["ru"] = Math.Min(1.0, ruRatio * 1.5);

            return scores;
        }

        private static Dictionary<string, double> DetectByCommonWords(string text)
        {
            var scores = new Dictionary<string, double>();
            var words = Tokenize(text);

            if (words.Count == 0)
                return scores;

            foreach (var lang in CommonWords)
            {
                int matchCount = words.Count(w => lang.Value.Contains(w));
                double ratio = (double)matchCount / words.Count;

                if (ratio > 0.05)
                {
                    scores[lang.Key] = Math.Min(1.0, ratio * 3);
                }
            }

            return scores;
        }

        private static List<string> Tokenize(string text)
        {
            var words = new List<string>();
            var current = new StringBuilder();

            foreach (var c in text)
            {
                if (char.IsLetter(c))
                {
                    current.Append(c);
                }
                else
                {
                    if (current.Length > 0)
                    {
                        words.Add(current.ToString());
                        current.Clear();
                    }
                }
            }

            if (current.Length > 0)
                words.Add(current.ToString());

            return words;
        }
    }
}
