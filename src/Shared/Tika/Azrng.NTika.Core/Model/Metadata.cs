using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Azrng.NTika.Core.Model
{
    public class Metadata
    {
        private readonly ConcurrentDictionary<string, string[]> _metadata = new();

        public Metadata()
        {
        }

        public string? Get(string name)
        {
            return _metadata.TryGetValue(name, out var values) ? values[0] : null;
        }

        public string? Get(Property property)
        {
            return Get(property.Name);
        }

        public int? GetInt(Property property)
        {
            if (property.PrimaryProperty.PropertyType != PropertyType.SIMPLE)
            {
                return null;
            }
            if (property.PrimaryProperty.ValueType != ValueType.INTEGER)
            {
                return null;
            }

            var v = Get(property);
            if (v == null)
            {
                return null;
            }
            return int.TryParse(v, out var result) ? result : null;
        }

        public DateTime? GetDate(Property property)
        {
            if (property.PrimaryProperty.PropertyType != PropertyType.SIMPLE)
            {
                return null;
            }
            if (property.PrimaryProperty.ValueType != ValueType.DATE)
            {
                return null;
            }

            var v = Get(property);
            if (v != null)
            {
                return ParseDate(v);
            }
            return null;
        }

        public string[] GetValues(Property property)
        {
            return GetValues(property.Name);
        }

        public string[] GetValues(string name)
        {
            return _metadata.TryGetValue(name, out var values) ? values : Array.Empty<string>();
        }

        public void Add(string name, string value)
        {
            if (_metadata.TryGetValue(name, out var values))
            {
                var newValues = new string[values.Length + 1];
                Array.Copy(values, newValues, values.Length);
                newValues[values.Length] = value;
                _metadata[name] = newValues;
            }
            else
            {
                _metadata[name] = new[] { value };
            }
        }

        public void Add(Property property, string value)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.PropertyType == PropertyType.COMPOSITE)
            {
                Add(property.PrimaryProperty, value);
                if (property.SecondaryExtractProperties != null)
                {
                    foreach (var secondary in property.SecondaryExtractProperties)
                    {
                        Add(secondary, value);
                    }
                }
            }
            else
            {
                if (_metadata.ContainsKey(property.Name))
                {
                    if (property.IsMultiValuePermitted)
                    {
                        Add(property.Name, value);
                    }
                    else
                    {
                        throw new PropertyTypeException(
                            $"{property.Name} : {property.PropertyType}");
                    }
                }
                else
                {
                    Set(property, value);
                }
            }
        }

        public void Set(string name, string? value)
        {
            if (value != null)
            {
                _metadata[name] = new[] { value };
            }
            else
            {
                _metadata.TryRemove(name, out _);
            }
        }

        public void Set(Property property, string? value)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.PropertyType == PropertyType.COMPOSITE)
            {
                Set(property.PrimaryProperty, value);
                if (property.SecondaryExtractProperties != null)
                {
                    foreach (var secondary in property.SecondaryExtractProperties)
                    {
                        Set(secondary, value);
                    }
                }
            }
            else
            {
                Set(property.Name, value);
            }
        }

        public void Set(Property property, string[] values)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.PropertyType == PropertyType.COMPOSITE)
            {
                Set(property.PrimaryProperty, values);
                if (property.SecondaryExtractProperties != null)
                {
                    foreach (var secondary in property.SecondaryExtractProperties)
                    {
                        Set(secondary, values);
                    }
                }
            }
            else
            {
                Set(property.Name, values);
            }
        }

        public void Set(string name, string[]? values)
        {
            if (values != null)
            {
                _metadata.TryRemove(name, out _);
                foreach (var v in values)
                {
                    Add(name, v);
                }
            }
            else
            {
                _metadata.TryRemove(name, out _);
            }
        }

        public void Set(Property property, int value)
        {
            if (property.PrimaryProperty.PropertyType != PropertyType.SIMPLE)
            {
                throw new PropertyTypeException(PropertyType.SIMPLE, property.PrimaryProperty.PropertyType);
            }
            if (property.PrimaryProperty.ValueType != ValueType.INTEGER)
            {
                throw new PropertyTypeException(ValueType.INTEGER, property.PrimaryProperty.ValueType);
            }
            Set(property, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(Property property, long value)
        {
            if (property.PrimaryProperty.PropertyType != PropertyType.SIMPLE)
            {
                throw new PropertyTypeException(PropertyType.SIMPLE, property.PrimaryProperty.PropertyType);
            }
            if (property.PrimaryProperty.ValueType != ValueType.REAL)
            {
                throw new PropertyTypeException(ValueType.REAL, property.PrimaryProperty.ValueType);
            }
            Set(property, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(Property property, bool value)
        {
            if (property.PrimaryProperty.PropertyType != PropertyType.SIMPLE)
            {
                throw new PropertyTypeException(PropertyType.SIMPLE, property.PrimaryProperty.PropertyType);
            }
            if (property.PrimaryProperty.ValueType != ValueType.BOOLEAN)
            {
                throw new PropertyTypeException(ValueType.BOOLEAN, property.PrimaryProperty.ValueType);
            }
            Set(property, value.ToString().ToLowerInvariant());
        }

        public void Set(Property property, double value)
        {
            if (property.PrimaryProperty.ValueType != ValueType.REAL &&
                property.PrimaryProperty.ValueType != ValueType.RATIONAL)
            {
                throw new PropertyTypeException(ValueType.REAL, property.PrimaryProperty.ValueType);
            }
            Set(property, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(Property property, DateTime date)
        {
            if (property.PrimaryProperty.PropertyType != PropertyType.SIMPLE)
            {
                throw new PropertyTypeException(PropertyType.SIMPLE, property.PrimaryProperty.PropertyType);
            }
            if (property.PrimaryProperty.ValueType != ValueType.DATE)
            {
                throw new PropertyTypeException(ValueType.DATE, property.PrimaryProperty.ValueType);
            }
            Set(property, date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        }

        public void Remove(string name)
        {
            _metadata.TryRemove(name, out _);
        }

        public string[] Names()
        {
            return _metadata.Keys.ToArray();
        }

        public int Size()
        {
            return _metadata.Count;
        }

        public bool IsMultiValued(string name)
        {
            return _metadata.TryGetValue(name, out var values) && values.Length > 1;
        }

        public bool IsMultiValued(Property property)
        {
            return IsMultiValued(property.Name);
        }

        public void SetAll(IDictionary<string, string> properties)
        {
            foreach (var entry in properties)
            {
                _metadata[entry.Key] = new[] { entry.Value };
            }
        }

        private static DateTime? ParseDate(string date)
        {
            if (DateTime.TryParse(date, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out var result))
            {
                return result;
            }
            return null;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Metadata other)
            {
                return false;
            }

            if (other.Size() != Size())
            {
                return false;
            }

            foreach (var name in Names())
            {
                var otherValues = other.GetValues(name);
                var thisValues = GetValues(name);
                if (otherValues.Length != thisValues.Length)
                {
                    return false;
                }
                for (var j = 0; j < otherValues.Length; j++)
                {
                    if (otherValues[j] != thisValues[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var h = 0;
            foreach (var entry in _metadata)
            {
                h ^= entry.Key.GetHashCode() ^ ((IStructuralEquatable)entry.Value).GetHashCode(StringComparer.Ordinal);
            }
            return h;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var name in Names())
            {
                foreach (var value in GetValues(name))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(name).Append('=').Append(value);
                }
            }
            return sb.ToString();
        }
    }
}
