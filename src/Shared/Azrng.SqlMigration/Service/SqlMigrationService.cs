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
        private readonly SqlMigrationOption _config;
        private readonly IServiceProvider _serviceProvider;

        public SqlMigrationService(ILogger<SqlMigrationService> logger, IOptionsSnapshot<SqlMigrationOption> option,
                                   IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = option.Get(SqlMigrationServiceExtension.CurrDbName);
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> MigrateAsync(string migrationName)
        {
            try
            {
                // 获取组件创建的表中当前程序的版本
                var dbVersionService = _serviceProvider.GetRequiredKeyedService<IDbVersionService>(migrationName);
                var version = await dbVersionService.GetCurrentVersionAsync(migrationName);

                // 只有在第一个版本的时候走自定义获取版本
                if (version == SqlMigrationConst.FirstVersionStr && _config.InitVersionSetterType != null)
                {
                    // 当获取不到版本的时候，调用初始化版本工人方法获取当前版本
                    var sertter = _serviceProvider.GetRequiredKeyedService<IInitVersionSetter>(migrationName);
                    version = await sertter.GetCurrentVersionAsync();
                }

                await CallbackAsync(migrationName, SqlMigrationStep.Prepare, version, string.Empty);

                await VersionUpAsync(migrationName, version);

                await CallbackAsync(migrationName, SqlMigrationStep.Success, version, string.Empty);
            }
            catch (Exception e)
            {
                await CallbackAsync(migrationName, SqlMigrationStep.Failed, string.Empty, string.Empty);
                _logger.LogError(e, $"异常：{e.Message}");
            }

            return true;
        }

        /// <summary>
        /// 版本升级
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="oldVersion">原来的版本</param>
        /// <returns></returns>
        private async Task VersionUpAsync(string migrationName, string oldVersion)
        {
            _logger.LogInformation($"{migrationName} SQL迁移脚本执行开始----当前版本：{oldVersion}");

            if (!Directory.Exists(_config.SqlRootPath))
            {
                _logger.LogInformation($"{migrationName} SQL迁移脚本执行结束----未找到目录：{_config.SqlRootPath}");
                return;
            }

            var oldVersionNum = GetVersionNum(migrationName, oldVersion);

            //获取需要升级的所有脚本
            var fileVersionList = new DirectoryInfo(_config.SqlRootPath)
                                  .GetFileSystemInfos()
                                  .Where(x => x.Name.StartsWith(_config.VersionPrefix) &&
                                              (x.Name.EndsWith(".sql") || x.Name.EndsWith(".txt")))
                                  .Select(x => new
                                               {
                                                   VersionNum = GetVersionNum(migrationName, x.Name),
                                                   Version = x.Name.ReplaceIfNotNullOrWhiteSpace(_config.VersionPrefix, string.Empty)
                                                              .Replace(".sql", string.Empty)
                                                              .Replace(".txt", string.Empty),
                                                   FilePath = x.FullName
                                               })
                                  .Where(x => x.VersionNum > oldVersionNum)
                                  .OrderBy(x => x.VersionNum)
                                  .ToList();

            var latestVersion = oldVersion;

            //遍历执行脚本 每个版本的数据在一个事务里
            try
            {
                foreach (var file in fileVersionList)
                {
                    latestVersion = file.Version;
                    await ExecuteFileAsync(migrationName, file.FilePath, oldVersion, file.Version);
                    _logger.LogInformation($"迁移名：{migrationName} 版本：{latestVersion}迁移脚本执行成功");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"迁移名：{migrationName} 版本：{latestVersion}迁移脚本执行失败");
                throw;
            }

            _logger.LogInformation($"{migrationName} SQL迁移脚本执行结束----当前版本：{latestVersion}");
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
            if (!await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdatePrepare, oldVersion, version)) return;

            var conn = _serviceProvider.GetRequiredKeyedService<IDbConnection>(migrationName);
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            using var uow = conn.BeginTransaction();
            try
            {
                var dbVersionService = _serviceProvider.GetRequiredKeyedService<IDbVersionService>(migrationName);
                await ExecuteSqlFromFile(migrationName, filePath);
                await dbVersionService.WriteVersionLogAsync(migrationName, version);
                uow.Commit();
                await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdateSuccess, oldVersion, version);
            }
            catch (Exception)
            {
                uow.Rollback();
                await CallbackAsync(migrationName, SqlMigrationStep.VersionUpdateFailed, oldVersion, version);
                throw;
            }
        }

        /// <summary>
        /// 执行脚本文件
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="filePath"></param>
        private async Task ExecuteSqlFromFile(string migrationName, string filePath)
        {
            var fileText = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(fileText)) return;

            var conn = _serviceProvider.GetRequiredKeyedService<IDbConnection>(migrationName);
            await conn.ExecuteAsync(fileText);
        }

        /// <summary>
        /// 版本号转数字
        /// </summary>
        /// <param name="migrationName">迁移名称</param>
        /// <param name="versionNumber">版本</param>
        /// <returns></returns>
        private long GetVersionNum(string migrationName, string versionNumber)
        {
            versionNumber = versionNumber.ToLower()
                                         .ReplaceIfNotNullOrWhiteSpace(_config.VersionPrefix, string.Empty)
                                         .Replace(".sql", string.Empty)
                                         .Replace(".txt", string.Empty);
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
                    $"{migrationName} SQL迁移脚本-获取版本号异常-存在非法的版本信息,versionNumber:{versionNumber},message:{ex.Message}");
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
            if (migrationHandler == null) return true;

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
                        await migrationHandler.VersionUpdateBeforeMigrateAsync(version);
                        break;
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
                _logger.LogError(ex, $"{migrationName} SQL迁移脚本回调异常,Step:{step},Version:{version}");
                if (step == SqlMigrationStep.Prepare) throw;

                return false;
            }
        }
    }
}