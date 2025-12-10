using System.Collections.Generic;

namespace Azrng.Core.Model
{
    /// <summary>
    /// 树形选项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeItem<T>
    {
        public T Item { get; set; }
        public IEnumerable<TreeItem<T>> Children { get; set; }
    }
}