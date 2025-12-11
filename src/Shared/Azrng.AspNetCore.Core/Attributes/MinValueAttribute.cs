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
            return value switch
            {
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