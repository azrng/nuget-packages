using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Common.HttpClients.Next.Test.Helpers
{
    /// <summary>
    /// 简单的 IOptionsMonitor 测试替身，按命名 key 返回对应 options
    /// </summary>
    internal sealed class FakeOptionsMonitor<T> : IOptionsMonitor<T> where T : class, new()
    {
        private readonly ConcurrentDictionary<string, T> _values = new();
        private readonly ConcurrentDictionary<string, List<Action<T, string?>>> _listeners = new();

        public FakeOptionsMonitor(T defaultValue)
        {
            _values[Options.DefaultName] = defaultValue;
        }

        public T CurrentValue => _values.GetOrAdd(Options.DefaultName, _ => new T());

        public T Get(string? name)
        {
            return _values.GetOrAdd(name ?? Options.DefaultName, _ => new T());
        }

        public IDisposable? OnChange(Action<T, string?> listener)
        {
            var key = Options.DefaultName;
            var list = _listeners.GetOrAdd(key, _ => new List<Action<T, string?>>());
            list.Add(listener);
            return new Unsubscribe(() => list.Remove(listener));
        }

        public void Set(string? name, T value)
        {
            var key = name ?? Options.DefaultName;
            _values[key] = value;
            if (_listeners.TryGetValue(key, out var list))
            {
                foreach (var listener in list)
                {
                    listener(value, key);
                }
            }
        }

        private sealed class Unsubscribe : IDisposable
        {
            private readonly Action _action;
            public Unsubscribe(Action action) { _action = action; }
            public void Dispose() => _action();
        }
    }
}
