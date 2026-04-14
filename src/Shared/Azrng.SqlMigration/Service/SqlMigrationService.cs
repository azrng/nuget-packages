using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace Azrng.SqlMigration.Service
{
    /// <summary>
    /// 初始化应用服务
    /// </summary>
    internal class SqlMigrationService : ISqlMigrationService
    {
        private readonly ILogger<SqlMigrationService> _logger;
        private readonly IOptionsSnapshot<SqlMigrationOption> _options;
        private readonly IServiceProvider _serviceProvider;

        public SqlMigrationService(ILogger<SqlMigrationService> logger,
            IOptionsSnapshot<SqlMigrationOption> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> MigrateAsync(string migrationName)
        {
            var dbVersionService = _serviceProvider.GetRequiredKeyedService<IDbVersionService>(migrationName);
            var config = _options.Get(migrationName);
            var oldVersion = SqlMigrationConst.FirstVersionStr;
            var latestVersion = SqlMigrationConst.FirstVersionStr;

            try
            {
                oldVersion = await dbVersionService.GetCurrentVersionAsync(migrationName);
                latestVersion = oldVersion;

                // 只有在第一个版本的时候走自定义获取版本
                if (oldVersion == SqlMigrationConst.FirstVersionStr && config.InitVersionSetterType != null)
                {
                    // 当获取不到版本的时候，调用初始化版本工人方法获取当前版本
                    var setter = _serviceProvider.GetRequiredKeyedService<IInitVersionSetter>(migrationName);
                    oldVersion = await setter.GetCurrentVersionAsync();
                    latestVersion = oldVersion;
                }

                if (!await CallbackAsync(migrationName, SqlMigrationStep.Prepare, oldVersion, oldVersion))
                {
                    _logger.LogInformation("{MigrationName} SQL迁移脚本被准备回调跳过", migrationName);
                    return false;
                }

                latestVersion = await VersionUpAsync(migrationName, oldVersion, config);

                await CallbackAsync(migrationName, SqlMigrationStep.Success, oldVersion, latestVersion);
                return true;
            }
            catch (Exception ex)
            {
                await CallbackAsync(migrationName, SqlMigrationStep.Failed, oldVersion, latestVersion);
                _logger.LogError(ex, "{MigrationName} SQL迁移脚本执行失败，原版本：{OldVersion}，当前版本：{LatestVersion}",
                    migrationName, oldVersion, latestVersion);
                throw;
            }
        }

        /// <summary>
        /// 版本升级
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="oldVersion">原来的版本</param>
        /// <param name="config">迁移配置</param>
        /// <returns></returns>
        private async Task<string> VersionUpAsync(string migrationName, string oldVersion, SqlMigrationOption config)
        {
            _logger.LogInformation("{MigrationName} SQL迁移脚本执行开始，当前版本：{OldVersion}", migrationName, oldVersion);

            if (!Directory.Exists(config.SqlRootPath))
            {
                _logger.LogInformation("{MigrationName} SQL迁移脚本执行结束，未找到目录：{SqlRootPath}", migrationName,
                    config.SqlRootPath);
                return oldVersion;
            }

            var oldVersionNum = GetVersionNum(migrationName, oldVersion, config.VersionPrefix);

            // 获取需要升级的所有脚本
            var fileVersionList = new DirectoryInfo(config.SqlRootPath)
                                  .GetFileSystemInfos()
                                  .Where(x => x.Name.StartsWith(config.VersionPrefix) &&
                                              (x.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) ||
                                               x.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)))
                                  .Select(x => new
                                               {
                                                   VersionNum = GetVersionNum(migrationName, x.Name, config.VersionPrefix),
                                                   Version = x.Name.ReplaceIfNotNullOrWhiteSpace(config.VersionPrefix, string.Empty)
                                                              .Replace(".sql", string.Empty, StringComparison.OrdinalIgnoreCase)
                                                              .Replace(".txt", string.Empty, StringComparison.OrdinalIgnoreCase),
                                                   FilePath = x.FullName
                                               })
                                  .Where(x => x.VersionNum > oldVersionNum)
                                  .OrderBy(x => x.VersionNum)
                                  .ToList();

            var latestVersion = oldVersion;

            try
            {
                foreach (var file in fileVersionList)
                {
                    await ExecuteFileAsync(migrationName, file.FilePath, oldVersion, file.Version);
                    latestVersion = file.Version;
                    _logger.LogInformation("迁移名：{MigrationName} 版本：{Version}迁移脚本执行成功", migrationName, latestVersion);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "迁移名：{MigrationName} 版本：{Version}迁移脚本执行失败", migrationName, latestVersion);
                throw;
            }

            _logger.LogInformation("{MigrationName} SQL迁移脚本执行结束，当前版本：{LatestVersion}", migrationName, latestVersion);
            return latestVersion;
        }

        /// <summary>
        /// 执行脚本文件
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="filePath"></param>
        /// <param name="oldVersion"></param>
        /// <param name="version"></param>
        private async Task ExecuteFileAsync(string migrationName, string filePath, string oldVersion, string version)
        {
            if (!await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdatePrepare, oldVersion, version))
            {
                _logger.LogInformation("迁移名：{MigrationName} 版本：{Version}被版本准备回调跳过", migrationName, version);
                return;
            }

            var conn = _serviceProvider.GetRequiredKeyedService<IDbConnection>(migrationName);
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            using var transaction = conn.BeginTransaction();
            try
            {
                var dbVersionService = _serviceProvider.GetRequiredKeyedService<IDbVersionService>(migrationName);
                await ExecuteSqlFromFile(filePath, conn, transaction);
                if (dbVersionService is ITransactionalDbVersionService transactionalDbVersionService)
                {
                    await transactionalDbVersionService.WriteVersionLogAsync(migrationName, version, conn, transaction);
                }
                else
                {
                    await dbVersionService.WriteVersionLogAsync(migrationName, version);
                }
                transaction.Commit();
                await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdateSuccess, oldVersion, version);
            }
            catch (Exception)
            {
                transaction.Rollback();
                await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdateFailed, oldVersion, version);
                throw;
            }
        }

        /// <summary>
        /// 执行脚本文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        private static async Task ExecuteSqlFromFile(string filePath, IDbConnection connection, IDbTransaction transaction)
        {
            var fileText = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(fileText))
            {
                return;
            }

            await connection.ExecuteAsync(fileText, transaction: transaction);
        }

        /// <summary>
        /// 版本号转数字
        /// </summary>
        /// <param name="migrationName">迁移名称</param>
        /// <param name="versionNumber">版本</param>
        /// <param name="versionPrefix">版本前缀</param>
        /// <returns></returns>
        private long GetVersionNum(string migrationName, string versionNumber, string versionPrefix)
        {
            versionNumber = versionNumber.ToLowerInvariant()
                                         .ReplaceIfNotNullOrWhiteSpace(versionPrefix, string.Empty)
                                         .Replace(".sql", string.Empty, StringComparison.OrdinalIgnoreCase)
                                         .Replace(".txt", string.Empty, StringComparison.OrdinalIgnoreCase);
            var arr = versionNumber.Split('.').ToList();

            try
            {
                var str = arr.Count switch
                {
                    4 =>
                        $"{Convert.ToInt32(arr[0]):000}{Convert.ToInt32(arr[1]):000}{Convert.ToInt32(arr[2]):000}{Convert.ToInt32(arr[3]):000}",
                    3 => $"{Convert.ToInt32(arr[0]):000}{Convert.ToInt32(arr[1]):000}{Convert.ToInt32(arr[2]):000}",
                    _ => throw new NotSupportedException($"不支持的版本格式：{versionNumber}")
                };

                return Convert.ToInt64(str);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{MigrationName} SQL迁移脚本-获取版本号异常-存在非法的版本信息,versionNumber:{VersionNumber},message:{Message}",
                    migrationName, versionNumber, ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 迁移步骤通知
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="step"></param>
        /// <param name="oldVersion">原始版本</param>
        /// <param name="version">新版本</param>
        /// <returns></returns>
        private async Task<bool> CallbackAsync(string migrationName, SqlMigrationStep step, string oldVersion, string version)
        {
            var migrationHandler = _serviceProvider.GetKeyedService<IMigrationHandler>(migrationName);
            if (migrationHandler == null)
            {
                return true;
            }

            try
            {
                switch (step)
                {
                    case SqlMigrationStep.Prepare:
                        return await migrationHandler.BeforeMigrateAsync(oldVersion);
                    case SqlMigrationStep.Success:
                        await migrationHandler.MigratedAsync(oldVersion, version);
                        break;
                    case SqlMigrationStep.Failed:
                        await migrationHandler.MigrateFailedAsync(oldVersion, version);
                        break;
                    case SqlMigrationStep.VersionUpdatePrepare:
                        return await migrationHandler.VersionUpdateBeforeMigrateAsync(version);
                    case SqlMigrationStep.VersionUpdateSuccess:
                        await migrationHandler.VersionUpdateMigratedAsync(version);
                        break;
                    case SqlMigrationStep.VersionUpdateFailed:
                        await migrationHandler.VersionUpdateMigrateFailedAsync(version);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(step), step, null);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MigrationName} SQL迁移脚本回调异常,Step:{Step},Version:{Version}", migrationName, step,
                    version);
                if (step == SqlMigrationStep.Prepare || step == SqlMigrationStep.VersionUpdatePrepare)
                {
                    throw;
                }

                return false;
            }
        }
    }
}
