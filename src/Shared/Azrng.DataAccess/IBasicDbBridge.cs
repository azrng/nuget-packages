using Azrng.Core.Model;
using Azrng.DataAccess.Dto;

namespace Azrng.DataAccess
{
    /// <summary>
    /// 数据库系统操作
    /// </summary>
    public interface IBasicDbBridge
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        DatabaseType DatabaseType { get; }

        /// <summary>
        /// 数据库帮助类
        /// </summary>
        IDbHelper DbHelper { get; }

        /// <summary>
        /// 获取schema name列表
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetSchemaNameListAsync();

        /// <summary>
        /// 获取数据库名列表
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetDatabaseNameListAsync();

        /// <summary>
        /// 获取schema信息列表
        /// </summary>
        /// <returns></returns>
        Task<List<GetSchemaListDto>> GetSchemaListAsync();

        /// <summary>
        /// 获取表列表
        /// </summary>
        /// <returns></returns>
        Task<List<SchemaTableDto>> GetTableNameListAsync();

        /// <summary>
        /// 获取表信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<GetTableInfoBySchemaDto>> GetTableInfoListAsync(string schemaName);

        /// <summary>
        /// 查询表信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<GetTableInfoBySchemaDto?> GetTableInfoAsync(string schemaName, string tableName);

        /// <summary>
        /// 查询列信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<List<ColumnInfoDto>> GetColumnListAsync(string schemaName, string tableName);

        /// <summary>
        /// 查询列信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<ColumnInfoDto>> GetColumnListAsync(string schemaName);

        /// <summary>
        /// 查询主键信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<List<PrimaryModel>> GetPrimaryListAsync(string schemaName, string tableName);

        /// <summary>
        /// 查询主键信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<PrimaryModel>> GetPrimaryListAsync(string schemaName);

        /// <summary>
        /// 查询外键信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<List<ForeignModel>> GetForeignListAsync(string schemaName, string tableName);

        /// <summary>
        /// 查询外键信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<ForeignModel>> GetForeignListAsync(string schemaName);

        /// <summary>
        /// 查询索引信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<List<IndexModel>> GetIndexListAsync(string schemaName, string tableName);

        /// <summary>
        /// 查询索引信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<IndexModel>> GetIndexListAsync(string schemaName);

        /// <summary>
        /// 查询视图列表
        /// </summary>
        /// <returns></returns>
        Task<List<ViewModel>> GetViewListAsync();

        /// <summary>
        /// 查询视图信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<ViewModel>> GetSchemaViewListAsync(string schemaName);

        /// <summary>
        /// 查询单个视图信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        Task<ViewModel?> GetSchemaViewAsync(string schemaName, string viewName);

        /// <summary>
        /// 获取数据库下存储过程
        /// </summary>
        /// <returns></returns>
        Task<List<DbProcModel>> GetProcListAsync();

        /// <summary>
        /// 查询存储过程信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<ProcModel>> GetSchemaProcListAsync(string schemaName);

        /// <summary>
        /// 查询 schema 下所有例程信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        Task<List<RoutineModel>> GetSchemaRoutineListAsync(string schemaName);

        /// <summary>
        /// 查询单个例程信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="routineName"></param>
        /// <returns></returns>
        Task<RoutineModel?> GetSchemaRoutineAsync(string schemaName, string routineName);

        /// <summary>
        /// 查询表时间戳
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<TableTimestampDto?> GetTableTimestampAsync(string schemaName, string tableName);
    }
}
