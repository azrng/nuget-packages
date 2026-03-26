using System;
using System.Text.RegularExpressions;

namespace Azrng.DataAccess.Validation;

internal static class SqlPaginationValidator
{
    private static readonly Regex IdentifierPattern =
        new(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)?$", RegexOptions.Compiled);

    internal static void ValidatePageArguments(int pageIndex, int pageSize)
    {
        if (pageIndex <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), "pageIndex must be greater than 0.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be greater than 0.");
        }
    }

    internal static string? NormalizeOrderColumn(string? orderColumn)
    {
        if (string.IsNullOrWhiteSpace(orderColumn))
        {
            return null;
        }

        var normalized = orderColumn.Trim();
        if (!IdentifierPattern.IsMatch(normalized))
        {
            throw new ArgumentException($"Invalid order column: {orderColumn}", nameof(orderColumn));
        }

        return normalized;
    }

    internal static string? NormalizeOrderDirection(string? orderDirection)
    {
        if (string.IsNullOrWhiteSpace(orderDirection))
        {
            return null;
        }

        var normalized = orderDirection.Trim().ToUpperInvariant();
        return normalized switch
        {
            "ASC" => "ASC",
            "DESC" => "DESC",
            _ => throw new ArgumentException($"Invalid order direction: {orderDirection}", nameof(orderDirection))
        };
    }
}
