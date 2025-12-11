using CommonCollect.Net;
using Microsoft.AspNetCore.Http;
using System;

namespace CommonCollect.Extension
{
    public class ExtCookie
    {
        #region 设置前端cookie

        /// <summary>
        /// 设置前端cookie
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="minutes">过期时长，单位：分钟</param>
        public static void SetCookies(string key, string value, int minutes = 30)
        {
            MyHttpContext.Current.Response.Cookies.Append(key, value, new CookieOptions
            {
                // 重要
                IsEssential = true,
                // 过期时间
                Expires = DateTime.Now.AddMinutes(minutes)
            });
        }

        #endregion 设置前端cookie

        #region 获取前端cookies

        /// <summary>
        /// 获取前端cookies
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回对应的值</returns>
        public static string GetCookies(string key)
        {
            MyHttpContext.Current.Request.Cookies.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            return value;
        }

        #endregion 获取前端cookies

        #region 设置前端cookie

        /// <summary>
        /// 设置前端cookie
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="minutes">过期时长，单位：分钟</param>
        public static void SetCookiesSecure(string key, string value, int minutes = 30)
        {
            MyHttpContext.Current.Response.Cookies.Append(key, value, new CookieOptions
            {
                // HttpOnly设置为true,表明为后台只读模式,前端无法通过JS来获取cookie值,可以有效的防止XXS攻击
                HttpOnly = false,
                // Secure设置为true,就是当你的网站开启了SSL(就是https),的时候,这个cookie值才会被传递
                Secure = true,
                // 过期时间
                Expires = DateTime.Now.AddMinutes(minutes),
                // 重要
                IsEssential = true
            });
        }

        #endregion 设置前端cookie

        #region 删除前端指定的cookie

        /// <summary>
        /// 删除前端指定的cookie
        /// </summary>
        /// <param name="key">键</param>
        public static void DeleteCookies(string key)
        {
            MyHttpContext.Current.Response.Cookies.Delete(key);
        }

        #endregion 删除前端指定的cookie
    }
}