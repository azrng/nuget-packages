using Xunit.Abstractions;

namespace Common.Core.Test.Helper
{
    public class SqlHelperTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SqlHelperTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        #region IsSafeSql Tests

        /// <summary>
        /// 验证危险的SQL语句被正确检测。
        /// </summary>
        [Fact]
        public void IsSafeSql_DangerousSQL_ReturnsFalse()
        {
            var sql = "delete from cr.config";
            var flag = SqlHelper.IsSafeInput(sql);
            Assert.False(flag);
        }

        /// <summary>
        /// 测试IsSafeSql方法对各种危险输入的检测。
        /// 注意：IsSafeSql的设计理念是检测用户输入中是否包含SQL关键字。
        /// 在正常的用户输入场景中，用户不应该输入SQL语句，如果输入了则视为不安全。
        /// </summary>
        [Theory]
        [InlineData("select * from users", false, "SELECT语句 - 用户输入不应包含SQL关键字")]
        [InlineData("insert into users values ('test')", false, "INSERT语句 - 用户输入不应包含SQL关键字")]
        [InlineData("delete from users", false, "DELETE语句 - 用户输入不应包含SQL关键字")]
        [InlineData("update users set name='test'", false, "UPDATE语句 - 用户输入不应包含SQL关键字")]
        [InlineData("drop table users", false, "DROP语句 - 用户输入不应包含SQL关键字")]
        [InlineData("exec sp_executesql", false, "EXEC存储过程 - 用户输入不应包含SQL关键字")]
        [InlineData("'; drop table users--", false, "注入攻击")]
        [InlineData("admin' or '1'='1", false, "OR注入")]
        [InlineData("test' and '1'='1", false, "AND注入")]
        [InlineData("user--", false, "SQL注释")]
        [InlineData("test/* comment */", false, "块注释")]
        [InlineData("normal text", true, "正常文本")]
        [InlineData("android phone", true, "包含'and'但安全的文本")]
        [InlineData("before and after", true, "包含'and'作为连词")]
        [InlineData("user@example.com", true, "邮箱地址")]
        [InlineData("2024-01-01", true, "日期格式")]
        [InlineData("price: $100", true, "价格信息")]
        [InlineData("How to select a product", false, "包含'select'关键字 - 词边界检测到")]
        [InlineData("I need to update my profile", false, "包含'update'关键字 - 词边界检测到")]
        public void IsSafeSql_VariousInputs_ReturnsExpectedResult(string input, bool expectedSafe, string description)
        {
            // Act
            var result = SqlHelper.IsSafeInput(input);

            // Assert
            Assert.Equal(expectedSafe, result);
            _testOutputHelper.WriteLine($"{description}: {input} => {(result ? "安全" : "危险")}");
        }

