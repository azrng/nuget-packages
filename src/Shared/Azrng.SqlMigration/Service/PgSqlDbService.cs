using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace Azrng.SqlMigration.Service
{
    public class PgSqlDbVersionService : IDbVersionService, ITransactionalDbVersionService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PgSqlDbVersionService> _logger;

        public PgSqlDbVersionService(IServiceProvider serviceProvider,
            ILogger<PgSqlDbVersionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<string> GetCurrentVersionAsync(string migrationName)
        {
            var conn = _serviceProvider.GetRequiredKeyedService<IDbConnection>(migrationName);
            var config = GetMigrationOption(migrationName);
            var versionLog = GetVersionLogInfo(config);

            try
            {
                const string querySql = @"SELECT count(1)
                                          FROM pg_class a
                                          WHERE a.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = @schema)
                                            AND a.relkind = 'r'
                                            AND a.relname = @tableName;";

                if (await conn.ExecuteScalarAsync<int>(querySql, new { schema = config.Schema, tableName = versionLog.TableName }) == 0)
                {
                    return SqlMigrationConst.FirstVersionStr;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MigrationName} SQL迁移脚本-查询版本表是否存在异常", migrationName);
                throw;
            }

            var sql =
                $"select {versionLog.QuotedVersionColumn} from {versionLog.QualifiedTableName} order by {versionLog.QuotedOrderByColumn} desc limit 1";
            var ret = await conn.ExecuteScalarAsync(sql);
            return ret?.ToString() ?? SqlMigrationConst.FirstVersionStr;
        }

        public Task WriteVersionLogAsync(string migrationName, string version)
        {
            var conn = _serviceProvider.GetRequiredKeyedService<IDbConnection>(migrationName);
            return WriteVersionLogAsync(migrationName, version, conn, transaction: null);
        }

        public async Task WriteVersionLogAsync(string migrationName, string version, IDbConnection connection,
            IDbTransaction? transaction)
        {
            var config = GetMigrationOption(migrationName);
            var versionLog = GetVersionLogInfo(config);
            var initSql = GetInitTableSql(config, versionLog);

            if (!string.IsNullOrWhiteSpace(initSql))
            {
                await connection.ExecuteAsync(initSql, transaction: transaction);
            }

            if (await HasLegacyCreatedTimeColumnAsync(connection, config, versionLog, transaction))
            {
                var legacySql =
                    $"insert into {versionLog.QualifiedTableName}({versionLog.QuotedVersionColumn}, \"created_time\") values(@version,@time);";
                var legacyParam = new
                {
                    version,
                    time = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddHours(8)
                };
                await connection.ExecuteAsync(legacySql, legacyParam, transaction);
                return;
            }

            var writeSql = $"insert into {versionLog.QualifiedTableName}({versionLog.QuotedVersionColumn}) values(@version);";
            await connection.ExecuteAsync(writeSql, new { version }, transaction);
        }

        private SqlMigrationOption GetMigrationOption(string migrationName)
        {
            var optionsSnapshot = _serviceProvider.GetRequiredService<IOptionsSnapshot<SqlMigrationOption>>();
            return optionsSnapshot.Get(migrationName);
        }

        private static VersionLogInfo GetVersionLogInfo(SqlMigrationOption config)
        {
            var versionLog = config.VersionLog;
            ValidateVersionLogOption(versionLog);

            return new VersionLogInfo(
                versionLog.TableName,
                $"{QuoteIdentifier(config.Schema)}.{QuoteIdentifier(versionLog.TableName)}",
                QuoteIdentifier(versionLog.IdColumn),
                QuoteIdentifier(versionLog.VersionColumn),
                QuoteIdentifier(versionLog.OrderByColumn));
        }

        private static void ValidateVersionLogOption(SqlVersionLogOption versionLog)
        {
            EnsureNotWhiteSpace(versionLog.TableName, nameof(versionLog.TableName));
            EnsureNotWhiteSpace(versionLog.IdColumn, nameof(versionLog.IdColumn));
            EnsureNotWhiteSpace(versionLog.VersionColumn, nameof(versionLog.VersionColumn));
            EnsureNotWhiteSpace(versionLog.OrderByColumn, nameof(versionLog.OrderByColumn));

            if (string.IsNullOrWhiteSpace(versionLog.InitTableSql) &&
                !versionLog.OrderByColumn.Equals(versionLog.IdColumn, StringComparison.OrdinalIgnoreCase) &&
                !versionLog.OrderByColumn.Equals(versionLog.VersionColumn, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"当 {nameof(SqlVersionLogOption.OrderByColumn)} 与 {nameof(SqlVersionLogOption.IdColumn)} 或 {nameof(SqlVersionLogOption.VersionColumn)} 不一致时，请通过 {nameof(SqlVersionLogOption.InitTableSql)} 自定义建表 SQL。");
            }
        }

        private static void EnsureNotWhiteSpace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("配置不能为空", paramName);
            }
        }

        private static string GetInitTableSql(SqlMigrationOption config, VersionLogInfo versionLog)
        {
            if (!string.IsNullOrWhiteSpace(config.VersionLog.InitTableSql))
            {
                return config.VersionLog.InitTableSql!;
            }

            var pkName = QuoteIdentifier($"pk_{config.VersionLog.TableName}");
            var versionIndexName = QuoteIdentifier($"ix_{config.VersionLog.TableName}_{config.VersionLog.VersionColumn}");

            return @$"CREATE TABLE if not exists {versionLog.QualifiedTableName} (
                                    {versionLog.QuotedIdColumn} bigint GENERATED BY DEFAULT AS IDENTITY,
                                    {versionLog.QuotedVersionColumn} text NOT NULL,
                                    CONSTRAINT {pkName} PRIMARY KEY ({versionLog.QuotedIdColumn})
                                );
                                CREATE UNIQUE INDEX if not exists {versionIndexName} ON {versionLog.QualifiedTableName} ({versionLog.QuotedVersionColumn});";
        }

        private static string QuoteIdentifier(string identifier)
        {
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }

        private static async Task<bool> HasLegacyCreatedTimeColumnAsync(IDbConnection connection, SqlMigrationOption config,
            VersionLogInfo versionLog, IDbTransaction? transaction)
        {
            const string querySql = @"SELECT count(1)
                                      FROM information_schema.columns
                                      WHERE table_schema = @schema
                                        AND table_name = @tableName
                                        AND column_name = 'created_time';";

            return await connection.ExecuteScalarAsync<int>(querySql,
                new { schema = config.Schema, tableName = versionLog.TableName }, transaction) > 0;
        }

        private sealed record VersionLogInfo(
            string TableName,
            string QualifiedTableName,
            string QuotedIdColumn,
            string QuotedVersionColumn,
            string QuotedOrderByColumn);
    }
}
