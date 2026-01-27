namespace System.ComponentModel
{
    /// <summary>
    /// 为枚举成员提供英文描述
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnglishDescriptionAttribute : Attribute
    {
        public EnglishDescriptionAttribute(string value) => Value = value;

        /// <summary>英文描述</summary>
        public string Value { get; }
    }
}