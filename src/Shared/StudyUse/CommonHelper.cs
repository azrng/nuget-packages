using System;
using System.Diagnostics;

namespace StudyUse
{
    public class CommonHelper
    {
        /// <summary>
        /// 防篡改检查
        /// </summary>
        /// <exception cref="ArgumentException">参数异常</exception>
        public static void AntiTamperCheck()
        {
            if (Debugger.IsAttached)
            {
                throw new ArgumentException("检测到违规操作");
            }
        }
    }
}