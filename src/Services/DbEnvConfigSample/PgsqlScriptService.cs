using Azrng.AspNetCore.DbEnvConfig;

namespace DbEnvConfigSample;

public class PgsqlScriptService : DefaultScriptService
{
    public override string GetInitTableDataScript()
    {
        var sql =
            $"INSERT INTO config.system_config (id, code, value, is_delete) VALUES (default, '{Guid.NewGuid()}', '{Guid.NewGuid()}', false);";
        return sql;
    }
}