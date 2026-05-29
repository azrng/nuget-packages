using System;
using System.Globalization;

namespace Azrng.NTika.Core.Utils
{
    public static class DateUtils
    {
        public static string FormatDate(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        public static DateTime? TryParse(string? date)
        {
            if (string.IsNullOrEmpty(date)) return null;

            if (DateTime.TryParse(date, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out var result))
            {
                return result;
            }
            return null;
        }
    }
}
