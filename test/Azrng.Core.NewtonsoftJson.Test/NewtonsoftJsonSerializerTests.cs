using Azrng.Core.DefaultJson.Test.Models;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.Core.NewtonsoftJson.Test
{
    public class NewtonsoftJsonSerializerTests
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITestOutputHelper _testOutputHelper;

        public NewtonsoftJsonSerializerTests(IJsonSerializer jsonSerializer, ITestOutputHelper testOutputHelper)
        {
            _jsonSerializer = jsonSerializer;
            _testOutputHelper = testOutputHelper;
        }

        #region ToJson

        [Fact]
        public void ToJson_SimpleObject_ReturnsValidJson()
        {
            var user = new UserInfo
            {
                UserId = 1,
                FirstName = "test",
                Salary = 99.5,
                IsAdmin = true,
                Roles = new List<string> { "admin" },
                CreatedAt = 123456789L
            };

            var json = _jsonSerializer.ToJson(user);

            _testOutputHelper.WriteLine(json);
            Assert.Contains("\"userId\":1", json);
            Assert.Contains("\"firstName\":\"test\"", json);
            Assert.Contains("\"salary\":99.5", json);
            Assert.Contains("\"isAdmin\":true", json);
        }

        [Fact]
        public void ToJson_CamelCasePropertyNames_ReturnsCamelCase()
        {
            var user = new UserInfo { UserId = 1, FirstName = "abc" };

            var json = _jsonSerializer.ToJson(user);

            Assert.Contains("\"userId\"", json);
            Assert.Contains("\"firstName\"", json);
            Assert.DoesNotContain("\"UserId\"", json);
            Assert.DoesNotContain("\"FirstName\"", json);
        }

        [Fact]
        public void ToJson_DateTimeProperty_ReturnsFormattedDate()
        {
            var info = new SerializerTestDateTimeInfo { Id = "1", DateTime = new DateTime(2025, 6, 15, 10, 30, 0) };

            var json = _jsonSerializer.ToJson(info);

            Assert.Contains("2025-06-15 10:30:00", json);
        }

        [Fact]
        public void ToJson_ObjectWithCollection_ReturnsJsonArray()
        {
            var user = new UserInfo
            {
                UserId = 1,
                Roles = new List<string> { "admin", "user", "guest" }
            };

            var json = _jsonSerializer.ToJson(user);

            Assert.Contains("[\"admin\",\"user\",\"guest\"]", json);
        }

        [Fact]
        public void ToJson_DefaultValuesObject_ReturnsDefaultJson()
        {
            var user = new UserInfo();

            var json = _jsonSerializer.ToJson(user);

            Assert.NotNull(json);
            Assert.Contains("\"userId\":0", json);
            Assert.Contains("\"salary\":0", json);
            Assert.Contains("\"isAdmin\":false", json);
        }

        #endregion

        #region ToObject

        [Fact]
        public void ToObject_ValidJson_ReturnsObject()
        {
            var json = "{\"userId\":10,\"firstName\":\"张三\",\"salary\":10.1,\"isAdmin\":false,\"roles\":[\"aa\",\"bb\"],\"createdAt\":123456789}";

            var result = _jsonSerializer.ToObject<UserInfo>(json);

            Assert.NotNull(result);
            Assert.Equal(10, result.UserId);
            Assert.Equal("张三", result.FirstName);
            Assert.False(result.IsAdmin);
            Assert.Equal(2, result.Roles.Count);
        }

        [Fact]
        public void ToObject_NullInput_ReturnsDefault()
        {
            string? json = null;

            var result = _jsonSerializer.ToObject<UserInfo>(json);

            Assert.Null(result);
        }

        [Fact]
        public void ToObject_EmptyString_ReturnsDefault()
        {
            var result = _jsonSerializer.ToObject<UserInfo>("");

            Assert.Null(result);
        }

        [Fact]
        public void ToObject_WhitespaceString_ReturnsDefault()
        {
            var result = _jsonSerializer.ToObject<UserInfo>("   ");

            Assert.Null(result);
        }

        [Fact]
        public void ToObject_DateTimeProperty_ReturnsParsedDateTime()
        {
            var json = "{\"id\":\"1\",\"dateTime\":\"2025-01-15 08:00:00\"}";

            var result = _jsonSerializer.ToObject<SerializerTestDateTimeInfo>(json);

            Assert.NotNull(result);
            Assert.Equal(new DateTime(2025, 1, 15, 8, 0, 0), result.DateTime);
        }

        [Fact]
        public void ToObject_EmptyRoles_ReturnsEmptyCollection()
        {
            var json = "{\"userId\":1,\"firstName\":\"test\",\"salary\":0,\"isAdmin\":false,\"roles\":[],\"createdAt\":0}";

            var result = _jsonSerializer.ToObject<UserInfo>(json);

            Assert.NotNull(result);
            Assert.Empty(result.Roles);
        }

        #endregion

        #region Clone

        [Fact]
        public void Clone_SimpleObject_ReturnsDeepCopy()
        {
            var user = new UserInfo
            {
                UserId = 1,
                FirstName = "test",
                Salary = 50.0,
                IsAdmin = true,
                Roles = new List<string> { "admin" },
                CreatedAt = 100L
            };

            var cloned = _jsonSerializer.Clone(user);

            Assert.NotNull(cloned);
            Assert.NotSame(user, cloned);
            Assert.Equal(user.UserId, cloned.UserId);
            Assert.Equal(user.FirstName, cloned.FirstName);
            Assert.Equal(user.Salary, cloned.Salary);
            Assert.Equal(user.IsAdmin, cloned.IsAdmin);
            Assert.Equal(user.CreatedAt, cloned.CreatedAt);
        }

        [Fact]
        public void Clone_CollectionProperty_ReturnsNewCollectionInstance()
        {
            var user = new UserInfo
            {
                UserId = 1,
                Roles = new List<string> { "a", "b" }
            };

            var cloned = _jsonSerializer.Clone(user);

            Assert.NotNull(cloned);
            Assert.NotSame(user.Roles, cloned.Roles);
            Assert.Equal(user.Roles, cloned.Roles);
        }

        [Fact]
        public void Clone_ModifyingClone_DoesNotAffectOriginal()
        {
            var user = new UserInfo { UserId = 1, FirstName = "original" };

            var cloned = _jsonSerializer.Clone(user);
            cloned!.FirstName = "modified";

            Assert.Equal("original", user.FirstName);
        }

        #endregion

        #region ToList

        [Fact]
        public void ToList_ValidJsonArray_ReturnsList()
        {
            var json = "[{\"userId\":1,\"firstName\":\"a\",\"salary\":0,\"isAdmin\":false,\"roles\":[],\"createdAt\":0},{\"userId\":2,\"firstName\":\"b\",\"salary\":0,\"isAdmin\":false,\"roles\":[],\"createdAt\":0}]";

            var result = _jsonSerializer.ToList<UserInfo>(json);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].UserId);
            Assert.Equal(2, result[1].UserId);
        }

        [Fact]
        public void ToList_EmptyArray_ReturnsEmptyList()
        {
            var result = _jsonSerializer.ToList<UserInfo>("[]");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ToList_NullInput_ReturnsNull()
        {
            string? json = null;

            var result = _jsonSerializer.ToList<UserInfo>(json);

            Assert.Null(result);
        }

        [Fact]
        public void ToList_EmptyString_ReturnsNull()
        {
            var result = _jsonSerializer.ToList<UserInfo>("");

            Assert.Null(result);
        }

        [Fact]
        public void ToList_WhitespaceString_ReturnsNull()
        {
            var result = _jsonSerializer.ToList<UserInfo>("   ");

            Assert.Null(result);
        }

        [Fact]
        public void ToList_SingleItem_ReturnsSingleItemList()
        {
            var json = "[{\"userId\":42,\"firstName\":\"solo\",\"salary\":100,\"isAdmin\":true,\"roles\":[\"r1\"],\"createdAt\":999}]";

            var result = _jsonSerializer.ToList<UserInfo>(json);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(42, result[0].UserId);
        }

        #endregion

    }

    file class SerializerTestDateTimeInfo
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }
    }
}
