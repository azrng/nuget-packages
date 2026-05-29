using Azrng.NTika.Core.Exception;

namespace Azrng.NTika.Core.Model
{
    public class PropertyTypeException : TikaException
    {
        public PropertyTypeException(PropertyType type)
            : base($"Invalid property type: {type}")
        {
        }

        public PropertyTypeException(PropertyType expected, PropertyType actual)
            : base($"Expected property type {expected}, but got {actual}")
        {
        }

        public PropertyTypeException(ValueType expected, ValueType actual)
            : base($"Expected value type {expected}, but got {actual}")
        {
        }

        public PropertyTypeException(string message)
            : base(message)
        {
        }
    }
}
