using Xunit;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Model
{
    public class PropertyTests
    {
        [Fact]
        public void InternalText_ShouldCreate()
        {
            var prop = Property.InternalText("test:prop");
            prop.Name.Should().Be("test:prop");
            prop.IsInternal.Should().BeTrue();
            prop.PropertyType.Should().Be(PropertyType.SIMPLE);
            prop.ValueType.Should().Be(Azrng.NTika.Core.Model.ValueType.TEXT);
        }

        [Fact]
        public void ExternalText_ShouldCreate()
        {
            var prop = Property.ExternalText("dc:title");
            prop.IsExternal.Should().BeTrue();
        }

        [Fact]
        public void InternalInteger_ShouldCreate()
        {
            var prop = Property.InternalInteger("test:int");
            prop.ValueType.Should().Be(Azrng.NTika.Core.Model.ValueType.INTEGER);
        }

        [Fact]
        public void InternalDate_ShouldCreate()
        {
            var prop = Property.InternalDate("test:date");
            prop.ValueType.Should().Be(Azrng.NTika.Core.Model.ValueType.DATE);
        }

        [Fact]
        public void Get_ShouldReturnRegisteredProperty()
        {
            var prop = Property.InternalText("test:registered");
            var found = Property.Get("test:registered");
            found.Should().BeSameAs(prop);
        }

        [Fact]
        public void Get_NonExistent_ShouldReturnNull()
        {
            Property.Get("nonexistent:prop").Should().BeNull();
        }

        [Fact]
        public void Composite_ShouldWork()
        {
            var primary = Property.InternalText("test:primary");
            var secondary = Property.InternalText("test:secondary");
            var composite = Property.Composite(primary, new[] { secondary });
            composite.PropertyType.Should().Be(PropertyType.COMPOSITE);
            composite.PrimaryProperty.Should().BeSameAs(primary);
        }

        [Fact]
        public void IsMultiValuePermitted_Bag_ShouldBeTrue()
        {
            var prop = Property.InternalTextBag("test:bag");
            prop.IsMultiValuePermitted.Should().BeTrue();
        }
    }
}
