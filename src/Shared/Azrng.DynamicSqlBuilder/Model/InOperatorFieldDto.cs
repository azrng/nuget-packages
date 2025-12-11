namespace Azrng.DynamicSqlBuilder.Model
{
    /// <summary>
    /// 用于sql 条件表达式 in查询的字段
    /// </summary>
    public class InOperatorFieldDto
    {
        public InOperatorFieldDto(string field, IEnumerable<object> ids, Type valueType = null)
        {
            Field = field;
            Ids = ids;
            ValueType = valueType ?? typeof(string);
        }

        /// <summary>
        /// 字段名
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 字段值
        /// </summary>
        public IEnumerable<object> Ids { get; set; }

        /// <summary>
        /// 值的实际类型
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// 创建 IN 操作符字段（泛型方法，自动推断类型）
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="field">字段名</param>
        /// <param name="values">值集合</param>
        /// <returns>InOperatorFieldDto 实例</returns>
        public static InOperatorFieldDto Create<T>(string field, IEnumerable<T> values)
        {
            return new InOperatorFieldDto(field, values?.Cast<object>() ?? Enumerable.Empty<object>(), typeof(T));
        }
    }

    /// <summary>
    /// 用于sql 条件表达式 not in查询的字段
    /// </summary>
    public class NotInOperatorFieldDto
    {
        public NotInOperatorFieldDto(string field, IEnumerable<object> ids, Type valueType = null)
        {
            Field = field;
            Ids = ids;
            ValueType = valueType ?? typeof(string);
        }

        /// <summary>
        /// 字段名
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 字段值
        /// </summary>
        public IEnumerable<object> Ids { get; set; }

        /// <summary>
        /// 值的实际类型
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// 创建 NOT IN 操作符字段（泛型方法，自动推断类型）
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="field">字段名</param>
        /// <param name="values">值集合</param>
        /// <returns>NotInOperatorFieldDto 实例</returns>
        public static NotInOperatorFieldDto Create<T>(string field, IEnumerable<T> values)
        {
            return new NotInOperatorFieldDto(field, values?.Cast<object>() ?? Enumerable.Empty<object>(), typeof(T));
        }
    }
}