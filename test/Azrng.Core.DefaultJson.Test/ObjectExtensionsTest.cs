using Azrng.Core.Json.Utils;
using Xunit;

namespace Azrng.Core.DefaultJson.Test
{
    public class ObjectExtensionsTest
    {
        [Fact]
        public void Clone_NullInput_ReturnsNull()
        {
            // Arrange
            Person? person = null;

            // Act
            var clonedPerson = JsonHelper.Clone(person);

            // Assert
            Assert.Null(clonedPerson);
        }

        [Fact]
        public void Clone_NonNullInput_ReturnsClonedObject()
        {
            // Arrange
            var person = new Person { Name = "John Doe", Age = 30, Address = new Address { Street = "123 Main St", City = "Anytown" } };

            // Act
            var clonedPerson = JsonHelper.Clone(person);

            // Assert
            Assert.NotNull(clonedPerson);
            Assert.NotSame(person, clonedPerson);
            Assert.Equal(person.Name, clonedPerson.Name);
            Assert.Equal(person.Age, clonedPerson.Age);
            Assert.NotSame(person.Address, clonedPerson.Address);
            Assert.Equal(person.Address.Street, clonedPerson.Address.Street);
            Assert.Equal(person.Address.City, clonedPerson.Address.City);
        }

        [Fact]
        public void Clone_ComplexObjectWithCollections_ReturnsClonedObject()
        {
            // Arrange
            var person = new Person
                         {
                             Name = "Jane Doe",
                             Age = 25,
                             Address = new Address { Street = "456 Elm St", City = "Othertown" },
                             PhoneNumbers =
                             [
                                 "123-456-7890",
                                 "987-654-3210"
                             ]
                         };

            // Act
            var clonedPerson = JsonHelper.Clone(person);

            // Assert
            Assert.NotNull(clonedPerson);
            Assert.NotSame(person, clonedPerson);
            Assert.Equal(person.Name, clonedPerson.Name);
            Assert.Equal(person.Age, clonedPerson.Age);
            Assert.NotSame(person.Address, clonedPerson.Address);
            Assert.Equal(person.Address.Street, clonedPerson.Address.Street);
            Assert.Equal(person.Address.City, clonedPerson.Address.City);
            Assert.NotSame(person.PhoneNumbers, clonedPerson.PhoneNumbers);
            Assert.Equal(person.PhoneNumbers, clonedPerson.PhoneNumbers);
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public string[] PhoneNumbers { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }
    }
}