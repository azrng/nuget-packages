namespace Azrng.NTika.Core.Config
{
    public class TimeoutLimits
    {
        public long ParseTimeoutMillis { get; set; } = -1;
        public long ReadLimit { get; set; } = -1;

        public override bool Equals(object? obj)
        {
            if (obj is not TimeoutLimits other) return false;
            return ParseTimeoutMillis == other.ParseTimeoutMillis &&
                   ReadLimit == other.ReadLimit;
        }

        public override int GetHashCode()
        {
            return ParseTimeoutMillis.GetHashCode() ^ ReadLimit.GetHashCode();
        }
    }
}
