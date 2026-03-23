using Azrng.Core.Model;
using System.Threading;

namespace Azrng.Database.DynamicSqlBuilder;

public class SqlBuilderOptions
{
    public DatabaseType Dialect { get; set; } = DatabaseType.PostgresSql;

    public bool EnableSqlLogging { get; set; }

    public bool EnableFieldNameValidation { get; set; } = true;

    public Action<string, object> OnSqlGenerated { get; set; }

    public static SqlBuilderOptions Default => new();

    public SqlBuilderOptions Clone()
    {
        return new SqlBuilderOptions
        {
            Dialect = Dialect,
            EnableSqlLogging = EnableSqlLogging,
            EnableFieldNameValidation = EnableFieldNameValidation,
            OnSqlGenerated = OnSqlGenerated
        };
    }
}

public static class SqlBuilderConfigurer
{
    private static readonly AsyncLocal<SqlBuilderOptions> CurrentOptions = new();

    public static SqlBuilderOptions GetCurrentOptions()
    {
        return CurrentOptions.Value ??= SqlBuilderOptions.Default;
    }

    public static void Configure(Action<SqlBuilderOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = GetCurrentOptions().Clone();
        configureOptions(options);
        CurrentOptions.Value = options;
    }

    public static void ResetToDefault()
    {
        CurrentOptions.Value = SqlBuilderOptions.Default;
    }
}
