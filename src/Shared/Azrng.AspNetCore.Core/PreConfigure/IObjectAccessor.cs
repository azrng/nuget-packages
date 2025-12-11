namespace Azrng.AspNetCore.Core.PreConfigure
{
    public interface IObjectAccessor<out T>
    {
        T? Value { get; }
    }

    /// <summary>
    /// 创建一个对象访问器，包一层
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectAccessor<T> : IObjectAccessor<T>
    {
        public T? Value { get; set; }

        public ObjectAccessor() { }

        public ObjectAccessor(T? obj)
        {
            Value = obj;
        }
    }
}