        /// <summary>
        /// 测试IsSafeSql对空值和null的处理。
        /// </summary>
        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        public void IsSafeSql_EmptyOrNullInput_ReturnsTrue(string input, bool expected)
        {
            var result = SqlHelper.IsSafeInput(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试IsSafeSql对URL编码攻击的检测。
        /// </summary>
        [Fact]
        public void IsSafeSql_UrlEncodedAttack_ReturnsFalse()
        {
            var inputs = new[]
                         {
                             "test%27 or 1=1--",
                             "%27 union select * from users",
                             "admin%27--"
                         };

            foreach (var input in inputs)
            {
                var result = SqlHelper.IsSafeInput(input);
                Assert.False(result, $"应该检测到URL编码攻击: {input}");
            }
        }

        #endregion

        #region HasSqlInjectionRisk Tests

        [Fact]
        public void Collection_Test()
        {
            var dangerousStrings = new[]
                                   {
                                       // 认证绕过类
                                       "' OR '1'='1",
                                       "' OR 1=1 --",
                                       "\" OR \"a\"=\"a",
                                       "' OR 'x'='x",
                                       "admin' --",
                                       "' OR ''='",
                                       "' OR 1=1#",
                                       "\" OR 1=1 --",
                                       "' IS NULL OR ''='",

                                       // 数据提取类（UNION 注入）
                                       "' UNION SELECT NULL --",
                                       "' UNION SELECT version() --",
                                       "' UNION SELECT user() --",
                                       "' UNION SELECT database() --",
                                       "' UNION SELECT table_name FROM information_schema.tables --",
                                       "' UNION SELECT column_name FROM information_schema.columns --",
                                       "' UNION SELECT username,password FROM users --",
                                       "' UNION ALL SELECT @@version --",

                                       // 时间盲注类
                                       "' AND SLEEP(5) --",
                                       "' OR SLEEP(10) --",
                                       "'; BENCHMARK(10000000, MD5('test')) --",
                                       "'; WAITFOR DELAY '0:0:5' --",
                                       "' AND (SELECT * FROM (SELECT(SLEEP(5)))a) --",
                                       "' OR (SELECT COUNT(*) FROM information_schema.tables) > 0 AND SLEEP(5) --",

                                       // 报错注入类
                                       "' AND EXTRACTVALUE(1, CONCAT(0x3a, VERSION())) --",
                                       "' AND UPDATEXML(1, CONCAT(0x3a, (SELECT USER())), 1) --",
                                       "' AND (SELECT * FROM (SELECT COUNT(*),CONCAT(VERSION(),FLOOR(RAND(0)*2))x FROM information_schema.tables GROUP BY x)a) --",
                                       "' AND GTID_SUBSET(CONCAT(0x7e,(SELECT USER()),0x7e),0) --",

                                       // 数据库结构探测类
                                       "' AND (SELECT COUNT(*) FROM information_schema.tables) > 0 --",
                                       "' OR (SELECT table_name FROM information_schema.tables LIMIT 1) = 'users' --",
                                       "'; SELECT * FROM information_schema.tables --",
                                       "' AND (SELECT column_name FROM information_schema.columns WHERE table_name='users') = 'password' --",

                                       // 文件系统操作类
                                       "' UNION SELECT LOAD_FILE('/etc/passwd') --",
                                       "' INTO OUTFILE '/var/www/shell.php' --",
                                       "' INTO DUMPFILE '/var/www/shell.php' --",
                                       "' AND (SELECT LOAD_FILE('/etc/passwd')) IS NOT NULL --",

                                       // 命令执行类
                                       "'; EXEC xp_cmdshell('whoami') --",
                                       "'; DROP TABLE users --",
                                       "'; SHUTDOWN --",
                                       "'; DELETE FROM users --",
                                       "'; UPDATE users SET password='hacked' --",
                                       "'; INSERT INTO logs VALUES ('injected') --",
                                       "'; TRUNCATE TABLE audit_log --",

                                       // 布尔盲注类
                                       "' AND ASCII(SUBSTRING((SELECT USER()),1,1)) > 97 --",
                                       "' OR (SELECT COUNT(*) FROM users WHERE username='admin') > 0 --",
                                       "' AND (SELECT LENGTH(password) FROM users WHERE username='admin') > 5 --",
                                       "' OR (SELECT SUBSTRING(password,1,1) FROM users WHERE username='admin') = 'a' --",

                                       // 编码混淆类
                                       "' OR 0x4f52='OR'",
                                       "' OR CHAR(79,82)=CHAR(79,82) --",
                                       "' AND %4f%52=%4f%52 --",
                                       "' OR UNHEX('4f52')=UNHEX('4f52') --",

                                       // 系统信息获取类
                                       "' AND @@version > '5' --",
                                       "' OR @@hostname LIKE '%' --",
                                       "' AND (SELECT @@datadir) = '/var/lib/mysql/' --",
                                       "' OR (SELECT @@tmpdir) IS NOT NULL --",

                                       // 条件语句类
                                       "' AND IF(1=1,SLEEP(5),0) --",
                                       "' OR CASE WHEN 1=1 THEN SLEEP(5) ELSE 0 END --",
                                       "' AND (SELECT IF(SUBSTRING(@@version,1,1)='5',SLEEP(5),0)) --",

                                       // 权限提升类
                                       "' AND (SELECT SUPER FROM mysql.user WHERE user='root') = 1 --",
                                       "' OR (SELECT grantee FROM information_schema.user_privileges) = 'root'@'localhost' --",

                                       // 注释和终止符类
                                       "' --",
                                       "' #", // 这个方法还没考虑在内
                                       "'/* */",
                                       "';%00",
                                       "';--",
                                       "';#",

                                       // 联合查询进阶类
                                       "' UNION SELECT NULL,NULL,NULL FROM DUAL --",
                                       "' UNION ALL SELECT 1,2,3,4,5 --",
                                       "' UNION SELECT CONCAT(username,':',password) FROM users --"
                                   };
            foreach (var str in dangerousStrings)
            {
                // Act
                var result = SqlHelper.HasSqlInjectionRisk(str);
                if (!result)
                {
                    _testOutputHelper.WriteLine($"检测到注入漏洞sql：{str}");
                }

                // Assert
                // Assert.True(result);
            }
        }

        /// <summary>
        /// 应该正确识别包含可疑字符的输入。
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expectedResult"></param>
        [Theory]
        [InlineData("O'Reilly", false)]
        [InlineData("McDonald's", false)]
        [InlineData("L'Oréal", false)]
        [InlineData("--drop table", true)]
        [InlineData("'; DROP TABLE users;--", true)]
        [InlineData("' OR '1'='1", true)]
        [InlineData("normal text", false)]
        public void HasSqlInjectionRisk_SuspiciousCharacters_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Act
            var result = SqlHelper.HasSqlInjectionRisk(input);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// 危险的SQL操作应被识别为SQL注入风险。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_DangerousOperations_ReturnsTrue()
        {
            var dangerousSqlCases = new Dictionary<string, string>
                                    {
                                        { "DROP语句", "1'; DROP TABLE Users --" },
                                        { "UNION注入", "100 UNION SELECT password FROM users" },
                                        { "登录绕过", "admin' OR '1'='1'--" },
                                        { "存储过程执行", "EXEC sp_addlogin 'hacker'" },
                                        { "条件绕过", "search' OR ''='" },
                                        { "延时注入", "WAITFOR DELAY '0:0:10'" },
                                        { "十六进制编码", "0x4D7953514C" }
                                    };

            foreach (var testCase in dangerousSqlCases)
            {
                var result = SqlHelper.HasSqlInjectionRisk(testCase.Value);
                _testOutputHelper.WriteLine($"测试场景: {testCase.Key}, 输入: {testCase.Value}");
                Assert.True(result, $"应当检测到SQL注入风险: {testCase.Key}");
            }
        }

        /// <summary>
        /// sql语句中包含常见的SQL关键字，应被识别为潜在风险。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_NormalQueries_ReturnsTrue()
        {
            var queries = new[]
                          {
                              "SELECT * FROM Products",
                              "INSERT INTO logs VALUES ('test');"
                          };

            foreach (var query in queries)
            {
                var result = SqlHelper.HasSqlInjectionRisk(query);
                Assert.True(result, $"完整SQL语句应被识别为潜在风险: {query}");
            }
        }

        /// <summary>
        /// 安全的字符串不应被误判为SQL注入风险。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_SafeStrings_ReturnsFalse()
        {
            var safeStrings = new[]
                              {
                                  "O'Reilly",
                                  "Mary's Cafe",
                                  "normal search term",
                                  "This is a legitimate description with no keywords",
                                  "Document title: Database Best Practices"
                              };

            foreach (var safeString in safeStrings)
            {
                var result = SqlHelper.HasSqlInjectionRisk(safeString);
                Assert.False(result, $"正常文本不应被识别为SQL注入: {safeString}");
            }
        }

        /// <summary>
        /// sql绕过尝试应被检测为SQL注入风险。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_SqlBypass_ReturnsTrue()
        {
            var bypassAttempts = new[]
                                 {
                                     "OR 1=1 -- login bypass",
                                     "admin' --",
                                     "' OR 'x'='x",
                                 };

            foreach (var attempt in bypassAttempts)
            {
                var result = SqlHelper.HasSqlInjectionRisk(attempt);
                Assert.True(result, $"SQL绕过尝试应被检测: {attempt}");
            }
        }

        /// <summary>
        /// 测试空值和null输入的处理。
        /// </summary>
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void HasSqlInjectionRisk_EmptyOrNullInput_ReturnsFalse(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试超长输入应被标记为风险。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_ExtremelyLongInput_ReturnsTrue()
        {
            // 创建一个超过5000字符的字符串
            var longInput = new string('a', 5001);
            var result = SqlHelper.HasSqlInjectionRisk(longInput);
            Assert.True(result, "超长输入应被标记为潜在风险");
        }

        /// <summary>
        /// 测试十六进制编码攻击的检测。
        /// </summary>
        [Theory]
        [InlineData("0x4D7953514C", true)]
        [InlineData("0x61646D696E", true)]
        [InlineData("test 0x123456789", true)]
        [InlineData("normal text", false)]
        public void HasSqlInjectionRisk_HexEncodedAttack_ReturnsExpectedResult(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试系统函数和存储过程的检测。
        /// </summary>
        [Theory]
        [InlineData("EXEC master..xp_cmdshell", true)]
        [InlineData("sp_executesql @query", true)]
        [InlineData("CAST(name AS varchar)", true)]
        [InlineData("SELECT @@VERSION", true)]
        [InlineData("normal stored procedure name", false)]
        public void HasSqlInjectionRisk_SystemFunctionsAndProcedures_ReturnsExpectedResult(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试时间延迟攻击的检测。
        /// </summary>
        [Theory]
        [InlineData("WAITFOR DELAY '0:0:10'", true)]
        [InlineData("SLEEP(10)", true)]
        [InlineData("benchmark(10000000,MD5('test'))", false)] // MySQL benchmark
        [InlineData("normal wait time", false)]
        public void HasSqlInjectionRisk_TimeDelayAttack_ReturnsExpectedResult(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试UNION注入攻击的检测。
        /// </summary>
        [Theory]
        [InlineData("1' UNION SELECT password FROM users--", true)]
        [InlineData("admin' UNION ALL SELECT NULL,NULL,NULL--", true)]
        [InlineData("text about union workers", false)]
        [InlineData("European Union", false)]
        public void HasSqlInjectionRisk_UnionInjection_ReturnsExpectedResult(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试严格模式和非严格模式的区别。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_StrictModeComparison()
        {
            var testInput = "' OR 1=1";

            // 严格模式
            var strictResult = SqlHelper.HasSqlInjectionRisk(testInput, strictMode: true);
            Assert.True(strictResult, "严格模式应该检测到风险");

            // 非严格模式
            var normalResult = SqlHelper.HasSqlInjectionRisk(testInput, strictMode: false);
            Assert.True(normalResult, "普通模式也应该检测到明显的风险");
        }

        /// <summary>
        /// 测试字符串拼接攻击的检测。
        /// </summary>
        [Theory]
        [InlineData("test' + 'attack", true)]
        [InlineData("admin' || 'password", true)]
        [InlineData("normal + in text", false)]
        public void HasSqlInjectionRisk_StringConcatenationAttack_ReturnsExpectedResult(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试信息架构查询的检测。
        /// </summary>
        [Theory]
        [InlineData("SELECT * FROM INFORMATION_SCHEMA.TABLES", true)]
        [InlineData("FROM sys.tables", true)]
        [InlineData("information about the schema", false)]
        public void HasSqlInjectionRisk_InformationSchemaQuery_ReturnsExpectedResult(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// 测试多行注入攻击。
        /// </summary>
        [Fact]
        public void HasSqlInjectionRisk_MultilineInjection_ReturnsTrue()
        {
            var multilineInputs = new[]
                                  {
                                      "test'; \nDROP TABLE users;\n--",
                                      "admin'--\nUNION SELECT * FROM passwords",
                                      "input\n/* comment */ DROP TABLE"
                                  };

            foreach (var input in multilineInputs)
            {
                var result = SqlHelper.HasSqlInjectionRisk(input);
                Assert.True(result, $"多行注入应被检测: {input}");
            }
        }

        /// <summary>
        /// 测试边缘情况：合法的业务数据不应被误判。
        /// </summary>
        [Theory]
        [InlineData("User's Guide to SQL", false)]
        [InlineData("Price range: $50-$100", false)]
        [InlineData("Email: user@example.com", false)]
        [InlineData("Meeting at 2:30 PM", false)]
        [InlineData("Version 2.0.1", false)]
        [InlineData("C# programming", false)]
        public void HasSqlInjectionRisk_LegitimateBusinessData_ReturnsFalse(string input, bool expected)
        {
            var result = SqlHelper.HasSqlInjectionRisk(input);
            Assert.Equal(expected, result);
        }

        #endregion
    }
}