using System.Collections;
using Azrng.Database.DynamicSqlBuilder.Model;

namespace Azrng.Database.DynamicSqlBuilder.Utils;

/// <summary>
/// 类型转换助手（增强版 - 包含详细错误处理）
/// </summary>
public static class TypeConvertHelper
{
    /// <summary>
    /// 转换单个值到目标类型
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="throwOnError">是否在转换失败时抛出异常（默认true）</param>
    /// <returns>转换后的值</returns>
    /// <exception cref="ArgumentNullException">值为null时</exception>
    /// <exception cref="InvalidOperationException">转换失败且throwOnError为true时</exception>
    public static object ConvertToTargetType(object value, Type targetType, bool throwOnError = true)
    {
        // 处理null值
        if (value == null || value == DBNull.Value)
        {
            return GetDefaultVaule(targetType);
        }

        try
        {
            // 如果类型已经匹配，直接返回
            if (value.GetType() == targetType)
            {
                return value;
            }

            // 处理可空类型
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // 根据目标类型进行转换
            object result = underlyingType switch
            {
                { } t when t == typeof(int) => Convert.ToInt32(value),
                { } t when t == typeof(long) => Convert.ToInt64(value),
                { } t when t == typeof(short) => Convert.ToInt16(value),
                { } t when t == typeof(decimal) => Convert.ToDecimal(value),
                { } t when t == typeof(double) => Convert.ToDouble(value),
                { } t when t == typeof(float) => Convert.ToSingle(value),
                { } t when t == typeof(string) => value?.ToString() ?? string.Empty,
                { } t when t == typeof(DateTime) => Convert.ToDateTime(value),
                { } t when t == typeof(bool) => Convert.ToBoolean(value),
                { } t when t == typeof(byte) => Convert.ToByte(value),
                { } t when t == typeof(sbyte) => Convert.ToSByte(value),
                { } t when t == typeof(uint) => Convert.ToUInt32(value),
                { } t when t == typeof(ulong) => Convert.ToUInt64(value),
                { } t when t == typeof(ushort) => Convert.ToUInt16(value),
                { } t when t.IsEnum => ConvertToEnum(value, t),
                { } t when t == typeof(Guid) => ConvertToGuid(value, t),
                _ => Convert.ChangeType(value, underlyingType)
            };

            return result;
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
        {
            if (throwOnError)
            {
                throw new InvalidOperationException(
                    $"无法将值 '{value}' 从类型 {value.GetType()} 转换为 {targetType}",
                    ex
                );
            }

            // 转换失败时返回默认值
            return GetDefaultVaule(targetType);
        }
    }

    /// <summary>
    /// 转换值集合到目标类型集合
    /// </summary>
    /// <param name="values">值集合</param>
    /// <param name="targetType">目标元素类型</param>
    /// <param name="throwOnError">是否在转换失败时抛出异常（默认false）</param>
    /// <returns>转换后的列表</returns>
    public static object ConvertToTargetType(IEnumerable<object> values, Type targetType, bool throwOnError = false)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var resultList = new List<object>();

        foreach (var value in values)
        {
            try
            {
                var convertedValue = ConvertToTargetType(value, targetType, throwOnError);
                resultList.Add(convertedValue);
            }
            catch (Exception)
            {
                if (throwOnError)
                    throw;
                // 跳过无法转换的值
            }
        }

        // 根据目标类型返回强类型列表
        var listType = typeof(List<>).MakeGenericType(targetType);
        var strongTypedList = (IList)Activator.CreateInstance(listType);

        foreach (var item in resultList)
        {
            strongTypedList.Add(item);
        }

        return strongTypedList;
    }

    /// <summary>
    /// 转换为枚举类型
    /// </summary>
    private static object ConvertToEnum(object value, Type enumType)
    {
        if (value is string stringValue)
        {
            return Enum.Parse(enumType, stringValue, ignoreCase: true);
        }
        else
        {
            return Enum.ToObject(enumType, Convert.ToInt32(value));
        }
    }

    /// <summary>
    /// 转换为Guid类型
    /// </summary>
    private static object ConvertToGuid(object value, Type targetType)
    {
        return value switch
        {
            string str when Guid.TryParse(str, out var guid) => guid,
            byte[] bytes => new Guid(bytes),
            _ => throw new InvalidCastException($"无法将 {value.GetType()} 转换为 Guid")
        };
    }

    /// <summary>
    /// 获取类型的默认值
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>默认值</returns>
    public static object GetDefaultVaule(Type type)
    {
        // 处理可空类型
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return string.Empty;

        return underlyingType.IsValueType ? Activator.CreateInstance(underlyingType) : null;
    }

    /// <summary>
    /// 转换操作符字符串为枚举
    /// </summary>
    /// <param name="operatorStr">操作符字符串</param>
    /// <returns>操作符枚举</returns>
    public static MatchOperator ConvertToEnum(string operatorStr)
    {
        if (string.IsNullOrWhiteSpace(operatorStr))
            return MatchOperator.Equal;

        return operatorStr.Trim().ToUpperInvariant() switch
        {
            "IN" => MatchOperator.In,
            "NOT IN" or "NOTIN" => MatchOperator.NotIn,
            "LIKE" => MatchOperator.Like,
            "NOT LIKE" or "NOTLIKE" => MatchOperator.NotLike,
            "<>" or "!=" => MatchOperator.NotEqual,
            "=" or "==" => MatchOperator.Equal,
            "AND" => MatchOperator.And,
            ">" => MatchOperator.GreaterThan,
            "<" => MatchOperator.LessThan,
            ">=" => MatchOperator.GreaterThanEqual,
            "<=" => MatchOperator.LessThanEqual,
            "BETWEEN" => MatchOperator.Between,
            _ => MatchOperator.Equal
        };
    }
}
