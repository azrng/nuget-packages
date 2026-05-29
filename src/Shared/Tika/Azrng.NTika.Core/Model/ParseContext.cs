using System;
using System.Collections.Concurrent;

namespace Azrng.NTika.Core.Model
{
    public class ParseContext
    {
        private readonly ConcurrentDictionary<Type, object> _context = new();

        public void Set<T>(T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            _context[typeof(T)] = value;
        }

        public T? Get<T>() where T : class
        {
            return _context.TryGetValue(typeof(T), out var value) ? value as T : null;
        }

        public T Get<T>(T defaultValue) where T : class
        {
            return Get<T>() ?? defaultValue;
        }

        public bool IsEmpty => _context.Count == 0;
    }
}
