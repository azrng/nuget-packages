using Xunit;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Model
{
    public class MetadataTests
    {
        [Fact]
        public void SetAndGet_ShouldWork()
        {
            var metadata = new Metadata();
            metadata.Set("key", "value");
            metadata.Get("key").Should().Be("value");
        }

        [Fact]
        public void Get_NonExistent_ShouldReturnNull()
        {
            var metadata = new Metadata();
            metadata.Get("missing").Should().BeNull();
        }

        [Fact]
        public void Add_ShouldAppend()
        {
            var metadata = new Metadata();
            metadata.Add("key", "value1");
            metadata.Add("key", "value2");
            metadata.GetValues("key").Should().BeEquivalentTo(new[] { "value1", "value2" });
        }

        [Fact]
        public void Set_ShouldReplace()
        {
            var metadata = new Metadata();
            metadata.Add("key", "value1");
            metadata.Set("key", "value2");
            metadata.Get("key").Should().Be("value2");
            metadata.GetValues("key").Should().HaveCount(1);
        }

        [Fact]
        public void Set_Null_ShouldRemove()
        {
            var metadata = new Metadata();
            metadata.Set("key", "value");
            metadata.Set("key", (string?)null);
            metadata.Get("key").Should().BeNull();
        }

        [Fact]
        public void Names_ShouldReturnAllKeys()
        {
            var metadata = new Metadata();
            metadata.Set("a", "1");
            metadata.Set("b", "2");
            metadata.Names().Should().BeEquivalentTo(new[] { "a", "b" });
        }

        [Fact]
        public void Size_ShouldReturnCount()
        {
            var metadata = new Metadata();
            metadata.Set("a", "1");
            metadata.Set("b", "2");
            metadata.Size().Should().Be(2);
        }

        [Fact]
        public void IsMultiValued_ShouldWork()
        {
            var metadata = new Metadata();
            metadata.Set("single", "1");
            metadata.Add("multi", "1");
            metadata.Add("multi", "2");
            metadata.IsMultiValued("single").Should().BeFalse();
            metadata.IsMultiValued("multi").Should().BeTrue();
        }

        [Fact]
        public void PropertyBased_SetAndGet_ShouldWork()
        {
            var prop = Property.InternalText("test:prop");
            var metadata = new Metadata();
            metadata.Set(prop, "value");
            metadata.Get(prop).Should().Be("value");
        }

        [Fact]
        public void Remove_ShouldWork()
        {
            var metadata = new Metadata();
            metadata.Set("key", "value");
            metadata.Remove("key");
            metadata.Get("key").Should().BeNull();
        }

        [Fact]
        public void Equality_SameContent_ShouldBeEqual()
        {
            var a = new Metadata();
            a.Set("key", "value");
            var b = new Metadata();
            b.Set("key", "value");
            a.Equals(b).Should().BeTrue();
        }

        [Fact]
        public void ToString_ShouldFormatCorrectly()
        {
            var metadata = new Metadata();
            metadata.Set("key", "value");
            metadata.ToString().Should().Be("key=value");
        }
    }
}
