using Microsoft.International.Converters.PinYinConverter;

namespace Common.Helpers
{
    /// <summary>
    /// 拼音帮助类
    /// </summary>
    public static class PinYinHelper
    {
        /// <summary>
        /// 汉字转化为大写拼音
        /// </summary>
        /// <param name="str">汉字</param>
        /// <returns>全拼</returns>
        public static string GetPinyinQuanPin(string str)
        {
            var r = string.Empty;
            foreach (var obj in str)
            {
                try
                {
                    var chineseChar = new ChineseChar(obj);
                    var t = chineseChar.Pinyins[0];
                    r += t.Substring(0, t.Length - 1);
                }
                catch
                {
                    r += obj.ToString();
                }
            }
            return r;
        }

        /// <summary>
        /// 汉字转化为拼音大写首字母
        /// </summary>
        /// <param name="str">汉字</param>
        /// <returns>首字母</returns>
        public static string GetFirstPinyin(string str)
        {
            string r = string.Empty;
            foreach (char obj in str)
            {
                try
                {
                    var chineseChar = new ChineseChar(obj);
                    string t = chineseChar.Pinyins[0];
                    r += t.Substring(0, 1);
                }
                catch
                {
                    r += obj.ToString();
                }
            }
            return r;
        }
    }
}