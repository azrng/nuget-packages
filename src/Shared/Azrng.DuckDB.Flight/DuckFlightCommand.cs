using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Apache.Arrow;

namespace Azrng.DuckDB.Flight;

public sealed class DuckFlightCommand : DbCommand
{
    private readonly DuckFlightConnection _conn;
    private readonly FlightParameterCollection _params = new();

    public DuckFlightCommand(DuckFlightConnection conn)
    {
        _conn = conn;
    }

    [AllowNull]
    public override string CommandText { get; set; } = "";

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; } = CommandType.Text;

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; } = UpdateRowSource.None;

    protected override DbConnection? DbConnection { get => _conn; set => throw new NotSupportedException(); }

    protected override DbParameterCollection DbParameterCollection => _params;

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() { }

    public override int ExecuteNonQuery() => throw new NotSupportedException("Read-only wrapper: use ExecuteReader.");

    public override object ExecuteScalar()
    {
        using var r = ExecuteReader();
        if (!r.Read())
        {
            throw new InvalidOperationException("No rows.");
        }

        return r.GetValue(0);
    }

    public override void Prepare() { }

    protected override DbParameter CreateDbParameter() => new FlightParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return ExecuteDbDataReaderAsync(behavior, default).GetAwaiter().GetResult();
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        if (_conn.State != ConnectionState.Open)
        {
            await _conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        var commandText = CommandText;
        if (_params.Count > 0)
        {
            commandText = SubstituteParameters(commandText);
        }

        var info = await _conn.SqlClient
                              .ExecuteAsync(commandText, Apache.Arrow.Flight.Sql.Transaction.NoTransaction, _conn.CallOptions,
                                  cancellationToken)
                              .ConfigureAwait(false);
        var batches = new List<RecordBatch>();
        foreach (var endpoint in info.Endpoints)
        {
            var stream = _conn.SqlClient.DoGetAsync(endpoint.Ticket, _conn.CallOptions, cancellationToken);
            await foreach (var batch in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                batches.Add(batch);
            }
        }

        return new DuckFlightDataReader(batches);
    }

    private string SubstituteParameters(string commandText)
    {
        var result = commandText;
        foreach (DbParameter param in _params)
        {
            var paramName = param.ParameterName;
            if (!paramName.StartsWith('@'))
            {
                paramName = "@" + paramName;
            }

            var value = FormatParameterValue(param.Value);
            result = result.Replace(paramName, value);
        }

        return result;
    }

    private static string FormatParameterValue(object? value)
    {
        if (value is null || value == DBNull.Value)
        {
            return "NULL";
        }

        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'",
            bool b => b ? "1" : "0",
            byte[] or IEnumerable<byte> => "NULL",
            _ => value.ToString() ?? "NULL"
        };
    }
}
