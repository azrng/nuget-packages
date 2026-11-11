namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// 验证最小值
    /// </summary>
    /// <remarks>已经包含了Required</remarks>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly int _minValue;

        public MinValueAttribute(int minValue = 1)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object? value)
        {
            //remarks 承诺“已经包含了Required”，null 必须校验失败，否则可空字段出现验证缺口
            return value switch
            {
                null => false,
                int valueAsInt => valueAsInt >= _minValue,
                long valueAsLong => valueAsLong >= _minValue,
                decimal valueAsDecimal => valueAsDecimal >= _minValue,
                double valueAsDouble => valueAsDouble >= _minValue,
                float valueAsFloat => valueAsFloat >= _minValue,
                _ => true
            };
        }
    }
}