namespace Azrng.DistributeLock.PostgreSql
{
    internal static class SystemDateTime
    {
        public static DateTime Now()
        {
            return DateTime.SpecifyKind(DateTime.UtcNow.AddHours(8), DateTimeKind.Unspecified);
        }
    }
}