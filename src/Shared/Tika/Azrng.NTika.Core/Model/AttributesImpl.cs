using System.Collections.Generic;
using Azrng.NTika.Core.Abstractions;

namespace Azrng.NTika.Core.Model
{
    public class AttributesImpl : IAttributes
    {
        private readonly List<(string uri, string localName, string qName, string type, string value)> _attributes = new();

        public int Length => _attributes.Count;

        public string GetUri(int index) => _attributes[index].uri;
        public string GetLocalName(int index) => _attributes[index].localName;
        public string GetQName(int index) => _attributes[index].qName;
        public string GetType(int index) => _attributes[index].type;
        public string GetValue(int index) => _attributes[index].value;

        public int GetIndex(string qName)
        {
            for (var i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].qName == qName) return i;
            }
            return -1;
        }

        public int GetIndex(string uri, string localName)
        {
            for (var i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].uri == uri && _attributes[i].localName == localName) return i;
            }
            return -1;
        }

        public string? GetType(string qName)
        {
            var index = GetIndex(qName);
            return index >= 0 ? _attributes[index].type : null;
        }

        public string? GetType(string uri, string localName)
        {
            var index = GetIndex(uri, localName);
            return index >= 0 ? _attributes[index].type : null;
        }

        public string? GetValue(string qName)
        {
            var index = GetIndex(qName);
            return index >= 0 ? _attributes[index].value : null;
        }

        public string? GetValue(string uri, string localName)
        {
            var index = GetIndex(uri, localName);
            return index >= 0 ? _attributes[index].value : null;
        }

        public void AddAttribute(string uri, string localName, string qName, string type, string value)
        {
            _attributes.Add((uri, localName, qName, type, value));
        }

        public void Clear()
        {
            _attributes.Clear();
        }

        public void SetAttribute(int index, string uri, string localName, string qName, string type, string value)
        {
            _attributes[index] = (uri, localName, qName, type, value);
        }

        public void RemoveAttribute(int index)
        {
            _attributes.RemoveAt(index);
        }
    }
}
