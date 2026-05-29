using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Azrng.NTika.Core.Model
{
    public sealed class Property : IComparable<Property>
    {
        private static readonly ConcurrentDictionary<string, Property> Properties = new();

        private readonly string _name;
        private readonly bool _internal;
        private readonly PropertyType _propertyType;
        private readonly ValueType _valueType;
        private readonly Property _primaryProperty;
        private readonly Property[]? _secondaryExtractProperties;
        private readonly HashSet<string>? _choices;

        private Property(string name, bool isInternal, PropertyType propertyType, ValueType valueType,
            string[]? choices, Property? primaryProperty, Property[]? secondaryExtractProperties)
        {
            _name = name;
            _internal = isInternal;
            _propertyType = propertyType;
            _valueType = valueType;

            if (choices != null)
            {
                _choices = new HashSet<string>(choices);
            }

            if (primaryProperty != null)
            {
                _primaryProperty = primaryProperty;
                _secondaryExtractProperties = secondaryExtractProperties;
            }
            else
            {
                _primaryProperty = this;
                _secondaryExtractProperties = null;
                Properties[name] = this;
            }
        }

        private Property(string name, bool isInternal, PropertyType propertyType, ValueType valueType,
            string[]? choices)
            : this(name, isInternal, propertyType, valueType, choices, null, null)
        {
        }

        private Property(string name, bool isInternal, ValueType valueType, string[]? choices)
            : this(name, isInternal, PropertyType.SIMPLE, valueType, choices)
        {
        }

        private Property(string name, bool isInternal, ValueType valueType)
            : this(name, isInternal, PropertyType.SIMPLE, valueType, null)
        {
        }

        private Property(string name, bool isInternal, PropertyType propertyType, ValueType valueType)
            : this(name, isInternal, propertyType, valueType, null)
        {
        }

        public string Name => _name;
        public bool IsInternal => _internal;
        public bool IsExternal => !_internal;
        public PropertyType PropertyType => _propertyType;
        public ValueType ValueType => _valueType;
        public Property PrimaryProperty => _primaryProperty;
        public Property[]? SecondaryExtractProperties => _secondaryExtractProperties;
        public IReadOnlyCollection<string>? Choices => _choices;

        public bool IsMultiValuePermitted
        {
            get
            {
                if (_propertyType == PropertyType.BAG || _propertyType == PropertyType.SEQ ||
                    _propertyType == PropertyType.ALT)
                {
                    return true;
                }

                if (_propertyType == PropertyType.COMPOSITE)
                {
                    return _primaryProperty.IsMultiValuePermitted;
                }

                return false;
            }
        }

        public static PropertyType? GetPropertyType(string key)
        {
            return Properties.TryGetValue(key, out var prop) ? prop.PropertyType : null;
        }

        public static Property? Get(string key)
        {
            return Properties.TryGetValue(key, out var prop) ? prop : null;
        }

        public static ISet<Property> GetProperties(string prefix)
        {
            var set = new SortedSet<Property>();
            var p = prefix + ":";
            foreach (var entry in Properties)
            {
                if (entry.Key.StartsWith(p, StringComparison.Ordinal))
                {
                    set.Add(entry.Value);
                }
            }
            return set;
        }

        public static Property InternalBoolean(string name)
            => new(name, true, ValueType.BOOLEAN);

        public static Property InternalClosedChoice(string name, params string[] choices)
            => new(name, true, ValueType.CLOSED_CHOICE, choices);

        public static Property InternalDate(string name)
            => new(name, true, ValueType.DATE);

        public static Property InternalDateBag(string name)
            => new(name, true, PropertyType.BAG, ValueType.DATE);

        public static Property InternalInteger(string name)
            => new(name, true, ValueType.INTEGER);

        public static Property InternalIntegerSequence(string name)
            => new(name, true, PropertyType.SEQ, ValueType.INTEGER);

        public static Property InternalRational(string name)
            => new(name, true, ValueType.RATIONAL);

        public static Property InternalOpenChoice(string name, params string[] choices)
            => new(name, true, ValueType.OPEN_CHOICE, choices);

        public static Property InternalReal(string name)
            => new(name, true, ValueType.REAL);

        public static Property InternalText(string name)
            => new(name, true, ValueType.TEXT);

        public static Property InternalTextBag(string name)
            => new(name, true, PropertyType.BAG, ValueType.TEXT);

        public static Property InternalURI(string name)
            => new(name, true, ValueType.URI);

        public static Property ExternalClosedChoice(string name, params string[] choices)
            => new(name, false, ValueType.CLOSED_CHOICE, choices);

        public static Property ExternalOpenChoice(string name, params string[] choices)
            => new(name, false, ValueType.OPEN_CHOICE, choices);

        public static Property ExternalDate(string name)
            => new(name, false, ValueType.DATE);

        public static Property ExternalReal(string name)
            => new(name, false, ValueType.REAL);

        public static Property ExternalRealSeq(string name)
            => new(name, false, PropertyType.SEQ, ValueType.REAL);

        public static Property ExternalInteger(string name)
            => new(name, false, ValueType.INTEGER);

        public static Property ExternalBoolean(string name)
            => new(name, false, ValueType.BOOLEAN);

        public static Property ExternalBooleanSeq(string name)
            => new(name, false, PropertyType.SEQ, ValueType.BOOLEAN);

        public static Property ExternalText(string name)
            => new(name, false, ValueType.TEXT);

        public static Property ExternalTextBag(string name)
            => new(name, false, PropertyType.BAG, ValueType.TEXT);

        public static Property Composite(Property primaryProperty, Property[]? secondaryExtractProperties)
        {
            if (primaryProperty == null)
            {
                throw new ArgumentNullException(nameof(primaryProperty));
            }

            if (primaryProperty.PropertyType == PropertyType.COMPOSITE)
            {
                throw new PropertyTypeException(primaryProperty.PropertyType);
            }

            if (secondaryExtractProperties != null)
            {
                foreach (var secondary in secondaryExtractProperties)
                {
                    if (secondary.PropertyType == PropertyType.COMPOSITE)
                    {
                        throw new PropertyTypeException(secondary.PropertyType);
                    }
                }
            }

            string[]? choices = null;
            if (primaryProperty.Choices != null)
            {
                choices = primaryProperty.Choices.ToArray();
            }

            return new Property(primaryProperty.Name, primaryProperty.IsInternal,
                PropertyType.COMPOSITE, ValueType.PROPERTY, choices,
                primaryProperty, secondaryExtractProperties);
        }

        public int CompareTo(Property? other)
        {
            if (other == null) return 1;
            return string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is Property other && _name.Equals(other._name, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
