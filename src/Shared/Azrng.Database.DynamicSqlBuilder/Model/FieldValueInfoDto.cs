namespace Azrng.Database.DynamicSqlBuilder.Model
{
    /// <summary>
    ///列字段值信息
    /// </summary>
    public class FieldValueInfoDto
    {
        public FieldValueInfoDto(string code, string value)
        {
            Code = code;
            Value = value;
        }

        public FieldValueInfoDto(object value)
        {
            Code = string.Empty;
            Value = value;
        }

        public FieldValueInfoDto(object code, object value)
        {
            Code = code;
            Value = value;
        }

        /// <summary>
        /// 键
        /// </summary>
        public object Code { get; set; }

        /// <summary>
        /// 值（统一使用object类型以支持多种数据类型）
        /// </summary>
        public object Value { get; set; }
    }
}