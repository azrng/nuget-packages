using System.Text.RegularExpressions;

namespace Azrng.Database.DynamicSqlBuilder.Validation
{
    public static class FieldNameValidator
    {
        private static readonly HashSet<string> SqlKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "CREATE", "ALTER", "DROP", "TRUNCATE", "COMMENT", "RENAME", "SELECT", "INSERT", "UPDATE", "DELETE",
            "MERGE", "CALL", "EXPLAIN", "PLAN", "FROM", "WHERE", "ORDER", "GROUP", "HAVING", "LIMIT", "OFFSET",
            "JOIN", "INNER", "OUTER", "LEFT", "RIGHT", "FULL", "CROSS", "NATURAL", "UNION", "INTERSECT", "EXCEPT",
            "MINUS", "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE", "IS", "NULL", "DISTINCT", "ALL",
            "ANY", "SOME", "ASC", "DESC", "WITH", "RECURSIVE", "CASE", "WHEN", "THEN", "ELSE", "END", "TABLE",
            "VIEW", "INDEX", "SEQUENCE", "SCHEMA", "DATABASE", "BEGIN", "COMMIT", "ROLLBACK", "TRANSACTION",
            "SAVEPOINT", "GRANT", "REVOKE", "DENY", "PRIMARY", "FOREIGN", "KEY", "REFERENCES", "UNIQUE", "CHECK",
            "DEFAULT", "CONSTRAINT", "COLLATE", "CHARACTER", "SET"
        };

        private static readonly Regex FieldNamePattern = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

        private const int MaxFieldNameLength = 128;

        private static bool IsValidFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length > MaxFieldNameLength)
            {
                return false;
            }

            if (IsQuotedFieldName(fieldName, out var unquotedFieldName))
            {
                return IsValidQuotedFieldName(unquotedFieldName);
            }

            if (fieldName.Contains('.'))
            {
                return IsValidTableAliasFieldName(fieldName);
            }

            return IsValidIdentifier(fieldName);
        }

        private static bool IsQuotedFieldName(string fieldName, out string unquotedFieldName)
        {
            unquotedFieldName = fieldName;

            if (fieldName.Length < 2)
            {
                return false;
            }

            if (fieldName.StartsWith("\"") && fieldName.EndsWith("\""))
            {
                unquotedFieldName = fieldName[1..^1];
                return true;
            }

            if (fieldName.StartsWith("[") && fieldName.EndsWith("]"))
            {
                unquotedFieldName = fieldName[1..^1];
                return true;
            }

            if (fieldName.StartsWith("`") && fieldName.EndsWith("`"))
            {
                unquotedFieldName = fieldName[1..^1];
                return true;
            }

            return false;
        }

        private static bool IsValidQuotedFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length > MaxFieldNameLength)
            {
                return false;
            }

            return !ContainsSuspiciousPatterns(fieldName);
        }

        private static bool IsValidIdentifier(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length > MaxFieldNameLength)
            {
                return false;
            }

            if (ContainsSuspiciousPatterns(fieldName))
            {
                return false;
            }

            return FieldNamePattern.IsMatch(fieldName) && !SqlKeywords.Contains(fieldName);
        }

        private static bool IsValidTableAliasFieldName(string fieldName)
        {
            var parts = fieldName.Split('.');
            if (parts.Length > 2)
            {
                return false;
            }

            return parts.All(part =>
                IsQuotedFieldName(part, out var unquotedPart)
                    ? IsValidQuotedFieldName(unquotedPart)
                    : IsValidIdentifier(part));
        }

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

        private static bool ContainsSuspiciousPatterns(string fieldName)
        {
            var suspiciousPatterns = new[]
            {
                "--",
                "/*",
                "*/",
                ";",
                "'",
                "\"",
                "<script"
            };

            if (suspiciousPatterns.Any(pattern => fieldName.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return fieldName.StartsWith("xp_", StringComparison.OrdinalIgnoreCase)
                   || fieldName.StartsWith("sp_", StringComparison.OrdinalIgnoreCase);
        }

        public static void ValidateFieldName(string fieldName, string paramName = null)
        {
            if (!IsValidFieldName(fieldName))
            {
                var message = string.IsNullOrWhiteSpace(fieldName)
                    ? "Field name cannot be empty."
                    : $"Invalid field name '{fieldName}'. Field names must start with a letter or underscore, contain only letters, numbers, underscores, or a single table alias prefix, and must not be SQL keywords.";

                throw new ArgumentException(message, paramName ?? nameof(fieldName));
            }
        }
    }
}
