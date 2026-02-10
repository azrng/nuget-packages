using System.Text;
using System.Web;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 编码操作类HttpUtility
    /// </summary>
    public static class UrlExtensions
    {
        /// <summary>
        /// UrlEncode
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string? UrlEncode(this string? target)
        {
            return target?.UrlEncode(Encoding.UTF8);
        }

        /// <summary>
        /// UrlEncode
        /// </summary>
        /// <param name="target"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string? UrlEncode(this string? target, Encoding encoding)
        {
            return target is null ? null : HttpUtility.UrlEncode(target, encoding);
        }

        /// <summary>
        /// UrlDecode
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string? UrlDecode(this string? target)
        {
            return target?.UrlDecode(Encoding.UTF8);
        }

        /// <summary>
        /// UrlDecode
        /// </summary>
        /// <param name="target"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string? UrlDecode(this string? target, Encoding encoding)
        {
            return target == null ? null : HttpUtility.UrlDecode(target, encoding);
        }

        /// <summary>
        /// AttributeEncode
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string AttributeEncode(this string target)
        {
            return HttpUtility.HtmlAttributeEncode(target);
        }

        /// <summary>
        /// HtmlEncode
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string target)
        {
            return HttpUtility.HtmlEncode(target);
        }

        /// <summary>
        /// HtmlDecode
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string HtmlDecode(this string target)
        {
            return HttpUtility.HtmlDecode(target);
        }
    }
}