using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Azrng.Core.Helpers
{
    public class SqlHelper
    {
        // 编译好的正则表达式，提高性能
        private static readonly Regex[] _sqlInjectionPatterns =
        [
            // 1. 注释符检测 (--, /* */)
            // 修改：# 必须在行首（可选空白后）才算SQL注释，避免误判 "C#"
            new Regex(@"--.*?$|^\s*#.*?$|/\*.*?\*/",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline),

            // 2. SQL关键词检测 (SELECT, INSERT, DROP 等)
            // 注意：UNION 单独出现可能是正常词汇(如"European Union")，所以移到后面与 SELECT/ALL 组合检测
            new Regex(
                @"\b(ALTER|BEGIN|CAST|CREATE|DELETE|DROP|EXEC(UTE)?|FETCH|GRANT|INSERT|KILL|OPEN|RENAME|SELECT|SHUTDOWN|TRUNCATE|UPDATE)\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // 3. 表达式检测 (OR 1=1, OR 'a'='a')
            new Regex(@"\bOR\s+[\w'""]+\s*=\s*[\w'""]+",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // 4. 语句分隔符检测 (;)
            new Regex(@";\s*[^\s]",
                RegexOptions.Compiled),

            // 5. 等待/延迟函数检测
            new Regex(@"\b(WAITFOR|SLEEP)\b\s*?\(?[0-9]+\)?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // 6. 系统函数和存储过程检测
            // 修改：移除单独的 VERSION，改为在特殊字符组合中检测 @@VERSION
            // XP_ 和 SP_ 是前缀，不需要后面的词边界
            new Regex(@"\b(CAST|CONVERT|DBCC)\b|XP_|SP_",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // 7. 特殊字符组合检测
            new Regex(@"0x[0-9A-Fa-f]{8,}|NULL,NULL|IF\(1=1|@@VERSION",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // 8. 字符串拼接和SQL片段检测
            new Regex(@"'\s*?\+\s*?|%27\s*?\+\s*?|\|\|",
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // 9. UNION注入检测 (UNION必须跟SELECT或ALL才算SQL注入)
            new Regex(@"\bUNION\s+(ALL\s+)?SELECT",
                RegexOptions.Compiled | RegexOptions.IgnoreCase)
        ];

        // 更复杂的SQL注入模式检测 (更精确但性能较低)
        private static readonly Regex _complexPattern =
            new Regex(@"('|\s*?)(OR|AND)\s+?(\d{1,10}|'[^']{1,50}')\s*?(=|LIKE)\s*?\3",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 检测输入字符串是否包含潜在的SQL注入攻击
        /// </summary>
        /// <param name="input">要检测的用户输入(要检测的是参数，不是完整的sql)</param>
        /// <param name="strictMode">严格模式（启用更精确但性能较低的检测）</param>
        /// <returns>如果检测到潜在SQL注入攻击，返回true；否则返回false</returns>
        public static bool HasSqlInjectionRisk(string input, bool strictMode = true)
        {
            // 空输入直接通过
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // 输入长度检查（超长输入可能有风险）
            if (input.Length > 5000) // 设置合理阈值
                return true;

            // 简单模式检测：检查是否包含特殊字符
            if (HasSuspiciousCharacters(input))
                return true;

            // 快速检查模式（高性能正则）
            if (_sqlInjectionPatterns.Any(pattern => pattern.IsMatch(input)))
                return true;

            // 启用更复杂的检测（性能较低）
            if (strictMode && _complexPattern.IsMatch(input))
                return true;

            // 上下文相关关键词检测
            if (DetectContextSpecificKeywords(input))
                return true;

            // 尝试SQL片段执行检测（谨慎使用）
            if (DetectSqlFragmentExecution(input))
                return true;

            return false;
        }

        /// <summary>
        /// 快速检测用户输入是否包含SQL关键字
        /// </summary>
        /// <param name="input">要检测的字符串</param>
        /// <returns>当检测到客户的输入中有攻击性危险字符串,则返回false,有效返回true。</returns>
        public static bool IsSafeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                //如果是空值,则跳出
                return true;
            }

            // 转换为小写以进行不区分大小写的比较
            var lowerInput = input.ToLower();

            // 检测危险的SQL关键字（使用词边界以避免误报）
            string[] dangerousKeywords =
            [
                "exec", "execute", "insert", "select", "delete", "update", "drop",
                "create", "alter", "truncate", "declare", "xp_", "sp_", "union",
                "script", "javascript", "vbscript", "onload", "onerror"
            ];

            foreach (var keyword in dangerousKeywords)
            {
                // 使用正则表达式检查词边界，避免 "android" 包含 "and" 的误报
                if (Regex.IsMatch(lowerInput, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }

            // 检测危险的字符组合
            string[] dangerousPatterns =
            {
                "--",      // SQL注释
                "/*",      // 块注释
                "*/",      // 块注释结束
                ";--",     // 语句分隔符加注释
                "';",      // 字符串结束加语句分隔符
                "' or ",   // 常见注入模式
                "' and ",  // 常见注入模式
                "1=1",     // 永真条件
                "1' or '1'='1", // 经典注入
                "' or ''='",    // 经典注入变体
                "%27",     // URL编码的单引号
                "char(",   // SQL字符函数
                "0x"       // 十六进制前缀（仅在开头或特定上下文中危险）
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (lowerInput.Contains(pattern))
                {
                    return false;
                }
            }

            //未检测到攻击字符串
            return true;
        }

        #region 私有方法

        /// <summary>
        /// 检测输入中是否包含可疑的特殊字符组合
        /// </summary>
        private static bool HasSuspiciousCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // 检查高风险特殊字符组合
            if (input.Contains("--") || // SQL注释
                input.Contains("/*") || // 块注释开始
                input.Contains("*/") || // 块注释结束
                input.Contains(";") || // 语句分隔符
                input.Contains("%27") || // URL编码的单引号
                input.Contains("char(") || // SQL字符函数
                input.Contains("xp_") || // 扩展存储过程
                input.Contains("0x")) // 十六进制前缀
            {
                return true;
            }

            // 智能检测单引号使用场景
            if (input.Contains("'"))
            {
                // 检查是否是SQL注入的常见模式
                var sqlInjectionPatterns = new[]
                                           {
                                               "' or '",
                                               "' and '",
                                               "';",
                                               "' --",
                                               "' /*",
                                               "'='",
                                               "' like '",
                                               "' union ",
                                               "WAITFOR DELAY '0:0:10'",
                                               "' in (",
                                               "' where ",
                                               "' from ",
                                               "' into "
                                           };

                foreach (var pattern in sqlInjectionPatterns)
                {
                    if (input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                // 检查连续的单引号（可能是转义或注入尝试）
                if (input.Contains("''") && input.Contains("'';"))
                {
                    return true;
                }

                // 检查单引号与其他SQL操作符的组合
                if (Regex.IsMatch(input, @"'\s*(OR|AND|UNION|SELECT|INSERT|DELETE|UPDATE|DROP)\s",
                    RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 上下文相关关键词检测
        /// </summary>
        private static bool DetectContextSpecificKeywords(string input)
        {
            // 处理SQL表达式相关的上下文关键词
            string[] dangerousKeywords =
            {
                "DROP TABLE",
                "DELETE FROM",
                "ALTER TABLE",
                "TRUNCATE TABLE",
                "CREATE TABLE",
                "EXEC sp_",
                "INSERT INTO",
                "SELECT * FROM",
                "UNION SELECT",
                "FROM INFORMATION_SCHEMA",
                "sys.tables"
            };

            foreach (var keyword in dangerousKeywords)
            {
                if (input.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 检查关键词是否在字符串内
                    if (!IsInsideQuotedString(input, keyword))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 检查关键词是否在引号字符串内部（避免误报）
        /// </summary>
        private static bool IsInsideQuotedString(string input, string substring)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(substring))
                return false;

            // 查找子字符串位置
            var index = input.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
            if (index < 0) return false;

            // 向前扫描到最近的非引号字符
            var singleQuoteCount = 0;

            for (var i = 0; i < index; i++)
            {
                if (input[i] == '\'')
                {
                    singleQuoteCount++;
                }
                else if (input[i] == '\"') { }
            }

            // 检查开始位置是否在引号内（奇数表示在字符串内）
            return (singleQuoteCount % 2) == 1;
        }

        /// <summary>
        /// 检测SQL片段执行（谨慎使用）
        /// </summary>
        private static bool DetectSqlFragmentExecution(string input)
        {
            // 高风险模式：系统命令执行
            string[] highRiskPatterns =
            {
                "EXEC master..xp_cmdshell",
                "OLE AUTOMATION PROCEDURES",
                "sp_OACreate",
                "sp_OAMethod",
                "sp_executeexternalcript"
            };

            foreach (var pattern in highRiskPatterns)
            {
                if (input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            // 文件系统访问检测
            if (Regex.IsMatch(input, @"\b(BULK INSERT|OPENROWSET|OPENQUERY)\b.*?FROM\s+['""]",
                    RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}