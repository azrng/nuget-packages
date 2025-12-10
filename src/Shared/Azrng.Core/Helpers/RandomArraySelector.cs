namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 随机数组元素返回
    /// </summary>
    /// <remarks>使用Fisher-Yates 洗牌算法实现</remarks>
    /// <typeparam name="T"></typeparam>
    public class RandomArraySelector<T>
    {
        private readonly T[] _items;
        private int _currentIndex;

        public RandomArraySelector(T[] array)
        {
            _items = (T[])array.Clone();
            _currentIndex = _items.Length;
        }

        public T GetNext()
        {
            if (_currentIndex == 0)
            {
                _currentIndex = _items.Length;
            }

            // 随机选择一个尚未返回的元素
            var randomIndex = RandomGenerator.Random.Value.Next(_currentIndex);
            var selectedItem = _items[randomIndex];

            // 将选中的元素与当前最后一个未选元素交换
            _items[randomIndex] = _items[_currentIndex - 1];
            _items[_currentIndex - 1] = selectedItem;

            _currentIndex--;
            return selectedItem;
        }
    }
}