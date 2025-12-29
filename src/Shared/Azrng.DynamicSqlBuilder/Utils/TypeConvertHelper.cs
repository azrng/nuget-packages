using Azrng.DynamicSqlBuilder.Model;
using Azrng.DynamicSqlBuilder.SqlOperation;

namespace Azrng.DynamicSqlBuilder.Utils;

public class TypeConvertHelper
{
    public static object ConvertToTargetType(object value, Type targetType)
    {
        if (value is null)
            throw new ArgumentNullException("参数不能为null", nameof(value));

        // 处理可空类型
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // 获取可空类型的实际类型
            targetType = Nullable.GetUnderlyingType(targetType);
        }

        if (targetType == typeof(int))
        {
            return Convert.ToInt32(value);
        }
        else if (targetType == typeof(long))
        {
            return Convert.ToInt64(value);
        }
        else if (targetType == typeof(decimal))
        {
            return Convert.ToDecimal(value);
        }
        else if (targetType == typeof(double))
        {
            return Convert.ToDouble(value);
        }
        else if (targetType == typeof(float))
        {
            return Convert.ToSingle(value);
        }
        else if (targetType == typeof(string))
        {
            return value?.ToString() ?? string.Empty;
        }
        else if (targetType == typeof(DateTime))
        {
            return Convert.ToDateTime(value);
        }
        else if (targetType == typeof(bool))
        {
            return Convert.ToBoolean(value);
        }
        else if (targetType.IsEnum)
        {
            if (value is string stringValue)
                return Enum.Parse(targetType, stringValue);
            else
                return Enum.ToObject(targetType, Convert.ToInt32(value));
        }
        else
        {
            // 默认使用 Convert.ChangeType 转换
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // 如果转换失败，返回原始值
                return value;
            }
        }
    }

    public static object ConvertToTargetType(IEnumerable<object> values, Type targetType)
    {
        if (targetType == typeof(int))
        {
            return values.Select(Convert.ToInt32).ToList();
        }
        else if (targetType == typeof(long))
        {
            return values.Select(Convert.ToInt64).ToList();
        }
        else if (targetType == typeof(decimal))
        {
            return values.Select(Convert.ToDecimal).ToList();
        }
        else if (targetType == typeof(double))
        {
            return values.Select(Convert.ToDouble).ToList();
        }
        else if (targetType == typeof(float))
        {
            return values.Select(Convert.ToSingle).ToList();
        }
        else if (targetType == typeof(string))
        {
            return values.Select(v => v?.ToString() ?? string.Empty).ToList();
        }
        else if (targetType == typeof(DateTime))
        {
            return values.Select(Convert.ToDateTime).ToList();
        }
        else if (targetType == typeof(bool))
        {
            return values.Select(Convert.ToBoolean).ToList();
        }
        else
        {
            // 默认返回原始列表
            return values.Select(v => v?.ToString() ?? string.Empty).ToList();
        }
    }

    public static MatchOperator ConvertToEnum(string operatorStr)
    {
        return operatorStr switch
        {
            SqlConstant.MatchOperatorIn => MatchOperator.In,
            SqlConstant.MatchOperatorNotIn => MatchOperator.NotIn,
            SqlConstant.MatchOperatorlike => MatchOperator.Like,
            SqlConstant.MatchOperatorNotlike => MatchOperator.NotLike,
            SqlConstant.MatchOperatorNotEqual => MatchOperator.NotEqual,
            SqlConstant.MatchOperatorEqual => MatchOperator.Equal,
            "And" or "AND" => MatchOperator.And,
            ">" => MatchOperator.GreaterThan,
            "<" => MatchOperator.LessThan,
            SqlConstant.MatchOperatorGreaterThanEqual => MatchOperator.GreaterThanEqual,
            SqlConstant.MatchOperatorLessThanEqual => MatchOperator.LessThanEqual,
            SqlConstant.MatchOperatorBetween => MatchOperator.Between,
            _ => MatchOperator.Equal
        };
    }
}