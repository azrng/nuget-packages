namespace Azrng.Core.Extension
{
    /// <summary>
    /// 人扩展
    /// </summary>
    public static class PersonExtensions
    {
        /// <summary>
        /// 根据身份证号码获取出生日期
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        public static string? GetBirthdayByIdCard(this string? idCard)
        {
            try
            {
                if (idCard.IsNullOrEmpty())
                    return null;
                return idCard!.Length switch
                {
                    18 => idCard.Substring(6, 4) + "-" + idCard.Substring(10, 2) + "-" + idCard.Substring(12, 2),
                    15 => "19" + idCard.Substring(6, 2) + "-" + idCard.Substring(8, 2) + "-" + idCard.Substring(10, 2),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据身份证号码获取性别
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns>0未知  1男  2女  3保密</returns>
        public static int GetSexByIdCard(this string idCard)
        {
            //性别码：18位身份证取第17位（顺序码末位），15位身份证取末位；奇数为男，偶数为女
            var sexCode = idCard?.Length switch
            {
                18 => idCard.Substring(16, 1),
                15 => idCard.Substring(14, 1),
                _ => null
            };

            return int.TryParse(sexCode, out var code) ? (code % 2 == 1 ? 1 : 2) : 0;
        }
    }
}