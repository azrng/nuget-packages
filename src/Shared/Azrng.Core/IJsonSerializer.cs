using System.Collections.Generic;

namespace Azrng.Core
{
    /// <summary>
    /// json序列化接口
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// 对象转Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string ToJson<T>(T obj) where T : class;

        /// <summary>
        /// 字符串转对象
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ToObject<T>(string json);

        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Clone<T>(T obj) where T : class;

        // /// <summary>
        // /// 对象转字典
        // /// </summary>
        // /// <param name="obj"></param>
        // /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // Dictionary<string, object> ToDictionary<T>(T obj) where T : class;

        /// <summary>
        /// json字符串转对象集合
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> ToList<T>(string json);
    }
}