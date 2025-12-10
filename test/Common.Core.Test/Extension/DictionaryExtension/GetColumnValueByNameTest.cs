namespace Common.Core.Test.Extension.DictionaryExtension
{
    public class GetColumnValueByNameTest
    {
        /// <summary>
        /// null 值返回默认值
        /// </summary>
        [Fact]
        public void NullValue_ReturnDefault()
        {
            var dict = new Dictionary<string, object> { { "id", "123456" } };
            var intValue = dict.GetColumnValueByName<int>("int");
            Assert.Equal(0, intValue);

            var stringValue = dict.GetColumnValueByName<string>("string");
            Assert.Null(stringValue);

            var dateValue = dict.GetColumnValueByName<DateTime>("date");
            Assert.Equal(DateTime.MinValue, dateValue);

            var date2Value = dict.GetColumnValueByName<DateTime?>("date2");
            Assert.Null(date2Value);

            var decimalValue = dict.GetColumnValueByName<decimal>("decimal");
            Assert.Equal(0, decimalValue);

            var decimal2Value = dict.GetColumnValueByName<decimal?>("decimal2");
            Assert.Null(decimal2Value);

            var doubleValue = dict.GetColumnValueByName<double>("double");
            Assert.Equal(0, doubleValue);

            var boolValue = dict.GetColumnValueByName<bool>("bool");
            Assert.False(boolValue);

            var guidValue = dict.GetColumnValueByName<Guid>("guid");
            Assert.Equal(Guid.Empty, guidValue);

            var byteValue = dict.GetColumnValueByName<byte>("byte");
            Assert.Equal(0, byteValue);

            var shortValue = dict.GetColumnValueByName<short>("short");
            Assert.Equal(0, shortValue);

            var longValue = dict.GetColumnValueByName<long>("long");
            Assert.Equal(0, longValue);
        }

        [Fact]
        public void IntValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", 123456 } };
            var result = dict.GetColumnValueByName<int>("id");
            Assert.Equal(123456, result);
        }

        [Fact]
        public void DecimalValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", 123456.789M } };
            var result = dict.GetColumnValueByName<decimal>("id");
            Assert.Equal(123456.789M, result);
        }

        [Fact]
        public void DoubleValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", 123456.789 } };
            var result = dict.GetColumnValueByName<double>("id");
            Assert.Equal(123456.789, result);
        }

        [Fact]
        public void DateTimeValue_Verify()
        {
            var date = DateTime.Now;
            var dict = new Dictionary<string, object> { { "id", date } };
            var result = dict.GetColumnValueByName<DateTime>("id");
            Assert.Equal(date, result);
        }

        [Fact]
        public void GuidValue_Verify()
        {
            var guid = Guid.NewGuid();
            var dict = new Dictionary<string, object> { { "id", guid } };
            var result = dict.GetColumnValueByName<Guid>("id");
            Assert.Equal(guid, result);
        }

        [Fact]
        public void BoolValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", true } };
            var result = dict.GetColumnValueByName<bool>("id");
            Assert.True(result);
        }

        [Fact]
        public void ByteValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", (byte)123 } };
            var result = dict.GetColumnValueByName<byte>("id");
            Assert.Equal(123, result);
        }

        [Fact]
        public void ShortValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", (short)123 } };
            var result = dict.GetColumnValueByName<short>("id");
            Assert.Equal(123, result);
        }

        [Fact]
        public void LongValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", (long)123456 } };
            var result = dict.GetColumnValueByName<long>("id");
            Assert.Equal(123456, result);
        }

        [Fact]
        public void NullableIntValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", 123456 } };
            var result = dict.GetColumnValueByName<int?>("id");
            Assert.Equal(123456, result);
        }

        [Fact]
        public void NullableDecimalValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", 123456.789M } };
            var result = dict.GetColumnValueByName<decimal?>("id");
            Assert.Equal(123456.789M, result);
        }

        [Fact]
        public void NullableDoubleValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", 123456.789 } };
            var result = dict.GetColumnValueByName<double?>("id");
            Assert.Equal(123456.789, result);
        }

        [Fact]
        public void NullableDateTimeValue_Verify()
        {
            var date = DateTime.Now;
            var dict = new Dictionary<string, object> { { "id", date } };
            var result = dict.GetColumnValueByName<DateTime?>("id");
            Assert.Equal(date, result);
        }

        [Fact]
        public void NullableGuidValue_Verify()
        {
            var guid = Guid.NewGuid();
            var dict = new Dictionary<string, object> { { "id", guid } };
            var result = dict.GetColumnValueByName<Guid?>("id");
            Assert.Equal(guid, result);
        }

        [Fact]
        public void NullableBoolValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", true } };
            var result = dict.GetColumnValueByName<bool?>("id");
            Assert.True(result);
        }

        [Fact]
        public void NullableByteValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", (byte)123 } };
            var result = dict.GetColumnValueByName<byte?>("id");
            Assert.Equal((byte)123, result);
        }

        [Fact]
        public void NullableShortValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", (short)123 } };
            var result = dict.GetColumnValueByName<short?>("id");
            Assert.Equal((short)123, result);
        }

        [Fact]
        public void NullableLongValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", (long)123456 } };
            var result = dict.GetColumnValueByName<long?>("id");
            Assert.Equal(123456, result);
        }

        [Fact]
        public void StringValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", "123456" } };
            var result = dict.GetColumnValueByName<string>("id");
            Assert.Equal("123456", result);
        }

        [Fact]
        public void NullStringValue_Verify()
        {
            var dict = new Dictionary<string, object> { { "id", null } };
            var result = dict.GetColumnValueByName<string>("id");
            Assert.Null(result);
        }
    }
}