using System.Collections.Generic;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Abstractions
{
    public interface ILanguageDetector
    {
        LanguageResult? Detect(string text);

        IList<LanguageResult> DetectAll(string text);
    }
}
