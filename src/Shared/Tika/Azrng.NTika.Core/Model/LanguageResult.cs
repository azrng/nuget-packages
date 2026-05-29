namespace Azrng.NTika.Core.Model
{
    public class LanguageResult
    {
        public LanguageResult(string language, LanguageConfidence confidence, double score)
        {
            Language = language;
            Confidence = confidence;
            Score = score;
        }

        /// <summary>
        /// ISO 639-1 language code (e.g., "en", "zh", "ja")
        /// </summary>
        public string Language { get; }

        public LanguageConfidence Confidence { get; }

        public double Score { get; }

        public override string ToString()
        {
            return $"{Language} ({Confidence}, {Score:F2})";
        }
    }
}
