namespace Azrng.SqlMigration
{
    internal static class Extension
    {
        public static string ReplaceIfNotNullOrWhiteSpace(this string value, string? oldValue, string newValue)
        {
            return string.IsNullOrWhiteSpace(oldValue) ? value : value.Replace(oldValue, newValue);
        }
    }
}