using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Common.HttpClients
{
    /// <summary>
    /// HTTP 请求头集合，支持同名多值。单值场景用索引器直接赋字符串，多值场景用 <see cref="Add(string, IEnumerable{string})"/> 追加。
    /// </summary>
    public sealed class HttpHeaders : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
    {
        private readonly Dictionary<string, List<string>> _store = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 按头名获取或设置单值。设置时覆盖该头的全部值；读取时返回首个值，不存在则抛 <see cref="KeyNotFoundException"/>。
        /// 支持集合初始化器：<c>new HttpHeaders { ["Authorization"] = "Bearer x" }</c>
        /// </summary>
        public string this[string key]
        {
            get => _store.TryGetValue(key, out var list) && list.Count > 0
                ? list[0]
                : throw new KeyNotFoundException($"Header '{key}' not found.");
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _store[key] = new List<string> { value };
            }
        }

        /// <summary>当前不同头名的数量</summary>
        public int Count => _store.Count;

        /// <summary>是否包含指定头名</summary>
        public bool ContainsKey(string key) => _store.ContainsKey(key);

        /// <summary>追加单值到指定头（同名累加，支持多个值）</summary>
        public void Add(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!_store.TryGetValue(key, out var list))
            {
                list = new List<string>();
                _store[key] = list;
            }

            list.Add(value);
        }

        /// <summary>追加多值到指定头（同名累加）</summary>
        public void Add(string key, IEnumerable<string> values)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (!_store.TryGetValue(key, out var list))
            {
                list = new List<string>();
                _store[key] = list;
            }

            list.AddRange(values);
        }

        /// <summary>获取指定头的全部值（不存在返回空列表）</summary>
        public IReadOnlyList<string> GetValues(string key)
            => _store.TryGetValue(key, out var list) ? list : Array.Empty<string>();

        /// <summary>移除指定头</summary>
        public bool Remove(string key) => _store.Remove(key);

        /// <summary>枚举所有头及其值列表</summary>
        public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
            => _store.Select(kv => new KeyValuePair<string, IReadOnlyList<string>>(kv.Key, kv.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
