using System;
using System.Threading;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 原子int
    /// </summary>
    public class AtomicInt : IEquatable<int>
    {
        private int _value;

        public AtomicInt(int value = 0)
        {
            _value = value;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <returns></returns>
        public int Get()
        {
            // int是32位整型，无论在32位还是64位系统中，读取都是原子操作
            return _value;
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <returns></returns>
        public int Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// 减少
        /// </summary>
        /// <returns></returns>
        public int Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }

        public bool Equals(int other)
        {
            return Get().Equals(other);
        }
    }
}