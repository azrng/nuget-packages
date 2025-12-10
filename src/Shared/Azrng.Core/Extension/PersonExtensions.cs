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
        public static string GetBirthdayByIdCard(this string idCard)
        {
            try
            {
                return idCard.Length switch
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
            var sex = "0"; //0未知  1男  2女  3保密
            try
            {
                sex = idCard.Length switch
                {
                    18 => idCard.Substring(14, 3),
                    15 => "19" + idCard.Substring(6, 2) + "-" + idCard.Substring(8, 2) + "-" + idCard.Substring(10, 2),
                    _ => sex
                };

                //性别代码为偶数是女性奇数为男性
                return int.Parse(sex) % 2 == 0 ? 0 : 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}