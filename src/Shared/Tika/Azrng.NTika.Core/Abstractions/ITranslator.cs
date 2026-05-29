namespace Azrng.NTika.Core.Abstractions
{
    public interface ITranslator
    {
        string Translate(string text, string sourceLanguage, string targetLanguage);
        string Translate(string text, string targetLanguage);
        bool IsAvailable { get; }
    }
}
