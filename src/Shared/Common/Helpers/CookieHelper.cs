//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.Security;
//using System.Web;

//namespace Common.Helpers
//{
//    /// <summary>
//    /// 浏览器Cookie的缓存
//    /// </summary>
//    public class CookieHelper
//    {
//        /// <summary>
//        /// 存储Cookie
//        /// </summary>
//        /// <param name="strName">键值</param>
//        /// <param name="strValue">存储内容</param>
//        public static void WriteCookie(string strName, string strValue)
//        {
//            var cookie = HttpContextManager.Current.Request.Cookies[strName];
//            if (cookie == null)
//            {
//                cookie = new HttpCookie(strName);
//            }
//            cookie.Value = HttpUtility.UrlEncode(strValue);
//            HttpContext.Current.Response.AppendCookie(cookie);
//        }

//        /// <summary>
//        /// 按有效期保存Cookie
//        /// </summary>
//        /// <param name="strName">键值</param>
//        /// <param name="strValue">存储内容</param>
//        /// <param name="expires">过期时间（小时）</param>
//        public static void WriteCookie(string strName, string strValue, int expires)
//        {
//            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
//            if (cookie == null)
//            {
//                cookie = new HttpCookie(strName);
//            }
//            cookie.Value = HttpUtility.UrlEncode(strValue);
//            cookie.Expires = DateTime.Now.AddHours(expires);
//            HttpContext.Current.Response.AppendCookie(cookie);
//        }

//        /// <summary>
//        /// 获取Cookie内容
//        /// </summary>
//        /// <param name="strName">键值</param>
//        /// <returns></returns>
//        public static string GetCookie(string strName)
//        {
//            string result;
//            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[strName] != null)
//            {
//                result = HttpContext.Current.Request.Cookies[strName].Value.ToString();
//            }
//            else
//            {
//                result = "";
//            }
//            return HttpUtility.UrlDecode(result);
//        }

//        bool IsTicketExpiration()
//        {
//            try
//            {
//                string cookiename = FormsAuthentication.FormsCookieName;
//                if (System.Web.HttpContext.Current.Request.Cookies[cookiename] == null)
//                {
//                    return true;
//                }
//                FormsAuthenticationTicket Ticket = FormsAuthentication.Decrypt(HttpContext.Current.Request.Cookies[cookiename].Value);
//                if (Ticket != null)
//                {
//                    DateTime Expiration = Ticket.Expiration;
//                    if (DateTime.Compare(Expiration, DateTime.Now) < 0)
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//                return true;
//            }
//            catch
//            {
//                return true;
//            }
//        }

//        /// <summary>
//        /// 存cookie
//        /// </summary>
//        /// <param name="info"></param>
//        /// <param name="sCookieName"></param>
//        public void SaveCookie(string info, string sCookieName)
//        {
//            var XmlUserData = info;

//            FormsAuthentication.Initialize();
//            var ticket = new FormsAuthenticationTicket(
//                1,
//                sCookieName,
//                DateTime.Now,
//                DateTime.Now.AddHours(24),
//                false,
//                XmlUserData
//                );

//            var cookie = new HttpCookie(
//                FormsAuthentication.FormsCookieName,
//                FormsAuthentication.Encrypt(ticket));
//            cookie.Expires = DateTime.Now.AddHours(24);
//            HttpContext.Current.Response.Cookies.Set(cookie);

//        }


//        public string GetCookie()
//        {
//            if (!IsTicketExpiration())
//            {
//                if (HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] != null)
//                {
//                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value);
//                    return ticket.UserData;
//                }
//                else
//                {
//                    return "";
//                }
//            }
//            else
//            {
//                FormsAuthentication.SignOut();
//                return "";
//            }

//        }
//    }
//}
