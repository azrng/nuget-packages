using System;
using System.Threading;

namespace CommonCollect
{
    /// <summary>
    /// 调度帮助类
    /// </summary>
    public class DispatcherHelper
    {
        private static int _seed = 0;

        /// <summary>
        /// 调度分配方法
        /// </summary>
        /// <param name="stringArray"></param>
        /// <param name="type">0随机 1平均</param>
        /// <returns></returns>
        public static string Dispatcher(string[] stringArray, int type = 0)
        {
            if (stringArray is null || stringArray.Length == 0)
                return string.Empty;

            if (type == 0)
            {
                // 随机
                return stringArray[new Random(_seed++).Next(0, stringArray.Length)];
            }
            else if (type == 1)
            {
                // 平均返回
                return stringArray[Interlocked.Increment(ref _seed) % stringArray.Length];
            }

            return stringArray[0];
        }
    }
}