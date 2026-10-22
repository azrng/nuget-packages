using System.Data;

namespace Azrng.SqlMigration.Service;

/// <summary>
/// 内部事务版本日志扩展，避免修改公共接口导致 breaking change。
/// </summary>
internal interface ITransactionalDbVersionService
{
    Task WriteVersionLogAsync(string migrationName, string version, IDbConnection connection, IDbTransaction? transaction);

    /// <summary>
    /// 幂等写入版本记录，已存在相同版本时不重复写入（用于基线版本落库）
    /// </summary>
    Task WriteVersionLogIfAbsentAsync(string migrationName, string version);
}
