using Azrng.Core.Extension;
using System;
using System.Collections.Generic;
using Xunit;

namespace Common.Test.Extension
{
    public class DictionaryTest
    {
        private static DateTime? _dateTime = null;
        private static int? _int = null;
        private static decimal? _decimal = null;
        public static Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "id", 123 },
            { "id2", _int },
            { "name", null },
            { "name2", "张三" },
            { "decimal", 12.42m },
            { "decimal2", _decimal },
            { "createtime", Convert.ToDateTime("2022-01-01") },
            { "createtime2", _dateTime}
        };

        /*
         获取值类型返回值类型
         获取时间返回时间
        获取字符串返回字符串
         获取null值，就给默认值

         */

        [Fact]
        public void GetString_ReturnOk()
        {
            var result = dict.GetColumnValueByName<string>("name2");
            Assert.Equal("张三", result);
        }

        [Fact]
        public void GetNullString_ReturnEmpty()
        {
            var result = dict.GetColumnValueByName<string>("name");
            Assert.Equal("", result);
        }

        [Fact]
        public void GetInt_ReturnOk()
        {
            var result = dict.GetColumnValueByName<int>("id");
            Assert.Equal(123, result);
        }

        [Fact]
        public void GetNullInt_Return0()
        {
            var result = dict.GetColumnValueByName<int>("id2");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDecimal_ReturnOk()
        {
            var result = dict.GetColumnValueByName<decimal>("decimal");
            Assert.Equal(12.42m, result);
        }

        [Fact]
        public void GetNullDecimal_Return0()
        {
            var result = dict.GetColumnValueByName<decimal?>("decimal2");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDateTime_ReturnOk()
        {
            var result = dict.GetColumnValueByName<DateTime>("createtime");
            Assert.Equal(Convert.ToDateTime("2022-01-01"), result);
        }

        [Fact]
        public void GetNullDateTime_ReturnNull()
        {
            var result = dict.GetColumnValueByName<DateTime?>("createtime2");
            Assert.Null(result);
        }
    }
}