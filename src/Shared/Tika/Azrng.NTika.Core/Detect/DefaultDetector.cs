using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Detect
{
    public class DefaultDetector : CompositeDetector
    {
        public DefaultDetector()
            : base(new MediaTypeRegistry(), new MagicDetector(), new NameDetector(), new TextDetector())
        {
        }

        public DefaultDetector(MediaTypeRegistry registry)
            : base(registry, new MagicDetector(), new NameDetector(), new TextDetector())
        {
        }
    }
}
