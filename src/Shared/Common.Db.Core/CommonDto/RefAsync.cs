namespace Azrng.Core.CommonDto
{
    /// <summary>
    /// 用于包装一个对象引用，支持隐式转换为 T 类型和从 T 类型转换。
    /// 通常用于需要在异步操作中引用对象的场景，如 RefAsync&lt;int&gt;
    /// </summary>
    /// <typeparam name="T">要包装的类型</typeparam>
    public class RefAsync<T>
    {
        /// <summary>
        /// 默认构造函数，初始化 Value 为默认值。
        /// </summary>
        public RefAsync() { }

        /// <summary>
        /// 通过指定值初始化 RefAsync 实例。
        /// </summary>
        /// <param name="value">要包装的值</param>
        public RefAsync(T value) => Value = value;

        /// <summary>
        /// 获取或设置包装的值。
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 重写 ToString 方法，返回包装值的字符串表示。
        /// 如果 Value 为 null，则返回空字符串。
        /// </summary>
        public override string ToString()
        {
            var obj = Value;
            return obj == null ? "" : obj.ToString();
        }

        /// <summary>
        /// 隐式转换操作符：将 RefAsync&lt;T&gt; 转换为 T 类型。
        /// 例如：RefAsync&lt;int&gt; r = 10; int i = r;
        /// </summary>
        public static implicit operator T(RefAsync<T> r) => r.Value;

        /// <summary>
        /// 隐式转换操作符：将 T 类型转换为 RefAsync&lt;T&gt;。
        /// 例如：RefAsync&lt;int&gt; r = 10;
        /// </summary>
        public static implicit operator RefAsync<T>(T value) => new RefAsync<T>(value);
    }
}