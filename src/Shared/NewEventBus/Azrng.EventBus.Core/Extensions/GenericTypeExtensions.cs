namespace Azrng.EventBus.Core.Extensions;

public static class GenericTypeExtensions
{
    /// <summary>
    /// 获取类型的普通类型名
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetGenericTypeName(this Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
        return $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
    }

    public static string GetGenericTypeName(this object @object)
    {
        return @object.GetType().GetGenericTypeName();
    }
}