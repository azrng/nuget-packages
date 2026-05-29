namespace Azrng.NTika.Core.Utils
{
    public static class StringUtils
    {
        public static bool IsBlank(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotBlank(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
