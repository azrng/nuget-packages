namespace Azrng.NTika.Core.Config
{
    public class OutputLimits
    {
        public int MaxStringLength { get; set; } = 100_000;

        public override bool Equals(object? obj)
        {
            if (obj is not OutputLimits other) return false;
            return MaxStringLength == other.MaxStringLength;
        }

        public override int GetHashCode()
        {
            return MaxStringLength.GetHashCode();
        }
    }
}
