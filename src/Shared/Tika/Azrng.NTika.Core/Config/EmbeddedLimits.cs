namespace Azrng.NTika.Core.Config
{
    public class EmbeddedLimits
    {
        public int MaxEmbeddedResources { get; set; } = 1000;
        public long MaxEmbeddedBytes { get; set; } = 100 * 1024 * 1024;
        public int MaxEmbeddedDepth { get; set; } = 10;
        public int MaxEmbeddedCount { get; set; } = 1000;

        public override bool Equals(object? obj)
        {
            if (obj is not EmbeddedLimits other) return false;
            return MaxEmbeddedResources == other.MaxEmbeddedResources &&
                   MaxEmbeddedBytes == other.MaxEmbeddedBytes &&
                   MaxEmbeddedDepth == other.MaxEmbeddedDepth &&
                   MaxEmbeddedCount == other.MaxEmbeddedCount;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(MaxEmbeddedResources, MaxEmbeddedBytes, MaxEmbeddedDepth, MaxEmbeddedCount);
        }
    }
}
