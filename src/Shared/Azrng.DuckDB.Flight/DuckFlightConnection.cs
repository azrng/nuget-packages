using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Apache.Arrow.Flight;
using Apache.Arrow.Flight.Client;
using Apache.Arrow.Flight.Sql;
using Apache.Arrow.Flight.Sql.Client;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;

namespace Azrng.DuckDB.Flight;

public sealed class DuckFlightConnection : DbConnection
{
    private const string AuthorizationHeaderName = "authorization";
    private const string BasicPrefix = "Basic ";
    private const string BearerPrefix = "Bearer ";

    private readonly Dictionary<string, string> _props = new(StringComparer.OrdinalIgnoreCase);
    private string _connectionString = string.Empty;
    private ConnectionState _state = ConnectionState.Closed;
    private GrpcChannel? _channel;
    private FlightSqlClient? _sqlClient;
    private FlightCallOptions? _callOptions;

    public DuckFlightConnection() { }

    public DuckFlightConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public FlightSqlClient SqlClient => _sqlClient ?? throw new InvalidOperationException("Connection is not open.");

    public FlightCallOptions CallOptions => _callOptions ?? throw new InvalidOperationException("Connection is not open.");

    [AllowNull]
    public override string ConnectionString
    {
        get => _connectionString;
        set
        {
            _connectionString = value ?? string.Empty;
            _props.Clear();
            foreach (var part in _connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2)
                {
                    _props[kv[0].Trim()] = kv[1].Trim();
                }
            }
        }
    }

    public override string Database => _props.TryGetValue("catalog", out var catalog) ? catalog : "";

    public override string DataSource => _props.TryGetValue("uri", out var uri) ? uri : "";

    public override string ServerVersion => "duckflight";

    public override ConnectionState State => _state;

    public override int ConnectionTimeout => 0;

    public override void Open()
    {
        OpenAsyncCore(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        return OpenAsyncCore(cancellationToken);
    }

    private async Task OpenAsyncCore(CancellationToken cancellationToken)
    {
        if (_state == ConnectionState.Open)
        {
            return;
        }

        var uri = Require("uri");
        var user = Require("username");
        var pass = _props.TryGetValue("password", out var p) ? p : "";
        var catalog = _props.TryGetValue("catalog", out var c) ? c : "";

        try
        {
            var address = NormalizeAddress(uri);
            _channel = GrpcChannel.ForAddress(address);
            var flightClient = new FlightClient(_channel);
            var sqlClient = new FlightSqlClient(flightClient);
            var authHeaders = await CreateAuthorizationHeaders(flightClient, user, pass, cancellationToken).ConfigureAwait(false);
            _sqlClient = sqlClient;
            _callOptions = new FlightCallOptions { Headers = authHeaders };

            if (!string.IsNullOrWhiteSpace(catalog))
            {
                await UseCatalogAsync(catalog, cancellationToken).ConfigureAwait(false);
            }

            _state = ConnectionState.Open;
        }
        catch
        {
            Close();
            throw;
        }
    }

    public override void Close()
    {
        _sqlClient = null;
        _callOptions = null;
        _channel?.Dispose();
        _channel = null;
        _state = ConnectionState.Closed;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException("Transactions are not supported.");

    public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() => new DuckFlightCommand(this);

    private async Task UseCatalogAsync(string catalog, CancellationToken cancellationToken)
    {
        var sql = $"USE {QuoteIdentifier(catalog)}";
        await SqlClient.ExecuteUpdateAsync(sql, Apache.Arrow.Flight.Sql.Transaction.NoTransaction, CallOptions, cancellationToken)
                       .ConfigureAwait(false);
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new InvalidOperationException("Connection string key Catalog cannot be empty.");
        }

        return "\"" + identifier.Trim().Replace("\"", "\"\"") + "\"";
    }

    private string Require(string key)
    {
        if (!_props.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
        {
            throw new InvalidOperationException($"Missing DSN key: {key}");
        }

        return v;
    }

    private static string NormalizeAddress(string uri)
    {
        if (uri.StartsWith("grpc://", StringComparison.OrdinalIgnoreCase))
        {
            return "http://" + uri.Substring("grpc://".Length);
        }

        if (uri.StartsWith("grpcs://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + uri.Substring("grpcs://".Length);
        }

        return uri;
    }

    private static async Task<Metadata> CreateAuthorizationHeaders(
        FlightClient flightClient,
        string user,
        string pass,
        CancellationToken cancellationToken)
    {
        var basicHeader = CreateBasicAuthorizationValue(user, pass);
        string? token = null;
        try
        {
            token = await HandshakeForBearerToken(flightClient, user, pass, basicHeader, cancellationToken).ConfigureAwait(false);
        }
        catch (RpcException ex) when (!cancellationToken.IsCancellationRequested &&
                                      (ex.StatusCode == StatusCode.Unimplemented || ex.StatusCode == StatusCode.Cancelled))
        {
            token = null;
        }

        return new Metadata { { AuthorizationHeaderName, string.IsNullOrWhiteSpace(token) ? basicHeader : $"{BearerPrefix}{token}" } };
    }

    private static async Task<string?> HandshakeForBearerToken(
        FlightClient flightClient,
        string user,
        string pass,
        string basicHeader,
        CancellationToken cancellationToken)
    {
        var headers = new Metadata { { AuthorizationHeaderName, basicHeader } };

        using var call = flightClient.Handshake(headers, null, cancellationToken);
        await call.RequestStream.WriteAsync(new FlightHandshakeRequest(CreateBasicAuthPayload(user, pass), 0)).ConfigureAwait(false);
        await call.RequestStream.CompleteAsync().ConfigureAwait(false);

        // GizmoSQL 在握手后通过「响应头」下发 Bearer 令牌（响应流 payload 与 trailers 均为空），
        // 因此必须先读 ResponseHeadersAsync，否则后续 SQL 调用会被服务器以
        // "No session ID in request context" 拒绝。token 解析优先级：响应头 → 响应流 → trailers。
        var responseHeaders = await call.ResponseHeadersAsync.ConfigureAwait(false);
        var authHeader = responseHeaders.GetValue(AuthorizationHeaderName);

        string? tokenFromPayload = null;
        while (await call.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
        {
            var payload = call.ResponseStream.Current.Payload;
            if (!payload.IsEmpty)
            {
                tokenFromPayload = payload.ToStringUtf8();
            }
        }

        var trailerAuth = call.GetTrailers().GetValue(AuthorizationHeaderName);
        var token = ExtractBearerToken(authHeader)
                    ?? ExtractBearerToken(tokenFromPayload)
                    ?? ExtractBearerToken(trailerAuth)
                    ?? tokenFromPayload;
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return token.Trim();
    }

    private static string? ExtractBearerToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? value[BearerPrefix.Length..].Trim()
            : null;
    }

    private static string CreateBasicAuthorizationValue(string user, string pass)
    {
        var basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user}:{pass}"));
        return $"{BasicPrefix}{basic}";
    }

    private static ByteString CreateBasicAuthPayload(string user, string pass)
    {
        using var stream = new MemoryStream();
        WriteStringField(stream, 1, user);
        WriteStringField(stream, 2, pass);
        return ByteString.CopyFrom(stream.ToArray());
    }

    private static void WriteStringField(Stream stream, int fieldNumber, string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        WriteVarint(stream, (ulong)((fieldNumber << 3) | 2));
        WriteVarint(stream, (ulong)bytes.Length);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteVarint(Stream stream, ulong value)
    {
        while (value > 0x7F)
        {
            stream.WriteByte((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }

        stream.WriteByte((byte)value);
    }
}
