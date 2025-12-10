using System;

namespace Azrng.Core.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatFileSize(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) { return $"{bytes} B"; }

            var exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }
}