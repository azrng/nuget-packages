using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using Npgsql;
using System.Text.Json;

namespace HttpTestApi.Storage;

/// <summary>
/// 基于 PostgreSQL 的日志存储实现
/// </summary>
public class PgSqlLogStore : LogStoreBase
{
    private readonly string _connectionString;
    private readonly ILogger<PgSqlLogStore> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private volatile bool _initialized;

    public PgSqlLogStore(IConfiguration configuration, ILogger<PgSqlLogStore> logger)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");
        _connectionString = string.IsNullOrEmpty(connectionString)
            ? "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=dev_log"
            : connectionString;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) return "(empty)";
        // 简单地隐藏密码部分
        var parts = connectionString.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = "Password=***";
            }
        }
        return string.Join(";", parts);
    }

    public override async ValueTask AddAsync(LogEntry? entry, CancellationToken cancellationToken = default)
    {
        if (entry == null)
        {
            return;
        }

        try
        {
            if (!await EnsureInitializedAsync(cancellationToken))
            {
                return;
            }

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dev_logs (
                    id, request_id, connection_id, timestamp, level, message,
                    request_path, request_method, response_status_code, elapsed_milliseconds,
                    source, exception, stack_trace, machine_name, application, app_version,
                    environment, process_id, thread_id, logger, action_id, action_name,
                    properties
                ) VALUES (
                    @Id, @RequestId, @ConnectionId, @Timestamp, @Level, @Message,
                    @RequestPath, @RequestMethod, @ResponseStatusCode, @ElapsedMilliseconds,
                    @Source, @Exception, @StackTrace, @MachineName, @Application, @AppVersion,
                    @Environment, @ProcessId, @ThreadId, @Logger, @ActionId, @ActionName,
                    @Properties::jsonb
                )";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", entry.Id ?? Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@RequestId", entry.RequestId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ConnectionId", entry.ConnectionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
            cmd.Parameters.AddWithValue("@Level", entry.Level.ToString());
            cmd.Parameters.AddWithValue("@Message", entry.Message ?? string.Empty);
            cmd.Parameters.AddWithValue("@RequestPath", entry.RequestPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@RequestMethod", entry.RequestMethod ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ResponseStatusCode", entry.ResponseStatusCode ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ElapsedMilliseconds", entry.ElapsedMilliseconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Source", entry.Source ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Exception", entry.Exception ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StackTrace", entry.StackTrace ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@MachineName", entry.MachineName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Application", entry.Application ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AppVersion", entry.AppVersion ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Environment", entry.Environment ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ProcessId", entry.ProcessId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ThreadId", entry.ThreadId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Logger", entry.Logger ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ActionId", entry.ActionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ActionName", entry.ActionName ?? (object)DBNull.Value);

            var propertiesJson = entry.Properties.Count > 0 ? JsonSerializer.Serialize(entry.Properties) : null;
            cmd.Parameters.AddWithValue("@Properties", propertiesJson ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            // Table missing; attempt init once more and retry.
            if (await EnsureInitializedAsync(cancellationToken))
            {
                try
                {
                    await using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync(cancellationToken);

                    const string sql = @"
                INSERT INTO dev_logs (
                    id, request_id, connection_id, timestamp, level, message,
                    request_path, request_method, response_status_code, elapsed_milliseconds,
                    source, exception, stack_trace, machine_name, application, app_version,
                    environment, process_id, thread_id, logger, action_id, action_name,
                    properties
                ) VALUES (
                    @Id, @RequestId, @ConnectionId, @Timestamp, @Level, @Message,
                    @RequestPath, @RequestMethod, @ResponseStatusCode, @ElapsedMilliseconds,
                    @Source, @Exception, @StackTrace, @MachineName, @Application, @AppVersion,
                    @Environment, @ProcessId, @ThreadId, @Logger, @ActionId, @ActionName,
                    @Properties::jsonb
                )";

                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Id", entry.Id ?? Guid.NewGuid().ToString());
                    cmd.Parameters.AddWithValue("@RequestId", entry.RequestId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConnectionId", entry.ConnectionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
                    cmd.Parameters.AddWithValue("@Level", entry.Level.ToString());
                    cmd.Parameters.AddWithValue("@Message", entry.Message ?? string.Empty);
                    cmd.Parameters.AddWithValue("@RequestPath", entry.RequestPath ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequestMethod", entry.RequestMethod ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ResponseStatusCode", entry.ResponseStatusCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ElapsedMilliseconds", entry.ElapsedMilliseconds ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Source", entry.Source ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Exception", entry.Exception ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StackTrace", entry.StackTrace ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MachineName", entry.MachineName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Application", entry.Application ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppVersion", entry.AppVersion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Environment", entry.Environment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProcessId", entry.ProcessId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ThreadId", entry.ThreadId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Logger", entry.Logger ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionId", entry.ActionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionName", entry.ActionName ?? (object)DBNull.Value);

                    var propertiesJson = entry.Properties.Count > 0 ? JsonSerializer.Serialize(entry.Properties) : null;
                    cmd.Parameters.AddWithValue("@Properties", propertiesJson ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (Exception retryEx)
                {
                    _logger.LogError(retryEx, "添加日志失败(重试)：{Message}", retryEx.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加日志失败：{Message}", ex.Message);
        }
    }

    public override async ValueTask AddBatchAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        if (entries == null)
        {
            return;
        }

        var entryList = entries.ToList();
        if (entryList.Count == 0)
        {
            return;
        }

        try
        {
            if (!await EnsureInitializedAsync(cancellationToken))
            {
                return;
            }

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            // 使用 NpgsqlBinaryImporter 进行批量导入，性能更好
            await using var importer = await conn.BeginBinaryImportAsync(
                "COPY dev_logs (" +
                "id, request_id, connection_id, timestamp, level, message, " +
                "request_path, request_method, response_status_code, elapsed_milliseconds, " +
                "source, exception, stack_trace, machine_name, application, app_version, " +
                "environment, process_id, thread_id, logger, action_id, action_name, " +
                "properties) FROM STDIN WITH (FORMAT BINARY)",
                cancellationToken);

            foreach (var entry in entryList)
            {
                if (entry == null) continue;

                importer.StartRow();

                importer.Write(entry.Id ?? Guid.NewGuid().ToString(), "text");
                importer.Write(entry.RequestId ?? (object)DBNull.Value, "text");
                importer.Write(entry.ConnectionId ?? (object)DBNull.Value, "text");
                importer.Write(entry.Timestamp, "timestamp");
                importer.Write(entry.Level.ToString(), "text");
                importer.Write(entry.Message ?? string.Empty, "text");
                importer.Write(entry.RequestPath ?? (object)DBNull.Value, "text");
                importer.Write(entry.RequestMethod ?? (object)DBNull.Value, "text");
                importer.Write(entry.ResponseStatusCode ?? (object)DBNull.Value, "integer");
                importer.Write(entry.ElapsedMilliseconds ?? (object)DBNull.Value, "bigint");
                importer.Write(entry.Source ?? (object)DBNull.Value, "text");
                importer.Write(entry.Exception ?? (object)DBNull.Value, "text");
                importer.Write(entry.StackTrace ?? (object)DBNull.Value, "text");
                importer.Write(entry.MachineName ?? (object)DBNull.Value, "text");
                importer.Write(entry.Application ?? (object)DBNull.Value, "text");
                importer.Write(entry.AppVersion ?? (object)DBNull.Value, "text");
                importer.Write(entry.Environment ?? (object)DBNull.Value, "text");
                importer.Write(entry.ProcessId ?? (object)DBNull.Value, "integer");
                importer.Write(entry.ThreadId ?? (object)DBNull.Value, "integer");
                importer.Write(entry.Logger ?? (object)DBNull.Value, "text");
                importer.Write(entry.ActionId ?? (object)DBNull.Value, "text");
                importer.Write(entry.ActionName ?? (object)DBNull.Value, "text");

                var propertiesJson = entry.Properties.Count > 0 ? JsonSerializer.Serialize(entry.Properties) : null;
                importer.Write(propertiesJson ?? (object)DBNull.Value, "jsonb");
            }

            await importer.CompleteAsync(cancellationToken);
            _logger.LogDebug("批量写入 {Count} 条日志成功", entryList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量添加日志失败：{Message}", ex.Message);

            // 如果批量导入失败，回退到逐条插入
            await FallbackToSingleInsertsAsync(entryList, cancellationToken);
        }
    }

    private async ValueTask FallbackToSingleInsertsAsync(List<LogEntry> entries, CancellationToken cancellationToken)
    {
        _logger.LogWarning("批量导入失败，回退到逐条插入模式");

        foreach (var entry in entries)
        {
            try
            {
                await AddAsync(entry, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回退模式插入单条日志失败");
            }
        }
    }

    public override async Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        if (!await EnsureInitializedAsync(cancellationToken))
        {
            return PageResult<LogEntry>.Create(new List<LogEntry>(), 0, query.PageIndex, query.PageSize);
        }

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = new System.Text.StringBuilder();
        sql.AppendLine("SELECT * FROM dev_logs WHERE 1=1");

        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrEmpty(query.Id))
        {
            conditions.Add("id = @Id");
            parameters.Add(new NpgsqlParameter("@Id", query.Id));
        }

        if (!string.IsNullOrEmpty(query.Keyword))
        {
            conditions.Add("(message ILIKE @Keyword OR request_path ILIKE @Keyword)");
            parameters.Add(new NpgsqlParameter("@Keyword", $"%{query.Keyword}%"));
        }

        if (query.MinLevel.HasValue)
        {
            conditions.Add("level >= @MinLevel");
            parameters.Add(new NpgsqlParameter("@MinLevel", query.MinLevel.Value.ToString()));
        }

        if (query.StartTime.HasValue)
        {
            conditions.Add("timestamp >= @StartTime");
            parameters.Add(new NpgsqlParameter("@StartTime", query.StartTime.Value));
        }

        if (query.EndTime.HasValue)
        {
            conditions.Add("timestamp <= @EndTime");
            parameters.Add(new NpgsqlParameter("@EndTime", query.EndTime.Value));
        }

        if (!string.IsNullOrEmpty(query.RequestId))
        {
            conditions.Add("request_id = @RequestId");
            parameters.Add(new NpgsqlParameter("@RequestId", query.RequestId));
        }

        if (!string.IsNullOrEmpty(query.Source))
        {
            conditions.Add("source = @Source");
            parameters.Add(new NpgsqlParameter("@Source", query.Source));
        }

        if (!string.IsNullOrEmpty(query.Application))
        {
            conditions.Add("application = @Application");
            parameters.Add(new NpgsqlParameter("@Application", query.Application));
        }

        if (conditions.Count > 0)
        {
            sql.AppendLine("AND ");
            sql.AppendLine(string.Join(" AND ", conditions));
        }

        // 获取总数
        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS subq";
        await using var countCmd = new NpgsqlCommand(countSql, conn);
        // 克隆参数以避免重复添加问题
        foreach (var param in parameters)
        {
            countCmd.Parameters.Add(new NpgsqlParameter(param.ParameterName, param.Value));
        }
        var totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));

        // 添加排序和分页
        sql.AppendLine("ORDER BY timestamp");
        if (query.OrderByTimeAscending)
        {
            sql.AppendLine("ASC");
        }
        else
        {
            sql.AppendLine("DESC");
        }

        sql.AppendLine("LIMIT @PageSize OFFSET @Skip");
        parameters.Add(new NpgsqlParameter("@PageSize", Math.Min(query.PageSize, 1000))); // 限制最大 1000
        parameters.Add(new NpgsqlParameter("@Skip", query.Skip));

        // 查询数据
        await using var dataCmd = new NpgsqlCommand(sql.ToString(), conn);
        // 克隆参数以避免重复添加问题
        foreach (var param in parameters)
        {
            dataCmd.Parameters.Add(new NpgsqlParameter(param.ParameterName, param.Value));
        }

        await using var reader = await dataCmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<LogEntry>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapToLogEntry(reader));
        }

        return PageResult<LogEntry>.Create(items, totalCount, query.PageIndex, query.PageSize);
    }

    public override async Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        if (!await EnsureInitializedAsync(cancellationToken))
        {
            return new List<LogEntry>();
        }

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = @"
            SELECT * FROM dev_logs
            WHERE request_id = @RequestId
            ORDER BY timestamp ASC";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RequestId", requestId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<LogEntry>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapToLogEntry(reader));
        }

        return items;
    }

    public override async Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime, CancellationToken cancellationToken = default)
    {
        if (!await EnsureInitializedAsync(cancellationToken))
        {
            return new List<TraceLogSummary>();
        }

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = new System.Text.StringBuilder();
        sql.AppendLine(@"
            SELECT
                request_id,
                COUNT(*) as log_count,
                MIN(timestamp) as first_timestamp,
                MAX(timestamp) as last_timestamp,
                MAX(request_path) as request_path,
                MAX(request_method) as request_method,
                MAX(response_status_code) as response_status_code,
                BOOL_OR(level IN ('Error', 'Critical')) as has_error
            FROM dev_logs
            WHERE request_id IS NOT NULL");

        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (startTime.HasValue)
        {
            conditions.Add("timestamp >= @StartTime");
            parameters.Add(new NpgsqlParameter("@StartTime", startTime.Value));
        }

        if (endTime.HasValue)
        {
            conditions.Add("timestamp <= @EndTime");
            parameters.Add(new NpgsqlParameter("@EndTime", endTime.Value));
        }

        if (conditions.Count > 0)
        {
            sql.AppendLine("AND ");
            sql.AppendLine(string.Join(" AND ", conditions));
        }

        sql.AppendLine(@"
            GROUP BY request_id
            ORDER BY last_timestamp DESC
            LIMIT 1000");

        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var summaries = new List<TraceLogSummary>();

        while (await reader.ReadAsync(cancellationToken))
        {
            summaries.Add(new TraceLogSummary
            {
                RequestId = reader.GetString(0),
                LogCount = reader.GetInt32(1),
                FirstTimestamp = reader.GetDateTime(2),
                LastTimestamp = reader.GetDateTime(3),
                RequestPath = reader.IsDBNull(4) ? null : reader.GetString(4),
                RequestMethod = reader.IsDBNull(5) ? null : reader.GetString(5),
                ResponseStatusCode = reader.IsDBNull(6) ? null : (int?)(reader.GetInt32(6)),
                HasError = reader.GetBoolean(7)
            });
        }

        return summaries;
    }

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始初始化 PostgreSQL 数据库...");
            _logger.LogInformation("正在创建数据库连接...");

            await using var conn = new NpgsqlConnection(_connectionString);

            _logger.LogInformation("正在打开数据库连接...");
            await conn.OpenAsync(cancellationToken);
            _logger.LogInformation("数据库连接成功");

            _logger.LogInformation("正在创建表和索引...");

            const string sql = @"
                CREATE TABLE IF NOT EXISTS dev_logs (
                    id VARCHAR(50) PRIMARY KEY,
                    request_id VARCHAR(50),
                    connection_id VARCHAR(100),
                    timestamp TIMESTAMP NOT NULL,
                    level VARCHAR(20) NOT NULL,
                    message TEXT,
                    request_path VARCHAR(500),
                    request_method VARCHAR(10),
                    response_status_code INTEGER,
                    elapsed_milliseconds BIGINT,
                    source VARCHAR(200),
                    exception TEXT,
                    stack_trace TEXT,
                    machine_name VARCHAR(200),
                    application VARCHAR(200),
                    app_version VARCHAR(50),
                    environment VARCHAR(50),
                    process_id INTEGER,
                    thread_id INTEGER,
                    logger VARCHAR(200),
                    action_id VARCHAR(100),
                    action_name VARCHAR(200),
                    properties JSONB,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                CREATE INDEX IF NOT EXISTS idx_dev_logs_timestamp ON dev_logs(timestamp DESC);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_request_id ON dev_logs(request_id);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_level ON dev_logs(level);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_application ON dev_logs(application);
                CREATE INDEX IF NOT EXISTS idx_dev_logs_source ON dev_logs(source);

                CREATE INDEX IF NOT EXISTS idx_dev_logs_properties_gin ON dev_logs USING gin(properties);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            _logger.LogInformation("正在执行 SQL 命令创建表结构...");
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("表结构创建成功");

            // 获取当前日志总数
            _logger.LogInformation("正在查询日志总数...");
            const string countSql = "SELECT COUNT(*) FROM dev_logs";
            await using var countCmd = new NpgsqlCommand(countSql, conn);
            var totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
            _logger.LogInformation("当前日志总数：{Count}", totalCount);

            _logger.LogInformation("PostgreSQL 日志表初始化成功，当前日志数：{Count}", totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化数据库失败：{Message}", ex.Message);
            throw;
        }
    }

    private async Task<bool> EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return true;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return true;
            }

            await InitializeAsync(cancellationToken);
            _initialized = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostgreSQL 日志表初始化失败：{Message}", ex.Message);
            return false;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static LogEntry MapToLogEntry(NpgsqlDataReader reader)
    {
        var entry = new LogEntry
        {
            Id = GetNonNullString(reader, "id"),
            RequestId = GetNullableString(reader, "request_id"),
            ConnectionId = GetNullableString(reader, "connection_id"),
            Timestamp = GetNonNullDateTime(reader, "timestamp"),
            Level = Enum.Parse<LogLevel>(GetNonNullString(reader, "level")),
            Message = GetNullableString(reader, "message") ?? string.Empty,
            RequestPath = GetNullableString(reader, "request_path"),
            RequestMethod = GetNullableString(reader, "request_method"),
            ResponseStatusCode = GetNullableInt(reader, "response_status_code"),
            ElapsedMilliseconds = GetNullableLong(reader, "elapsed_milliseconds"),
            Source = GetNullableString(reader, "source"),
            Exception = GetNullableString(reader, "exception"),
            StackTrace = GetNullableString(reader, "stack_trace"),
            MachineName = GetNullableString(reader, "machine_name"),
            Application = GetNullableString(reader, "application"),
            AppVersion = GetNullableString(reader, "app_version"),
            Environment = GetNullableString(reader, "environment"),
            ProcessId = GetNullableInt(reader, "process_id"),
            ThreadId = GetNullableInt(reader, "thread_id"),
            Logger = GetNullableString(reader, "logger"),
            ActionId = GetNullableString(reader, "action_id"),
            ActionName = GetNullableString(reader, "action_name")
        };

        // 解析 JSONB properties
        var propOrdinal = reader.GetOrdinal("properties");
        if (!reader.IsDBNull(propOrdinal))
        {
            using var jsonDoc = JsonDocument.Parse(reader.GetString(propOrdinal));
            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
            {
                entry.Properties[prop.Name] = prop.Value.ToString();
            }
        }

        return entry;
    }

    private static string GetNonNullString(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.GetString(ordinal);
    }

    private static DateTime GetNonNullDateTime(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.GetDateTime(ordinal);
    }

    private static string? GetNullableString(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? GetNullableInt(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : (int?)reader.GetInt32(ordinal);
    }

    private static long? GetNullableLong(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : (long?)reader.GetInt64(ordinal);
    }
}
