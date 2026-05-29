using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Parser
{
    public class EmptyTranslator : ITranslator
    {
        public static readonly EmptyTranslator Instance = new();

        public bool IsAvailable => false;

        public string Translate(string text, string sourceLanguage, string targetLanguage)
        {
            return text;
        }

        public string Translate(string text, string targetLanguage)
        {
            return text;
        }
    }
}
