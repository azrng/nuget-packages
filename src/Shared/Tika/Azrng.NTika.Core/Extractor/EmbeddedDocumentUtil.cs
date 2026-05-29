using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.Model;

namespace Azrng.NTika.Core.Extractor
{
    public static class EmbeddedDocumentUtil
    {
        public static IEmbeddedDocumentExtractor GetEmbeddedDocumentExtractor(ParseContext context)
        {
            var existing = context.Get<IEmbeddedDocumentExtractor>();
            if (existing != null)
                return existing;

            var limits = context.Get<EmbeddedLimits>() ?? new EmbeddedLimits();
            var extractor = new ParsingEmbeddedDocumentExtractor(limits);
            context.Set<IEmbeddedDocumentExtractor>(extractor);
            return extractor;
        }

        public static int GetCurrentDepth(ParseContext context)
        {
            var depthObj = context.Get<EmbeddedDepthTracker>();
            return depthObj?.Depth ?? 0;
        }

        public static void PushDepth(ParseContext context)
        {
            var tracker = context.Get<EmbeddedDepthTracker>();
            if (tracker == null)
            {
                tracker = new EmbeddedDepthTracker();
                context.Set(tracker);
            }

            var limits = context.Get<EmbeddedLimits>() ?? new EmbeddedLimits();
            if (limits.MaxEmbeddedDepth >= 0 && tracker.Depth >= limits.MaxEmbeddedDepth)
            {
                throw new EmbeddedLimitReachedException(
                    $"Embedded document depth ({tracker.Depth + 1}) exceeds the configured limit ({limits.MaxEmbeddedDepth}).");
            }

            tracker.Depth++;
        }

        public static void PopDepth(ParseContext context)
        {
            var tracker = context.Get<EmbeddedDepthTracker>();
            if (tracker != null && tracker.Depth > 0)
                tracker.Depth--;
        }

        public class EmbeddedDepthTracker
        {
            public int Depth { get; set; }
        }
    }
}
