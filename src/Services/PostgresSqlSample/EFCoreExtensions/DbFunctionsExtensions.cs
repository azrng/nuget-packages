using Microsoft.EntityFrameworkCore;

namespace PostgresSqlSample.EFCoreExtensions
{
    public static class DbFunctionsExtensions
    {
        /// <summary>
        /// 转换时间为字符串类型
        /// </summary>
        /// <param name="_"></param>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string ToChar(this DbFunctions _, DateTime input, string format)
        {
            throw new InvalidOperationException("该方法仅用于实体框架，没有内存实现。");
        }
    }
}