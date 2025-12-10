using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 文本压缩解压缩帮助类
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp
    /// </remarks>
    public static class CompressHelper
    {
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Base64格式字符串</returns>
        public static string Compress(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }

            return Convert.ToBase64String(mso.ToArray());
        }

        // TODO 代完善
        // /// <summary>
        // /// 压缩
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="model"></param>
        // /// <returns>Base64格式字符串</returns>
        // public static string Compress<T>(T model)
        // {
        //     if (model == null)
        //         return default;
        //     var s = JsonConvert.SerializeObject(model);
        //     return Compress(s);
        // }

        /// <summary>
        /// 从Base64String解压
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Decompress(string str)
        {
            var bytes = Convert.FromBase64String(str);
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }

        // TODO 代完善
        // /// <summary>
        // /// 从Base64String解压
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="s"></param>
        // /// <returns></returns>
        // public static T Decompress<T>(string s)
        // {
        //     if (string.IsNullOrWhiteSpace(s))
        //     {
        //         return default;
        //     }
        //
        //     var str = Decompress(s);
        //     return JsonConvert.DeserializeObject<T>(str);
        // }
    }
}