using System.Collections;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// 校验集合的内容不为空
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class CollectionNotEmptyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is IEnumerable collection && collection.GetEnumerator().MoveNext();
        }
    }
}