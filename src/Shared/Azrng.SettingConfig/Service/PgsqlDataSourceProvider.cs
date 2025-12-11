using Azrng.Core.Extension;
using Azrng.SettingConfig.Dto;
using Azrng.SettingConfig.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Azrng.SettingConfig.Service;

/// <summary>
/// pgsql存储提供者
/// </summary>
internal class PgsqlDataSourceProvider : IDataSourceProvider
{
    private readonly DashboardOptions _options;
    private readonly IDapperRepository _dapperRepository;
    private readonly ILogger<PgsqlDataSourceProvider> _logger;

    public PgsqlDataSourceProvider(IDapperRepository dapperRepository, ILogger<PgsqlDataSourceProvider> logger,
        IOptions<DashboardOptions> options)
    {
        _dapperRepository = dapperRepository;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<bool> InitAsync()
    {
        var checkExist = $@"SELECT count(1)
FROM pg_class a
LEFT OUTER JOIN pg_description b ON b.objsubid = 0 AND a.oid = b.objoid
WHERE a.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = '{_options.DbSchema}')
AND a.relkind = 'r' and a.relname='system_config';";

        _logger.LogInformation($"queryDb  检查表是否存在SQL：{checkExist}");
        var num = await _dapperRepository.ExecuteScalarAsync<int>(checkExist);
        if (num > 0)
            return true;

        var sb = new StringBuilder($"CREATE SCHEMA IF NOT EXISTS {_options.DbSchema};");
        // 创建配置表
        sb.Append($@"create table if not exists {_options.DbSchema}.system_config
(
    id             serial         not null
            primary key,
    key            varchar(100)    not null,
    name           varchar(50)    not null,
    value          text            not null,
    description    text default '' not null,
    version        varchar(50)     not null,
    create_user_id varchar(50) default ''   not null,
    create_time    timestamp       not null,
    update_user_id varchar(50) default ''   not null,
    update_time    timestamp       not null,
    is_deleted     bool not null
);
comment on table {_options.DbSchema}.system_config is '系统配置表';
comment on column {_options.DbSchema}.system_config.id is '标识列';
comment on column {_options.DbSchema}.system_config.key is '配置key';
comment on column {_options.DbSchema}.system_config.name is '配置名';
comment on column {_options.DbSchema}.system_config.value is '配置值';
comment on column {_options.DbSchema}.system_config.description is '描述信息';
comment on column {_options.DbSchema}.system_config.version is '版本标识';
comment on column {_options.DbSchema}.system_config.create_user_id is '创建人ID';
comment on column {_options.DbSchema}.system_config.create_time is '创建时间';
comment on column {_options.DbSchema}.system_config.update_user_id is '更新人id';
comment on column {_options.DbSchema}.system_config.update_time is '更新时间';
comment on column {_options.DbSchema}.system_config.is_deleted is '是否删除';
create unique index system_config_key_uindex
    on {_options.DbSchema}.system_config (key);");
        // 创建配置历史表
        sb.Append($@"create table if not exists {_options.DbSchema}.system_config_history
(
    id             serial         not null
            primary key,
    key            varchar(100)    not null,
    value          text            not null,
    version        varchar(50)     not null,
    update_user_id varchar(50) default ''   not null,
    update_time    timestamp       not null
);

comment on table {_options.DbSchema}.system_config_history is '系统配置版本表';
comment on column {_options.DbSchema}.system_config_history.id is '标识列';
comment on column {_options.DbSchema}.system_config_history.key is '配置key';
comment on column {_options.DbSchema}.system_config_history.value is '配置值';
comment on column {_options.DbSchema}.system_config_history.version is '版本标识';
comment on column {_options.DbSchema}.system_config_history.update_user_id is '更新人id';
comment on column {_options.DbSchema}.system_config_history.update_time is '更新时间';");

        // 创建触发器
        sb.Append($@"DROP TRIGGER IF EXISTS ""trigger_AddSystemConfigFlow"" ON {_options.DbSchema}.system_config;
DROP FUNCTION IF EXISTS {_options.DbSchema}.""func_addSystemConfigFlow""();

CREATE OR REPLACE FUNCTION {_options.DbSchema}.""func_addSystemConfigFlow""() RETURNS trigger AS $BODY$ BEGIN
        INSERT INTO {_options.DbSchema}.system_config_history (""key"",""value"",version,update_user_id,update_time)
        VALUES (OLD.key,OLD.value,OLD.version,1,CURRENT_TIMESTAMP);

    RETURN NEW;

END $BODY$ LANGUAGE plpgsql ;

CREATE TRIGGER ""trigger_AddSystemConfigFlow"" AFTER UPDATE OF ""value"" ON {_options.DbSchema}.system_config FOR EACH ROW
EXECUTE PROCEDURE {_options.DbSchema}.""func_addSystemConfigFlow""();");
        _logger.LogInformation($"queryDb  创建表SQL：{sb}");
        return await _dapperRepository.ExecuteAsync(sb.ToString()) > 0;
    }

    public async Task<List<GetSettingInfoDto>> GetPageListAsync(int pageIndex, int pageSize, string keyword,
        string version)
    {
        var sb = new StringBuilder(
            $"select config.id,config.key,config.name,config.value,config.description,config.version ");
        sb.Append($"from {_options.DbSchema}.system_config config where config.is_deleted=false")
            .AppendIF(keyword.IsNotNullOrWhiteSpace(),
                " and (config.key like @keyword or config.name like @keyword)")
            .AppendIF(version.IsNotNullOrWhiteSpace(), " and config.version='@version'")
            .Append($" order by config.create_time desc limit {pageSize} offset  {pageSize * (pageIndex - 1)};");
        _logger.LogInformation($"SQL：{sb}");
        return await _dapperRepository.QueryAsync<GetSettingInfoDto>(sb.ToString(),
            new { keyword = $"%{keyword}%", version });
    }

    public async Task<int> GetConfigCount()
    {
        var sql = $"select count(key) from {_options.DbSchema}.system_config where is_deleted=false";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.ExecuteScalarAsync<int>(sql);
    }

    public async Task<GetConfigDetailsResult> GetConfigDetails(int configId)
    {
        var sql =
            $@"select config.key,config.name,config.value,config.description,config.version,config.id
from {_options.DbSchema}.system_config config
                     where config.is_deleted=false and config.id={configId}";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.QueryFirstOrDefaultAsync<GetConfigDetailsResult>(sql);
    }

    public async Task<GetConfigInfoDto> GetConfigInfoAsync(int configId)
    {
        var sql = $"select key,name from {_options.DbSchema}.system_config where id=@id";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.QueryFirstOrDefaultAsync<GetConfigInfoDto>(sql, new { id = configId });
    }

    public async Task<string> GetConfigKeyAsync(int configVersionId)
    {
        var sql = $"select key from {_options.DbSchema}.system_config where id=@id";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.ExecuteScalarAsync<string>(sql, new { id = configVersionId });
    }

    public async Task<bool> UpdateConfigVersionAsync(int configId, string value, string description,
        string updateUserId)
    {
        var sql =
            $"update {_options.DbSchema}.system_config set value=@value,description=@description,update_time=@update_time,update_user_id=@update_user_id where id=@configId";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.ExecuteAsync(sql,
            new
            {
                configId,
                value,
                description,
                update_time = DateTime.Now,
                update_user_id = updateUserId
            }) > 0;
    }

    public async Task<List<GetConfigVersionListResult>> GetConfigHistoryListAsync(string key)
    {
        var sql =
            $@"select id hisoryId,key,value,version,update_time updateTime from {_options.DbSchema}.system_config_history where key=@key  order by  update_time desc";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.QueryAsync<GetConfigVersionListResult>(sql, new { key });
    }

    public async Task<bool> DeleteConfigAsync(int configId)
    {
        var sql = $"update {_options.DbSchema}.system_config set is_deleted=true where id=@configId";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.ExecuteAsync(sql, new { configId }) > 0;
    }

    public async Task<bool> RestoreConfigAsync(int historyId)
    {
        var sql = $@"update {_options.DbSchema}.system_config config set value=history.value
from  (select key,value from {_options.DbSchema}.system_config_history where id=@historyId) history
where config.key=history.key";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.ExecuteAsync(sql, new { historyId }) > 0;
    }

    public async Task<string> GetConfigValueAsync(string key)
    {
        var sql =
            $@"select config.value
from {_options.DbSchema}.system_config config
                     where config.is_deleted=false and config.key=@key";
        _logger.LogInformation($"SQL：{sql}");
        return await _dapperRepository.ExecuteScalarAsync<string>(sql, new { key });
    }

    public async Task<bool> UpdateConfigValueAsync(string key, string value, string updateUserId = null)
    {
        var sql =
            $"update {_options.DbSchema}.system_config set value=@value,update_user_id=@update_user_id,update_time=@update_time where key=@key";
        return await _dapperRepository.ExecuteAsync(sql,
                   new
                   {
                       key,
                       value,
                       update_user_id = updateUserId ?? "admin",
                       update_time = DateTime.Now.ToNowDateTime()
                   }) >
               0;
    }

    public async Task<bool> AddConfigListAsync(List<AddSettingInfoDto> addSettingInfos)
    {
        var codes = addSettingInfos.Select(t => t.Key).ToList();
        // 查询已经存在的code
        var existCodes = await _dapperRepository.QueryAsync<string>(
            $"select key from {_options.DbSchema}.system_config where key =ANY(@codes)",
            new { codes });

        // 留下不存在的key
        addSettingInfos = addSettingInfos.Where(t => !existCodes.Contains(t.Key)).ToList();
        // 将该集合批量添加到数据库中
        if (!addSettingInfos.Any())
        {
            return false;
        }

        var sb = new StringBuilder();
        sb.Append(
            $"insert into {_options.DbSchema}.system_config(key,name,value,description,version,create_time,create_user_id,update_user_id,update_time,is_deleted) values ");
        foreach (var addSettingInfo in addSettingInfos)
        {
            sb.Append(
                $"('{addSettingInfo.Key}','{addSettingInfo.Name}','{addSettingInfo.Value}','{addSettingInfo.Description}','{addSettingInfo.Version}',now(),'admin','admin',now(),false),");
        }

        sb.Remove(sb.Length - 1, 1);
        return await _dapperRepository.ExecuteAsync(sb.ToString()) > 0;
    }
}