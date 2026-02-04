using System.Collections.Generic;

namespace Azrng.Core.Model
{
    /// <summary>
    /// 树扁平项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class TreeFlatItem<T, T2>
    {
        /// <summary>
        /// id
        /// </summary>
        public T Id { get; set; } = default!;

        /// <summary>
        /// 父类id
        /// </summary>
        public T ParentId { get; set; } = default!;

        /// <summary>
        /// 子项
        /// </summary>
        public List<T2> Children { get; set; } = new List<T2>(0);
    }
}