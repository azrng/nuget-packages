namespace Azrng.AspNetCore.Core.PreConfigure
{
    /// <summary>
    /// 对象访问器接口，用于包装和访问对象实例
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public interface IObjectAccessor<out T>
    {
        /// <summary>
        /// 获取或设置对象值
        /// </summary>
        T? Value { get; }
    }

    /// <summary>
    /// 对象访问器实现，用于包装对象实例并注入到容器中
    /// </summary>
    /// <typeparam name="T">对象类型，必须为引用类型</typeparam>
    public class ObjectAccessor<T> : IObjectAccessor<T>
    {
        /// <summary>
        /// 获取或设置对象值
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// 创建一个空的对象访问器
        /// </summary>
        public ObjectAccessor() { }

        /// <summary>
        /// 创建一个包含指定对象的对象访问器
        /// </summary>
        /// <param name="obj">要包装的对象</param>
        public ObjectAccessor(T? obj)
        {
            Value = obj;
        }
    }
}