using System.Text.RegularExpressions;

namespace Azrng.DynamicSqlBuilder.Validation
{
    /// <summary>
    /// 字段名验证器 - 防止SQL注入和无效字段名
    /// </summary>
    public static class FieldNameValidator
    {
        /// <summary>
        /// SQL关键字列表（不应用作字段名）
        /// </summary>
        private static readonly HashSet<string> SqlKeywords = new(StringComparer.OrdinalIgnoreCase)
                                                              {
                                                                  // SQL DDL keywords
                                                                  "CREATE",
                                                                  "ALTER",
                                                                  "DROP",
                                                                  "TRUNCATE",
                                                                  "COMMENT",
                                                                  "RENAME",
                                                                  "SELECT",
                                                                  "INSERT",
                                                                  "UPDATE",
                                                                  "DELETE",
                                                                  "MERGE",
                                                                  "CALL",
                                                                  "EXPLAIN",
                                                                  "PLAN",
                                                                  "FROM",
                                                                  "WHERE",
                                                                  "ORDER",
                                                                  "GROUP",
                                                                  "HAVING",
                                                                  "LIMIT",
                                                                  "OFFSET",
                                                                  "JOIN",
                                                                  "INNER",
                                                                  "OUTER",
                                                                  "LEFT",
                                                                  "RIGHT",
                                                                  "FULL",
                                                                  "CROSS",
                                                                  "NATURAL",
                                                                  "UNION",
                                                                  "INTERSECT",
                                                                  "EXCEPT",
                                                                  "MINUS",
                                                                  "AND",
                                                                  "OR",
                                                                  "NOT",
                                                                  "IN",
                                                                  "EXISTS",
                                                                  "BETWEEN",
                                                                  "LIKE",
                                                                  "IS",
                                                                  "NULL",
                                                                  "DISTINCT",
                                                                  "ALL",
                                                                  "ANY",
                                                                  "SOME",
                                                                  "ASC",
                                                                  "DESC",
                                                                  "WITH",
                                                                  "RECURSIVE",
                                                                  "CASE",
                                                                  "WHEN",
                                                                  "THEN",
                                                                  "ELSE",
                                                                  "END",
                                                                  "TABLE",
                                                                  "VIEW",
                                                                  "INDEX",
                                                                  "SEQUENCE",
                                                                  "SCHEMA",
                                                                  "DATABASE",
                                                                  "BEGIN",
                                                                  "COMMIT",
                                                                  "ROLLBACK",
                                                                  "TRANSACTION",
                                                                  "SAVEPOINT",
                                                                  "GRANT",
                                                                  "REVOKE",
                                                                  "DENY",
                                                                  "PRIMARY",
                                                                  "FOREIGN",
                                                                  "KEY",
                                                                  "REFERENCES",
                                                                  "UNIQUE",
                                                                  "CHECK",
                                                                  "DEFAULT",
                                                                  "CONSTRAINT",
                                                                  "COLLATE",
                                                                  "CHARACTER",
                                                                  "SET"
                                                              };

        /// <summary>
        /// 字段名正则表达式模式
        /// 只允许字母、数字、下划线，且必须以字母或下划线开头
        /// </summary>
        private static readonly Regex FieldNamePattern = new(@"^[a-zA-Z_.][a-zA-Z0-9_.]*$", RegexOptions.Compiled);

        /// <summary>
        /// 最大字段名长度（大多数数据库支持128字符）
        /// </summary>
        private const int MaxFieldNameLength = 128;

        /// <summary>
        /// 验证字段名是否有效
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <returns>是否有效</returns>
        private static bool IsValidFieldName(string fieldName)
        {
            // 检查是否为空
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            // 检查长度
            if (fieldName.Length > MaxFieldNameLength)
            {
                return false;
            }

            // 检查格式（只允许字母、数字、下划线）
            if (!FieldNamePattern.IsMatch(fieldName))
            {
                return false;
            }

            // 检查是否是SQL关键字
            if (SqlKeywords.Contains(fieldName))
            {
                return false;
            }

            // 检查是否包含特殊字符组合（SQL注入尝试）
            if (ContainsSuspiciousPatterns(fieldName))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证多个字段名
        /// </summary>
        /// <param name="fieldNames">字段名集合</param>
        /// <param name="invalidFieldNames">无效的字段名列表（输出参数）</param>
        /// <returns>是否全部有效</returns>
        public static bool AreValidFieldNames(IEnumerable<string> fieldNames, out List<string> invalidFieldNames)
        {
            invalidFieldNames = new List<string>();

            foreach (var fieldName in fieldNames)
            {
                if (!IsValidFieldName(fieldName))
                {
                    invalidFieldNames.Add(fieldName);
                }
            }

            return invalidFieldNames.Count == 0;
        }

        /// <summary>
        /// 检查字段名是否包含可疑模式（SQL注入尝试）
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <returns>是否包含可疑模式</returns>
        private static bool ContainsSuspiciousPatterns(string fieldName)
        {
            var suspiciousPatterns = new[]
                                     {
                                         "--", // SQL注释
                                         "/*", // 多行注释开始
                                         "*/", // 多行注释结束
                                         ";", // 语句分隔符
                                         "'", // 单引号
                                         "\"", // 双引号
                                         "xp_", // SQL Server扩展存储过程前缀
                                         "sp_", // SQL Server系统存储过程前缀
                                         "DROP",         // DROP TABLE关键字
                                         "DELETE",       // DELETE关键字
                                         "TRUNCATE", // TRUNCATE关键字
                                         "EXEC", // EXECUTE关键字
                                         "EXECUTE", // EXECUTE完整关键字
                                         "SCRIPT", // SCRIPT关键字
                                         "JAVASCRIPT", // JAVASCRIPT关键字
                                         "<script", // Script标签
                                         "ONERROR", // JavaScript事件
                                         "ONLOAD" // JavaScript事件
                                     };

            var upperFieldName = fieldName.ToUpper();

            return suspiciousPatterns.Contains(upperFieldName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 验证并抛出异常（如果无效）
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="paramName">参数名（用于异常消息）</param>
        /// <exception cref="ArgumentException">字段名无效时抛出</exception>
        public static void ValidateFieldName(string fieldName, string paramName = null)
        {
            if (!IsValidFieldName(fieldName))
            {
                var message = string.IsNullOrWhiteSpace(fieldName)
                    ? "字段名不能为空"
                    : $"字段名 '{fieldName}' 无效。字段名必须以字母或下划线开头，只能包含字母、数字和下划线，且不能是SQL关键字。";

                throw new ArgumentException(message, paramName ?? nameof(fieldName));
            }
        }
    }
}