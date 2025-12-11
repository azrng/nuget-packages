using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonCollect.Extension
{
    /// <summary>
    /// 拓展红包算法
    /// </summary>
    public class ExtRedPackageList
    {
        private static List<int> _arr = new List<int>();

        /// <summary>
        /// 红包算法  正常可以使用
        /// </summary>
        /// <param name="total">红包总金额</param>
        /// <param name="num">红包个数</param>
        /// <returns>红包金额列表</returns>
        public static List<decimal> CalRedPacket(decimal total, int num)
        {
            _arr.Clear();
            var returnValue = new List<decimal>();
            var deci = 0.01m; //红包单位
            var totalDec = Convert.ToInt32(total / deci);

            for (int i = 0; i < num - 1; i++)
            {
                GenerateRandom(totalDec);
            }

            _arr = _arr.OrderBy(x => x).ToList();
            returnValue.Add(_arr[0]);
            for (int i = 1; i < _arr.Count(); i++)
            {
                var value = _arr[i] - _arr[i - 1];
                returnValue.Add(value);
            }

            returnValue.Add(totalDec - _arr[_arr.Count() - 1]);

            returnValue = returnValue.Select(x => Convert.ToDecimal(x * 0.01m)).ToList(); //如果单位分   那么这个就弄0.01  还可以弄0.1

            if (returnValue.Count() != num)
            {
                CalRedPacket(total, num);
            }

            return returnValue;
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="totalDec"></param>
        /// <returns></returns>
        private static int GenerateRandom(int totalDec)
        {
            var ran = new Random(Guid.NewGuid().GetHashCode() + Guid.NewGuid().GetHashCode());
            var random = ran.Next(1, totalDec);
            if (_arr.Contains(random))
            {
                GenerateRandom(totalDec);
            }
            else
            {
                _arr.Add(random);
            }

            return random;
        }
    }
}